using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Aplicacion;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Xunit;

namespace Spd.Aplicacion.Tests;

public sealed class ServicioCifradoTests : IDisposable
{
    private readonly List<string> _carpetasTemporales = [];

    private (ServicioCifrado Servicio, SqliteConnection Conexion, string RutaDb) Crear()
    {
        var carpeta = Path.Combine(Path.GetTempPath(), $"spd-cifrado-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(carpeta);
        _carpetasTemporales.Add(carpeta);

        var rutaDb = Path.Combine(carpeta, "spd.db");
        var conexion = new SqliteConnection($"Data Source={rutaDb};Pooling=False");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();

        var auditoria = new RegistradorAuditoria(conexion);
        var servicio = new ServicioCifrado(conexion, rutaDb, carpeta, auditoria, new GeneradorFraseRecuperacion());
        return (servicio, conexion, rutaDb);
    }

    // Activa el cifrado con el flujo completo de dos pasos (generar → confirmar), igual que lo
    // haría la Presentación tras mostrar la frase y marcar la casilla de confirmación.
    private static ResultadoActivarCifrado Activar(ServicioCifrado servicio, string contrasenaMaestra, int administradorId = 1)
    {
        var (mek, frase) = servicio.GenerarClaveYFraseNuevas();
        return servicio.ActivarCifrado(mek, frase, contrasenaMaestra, administradorId);
    }

    [Fact]
    public void EstaActivo_es_falso_para_una_base_recien_creada_sin_cifrar()
    {
        var (servicio, _, _) = Crear();

        Assert.False(servicio.EstaActivo());
    }

    [Fact]
    public void GenerarClaveYFraseNuevas_no_escribe_nada_hasta_que_se_llama_a_ActivarCifrado()
    {
        // CA-1003: la frase se genera y se muestra ANTES de tocar la base de datos; generar la
        // frase por sí sola no debe activar el cifrado.
        var (servicio, _, _) = Crear();

        var (mek, frase) = servicio.GenerarClaveYFraseNuevas();

        Assert.Equal(24, frase.Length);
        Assert.Equal(32, mek.Length);
        Assert.False(servicio.EstaActivo());
    }

    [Fact]
    public void ActivarCifrado_hace_que_abrir_sin_clave_falle_y_con_la_clave_correcta_funcione()
    {
        var (servicio, conexion, rutaDb) = Crear();

        var resultado = Activar(servicio, "contraseña-maestra-inicial");

        Assert.True(resultado.Exito);
        Assert.NotNull(resultado.FraseRecuperacion);
        Assert.Equal(24, resultado.FraseRecuperacion!.Length);
        Assert.True(servicio.EstaActivo());

        // Reabrir SIN clave, con Pooling=False para no reutilizar un descriptor ya autenticado
        // (research.md, hallazgo crítico de la prueba de Decisión 1).
        using (var sinClave = new SqliteConnection($"Data Source={rutaDb};Pooling=False"))
        {
            sinClave.Open();
            Assert.Throws<SqliteException>(() => sinClave.ExecuteScalar<long>("SELECT count(*) FROM sqlite_master;"));
        }

        // Con la clave correcta, sí se puede leer.
        var mek = servicio.DesenvolverMek("contraseña-maestra-inicial", esFraseDeRecuperacion: false);
        Assert.NotNull(mek);
        using (var conClave = new SqliteConnection($"Data Source={rutaDb};Pooling=False"))
        {
            conClave.Open();
            conClave.Execute($"PRAGMA key = \"x'{Convert.ToHexString(mek!)}'\";");
            var total = conClave.ExecuteScalar<long>("SELECT count(*) FROM sqlite_master;");
            Assert.True(total > 0);
        }

        conexion.Dispose();
    }

    [Fact]
    public void DesenvolverMek_con_contrasena_o_con_frase_de_recuperacion_da_la_misma_clave()
    {
        var (servicio, conexion, _) = Crear();
        var resultado = Activar(servicio, "contraseña-maestra");

        var mekPorContrasena = servicio.DesenvolverMek("contraseña-maestra", esFraseDeRecuperacion: false);
        var mekPorFrase = servicio.DesenvolverMek(string.Join(' ', resultado.FraseRecuperacion!), esFraseDeRecuperacion: true);

        Assert.NotNull(mekPorContrasena);
        Assert.Equal(mekPorContrasena, mekPorFrase);

        conexion.Dispose();
    }

    [Fact]
    public void DesenvolverMek_con_secreto_incorrecto_devuelve_null()
    {
        var (servicio, conexion, _) = Crear();
        Activar(servicio, "contraseña-maestra");

        Assert.Null(servicio.DesenvolverMek("contraseña-incorrecta", esFraseDeRecuperacion: false));

        conexion.Dispose();
    }

    [Fact]
    public void CambiarContrasenaMaestra_invalida_la_contrasena_anterior_y_genera_una_frase_nueva()
    {
        var (servicio, conexion, rutaDb) = Crear();
        var activacion = Activar(servicio, "contraseña-vieja");

        var (mekNueva, fraseNueva) = servicio.GenerarClaveYFraseNuevas();
        var resultado = servicio.CambiarContrasenaMaestra("contraseña-vieja", "contraseña-nueva", mekNueva, fraseNueva, administradorQueEjecutaId: 1);

        Assert.True(resultado.Exito);
        Assert.NotEqual(activacion.FraseRecuperacion, resultado.FraseRecuperacionNueva);
        Assert.Null(servicio.DesenvolverMek("contraseña-vieja", esFraseDeRecuperacion: false));
        var mekActual = servicio.DesenvolverMek("contraseña-nueva", esFraseDeRecuperacion: false);
        Assert.NotNull(mekActual);
        Assert.Equal(mekNueva, mekActual);

        // La conexión ya autenticada (rekey in situ) sigue sirviendo, sin haber exportado nada.
        var total = conexion.ExecuteScalar<long>("SELECT count(*) FROM sqlite_master;");
        Assert.True(total > 0);

        conexion.Dispose();
    }

    [Fact]
    public void DesactivarCifrado_deja_el_fichero_legible_sin_clave()
    {
        var (servicio, conexion, rutaDb) = Crear();
        Activar(servicio, "contraseña-maestra");

        var resultado = servicio.DesactivarCifrado("contraseña-maestra", administradorQueEjecutaId: 1);

        Assert.True(resultado.Exito);
        Assert.False(servicio.EstaActivo());
        using (var sinClave = new SqliteConnection($"Data Source={rutaDb};Pooling=False"))
        {
            sinClave.Open();
            var total = sinClave.ExecuteScalar<long>("SELECT count(*) FROM sqlite_master;");
            Assert.True(total > 0);
        }

        conexion.Dispose();
    }

    [Fact]
    public void DesactivarCifrado_con_contrasena_incorrecta_falla_sin_tocar_el_fichero()
    {
        var (servicio, conexion, _) = Crear();
        Activar(servicio, "contraseña-maestra");

        var resultado = servicio.DesactivarCifrado("contraseña-incorrecta", administradorQueEjecutaId: 1);

        Assert.False(resultado.Exito);
        Assert.True(servicio.EstaActivo());

        conexion.Dispose();
    }

    [Fact]
    public void Cada_operacion_registra_en_auditoria()
    {
        var (servicio, conexion, _) = Crear();
        Activar(servicio, "contraseña-maestra");
        var (mekNueva, fraseNueva) = servicio.GenerarClaveYFraseNuevas();
        servicio.CambiarContrasenaMaestra("contraseña-maestra", "contraseña-nueva", mekNueva, fraseNueva, administradorQueEjecutaId: 1);
        servicio.DesactivarCifrado("contraseña-nueva", administradorQueEjecutaId: 1);

        var acciones = conexion.Query<string>("SELECT accion FROM Auditoria ORDER BY id").ToList();

        Assert.Contains("ACTIVAR_CIFRADO", acciones);
        Assert.Contains("CAMBIAR_CONTRASENA_MAESTRA", acciones);
        Assert.Contains("DESACTIVAR_CIFRADO", acciones);

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
