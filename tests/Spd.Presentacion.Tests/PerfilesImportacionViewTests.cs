using Avalonia.Headless.XUnit;
using Microsoft.Data.Sqlite;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Spd.Presentacion.Views.Configuracion;
using Xunit;

namespace Spd.Presentacion.Tests;

/// <summary>Regresión F5 de `/speckit-analyze` (Spec 001): mismo patrón de `ListBox`+`ItemTemplate`
/// con comando de ancestro; se fuerza la realización con un perfil real.</summary>
public sealed class PerfilesImportacionViewTests
{
    [AvaloniaFact]
    public void PerfilesImportacionWindow_se_construye_y_muestra_con_un_perfil_real_sin_lanzar()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();

        var auditoria = new RegistradorAuditoria(conexion);
        var servicio = new ServicioPerfilesImportacion(new RepositorioPerfilesImportacion(conexion), auditoria);
        servicio.Crear(
            new DatosAltaPerfilImportacion(
                "Perfil de prueba", TipoPerfilImportacion.Pacientes, ",", "UTF-8", true,
                [new ParCampoColumna("Nombre", "0")], null),
            null);

        var ventana = new PerfilesImportacionWindow(servicio, usuarioActualId: null);

        ventana.Show();
    }
}
