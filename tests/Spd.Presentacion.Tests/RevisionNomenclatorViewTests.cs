using Avalonia.Headless.XUnit;
using Microsoft.Data.Sqlite;
using Spd.Aplicacion;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Spd.Presentacion.Views.Medicamentos;
using Xunit;

namespace Spd.Presentacion.Tests;

/// <summary>Regresión (mismo patrón que `CatalogoMedicamentosViewTests`): `RevisionNomenclatorView`
/// usa dos `ListBox` con `ItemTemplate` y bindings de comando a un ancestro.</summary>
public sealed class RevisionNomenclatorViewTests
{
    [AvaloniaFact]
    public void RevisionNomenclatorWindow_se_construye_y_muestra_con_una_fila_real_sin_lanzar()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();

        var auditoria = new RegistradorAuditoria(conexion);
        var repositorio = new RepositorioMedicamentos(conexion);
        var servicioMedicamentos = new ServicioMedicamentos(repositorio, auditoria);
        servicioMedicamentos.Crear(new DatosAltaMedicamento("111111", "Nombre antiguo"), usuarioQueEjecutaId: null);

        var directorioNomenclator = Path.Combine(AppContext.BaseDirectory, "nomenclator");
        Directory.CreateDirectory(directorioNomenclator);
        File.WriteAllText(
            Path.Combine(directorioNomenclator, "nomenclator.csv"),
            "CN,Nombre\n111111,Nombre nuevo del laboratorio\n222222,Ibuprofeno 600mg\n");

        var servicioImportacion = new ServicioImportacionNomenclator(new LectorNomenclatorCsv(), repositorio, auditoria);
        var ventana = new RevisionNomenclatorWindow(servicioImportacion, usuarioActualId: null);

        // Show() fuerza la realización del ItemTemplate de ambos ListBox con filas reales.
        ventana.Show();
    }
}
