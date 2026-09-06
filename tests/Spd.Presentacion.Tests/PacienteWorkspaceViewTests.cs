using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Microsoft.Data.Sqlite;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Spd.Presentacion.Pacientes;
using Spd.Presentacion.ViewModels;
using Spd.Presentacion.Views.Pacientes;
using Xunit;

namespace Spd.Presentacion.Tests;

/// <summary>Spec 015 CA-1520/1521/1522: el espacio del paciente sustituye a siete ventanas por siete
/// pestañas con una cabecera común, y lo que hace una pestaña se ve en las demás sin recargar.</summary>
public sealed class PacienteWorkspaceViewTests
{
    /// <summary>Un paciente de verdad, con tratamiento en SPD, envase y sesión de preparación: sin
    /// datos, las plantillas de fila no se realizan y el test no probaría nada (regresión F5).</summary>
    private static (PacienteWorkspaceViewModel Vm, ServiciosAplicacion Servicios, Paciente Paciente) Montar(
        SqliteConnection conexion, bool conSesionDePreparacion = false)
    {
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();
        new RepositorioFarmacia(conexion).Crear(new Farmacia
        {
            CodigoSanitario = "PO-001", Nombre = "Farmacia de Prueba", TitularOComunidadBienes = "Titular",
            Cif = "B00000000", Direccion = "Calle Falsa 1", Cp = "36000", Poblacion = "Pontevedra",
            Telefono = "986000000", PrefijoNumSpd = "F-", DiasAntelacionListado = 0
        });

        var servicios = FabricaServiciosTest.Todos(conexion);
        var elaborador = servicios.Usuarios.CrearUsuario(
            new DatosAltaUsuario("Elena", "Ruiz", "elena", "contraseña-inicial", Rol.Elaborador, null, null), null).Usuario;

        var diaLejano = DiasSemana.Codigos[((int)DateTime.Today.DayOfWeek + 6 + 3) % 7];
        var paciente = servicios.Pacientes.Crear(
            new DatosAltaPaciente("José", "Núñez", null, "12345678Z", null, null, null, null, null, null,
                null, null, null, null, null, "Penicilina", null, false, null, diaLejano, 1), null);
        servicios.Pacientes.CambiarEstado(paciente.Id, EstadoPaciente.Activo, null, null);

        var medicamentos = new RepositorioMedicamentos(conexion);
        var medicamento = new Medicamento { Cn = "654321", Nombre = "Paracetamol 1g", UnidadesEnvase = 28 };
        medicamento.Id = medicamentos.Crear(medicamento);
        new RepositorioTratamientos(conexion).Crear(new Tratamiento
        {
            PacienteId = paciente.Id, MedicamentoId = medicamento.Id, EnSpd = true, PautaD = FraccionDosis.Uno,
            FechaInicio = new DateOnly(2026, 1, 1), FechaPrescripcionInicial = new DateOnly(2026, 1, 1)
        });
        new RepositorioEnvases(conexion).Crear(new Envase
        {
            PacienteId = paciente.Id, MedicamentoId = medicamento.Id, Serie = "S1",
            Caducidad = new DateOnly(2030, 1, 1), UnidadesIniciales = 28, UnidadesRestantes = 28
        });
        if (conSesionDePreparacion)
        {
            // Abrir sesión exige idoneidad y consentimiento vigentes (FR-602, Art. I.3): la
            // aplicación no deja preparar sin ellos, y el montaje del test tampoco puede saltárselo.
            RegistrarIdoneidadYConsentimiento(servicios, paciente.Id);
            servicios.Preparacion.CrearSesion(paciente.Id, elaborador.Id);
        }

        var actual = servicios.Pacientes.ObtenerPorId(paciente.Id)!;
        return (new PacienteWorkspaceViewModel(servicios, actual, elaborador.Id), servicios, actual);
    }

    [AvaloniaFact]
    public void Recorre_las_siete_pestanas_realizando_cada_vista_CA_1520()
    {
        using var conexion = new SqliteConnection("Data Source=:memory:");
        var (vm, _, _) = Montar(conexion, conSesionDePreparacion: true);

        Assert.Equal(7, vm.Pestanas.Count);
        Assert.Equal(
            new[]
            {
                PestanaPaciente.Datos, PestanaPaciente.Idoneidad, PestanaPaciente.Tratamiento,
                PestanaPaciente.Deposito, PestanaPaciente.Preparacion, PestanaPaciente.Comunicaciones,
                PestanaPaciente.Documentos
            },
            vm.Pestanas.Select(p => p.Clave));

        // La vista del espacio entero, con la cabecera y la barra de pestañas.
        var espacio = AnfitrionDeVista.Anfitrion(new PacienteWorkspaceView { DataContext = vm });
        espacio.Show();
        espacio.Close();

        // Y cada pestaña por separado: la regresión F5 solo aparece si la plantilla se realiza.
        foreach (var pestana in vm.Pestanas)
        {
            vm.PestanaSeleccionada = pestana;
            var vista = new ViewLocator().Build(pestana.Contenido);
            Assert.False(vista is TextBlock, $"No se encontró la vista de la pestaña {pestana.Clave}.");
            Assert.True(AyudaContextual.SeccionPorVista.ContainsKey(vista!.GetType().Name),
                $"La vista {vista.GetType().Name} no está en la tabla de ayuda contextual (FR-1505).");

            vista.DataContext = pestana.Contenido;
            var ventana = AnfitrionDeVista.Anfitrion(vista);
            ventana.Show();
            ventana.Close();
        }
    }

