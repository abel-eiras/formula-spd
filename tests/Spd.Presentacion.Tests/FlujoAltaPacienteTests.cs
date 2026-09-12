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
using Spd.Presentacion.Views.Pacientes;
using Xunit;

namespace Spd.Presentacion.Tests;

/// <summary>El alta real de un paciente nuevo que llega con un médico nuevo (Spec 001 FR-031/FR-033,
/// Spec 015 FR-1530).
///
/// Es el orden en que ocurre en el mostrador y por tanto el que manda: **se empieza por el paciente**.
/// Cuando toca asignarle el médico y no está en el catálogo, se da de alta **sin salir de su ficha**.
/// Lo contrario —obligar a pasar antes por el catálogo de médicos— sería pedirle al farmacéutico que
/// interrumpa lo que está haciendo para ir a otra pantalla, que es justo lo que la Spec 015 vino a
/// quitar.
///
/// Este test existe porque el guion de la prueba manual llegó a decir lo contrario: que médicos y
/// contactos eran un paso previo. El código nunca lo exigió, y aquí queda fijado para que no se
/// vuelva a colar esa dependencia.</summary>
public sealed class FlujoAltaPacienteTests
{
    private static (ServiciosAplicacion Servicios, SqliteConnection Conexion) Montar()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();
        new RepositorioFarmacia(conexion).Crear(new Farmacia
        {
            CodigoSanitario = "PO-001", Nombre = "Farmacia de Prueba", TitularOComunidadBienes = "Titular",
            Cif = "B00000000", Direccion = "Calle Falsa 1", Cp = "36000", Poblacion = "Pontevedra",
            Telefono = "986000000"
        });
        return (FabricaServiciosTest.Todos(conexion), conexion);
    }

    [AvaloniaFact]
    public void Un_paciente_nuevo_con_medico_nuevo_se_da_de_alta_sin_salir_de_su_ficha()
    {
        var (servicios, conexion) = Montar();
        using var c = conexion;

        // El catálogo de médicos está vacío: nadie ha pasado antes por él.
        Assert.Empty(servicios.Medicos.ListarActivos());

        var vm = new PacienteWorkspaceViewModel(servicios, paciente: null, usuarioActualId: null);
        var ficha = Assert.IsType<FichaPacienteViewModel>(vm.Pestanas[0].Contenido);

        // La vista se realiza de verdad: el selector y los contactos están dentro de la ficha, no en
        // otra pantalla (regresión F5).
        var vista = AnfitrionDeVista.Anfitrion(new PacienteWorkspaceView { DataContext = vm });
        vista.Show();

        // 1. Se empieza por el paciente.
        ficha.Nombre = "Ana";
        ficha.Apellidos = "Ríos";
        ficha.Dni = "12345678Z";

        // 2. Toca el médico y no está. Se busca desde la propia ficha.
        ficha.SelectorMedico.Texto = "vidal";
        Assert.Empty(ficha.SelectorMedico.Resultados);

        // 3. Se da de alta ahí mismo: el panel llega con el apellido ya escrito.
        ficha.SelectorMedico.AbrirNuevoCommand.Execute(null);
        Assert.Equal("vidal", ficha.SelectorMedico.NuevoApellidos);
        ficha.SelectorMedico.NuevoApellidos = "Vidal Núñez";
        ficha.SelectorMedico.NuevoNombre = "Marta";
        ficha.SelectorMedico.NuevoCentro = "CS Lérez";
        ficha.SelectorMedico.CrearNuevoCommand.Execute(null);

        var medico = Assert.Single(servicios.Medicos.ListarActivos());
        Assert.Equal(medico.Id, ficha.SelectorMedico.Seleccionado?.Id);

        // Lo que importa: el paciente **todavía no existe** y ya tiene su médico elegido. Nada obligó
        // a guardarlo antes, ni a salir de la ficha, ni se perdió lo tecleado.
        Assert.Null(ficha.Paciente);
        Assert.Equal("Ana", ficha.Nombre);
        Assert.Equal("Ríos", ficha.Apellidos);
        Assert.Equal("12345678Z", ficha.Dni);

        // 4. Se guarda el paciente: el médico queda asignado en el mismo gesto.
        ficha.GuardarCommand.Execute(null);
        Assert.NotNull(ficha.Paciente);
        Assert.Equal(medico.Id, servicios.Pacientes.ObtenerPorId(ficha.Paciente!.Id)!.MedicoId);
    }

    [AvaloniaFact]
    public void Los_contactos_solo_esperan_el_primer_guardado_y_se_anaden_en_la_misma_ficha()
    {
        var (servicios, conexion) = Montar();
        using var c = conexion;

        var vm = new PacienteWorkspaceViewModel(servicios, paciente: null, usuarioActualId: null);
        var ficha = Assert.IsType<FichaPacienteViewModel>(vm.Pestanas[0].Contenido);

        // Antes del primer guardado no hay ficha a la que colgar un contacto (FR-020 referencia
        // `paciente_id`). La pantalla lo dice y desactiva el botón, en vez de aceptarlo y perderlo.
        Assert.False(ficha.Contactos.HayPaciente);

        ficha.Nombre = "Ana";
        ficha.Apellidos = "Ríos";
        ficha.Dni = "12345678Z";
        ficha.GuardarCommand.Execute(null);

        // En cuanto existe, los contactos se añaden en esta misma pestaña: no se cambia de pantalla.
        Assert.True(ficha.Contactos.HayPaciente);

        ficha.Contactos.NuevoCommand.Execute(null);
        ficha.Contactos.Apellidos = "Ríos";
        ficha.Contactos.Nombre = "Luis";
        ficha.Contactos.Dni = "11111111H";
        ficha.Contactos.RetiraMedicacion = true;
        ficha.Contactos.GuardarCommand.Execute(null);

        var contacto = Assert.Single(ficha.Contactos.Contactos);
        Assert.True(contacto.RetiraMedicacion);
        Assert.False(ficha.Contactos.RetiraElPropioPaciente);

        // Y el paciente sigue siendo el mismo: no se reabrió ni se perdió el contexto.
        Assert.Equal(ficha.Paciente!.Id, vm.Contexto.PacienteId);
    }
}
