using System.Security.Cryptography;
using System.Text;
using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;

namespace Spd.Aplicacion.Tests;

/// <summary>FR-053 de punta a punta: API de GitHub simulada, una «instalación» en una carpeta temporal y
/// claves efímeras. Cada fallo tiene que dejar la instalación exactamente como estaba.</summary>
public sealed class ServicioInstalacionActualizacionesTests : IDisposable
{
    private const string Etiqueta = "v0.2.0";
    private const string Exe = "FormulaSPD.exe";
    private const string NombrePaquete = "FormulaSPD-0.2.0-windows-x64.zip";
    private const string UrlRelease = "https://api.github.com/repos/abel-eiras/formula-spd/releases/tags/v0.2.0";
    private const string UrlDescargas = "https://github.com/abel-eiras/formula-spd/releases/download/v0.2.0/";

    private readonly CarpetaTemporalDePrueba _carpeta = new();
    private readonly RoutingHttpMessageHandler _handler = new();
    private readonly ECDsa _clave = ECDsa.Create(ECCurve.NamedCurves.nistP256);
    private readonly SqliteConnection _conexion = new("Data Source=:memory:");
    private readonly BackupFalso _backup = new();
    private readonly byte[] _paquete;

    public ServicioInstalacionActualizacionesTests()
    {
        _conexion.Open();
        new AplicadorMigraciones(_conexion).Aplicar();

        _carpeta.Escribir(Path.Combine("instalacion", Exe), "exe viejo");
        _carpeta.Escribir(Path.Combine("instalacion", "spd.db"), "datos de la farmacia");
        _backup.AlGenerar = () => File.ReadAllText(RutaInstalada(Exe));

        var raiz = ExtractorPaqueteActualizacionTests.CrearCarpetaDePaquete(_carpeta, "FormulaSPD-0.2.0-windows-x64", Exe, "exe nuevo");
        _paquete = File.ReadAllBytes(ExtractorPaqueteActualizacionTests.CrearZip(_carpeta, raiz, NombrePaquete));
    }

    public void Dispose()
    {
        _clave.Dispose();
        _conexion.Dispose();
        _carpeta.Dispose();
    }

    private string Instalacion => _carpeta.Sub("instalacion");

    private string RutaInstalada(string relativa) => Path.Combine(Instalacion, relativa);

    private ServicioInstalacionActualizaciones Servicio(string? procesoEnMarcha = null)
        => new(
            new HttpClient(_handler) { BaseAddress = new Uri("https://api.github.com/") },
            _backup, new RegistradorAuditoria(_conexion),
            new EntornoInstalacion(Instalacion, procesoEnMarcha ?? RutaInstalada(Exe), "windows-x64"),
            _clave.ExportSubjectPublicKeyInfoPem());

    /// <summary>Publica en el GitHub simulado una release como la que genera la CI.</summary>
    private void Publicar(bool conFirma = true, string? sumaDelPaquete = null, byte[]? firma = null)
    {
        var sumas = Encoding.UTF8.GetBytes(
            $"{sumaDelPaquete ?? Convert.ToHexString(SHA256.HashData(_paquete)).ToLowerInvariant()}  {NombrePaquete}\n"
            + "0000  FormulaSPD-0.2.0-linux-x64.tar.gz\n");
        var nombres = new List<string> { NombrePaquete, "FormulaSPD-0.2.0-linux-x64.tar.gz", "SHA256SUMS.txt" };
        if (conFirma) nombres.Add("SHA256SUMS.txt.sig");

        var assets = string.Join(",", nombres.Select(n => $$"""{"name":"{{n}}","browser_download_url":"{{UrlDescargas}}{{n}}","size":10}"""));
        _handler.Responder(UrlRelease, $$"""{"tag_name":"{{Etiqueta}}","assets":[{{assets}}]}""");
        _handler.Responder(UrlDescargas + NombrePaquete, _paquete);
        _handler.Responder(UrlDescargas + "SHA256SUMS.txt", sumas);
        _handler.Responder(UrlDescargas + "SHA256SUMS.txt.sig", firma ?? VerificadorFirmaActualizacionesTests.Firmar(_clave, sumas));
    }

    private Task<ResultadoInstalacionActualizacion> Instalar(IProgress<int>? progreso = null)
        => Servicio().InstalarAsync(Etiqueta, 1, progreso, CancellationToken.None);

    private void AssertInstalacionIntacta()
    {
        Assert.Equal("exe viejo", File.ReadAllText(RutaInstalada(Exe)));
        Assert.False(File.Exists(RutaInstalada(Exe + ".anterior")));
        Assert.False(File.Exists(RutaInstalada("LEEME.txt")));
        Assert.Equal("datos de la farmacia", File.ReadAllText(RutaInstalada("spd.db")));
        Assert.False(Directory.Exists(RutaInstalada("actualizacion")));
    }

    private string[] Auditoria()
        => _conexion.Query<string>("SELECT detalle FROM Auditoria WHERE accion = 'INSTALAR_ACTUALIZACION'").ToArray();

