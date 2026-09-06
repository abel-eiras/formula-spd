using Microsoft.Data.Sqlite;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Xunit;

namespace Spd.Aplicacion.Tests;

/// <summary>Spec 001 US3 (FR-020..FR-023, CA-008): los contactos del paciente. Las reglas de
/// exclusividad y de DNI obligatorio son invariantes de dominio (Art. I.3), no validaciones de
/// pantalla: se comprueban aquí, donde no hay interfaz que las pueda saltar.</summary>
public sealed class ServicioContactosTests
{
    private static (ServicioContactos Servicio, int PacienteId, SqliteConnection Conexion) Montar()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();

        var auditoria = new RegistradorAuditoria(conexion);
        var repositorioFarmacia = new RepositorioFarmacia(conexion);
        repositorioFarmacia.Crear(new Farmacia
        {
            CodigoSanitario = "PO-001", Nombre = "F", TitularOComunidadBienes = "T", Cif = "B0",
            Direccion = "D", Cp = "36000", Poblacion = "P", Telefono = "9"
        });
        var repositorioPacientes = new RepositorioPacientes(conexion);
        var paciente = new ServicioPacientes(repositorioPacientes, repositorioFarmacia, auditoria).Crear(
            new DatosAltaPaciente("Ana", "Ríos", null, "12345678Z", null, null, null, null, null, null,
                null, null, null, null, null, null, null, false, null, null, 1), null);

        return (new ServicioContactos(new RepositorioContactos(conexion), repositorioPacientes, auditoria),
                paciente.Id, conexion);
    }

    [Fact]
    public void Representante_o_persona_autorizada_sin_dni_no_se_guarda_CA_008()
    {
        var (servicio, pacienteId, conexion) = Montar();
        using var c = conexion;

        foreach (var tipo in new[] { TipoContacto.RepresentanteLegal, TipoContacto.PersonaAutorizada })
        {
            var error = Assert.Throws<ErrorValidacionException>(() =>
                servicio.Crear(pacienteId, new DatosContacto(tipo, "Luis", "Ríos"), null));
            Assert.Contains("FR-022", error.Message);
        }

        // Un familiar o un cuidador sí pueden no tener DNI: no firman nada.
        servicio.Crear(pacienteId, new DatosContacto(TipoContacto.Familiar, "Luis", "Ríos"), null);
        servicio.Crear(pacienteId, new DatosContacto(TipoContacto.Cuidador, "Eva", "Sanz"), null);
        Assert.Equal(2, servicio.ListarDePaciente(pacienteId).Count);
    }

    [Fact]
    public void Retirar_medicacion_exige_dni_y_es_exclusivo_FR_021b_FR_021c()
    {
        var (servicio, pacienteId, conexion) = Montar();
        using var c = conexion;

        // FR-021c: sin DNI no se puede marcar como quien retira.
        var error = Assert.Throws<ErrorValidacionException>(() => servicio.Crear(
            pacienteId, new DatosContacto(TipoContacto.Familiar, "Luis", "Ríos", RetiraMedicacion: true), null));
        Assert.Contains("FR-021c", error.Message);

        var primero = servicio.Crear(pacienteId,
            new DatosContacto(TipoContacto.Familiar, "Luis", "Ríos", "11111111H", RetiraMedicacion: true), null);
        var segundo = servicio.Crear(pacienteId,
            new DatosContacto(TipoContacto.Cuidador, "Eva", "Sanz", "22222222J", RetiraMedicacion: true), null);

        // FR-021b: marcar a otro es la forma de cambiar quién retira; el anterior se desmarca solo.
        var contactos = servicio.ListarDePaciente(pacienteId);
        Assert.False(contactos.Single(x => x.Id == primero.Id).RetiraMedicacion);
        Assert.True(contactos.Single(x => x.Id == segundo.Id).RetiraMedicacion);
        Assert.Single(contactos, x => x.RetiraMedicacion);
    }

    [Fact]
    public void Solo_un_contacto_puede_ser_principal_FR_021()
    {
        var (servicio, pacienteId, conexion) = Montar();
        using var c = conexion;

        var primero = servicio.Crear(pacienteId,
            new DatosContacto(TipoContacto.Familiar, "Luis", "Ríos", EsPrincipal: true), null);
        var segundo = servicio.Crear(pacienteId,
            new DatosContacto(TipoContacto.Familiar, "Eva", "Sanz", EsPrincipal: true), null);

        var contactos = servicio.ListarDePaciente(pacienteId);
        Assert.False(contactos.Single(x => x.Id == primero.Id).EsPrincipal);
        Assert.True(contactos.Single(x => x.Id == segundo.Id).EsPrincipal);

        // Y también al editar, no solo al crear: es el mismo invariante.
        servicio.Actualizar(primero.Id,
            new DatosContacto(TipoContacto.Familiar, "Luis", "Ríos", EsPrincipal: true), null);
        Assert.Single(servicio.ListarDePaciente(pacienteId), x => x.EsPrincipal);
    }

    [Fact]
    public void La_baja_es_logica_y_no_aparece_salvo_ver_historico_FR_023()
    {
        var (servicio, pacienteId, conexion) = Montar();
        using var c = conexion;
        var contacto = servicio.Crear(pacienteId,
            new DatosContacto(TipoContacto.Familiar, "Luis", "Ríos", "11111111H",
                EsPrincipal: true, RetiraMedicacion: true), null);

        servicio.DarDeBaja(contacto.Id, null);

        Assert.Empty(servicio.ListarDePaciente(pacienteId));
        var historico = Assert.Single(servicio.ListarDePaciente(pacienteId, incluirBaja: true));

        // Art. III.1: sigue existiendo, con su fecha de baja.
        Assert.Equal(contacto.Id, historico.Id);
        Assert.False(historico.Activo);
        Assert.NotNull(historico.FechaBaja);

        // Y deja de ser el principal y el que retira: si no, el Anexo imprimiría a alguien que ya no
        // está y el listado de retirada tomaría su DNI.
        Assert.False(historico.EsPrincipal);
        Assert.False(historico.RetiraMedicacion);
    }

    [Fact]
    public void Alta_y_baja_de_contacto_quedan_en_auditoria_Art_VII_6()
    {
        var (servicio, pacienteId, conexion) = Montar();
        using var c = conexion;
        var contacto = servicio.Crear(pacienteId, new DatosContacto(TipoContacto.Familiar, "Luis", "Ríos"), null);
        servicio.Actualizar(contacto.Id, new DatosContacto(TipoContacto.Cuidador, "Luis", "Ríos"), null);
        servicio.DarDeBaja(contacto.Id, null);

        var acciones = Dapper.SqlMapper.Query<string>(conexion,
            "SELECT accion FROM Auditoria WHERE entidad = 'Contacto' AND entidad_id = @id ORDER BY id",
            new { id = contacto.Id });

        Assert.Equal(["CREAR", "MODIFICAR", "BAJA"], acciones);
    }

    [Fact]
    public void Un_contacto_exige_paciente_existente_y_nombre_y_apellidos()
    {
        var (servicio, pacienteId, conexion) = Montar();
        using var c = conexion;

        Assert.Throws<ErrorValidacionException>(() =>
            servicio.Crear(9999, new DatosContacto(TipoContacto.Familiar, "Luis", "Ríos"), null));
        Assert.Throws<ErrorValidacionException>(() =>
            servicio.Crear(pacienteId, new DatosContacto(TipoContacto.Familiar, "", "Ríos"), null));
    }
}
