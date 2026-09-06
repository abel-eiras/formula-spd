using System;
using System.Linq;
using Avalonia.Headless.XUnit;
using Microsoft.Data.Sqlite;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Spd.Presentacion.Pacientes;
using Spd.Presentacion.ViewModels;
using Xunit;

namespace Spd.Presentacion.Tests;

/// <summary>Spec 008 FR-805 y Spec 006 FR-663: si en la entrega el paciente refiere cambios de
/// medicación, se abre la comunicación al médico ya preparada.
///
/// El destinatario es el médico **de cabecera del paciente**. Hasta ahora se tomaba el prescriptor de
/// la primera línea que apareciera, que con dos tratamientos de médicos distintos podía ser
/// cualquiera de los dos; y en la práctica el paciente nunca tenía médico de cabecera porque la ficha
/// no ofrecía dónde ponerlo.</summary>
public sealed class AvisoCambioReferidoTests
{
    private static (PacienteWorkspaceViewModel Vm, ServiciosAplicacion Servicios, Paciente Paciente, int MedicoCabeceraId)
        Montar(SqliteConnection conexion, bool conMedicoDeCabecera)
    {
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();
        new RepositorioFarmacia(conexion).Crear(new Farmacia
        {
            CodigoSanitario = "PO-001", Nombre = "F", TitularOComunidadBienes = "T", Cif = "B0",
            Direccion = "D", Cp = "36000", Poblacion = "P", Telefono = "9",
            PrefijoNumSpd = "F-", DiasAntelacionListado = 0
        });

        var servicios = FabricaServiciosTest.Todos(conexion);
        var elaborador = servicios.Usuarios.CrearUsuario(
            new DatosAltaUsuario("Elena", "Ruiz", "elena", "contraseña-inicial", Rol.Elaborador, null, null), null).Usuario;

        var cabecera = servicios.Medicos.Crear(new DatosMedico("Marta", "Vidal", "36/1"), null).Medico;
        var otro = servicios.Medicos.Crear(new DatosMedico("Luis", "Otero", "36/2"), null).Medico;

        var diaLejano = DiasSemana.Codigos[((int)DateTime.Today.DayOfWeek + 6 + 3) % 7];
        var paciente = servicios.Pacientes.Crear(
            new DatosAltaPaciente("José", "Núñez", null, "12345678Z", null, null, null, null, null, null,
                null, null, null, conMedicoDeCabecera ? cabecera.Id : null,
                null, null, null, false, null, diaLejano, 1), null);
        servicios.Pacientes.CambiarEstado(paciente.Id, EstadoPaciente.Activo, null, null);

        var medicamentos = new RepositorioMedicamentos(conexion);
        var medicamento = new Medicamento { Cn = "654321", Nombre = "Paracetamol 1g", UnidadesEnvase = 28 };
        medicamento.Id = medicamentos.Crear(medicamento);
        // Prescrito por el OTRO médico: si el aviso tomara el prescriptor, saldría este y no el de cabecera.
        new RepositorioTratamientos(conexion).Crear(new Tratamiento
        {
            PacienteId = paciente.Id, MedicamentoId = medicamento.Id, EnSpd = true, PautaD = FraccionDosis.Uno,
            MedicoId = otro.Id,
            FechaInicio = new DateOnly(2026, 1, 1), FechaPrescripcionInicial = new DateOnly(2026, 1, 1)
        });
        new RepositorioEnvases(conexion).Crear(new Envase
        {
            PacienteId = paciente.Id, MedicamentoId = medicamento.Id, Serie = "S1",
            Caducidad = new DateOnly(2030, 1, 1), UnidadesIniciales = 28, UnidadesRestantes = 28
        });

        servicios.Idoneidad.RegistrarEvaluacion(paciente.Id,
            new DatosEvaluacionIdoneidad(true, true, true, true, true, true, true, true, true, null,
                ResultadoIdoneidad.Apto, null), null);
        var consentimiento = servicios.Idoneidad.CrearConsentimiento(paciente.Id, TipoConsentimiento.Paciente, null, null);
        servicios.Idoneidad.RegistrarFirma(consentimiento.Id, DateOnly.FromDateTime(DateTime.Today), null);

        // Sesión llevada hasta VERIFICADO para poder entregar.
        var spds = servicios.Preparacion.CrearSesion(paciente.Id, elaborador.Id);
        var material = servicios.Preparacion.ListarMaterialesActivos().FirstOrDefault()
                       ?? servicios.Preparacion.CrearMaterial("Blíster estándar", "L-1", DateOnly.FromDateTime(DateTime.Today));
        var lectura = servicios.Preparacion.ObtenerOCrearLecturaAmbiental(21, 45, null);
        foreach (var spd in spds)
        {
            servicios.Preparacion.AsignarMaterial(spd.Id, material.Id);
            servicios.Preparacion.PasarAPreparado(spd.Id, lectura.Id, null);
            servicios.Preparacion.Verificar(spd.Id, elaborador.Id,
                new ChecklistVerificacion(true, true, true, true, true, true, true, true),
                "Farmacia unipersonal: elaborador y verificador coinciden.", null);
        }

        var actual = servicios.Pacientes.ObtenerPorId(paciente.Id)!;
        return (new PacienteWorkspaceViewModel(servicios, actual, elaborador.Id), servicios, actual, cabecera.Id);
    }

