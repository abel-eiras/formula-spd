using Avalonia.Headless.XUnit;
using Microsoft.Data.Sqlite;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Spd.Presentacion.Views.Configuracion;
using Spd.Presentacion.ViewModels;
using Xunit;

namespace Spd.Presentacion.Tests;

/// <summary>Reproduce el cierre inesperado reportado al abrir Configuración/Usuarios: la
/// DataTemplate del ListBox usaba un patrón de binding ($parent + cast de tipo bajo compiled
/// bindings) que fallaba en tiempo de ejecución al realizar la plantilla con un Usuario real, y el
/// fallo no controlado abortaba el proceso. Este test fuerza esa misma realización.</summary>
public sealed class UsuariosWindowTests
{
    [AvaloniaFact]
    public void UsuariosWindow_se_construye_y_muestra_con_un_usuario_real_sin_lanzar()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();

        var repositorio = new RepositorioUsuarios(conexion);
        var servicio = new ServicioUsuarios(repositorio, new HasheadorArgon2id(), new RegistradorAuditoria(conexion));
        var administrador = servicio.CrearUsuario(
            new DatosAltaUsuario("Ana", "Administradora", "ana.admin", "contraseña-inicial", Rol.Administrador, null, null),
            administradorQueEjecutaId: null);

        var ventana = AnfitrionDeVista.Anfitrion(new UsuariosView { DataContext = new UsuariosViewModel(servicio, administrador.Usuario.Id) });

        // Show() fuerza la realización del ItemTemplate del ListBox para el usuario recién creado
        // — es exactamente el paso en el que se producía el cierre inesperado.
        ventana.Show();
    }
}