    /// <summary>CA-1521. Es el defecto que el diseño anterior tenía de verdad: la ficha seguía
    /// diciendo EVALUACION después de que la ventana de idoneidad hubiera activado al paciente, y
    /// solo se corregía al cerrarla. Aquí no hay ventana que cerrar.</summary>
    [AvaloniaFact]
    public void Activar_al_paciente_desde_idoneidad_actualiza_la_cabecera_sin_recargar_CA_1521()
    {
        using var conexion = new SqliteConnection("Data Source=:memory:");
        var (vm, servicios, paciente) = Montar(conexion);

        // El paciente arranca activo pero sin idoneidad ni consentimiento: la cabecera lo dice.
        Assert.Equal("Sin evaluación", vm.Cabecera.Idoneidad);
        Assert.Equal(1, vm.Pestanas.Single(p => p.Clave == PestanaPaciente.Idoneidad).Pendiente);

        RegistrarIdoneidadYConsentimiento(servicios, paciente.Id);

        // Nadie ha vuelto a leer el paciente a mano: basta con que el contexto avise.
        vm.Contexto.Recargar();
        vm.RecalcularIndicadores();

        Assert.Equal("En regla", vm.Cabecera.Idoneidad);
        Assert.Null(vm.Pestanas.Single(p => p.Clave == PestanaPaciente.Idoneidad).Pendiente);
    }

    /// <summary>CA-1520: en un alta nueva solo hay pestaña de Datos —sin `paciente_id` no hay nada
    /// que registrar— y las otras seis aparecen al guardar, sin cerrar ni reabrir nada.</summary>
    [AvaloniaFact]
    public void Un_alta_nueva_empieza_solo_con_datos_y_gana_las_pestanas_al_guardar_CA_1520()
    {
        using var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();
        new RepositorioFarmacia(conexion).Crear(new Farmacia
        {
            CodigoSanitario = "PO-001", Nombre = "F", TitularOComunidadBienes = "T", Cif = "B0",
            Direccion = "D", Cp = "36000", Poblacion = "P", Telefono = "9"
        });
        var servicios = FabricaServiciosTest.Todos(conexion);
        var vm = new PacienteWorkspaceViewModel(servicios, paciente: null, usuarioActualId: null);

        var pestana = Assert.Single(vm.Pestanas);
        Assert.Equal(PestanaPaciente.Datos, pestana.Clave);
        Assert.Equal("Paciente nuevo", vm.Cabecera.NombreCompleto);

        var datos = Assert.IsType<FichaPacienteViewModel>(pestana.Contenido);
        datos.Nombre = "Ana";
        datos.Apellidos = "Ríos";
        // FR-003/CA-002: sin DNI, CIP ni fecha de nacimiento el alta no se guarda.
        datos.Dni = "12345678Z";
        datos.GuardarCommand.Execute(null);

        Assert.Equal(7, vm.Pestanas.Count);
        Assert.Equal("Ana Ríos", vm.Cabecera.NombreCompleto);
        Assert.NotEqual("(sin asignar todavía)", vm.Cabecera.NumFicha);
    }

    /// <summary>CA-1522: lo que antes abría una ventana de comunicaciones prerrellenada ahora cambia
    /// de pestaña y deja el formulario listo, sin perder de vista al paciente.</summary>
    [AvaloniaFact]
    public void Pedir_comunicaciones_desde_otra_pestana_cambia_y_prerrellena_CA_1522()
    {
        using var conexion = new SqliteConnection("Data Source=:memory:");
        var (vm, _, paciente) = Montar(conexion);
        var medicoId = new RepositorioMedicos(conexion).Crear(new Medico { Nombre = "Marta", Apellidos = "Vidal" });

        vm.Contexto.IrA(PestanaPaciente.Comunicaciones,
            new DatosAltaComunicacionMedico(paciente.Id, medicoId, TipoComunicacionMedico.Incidencia,
                "Interacción detectada", "Sustituir por otro principio activo"));

        Assert.Equal(PestanaPaciente.Comunicaciones, vm.PestanaSeleccionada!.Clave);
        var comunicaciones = Assert.IsType<ComunicacionesMedicoViewModel>(vm.PestanaSeleccionada.Contenido);
        Assert.Equal(medicoId, comunicaciones.MedicoId);
        Assert.Equal("Interacción detectada", comunicaciones.IncidenciasDetectadas);
    }

    private static void RegistrarIdoneidadYConsentimiento(ServiciosAplicacion servicios, int pacienteId)
    {
        servicios.Idoneidad.RegistrarEvaluacion(pacienteId, EvaluacionApta(), null);
        var consentimiento = servicios.Idoneidad.CrearConsentimiento(pacienteId, TipoConsentimiento.Paciente, null, null);
        servicios.Idoneidad.RegistrarFirma(consentimiento.Id, DateOnly.FromDateTime(DateTime.Today), null);
    }

    private static DatosEvaluacionIdoneidad EvaluacionApta() =>
        new(true, true, true, true, true, true, true, true, true, null, ResultadoIdoneidad.Apto, null);
}
