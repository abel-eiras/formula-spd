using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Spd.Presentacion.Pacientes;
using Xunit;

namespace Spd.Presentacion.Tests;

/// <summary>Spec 015 CA-1521 (research.md Decisión 5): el paciente se carga una vez y quien lo
/// cambia avisa; es lo que sustituye al parche de releer la ficha al cerrar una ventana.</summary>
public sealed class PacienteContextoTests
{
    private static (PacienteContexto Contexto, IServicioPacientes Servicio, Paciente Paciente) Montar(SqliteConnection conexion)
    {
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();
        var repositorioFarmacia = new RepositorioFarmacia(conexion);
        repositorioFarmacia.Crear(new Farmacia
        {
            CodigoSanitario = "PO-001", Nombre = "F", TitularOComunidadBienes = "T", Cif = "B0",
            Direccion = "D", Cp = "36000", Poblacion = "P", Telefono = "9"
        });
        var repositorioPacientes = new RepositorioPacientes(conexion);
        var servicio = new ServicioPacientes(repositorioPacientes, repositorioFarmacia, new RegistradorAuditoria(conexion));
        var paciente = servicio.Crear(
            new DatosAltaPaciente("Ana", "Ríos", null, "12345678Z", null, null, null, null, null, null,
                null, null, null, null, null, null, null, false, null, null, 1), null);

        var contexto = new PacienteContexto(servicio);
        contexto.Establecer(paciente);
        return (contexto, servicio, paciente);
    }

    [Fact]
    public void Recargar_trae_el_cambio_hecho_por_otro_y_avisa_a_los_suscriptores()
    {
        using var conexion = new SqliteConnection("Data Source=:memory:");
        var (contexto, servicio, paciente) = Montar(conexion);

        var avisos = new List<EstadoPaciente?>();
        contexto.Cambiado += p => avisos.Add(p?.Estado);

        Assert.Equal(EstadoPaciente.Evaluacion, contexto.Paciente!.Estado);

        // Otro actor cambia el estado por debajo, como hace la idoneidad (Spec 002 FR-213).
        servicio.CambiarEstado(paciente.Id, EstadoPaciente.Activo, null, null);
        Assert.Equal(EstadoPaciente.Evaluacion, contexto.Paciente.Estado);

        contexto.Recargar();

        Assert.Equal(EstadoPaciente.Activo, contexto.Paciente!.Estado);
        Assert.Equal([EstadoPaciente.Activo], avisos);
    }

    [Fact]
    public void Sin_paciente_no_hay_identificador_ni_recarga_que_valga()
    {
        using var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();
        var servicio = new ServicioPacientes(
            new RepositorioPacientes(conexion), new RepositorioFarmacia(conexion), new RegistradorAuditoria(conexion));

        var contexto = new PacienteContexto(servicio);
        var avisos = 0;
        contexto.Cambiado += _ => avisos++;

        Assert.Null(contexto.PacienteId);
        // En alta nueva no hay nada que releer: recargar no debe fallar ni inventar avisos.
        contexto.Recargar();
        Assert.Equal(0, avisos);
    }

    [Fact]
    public void Pedir_una_pestana_llega_con_su_argumento()
    {
        using var conexion = new SqliteConnection("Data Source=:memory:");
        var (contexto, _, paciente) = Montar(conexion);

        (PestanaPaciente Pestana, object? Argumento)? recibido = null;
        contexto.PestanaSolicitada += (p, a) => recibido = (p, a);

        var prerrelleno = new DatosAltaComunicacionMedico(
            paciente.Id, null, TipoComunicacionMedico.Incidencia, "Interacción", "Sustituir");
        contexto.IrA(PestanaPaciente.Comunicaciones, prerrelleno);

        Assert.NotNull(recibido);
        Assert.Equal(PestanaPaciente.Comunicaciones, recibido!.Value.Pestana);
        Assert.Same(prerrelleno, recibido.Value.Argumento);
    }
}
