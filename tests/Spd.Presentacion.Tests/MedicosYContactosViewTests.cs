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
using Spd.Presentacion.Views.Medicos;
using Spd.Presentacion.Views.Pacientes;
using Xunit;

namespace Spd.Presentacion.Tests;

/// <summary>Spec 001 US2/US3 (CA-005, CA-008): el selector de médico, el catálogo y los contactos.
/// Antes de esto no había forma de dar de alta un médico ni un contacto desde la aplicación, así que
/// todo campo que los referenciaba estaba condenado a quedarse vacío.</summary>
public sealed class MedicosYContactosViewTests
{
    private static (ServiciosAplicacion Servicios, SqliteConnection Conexion) Montar()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();
        new RepositorioFarmacia(conexion).Crear(new Farmacia
        {
            CodigoSanitario = "PO-001", Nombre = "Farmacia de Prueba", TitularOComunidadBienes = "Titular",
            Cif = "B00000000", Direccion = "Calle Falsa 1", Cp = "36000", Poblacion = "Pontevedra", Telefono = "986000000"
        });
        return (FabricaServiciosTest.Todos(conexion), conexion);
    }

    /// <summary>CA-005: el autocompletado enseña lo que encuentra y «Nuevo médico…» crea el médico y
    /// lo deja seleccionado **sin salir** de donde se estaba, que es el punto del FR-033.</summary>
    [AvaloniaFact]
    public void El_selector_busca_y_da_de_alta_dejando_el_medico_elegido_CA_005()
    {
        var (servicios, conexion) = Montar();
        using var c = conexion;
        var vm = new SelectorMedicoViewModel(servicios.Medicos, usuarioActualId: null);

        var ventana = AnfitrionDeVista.Anfitrion(new SelectorMedicoView { DataContext = vm });
        ventana.Show();

        // Sin médicos en el catálogo no hay resultados, pero el alta sigue disponible.
        vm.Texto = "vidal";
        Assert.Empty(vm.Resultados);

        // El panel se abre con lo tecleado ya puesto como apellidos.
        vm.AbrirNuevoCommand.Execute(null);
        Assert.True(vm.PanelNuevoAbierto);
        Assert.Equal("vidal", vm.NuevoApellidos);

        vm.NuevoApellidos = "Vidal Núñez";
        vm.NuevoNombre = "Marta";
        vm.NuevoCentro = "CS Lérez";
        vm.CrearNuevoCommand.Execute(null);

        // FR-033: queda seleccionado, el panel se cierra y el campo de búsqueda se limpia.
        Assert.False(vm.PanelNuevoAbierto);
        Assert.NotNull(vm.Seleccionado);
        Assert.Equal("Vidal Núñez", vm.Seleccionado!.Apellidos);
        Assert.True(vm.HaySeleccion);
        Assert.Equal("Vidal Núñez, Marta — CS Lérez", vm.EtiquetaSeleccionado);

        // Y ahora sí aparece al buscarlo, sin tildes.
        vm.LimpiarCommand.Execute(null);
        vm.Texto = "nunez";
        Assert.Single(vm.Resultados);
    }

    [AvaloniaFact]
    public void El_selector_no_deja_crear_un_medico_sin_nombre_y_lo_dice()
    {
        var (servicios, conexion) = Montar();
        using var c = conexion;
        var vm = new SelectorMedicoViewModel(servicios.Medicos, null);

        vm.AbrirNuevoCommand.Execute(null);
        vm.NuevoApellidos = "Vidal";
        vm.NuevoNombre = string.Empty;
        vm.CrearNuevoCommand.Execute(null);

        Assert.True(vm.PanelNuevoAbierto, "El panel debe seguir abierto para poder corregir.");
        Assert.Null(vm.Seleccionado);
        Assert.Contains("obligatorios", vm.Mensaje);
    }

    [AvaloniaFact]
    public void El_catalogo_de_medicos_se_realiza_y_cuenta_los_pacientes_de_cabecera_FR_037()
    {
        var (servicios, conexion) = Montar();
        using var c = conexion;
        var medico = servicios.Medicos.Crear(new DatosMedico("Marta", "Vidal", "36/1", null, "CS Lérez"), null).Medico;
        var paciente = servicios.Pacientes.Crear(
            new DatosAltaPaciente("Ana", "Ríos", null, "12345678Z", null, null, null, null, null, null,
                null, null, null, MedicoId: medico.Id, null, null, null, false, null, null, 1), null);
        servicios.Pacientes.CambiarEstado(paciente.Id, EstadoPaciente.Activo, null, null);

        var vm = new CatalogoMedicosViewModel(servicios.Medicos, null);
        var ventana = AnfitrionDeVista.Anfitrion(new CatalogoMedicosView { DataContext = vm });
        ventana.Show();

        var fila = Assert.Single(vm.Resultados);
        Assert.Equal(1, fila.PacientesDeCabecera);
        Assert.False(fila.SePuedeDarDeBaja);

        // FR-036: la baja se rechaza diciendo a quién hay que reasignar.
        vm.DarDeBajaCommand.Execute(fila);
        Assert.Contains("Ana Ríos", vm.Mensaje);
        Assert.Single(vm.Resultados);
    }

    /// <summary>CA-008 en pantalla: la regla del DNI la aplica el servicio, y la vista la enseña en
    /// vez de tragársela.</summary>
    [AvaloniaFact]
    public void Los_contactos_se_realizan_y_avisan_de_lo_que_falta_CA_008()
    {
        var (servicios, conexion) = Montar();
        using var c = conexion;
        var paciente = servicios.Pacientes.Crear(
            new DatosAltaPaciente("Ana", "Ríos", null, "12345678Z", null, null, null, null, null, null,
                null, null, null, null, null, null, null, false, null, null, 1), null);

        var contexto = new PacienteContexto(servicios.Pacientes);
        contexto.Establecer(paciente);
        var vm = new ContactosPacienteViewModel(servicios.Contactos, contexto, null);

        var ventana = AnfitrionDeVista.Anfitrion(new ContactosPacienteView { DataContext = vm });
        ventana.Show();

        Assert.True(vm.HayPaciente);
        Assert.True(vm.SinContactos);
        // FR-021b: sin nadie marcado, retira el propio paciente y la pantalla lo dice.
        Assert.True(vm.RetiraElPropioPaciente);

        // Un representante legal sin DNI no se guarda, y el panel sigue abierto para corregirlo.
        vm.NuevoCommand.Execute(null);
        vm.Tipo = TipoContacto.RepresentanteLegal;
        vm.Apellidos = "Ríos";
        vm.Nombre = "Luis";
        vm.GuardarCommand.Execute(null);
        Assert.True(vm.PanelAbierto);
        Assert.Contains("FR-022", vm.Mensaje);
        Assert.True(vm.SinContactos);

        // Con DNI entra, y al marcarlo como quien retira deja de retirar el paciente.
        vm.Dni = "11111111H";
        vm.RetiraMedicacion = true;
        vm.GuardarCommand.Execute(null);
        Assert.False(vm.PanelAbierto);
        var contacto = Assert.Single(vm.Contactos);
        Assert.True(contacto.RetiraMedicacion);
        Assert.False(vm.RetiraElPropioPaciente);

        // FR-023: la baja es lógica; desaparece de la lista pero está en el histórico.
        vm.DarDeBajaCommand.Execute(contacto);
        Assert.True(vm.SinContactos);
        vm.VerHistorico = true;
        Assert.Single(vm.Contactos);
        Assert.False(vm.Contactos[0].Activo);
    }

    /// <summary>La ficha ganó el médico de cabecera: hasta ahora el alta guardaba `MedicoId: null`
    /// siempre, así que ningún paciente llegaba a tener médico.</summary>
    [AvaloniaFact]
    public void La_ficha_guarda_el_medico_de_cabecera_elegido_FR_031()
    {
        var (servicios, conexion) = Montar();
        using var c = conexion;
        var medico = servicios.Medicos.Crear(new DatosMedico("Marta", "Vidal", "36/1"), null).Medico;

        var vm = new PacienteWorkspaceViewModel(servicios, paciente: null, usuarioActualId: null);
        var ficha = Assert.IsType<FichaPacienteViewModel>(vm.Pestanas[0].Contenido);

        ficha.Nombre = "Ana";
        ficha.Apellidos = "Ríos";
        ficha.Dni = "12345678Z";
        ficha.SelectorMedico.ElegirCommand.Execute(medico);
        ficha.GuardarCommand.Execute(null);

        var guardado = servicios.Pacientes.ObtenerPorId(ficha.Paciente!.Id)!;
        Assert.Equal(medico.Id, guardado.MedicoId);

        // Y al releer, el selector se coloca solo sobre su médico.
        var reabierta = new PacienteWorkspaceViewModel(servicios, guardado, null);
        var fichaReabierta = Assert.IsType<FichaPacienteViewModel>(reabierta.Pestanas[0].Contenido);
        Assert.Equal(medico.Id, fichaReabierta.SelectorMedico.Seleccionado?.Id);
    }
}
