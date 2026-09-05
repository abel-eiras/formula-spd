using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Xunit;

namespace Spd.Aplicacion.Tests;

public sealed class ServicioBackupTests : IDisposable
{
    private readonly List<string> _carpetasTemporales = [];

    private string NuevaCarpetaTemporal()
    {
        var ruta = Path.Combine(Path.GetTempPath(), $"spd-backup-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(ruta);
        _carpetasTemporales.Add(ruta);
        return ruta;
    }

    private static (ServicioBackup Servicio, SqliteConnection Conexion, string RutaBackup) Crear(string? rutaBackup = null)
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();

        var repositorioFarmacia = new RepositorioFarmacia(conexion);
        var auditoria = new RegistradorAuditoria(conexion);
        repositorioFarmacia.Crear(new Farmacia
        {
            CodigoSanitario = "PO-001", Nombre = "Farmacia de Prueba", TitularOComunidadBienes = "Titular",
            Cif = "B00000000", Direccion = "Calle Falsa 1", Cp = "36000", Poblacion = "Pontevedra",
            Telefono = "986000000", RutaBackup = rutaBackup
        });

        var servicio = new ServicioBackup(conexion, repositorioFarmacia, auditoria);
        return (servicio, conexion, rutaBackup ?? string.Empty);
    }

    [Fact]
    public void GenerarBackup_produce_un_zip_cuya_base_de_datos_pasa_integrity_check()
    {
        var rutaBackup = NuevaCarpetaTemporal();
        var (servicio, _, _) = Crear(rutaBackup);

        var resultado = servicio.GenerarBackup(usuarioQueEjecutaId: 1, esAutomatico: true);

        Assert.True(resultado.Exito);
        Assert.NotNull(resultado.RutaZip);
        Assert.True(File.Exists(resultado.RutaZip));

        var carpetaExtraccion = NuevaCarpetaTemporal();
        System.IO.Compression.ZipFile.ExtractToDirectory(resultado.RutaZip!, carpetaExtraccion, overwriteFiles: true);
        var rutaDbExtraida = Path.Combine(carpetaExtraccion, "spd.db");
        Assert.True(File.Exists(rutaDbExtraida));

        using var conexionExtraida = new SqliteConnection($"Data Source={rutaDbExtraida}");
        conexionExtraida.Open();
        var resultadoIntegridad = conexionExtraida.ExecuteScalar<string>("PRAGMA integrity_check;");
        Assert.Equal("ok", resultadoIntegridad);
    }

    [Fact]
    public void GenerarBackup_con_ruta_inaccesible_devuelve_fallo_explicito_sin_lanzar()
    {
        // Un fichero (no carpeta) en la ruta de backup hace que Directory.CreateDirectory falle.
        var rutaConflictiva = Path.Combine(Path.GetTempPath(), $"spd-backup-conflicto-{Guid.NewGuid():N}");
        File.WriteAllText(rutaConflictiva, "no soy una carpeta");
        try
        {
            var (servicio, _, _) = Crear(rutaConflictiva);

            var resultado = servicio.GenerarBackup(usuarioQueEjecutaId: 1, esAutomatico: true);

            Assert.False(resultado.Exito);
            Assert.NotNull(resultado.Motivo);
        }
        finally
        {
            File.Delete(rutaConflictiva);
        }
    }

    [Fact]
    public void GenerarBackup_registra_en_auditoria_segun_si_es_automatico_o_manual()
    {
        var rutaBackup = NuevaCarpetaTemporal();
        var (servicio, conexion, _) = Crear(rutaBackup);

        servicio.GenerarBackup(usuarioQueEjecutaId: 1, esAutomatico: true);
        servicio.GenerarBackup(usuarioQueEjecutaId: 1, esAutomatico: false);

        var acciones = conexion.Query<string>("SELECT accion FROM Auditoria ORDER BY id").ToList();
        Assert.Contains("BACKUP_AUTOMATICO", acciones);
        Assert.Contains("BACKUP_MANUAL", acciones);
    }

    [Fact]
    public void RestaurarDesdeZip_descomprime_y_registra_en_auditoria_con_el_administrador()
    {
        var rutaBackup = NuevaCarpetaTemporal();
        var (servicio, conexion, _) = Crear(rutaBackup);
        var resultadoBackup = servicio.GenerarBackup(usuarioQueEjecutaId: 1, esAutomatico: false);
        var carpetaDestino = NuevaCarpetaTemporal();

        var resultado = servicio.RestaurarDesdeZip(resultadoBackup.RutaZip!, carpetaDestino, administradorQueEjecutaId: 7);

        Assert.True(resultado.Exito);
        Assert.True(File.Exists(Path.Combine(carpetaDestino, "spd.db")));
        var fila = conexion.QuerySingle(
            "SELECT usuario_id AS UsuarioId FROM Auditoria WHERE accion = 'RESTAURAR_BACKUP'");
        Assert.Equal(7L, (long)fila.UsuarioId);
    }

    [Fact]
    public void GenerarBackup_aplica_rotacion_30_diarios_mas_12_mensuales_promovidos()
    {
        var rutaBackup = NuevaCarpetaTemporal();
        var (servicio, _, _) = Crear(rutaBackup);

        // 40 backups "diarios" simulados, con fechas fijas (no dependen del reloj real): del
        // 2020-01-01 al 2020-02-09, un fichero por día. Un contenido vacío basta: la rotación solo
        // lee el nombre del fichero (research.md Decisión 6).
        var fechaBase = new DateTime(2020, 1, 1, 1, 0, 0);
        for (var i = 0; i < 40; i++)
        {
            var fecha = fechaBase.AddDays(i);
            File.WriteAllText(Path.Combine(rutaBackup, $"spd-{fecha:yyyyMMdd-HHmm}.zip"), "relleno");
        }

        // El backup 41, con la fecha real de hoy — siempre el más reciente de todos.
        var resultado = servicio.GenerarBackup(usuarioQueEjecutaId: 1, esAutomatico: true);
        Assert.True(resultado.Exito);

        var restantes = Directory.EnumerateFiles(rutaBackup, "spd-*.zip").Select(Path.GetFileName).ToHashSet();

        // Enero (mes con más backups, día 1 a 31) solo conserva su primero (2020-01-01) como
        // mensual promovido; los días 2020-01-02..2020-01-11 quedan fuera de los 30 diarios y no
        // son el primero de ningún mes, así que se eliminan.
        Assert.Contains("spd-20200101-0100.zip", restantes);
        Assert.DoesNotContain("spd-20200105-0100.zip", restantes);
        // Los últimos días de la ventana (dentro de los 30 diarios más recientes) sobreviven.
        Assert.Contains("spd-20200209-0100.zip", restantes);
        Assert.Contains(resultado.RutaZip is not null ? Path.GetFileName(resultado.RutaZip) : "", restantes);
        // 30 diarios + el primero de enero, promovido (el resto de meses ya cae dentro de los 30 diarios).
        Assert.Equal(31, restantes.Count);
    }

    public void Dispose()
    {
        foreach (var carpeta in _carpetasTemporales)
        {
            if (Directory.Exists(carpeta)) Directory.Delete(carpeta, recursive: true);
        }
    }
}
