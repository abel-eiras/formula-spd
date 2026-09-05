using Avalonia.Headless.XUnit;
using Microsoft.Data.Sqlite;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Spd.Presentacion.Views.Configuracion;
using Xunit;

namespace Spd.Presentacion.Tests;

public sealed class SeguridadWindowTests
{
    [AvaloniaFact]
    public void SeguridadWindow_se_construye_y_muestra_sin_lanzar()
    {
        var carpeta = Path.Combine(Path.GetTempPath(), $"spd-seguridad-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(carpeta);
        var rutaDb = Path.Combine(carpeta, "spd.db");

        var conexion = new SqliteConnection($"Data Source={rutaDb};Pooling=False");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();
        var auditoria = new RegistradorAuditoria(conexion);
        var servicioCifrado = new ServicioCifrado(conexion, rutaDb, carpeta, auditoria, new GeneradorFraseRecuperacion());

        var ventana = new SeguridadWindow(servicioCifrado, administradorActualId: 1);

        ventana.Show();
    }
}