    [AvaloniaFact]
    public void Referir_cambios_abre_la_comunicacion_al_medico_de_cabecera_FR_805()
    {
        using var conexion = new SqliteConnection("Data Source=:memory:");
        var (vm, _, _, medicoCabeceraId) = Montar(conexion, conMedicoDeCabecera: true);

        var preparacion = (PreparacionViewModel)vm.Pestanas
            .Single(p => p.Clave == PestanaPaciente.Preparacion).Contenido;

        preparacion.EntregadoA = "El propio paciente";
        preparacion.PrimeraEntrega = true;
        preparacion.CambiosMedicacionReferidos = true;
        preparacion.EntregarTodosLosPendientesCommand.Execute(null);

        // FR-1530: se cambia a la pestaña de comunicaciones, sin abrir ninguna ventana.
        Assert.Equal(PestanaPaciente.Comunicaciones, vm.PestanaSeleccionada!.Clave);

        var comunicaciones = (ComunicacionesMedicoViewModel)vm.PestanaSeleccionada.Contenido;
        // El destinatario es el de cabecera, no el prescriptor del tratamiento.
        Assert.Equal(medicoCabeceraId, comunicaciones.SelectorMedico.Seleccionado?.Id);
        Assert.Equal(TipoComunicacionMedico.Incidencia, comunicaciones.Tipo);
        Assert.Contains("refiere cambios", comunicaciones.IncidenciasDetectadas);
    }

    [AvaloniaFact]
    public void Sin_medico_de_cabecera_se_cae_al_prescriptor_y_nunca_se_adivina()
    {
        using var conexion = new SqliteConnection("Data Source=:memory:");
        var (vm, _, _, medicoCabeceraId) = Montar(conexion, conMedicoDeCabecera: false);

        var preparacion = (PreparacionViewModel)vm.Pestanas
            .Single(p => p.Clave == PestanaPaciente.Preparacion).Contenido;

        preparacion.EntregadoA = "El propio paciente";
        preparacion.PrimeraEntrega = true;
        preparacion.CambiosMedicacionReferidos = true;
        preparacion.EntregarTodosLosPendientesCommand.Execute(null);

        Assert.Equal(PestanaPaciente.Comunicaciones, vm.PestanaSeleccionada!.Clave);
        var comunicaciones = (ComunicacionesMedicoViewModel)vm.PestanaSeleccionada.Contenido;

        // Cae al prescriptor del tratamiento, que aquí es otro médico distinto del de cabecera.
        Assert.NotNull(comunicaciones.SelectorMedico.Seleccionado);
        Assert.NotEqual(medicoCabeceraId, comunicaciones.SelectorMedico.Seleccionado!.Id);
    }
}
