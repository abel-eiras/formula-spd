using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Xunit;

namespace Spd.Aplicacion.Tests;

/// <summary>FR-1014, hallazgo de `/speckit-analyze`: ni ServicioBackupTests ni ServicioCifradoTests
/// prueban la interacción entre las dos user stories. Con el cifrado activo, un backup debe
/// contener una base igual de cifrada — el backup no añade ni quita seguridad.</summary>
public sealed class ServicioBackupConCifradoTests : IDisposable
{
    private readonly List<string> _carpetasTemporales = [];

    private string NuevaCarpetaTemporal()
    {
        var ruta = Path.Combine(Path.GetTempPath(), $"spd-backup-cifrado-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(ruta);
        _carpetasTemporales.Add(ruta);
        return ruta;
    }

    [Fact]
    public void GenerarBackup_de_una_base_cifrada_produce_un_zip_cuya_base_interna_tambien_exige_la_clave()
    {
        var carpetaInstalacion = NuevaCarpetaTemporal();
        var rutaDb = Path.Combine(carpetaInstalacion, "spd.db");
        var conexion = new SqliteConnection($"Data Source={rutaDb};Pooling=False");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();

        var auditoria = new RegistradorAuditoria(conexion);
        var repositorioFarmacia = new RepositorioFarmacia(conexion);
        var rutaBackup = NuevaCarpetaTemporal();
        repositorioFarmacia.Crear(new Farmacia
        {
            CodigoSanitario = "PO-001", Nombre = "Farmacia de Prueba", TitularOComunidadBienes = "Titular",
            Cif = "B00000000", Direccion = "Calle Falsa 1", Cp = "36000", Poblacion = "Pontevedra",
            Telefono = "986000000", RutaBackup = rutaBackup
        });

        var servicioCifrado = new ServicioCifrado(conexion, rutaDb, carpetaInstalacion, auditoria, new GeneradorFraseRecuperacion());
        var (mek, frase) = servicioCifrado.GenerarClaveYFraseNuevas();
        servicioCifrado.ActivarCifrado(mek, frase, "contraseña-maestra", administradorQueEjecutaId: 1);

        var servicioBackup = new ServicioBackup(conexion, repositorioFarmacia, auditoria);
        var resultado = servicioBackup.GenerarBackup(usuarioQueEjecutaId: 1, esAutomatico: false);
        Assert.True(resultado.Exito);

        var carpetaExtraccion = NuevaCarpetaTemporal();
        System.IO.Compression.ZipFile.ExtractToDirectory(resultado.RutaZip!, carpetaExtraccion, overwriteFiles: true);
        var rutaDbExtraida = Path.Combine(carpetaExtraccion, "spd.db");

        using var sinClave = new SqliteConnection($"Data Source={rutaDbExtraida};Pooling=False");
        sinClave.Open();
        Assert.Throws<SqliteException>(() => sinClave.ExecuteScalar<long>("SELECT count(*) FROM sqlite_master;"));

        conexion.Dispose();
    }

    public void Dispose()
    {
        foreach (var carpeta in _carpetasTemporales)
        {
            if (Directory.Exists(carpeta)) Directory.Delete(carpeta, recursive: true);
        }
    }
}
