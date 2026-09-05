using Avalonia.Headless.XUnit;
using Microsoft.Data.Sqlite;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Spd.Presentacion.ViewModels;
using Spd.Presentacion.Views;
using Xunit;

namespace Spd.Presentacion.Tests;

public sealed class MainWindowTests
{
    [AvaloniaFact]
    public void MainWindow_se_construye_y_muestra_para_un_Administrador_sin_lanzar()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();

        var repositorioUsuarios = new RepositorioUsuarios(conexion);
        var auditoria = new RegistradorAuditoria(conexion);
        var hasheador = new HasheadorArgon2id();
        var servicioUsuarios = new ServicioUsuarios(repositorioUsuarios, hasheador, auditoria);
        var administrador = servicioUsuarios.CrearUsuario(
            new DatosAltaUsuario("Ana", "Administradora", "ana.admin", "contraseña-inicial", Rol.Administrador, null, null),
            administradorQueEjecutaId: null).Usuario;

        var repositorioFarmacia = new RepositorioFarmacia(conexion);
        repositorioFarmacia.Crear(new Farmacia
        {
            CodigoSanitario = "PO-001", Nombre = "Farmacia de Prueba", TitularOComunidadBienes = "Titular",
            Cif = "B00000000", Direccion = "Calle Falsa 1", Cp = "36000", Poblacion = "Pontevedra",
            Telefono = "986000000"
        });
        var servicioFarmacia = new ServicioConfiguracionFarmacia(repositorioFarmacia, auditoria);
        var gestorLogo = new GestorLogoFarmacia();
        var servicioActualizaciones = new ServicioActualizaciones(new HttpClient(), auditoria);
        var servicioNomenclator = new ServicioNomenclator(new HttpClient(), auditoria);
        var servicioMedicamentos = new ServicioMedicamentos(new RepositorioMedicamentos(conexion), auditoria);

        var viewModel = new MainViewModel(
            servicioUsuarios, servicioFarmacia, gestorLogo, servicioActualizaciones, servicioNomenclator,
            servicioMedicamentos, administrador);
        var ventana = new MainWindow { DataContext = viewModel };

        ventana.Show();
    }
}