    [Fact]
    public async Task Con_todo_correcto_hace_copia_antes_de_sustituir_instala_y_limpia()
    {
        Publicar();
        var porcentajes = new List<int>();

        var resultado = await Instalar(new ProgresoSincrono(porcentajes.Add));

        Assert.True(resultado.Exito, resultado.Motivo);
        Assert.Equal(RutaInstalada(Exe), resultado.RutaEjecutable);
        Assert.Equal("exe nuevo", File.ReadAllText(RutaInstalada(Exe)));
        Assert.Equal("exe viejo", File.ReadAllText(RutaInstalada(Exe + ".anterior")));
        Assert.Equal("leeme nuevo", File.ReadAllText(RutaInstalada("LEEME.txt")));
        Assert.Equal("datos de la farmacia", File.ReadAllText(RutaInstalada("spd.db")));
        Assert.Equal(["exe viejo"], _backup.EstadoAlHacerLaCopia);
        Assert.False(Directory.Exists(RutaInstalada("actualizacion")));
        Assert.Equal(100, porcentajes.Last());
        Assert.Equal(["version=v0.2.0"], Auditoria());
    }

    [Fact]
    public async Task Una_release_sin_firma_no_se_instala_y_se_ofrece_la_descarga_manual()
    {
        Publicar(conFirma: false);

        var resultado = await Instalar();

        Assert.False(resultado.Exito);
        Assert.True(resultado.SoloManual);
        Assert.Contains("sin firma", resultado.Motivo);
        Assert.DoesNotContain(UrlDescargas + NombrePaquete, _handler.Peticiones);
        AssertInstalacionIntacta();
    }

    [Fact]
    public async Task Una_firma_de_otra_clave_aborta_sin_descargar_el_paquete()
    {
        using var otra = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        Publicar(firma: VerificadorFirmaActualizacionesTests.Firmar(otra, "lo que sea"u8.ToArray()));

        var resultado = await Instalar();

        Assert.False(resultado.Exito);
        Assert.Contains("firma", resultado.Motivo);
        Assert.DoesNotContain(UrlDescargas + NombrePaquete, _handler.Peticiones);
        Assert.Empty(_backup.EstadoAlHacerLaCopia);
        AssertInstalacionIntacta();
    }

    [Fact]
    public async Task Un_paquete_que_no_coincide_con_su_suma_aborta_sin_tocar_nada()
    {
        Publicar(sumaDelPaquete: new string('a', 64));

        var resultado = await Instalar();

        Assert.False(resultado.Exito);
        Assert.Contains("suma de comprobación", resultado.Motivo);
        Assert.Empty(_backup.EstadoAlHacerLaCopia);
        AssertInstalacionIntacta();
    }

    [Fact]
    public async Task Si_la_copia_de_seguridad_falla_no_se_instala_nada()
    {
        Publicar();
        _backup.Resultado = ResultadoBackup.Fallido("No hay ninguna ruta de backup configurada.");

        var resultado = await Instalar();

        Assert.False(resultado.Exito);
        Assert.Contains("copia de seguridad", resultado.Motivo);
        AssertInstalacionIntacta();
    }

    [Fact]
    public async Task Fuera_de_la_version_empaquetada_se_rechaza_sin_abrir_red()
    {
        Publicar();

        var resultado = await Servicio(procesoEnMarcha: "/usr/lib/dotnet/dotnet")
            .InstalarAsync(Etiqueta, 1, null, CancellationToken.None);

        Assert.False(resultado.Exito);
        Assert.True(resultado.SoloManual);
        Assert.Empty(_handler.Peticiones);
        AssertInstalacionIntacta();
    }

    [Fact]
    public async Task Una_etiqueta_que_no_existe_en_github_es_un_fallo_explicado_y_auditado()
    {
        var resultado = await Instalar();

        Assert.False(resultado.Exito);
        Assert.Contains("conexión", resultado.Motivo);
        Assert.Single(Auditoria());
        AssertInstalacionIntacta();
    }

    [Fact]
    public async Task Cancelar_durante_la_descarga_deja_la_instalacion_intacta()
    {
        Publicar();
        using var cancelacion = new CancellationTokenSource();
        cancelacion.Cancel();

        var resultado = await Servicio().InstalarAsync(Etiqueta, 1, null, cancelacion.Token);

        Assert.False(resultado.Exito);
        Assert.Contains("cancelada", resultado.Motivo);
        AssertInstalacionIntacta();
    }

    /// <summary>`Progress&lt;T&gt;` publica en el contexto de sincronización; en un test no lo hay y los
    /// porcentajes llegarían tarde y desordenados.</summary>
    private sealed class ProgresoSincrono(Action<int> alInformar) : IProgress<int>
    {
        public void Report(int value) => alInformar(value);
    }

    /// <summary>Registra en qué estado estaba la instalación en el momento de la copia, para comprobar que se
    /// hace antes de sustituir nada.</summary>
    private sealed class BackupFalso : IServicioBackup
    {
        public ResultadoBackup Resultado { get; set; } = ResultadoBackup.Exitoso("/copias/spd.zip");
        public Func<string>? AlGenerar { get; set; }
        public List<string> EstadoAlHacerLaCopia { get; } = [];

        public ResultadoBackup GenerarBackup(int? usuarioQueEjecutaId, bool esAutomatico)
        {
            if (AlGenerar is not null) EstadoAlHacerLaCopia.Add(AlGenerar());
            return Resultado;
        }

        public IReadOnlyList<BackupInfo> ListarBackups() => throw new NotSupportedException();

        public ResultadoRestauracion RestaurarDesdeZip(string rutaZip, string carpetaDestino, int administradorQueEjecutaId)
            => throw new NotSupportedException();
    }
}
