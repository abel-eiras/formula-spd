using Avalonia.Headless.XUnit;
using Microsoft.Data.Sqlite;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Spd.Presentacion.Views.Preparacion;
using Xunit;

namespace Spd.Presentacion.Tests;

/// <summary>Regresión F5 de `/speckit-analyze` (Spec 001): mismo patrón de `ListBox`+`ItemTemplate`
/// con comando de ancestro; se fuerza la realización con un SPD real ya creado.</summary>
public sealed class PreparacionViewTests
{
    [AvaloniaFact]
    public void PreparacionWindow_se_construye_y_muestra_con_un_spd_real_sin_lanzar()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();

        var repositorioFarmacia = new RepositorioFarmacia(conexion);
        repositorioFarmacia.Crear(new Farmacia
        {
            CodigoSanitario = "PO-001", Nombre = "Farmacia de Prueba", TitularOComunidadBienes = "Titular",
            Cif = "B00000000", Direccion = "Calle Falsa 1", Cp = "36000", Poblacion = "Pontevedra",
            Telefono = "986000000", PrefijoNumSpd = "F-"
        });
        var auditoria = new RegistradorAuditoria(conexion);
        var repositorioPacientes = new RepositorioPacientes(conexion);
        var servicioPacientes = new ServicioPacientes(repositorioPacientes, repositorioFarmacia, auditoria);
        var diaLejano = DiasSemana.Codigos[((int)DateTime.Today.DayOfWeek + 6 + 3) % 7];
        var paciente = servicioPacientes.Crear(
            new DatosAltaPaciente("José", "Núñez", null, "12345678Z", null, null, null, null, null, null,
                null, null, null, null, null, null, null, false, null, diaLejano, 1),
            usuarioQueEjecutaId: null);
        servicioPacientes.CambiarEstado(paciente.Id, EstadoPaciente.Activo, null, null);

        var repositorioMedicamentos = new RepositorioMedicamentos(conexion);
        var medicamento = new Medicamento { Cn = "654321", Nombre = "Paracetamol 1g", UnidadesEnvase = 28 };
        medicamento.Id = repositorioMedicamentos.Crear(medicamento);

        var repositorioTratamientos = new RepositorioTratamientos(conexion);
        repositorioTratamientos.Crear(new Tratamiento
        {
            PacienteId = paciente.Id, MedicamentoId = medicamento.Id, EnSpd = true, PautaD = FraccionDosis.Uno,
            FechaInicio = new DateOnly(2026, 1, 1), FechaPrescripcionInicial = new DateOnly(2026, 1, 1)
        });

        var repositorioEnvases = new RepositorioEnvases(conexion);
        repositorioEnvases.Crear(new Envase
        {
            PacienteId = paciente.Id, MedicamentoId = medicamento.Id, Serie = "S1",
            Caducidad = new DateOnly(2030, 1, 1), UnidadesIniciales = 28, UnidadesRestantes = 28
        });

        var servicioEnvases = new ServicioEnvases(repositorioEnvases, repositorioTratamientos, repositorioPacientes, auditoria);
        var servicioListadoRetirada = new ServicioListadoRetirada(
            repositorioPacientes, new RepositorioContactos(conexion), repositorioTratamientos, repositorioMedicamentos,
            repositorioEnvases, repositorioFarmacia, new ComprobadorCoberturaSpdNulo(), auditoria);
        var servicioPreparacion = new ServicioPreparacion(
            new RepositorioSpd(conexion), new RepositorioSpdLineas(conexion), new RepositorioSpdLineaEnvases(conexion),
            new RepositorioSpdVerificaciones(conexion), new RepositorioSpdModificaciones(conexion),
            new RepositorioRegistrosAmbientales(conexion), new RepositorioMaterialAcondicionamiento(conexion),
            repositorioPacientes, repositorioTratamientos, repositorioMedicamentos, repositorioEnvases,
            repositorioFarmacia, new ServicioAsignacionEnvases(repositorioEnvases, repositorioTratamientos, auditoria),
            servicioEnvases, servicioListadoRetirada, new ComprobadorIdoneidadYConsentimientoNulo(), auditoria);
        var servicioMedicamentos = new ServicioMedicamentos(repositorioMedicamentos, auditoria);

        var repositorioUsuarios = new RepositorioUsuarios(conexion);
        var elaborador = new Usuario { Nombre = "Elena", Apellidos = "Ruiz", Login = "elena", HashPassword = "x", Rol = Rol.Elaborador };
        elaborador.Id = repositorioUsuarios.Crear(elaborador);

        servicioPreparacion.CrearSesion(paciente.Id, elaborador.Id);

        var ventana = new PreparacionWindow(servicioPreparacion, servicioMedicamentos, paciente.Id, usuarioActualId: null);

        ventana.Show();
    }
}
