using Avalonia.Headless.XUnit;
using Microsoft.Data.Sqlite;
using Spd.Aplicacion;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Spd.Presentacion.Views.Medicamentos;
using Spd.Presentacion.Navegacion;
using Spd.Presentacion.ViewModels;
using Xunit;

namespace Spd.Presentacion.Tests;

/// <summary>Regresión (mismo patrón que `BuscadorPacientesViewTests`/`UsuariosWindowTests`):
/// `CatalogoMedicamentosView` usa un `ListBox` con `ItemTemplate` y un binding de comando a un
/// ancestro, la categoría de bug que causó un cierre inesperado (SIGABRT) en Spec 000.</summary>
public sealed class CatalogoMedicamentosViewTests
{
    [AvaloniaFact]
    public void CatalogoMedicamentosWindow_se_construye_y_muestra_con_un_medicamento_real_sin_lanzar()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();

        var auditoria = new RegistradorAuditoria(conexion);
        var repositorio = new RepositorioMedicamentos(conexion);
        var servicio = new ServicioMedicamentos(repositorio, auditoria);
        servicio.Crear(new DatosAltaMedicamento("654321", "Paracetamol 1g"), usuarioQueEjecutaId: null);
        var servicioImportacion = new ServicioImportacionNomenclator(new LectorNomenclatorCsv(), repositorio, auditoria);
        var servicioConsultaCima = new ServicioConsultaCima(new HttpClient(), auditoria);

        var ventana = AnfitrionDeVista.Anfitrion(new CatalogoMedicamentosView { DataContext = new CatalogoMedicamentosViewModel(servicio, servicioImportacion, servicioConsultaCima, new Navegador(true), usuarioActualId: null) { Fragmento = "654321" } });

        // Show() fuerza la realización del ItemTemplate del ListBox para el medicamento recién creado.
        ventana.Show();
    }
}
