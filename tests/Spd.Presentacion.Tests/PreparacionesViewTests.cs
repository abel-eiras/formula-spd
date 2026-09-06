using Avalonia.Headless.XUnit;
using Microsoft.Data.Sqlite;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Spd.Presentacion.Views.Preparacion;
using Xunit;

namespace Spd.Presentacion.Tests;

/// <summary>Regresión F5: `ListBox`+`ItemTemplate` con comando de ancestro y casilla de selección;
/// se fuerza la realización con un SPD real (Spec 006 FR-690, Spec 007 FR-720).</summary>
public sealed class PreparacionesViewTests
{
    [AvaloniaFact]
    public void PreparacionesWindow_se_construye_y_muestra_con_un_spd_real_sin_lanzar()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();

        var repositorioFarmacia = new RepositorioFarmacia(conexion);
        repositorioFarmacia.Crear(new Farmacia
        {
            CodigoSanitario = "PO-001", Nombre = "Farmacia de Prueba", TitularOComunidadBienes = "Titular", Cif = "B00000000",
            Direccion = "Calle Falsa 1", Cp = "36000", Poblacion = "Pontevedra", Telefono = "986000000", PrefijoNumSpd = "F-", DiasAntelacionListado = 0
        });
        var auditoria = new RegistradorAuditoria(conexion);
        var repositorioPacientes = new RepositorioPacientes(conexion);
        var servicioPacientes = new ServicioPacientes(repositorioPacientes, repositorioFarmacia, auditoria);
        var diaLejano = DiasSemana.Codigos[((int)DateTime.Today.DayOfWeek + 6 + 3) % 7];
        var paciente = servicioPacientes.Crear(
            new DatosAltaPaciente("José", "Núñez", null, "12345678Z", null, null, null, null, null, null,
                null, null, null, null, null, null, null, false, null, diaLejano, 1), null);
        servicioPacientes.CambiarEstado(paciente.Id, EstadoPaciente.Activo, null, null);

        var repositorioMedicamentos = new RepositorioMedicamentos(conexion);
        var medicamento = new Medicamento { Cn = "654321", Nombre = "Paracetamol 1g", UnidadesEnvase = 28 };
        medicamento.Id = repositorioMedicamentos.Crear(medicamento);
        var repositorioTratamientos = new RepositorioTratamientos(conexion);
        repositorioTratamientos.Crear(new Tratamiento { PacienteId = paciente.Id, MedicamentoId = medicamento.Id, EnSpd = true, PautaD = FraccionDosis.Uno, FechaInicio = new DateOnly(2026, 1, 1), FechaPrescripcionInicial = new DateOnly(2026, 1, 1) });
        var repositorioEnvases = new RepositorioEnvases(conexion);
        repositorioEnvases.Crear(new Envase { PacienteId = paciente.Id, MedicamentoId = medicamento.Id, Serie = "S1", Caducidad = new DateOnly(2030, 1, 1), UnidadesIniciales = 28, UnidadesRestantes = 28 });

        var servicioEnvases = new ServicioEnvases(repositorioEnvases, repositorioTratamientos, repositorioPacientes, auditoria);
        var servicioListadoRetirada = new ServicioListadoRetirada(repositorioPacientes, new RepositorioContactos(conexion), repositorioTratamientos,
            repositorioMedicamentos, repositorioEnvases, repositorioFarmacia, new ComprobadorCoberturaSpdNulo(), auditoria);
        var repositorioSpd = new RepositorioSpd(conexion);
        var servicioPreparacion = new ServicioPreparacion(
            repositorioSpd, new RepositorioSpdLineas(conexion), new RepositorioSpdLineaEnvases(conexion), new RepositorioSpdVerificaciones(conexion),
            new RepositorioSpdModificaciones(conexion), new RepositorioRegistrosAmbientales(conexion), new RepositorioMaterialAcondicionamiento(conexion),
            repositorioPacientes, repositorioTratamientos, repositorioMedicamentos, repositorioEnvases, repositorioFarmacia,
            new ServicioAsignacionEnvases(repositorioEnvases, repositorioTratamientos, auditoria), servicioEnvases, servicioListadoRetirada,
            new ComprobadorIdoneidadYConsentimientoNulo(), auditoria);
        var repositorioUsuarios = new RepositorioUsuarios(conexion);
        var hasheador = new HasheadorArgon2id();
        var servicioUsuarios = new ServicioUsuarios(repositorioUsuarios, hasheador, auditoria);
        var elaborador = servicioUsuarios.CrearUsuario(new DatosAltaUsuario("Elena", "Ruiz", "elena", "contraseña-inicial", Rol.Elaborador, null, null), null).Usuario;
        servicioPreparacion.CrearSesion(paciente.Id, elaborador.Id);

        var documentos = FabricaServiciosTest.GeneracionDocumentos(conexion);
        var comunicaciones = new ServicioComunicacionesMedico(new RepositorioComunicacionesMedico(conexion), repositorioPacientes, repositorioTratamientos, auditoria);
        var lote = new ServicioGeneracionLote(servicioPreparacion, documentos, repositorioSpd, repositorioPacientes);

        var ventana = new PreparacionesWindow(
            servicioPreparacion, servicioPacientes, servicioUsuarios, new ServicioMedicamentos(repositorioMedicamentos, auditoria),
            documentos, comunicaciones, lote, usuarioActualId: elaborador.Id);

        ventana.Show();
    }
}
