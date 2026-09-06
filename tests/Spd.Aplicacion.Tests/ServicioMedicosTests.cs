using Microsoft.Data.Sqlite;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Xunit;

namespace Spd.Aplicacion.Tests;

/// <summary>Spec 001 US2 (FR-030..FR-037, CA-004..CA-007): el catálogo de médicos.</summary>
public sealed class ServicioMedicosTests
{
    private static (ServicioMedicos Servicio, IServicioPacientes Pacientes, SqliteConnection Conexion) Montar()
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

        return (new ServicioMedicos(new RepositorioMedicos(conexion), repositorioPacientes, auditoria),
                new ServicioPacientes(repositorioPacientes, repositorioFarmacia, auditoria),
                conexion);
    }

    private static DatosMedico Vidal => new("Marta", "Vidal Núñez", "36/12345", "Medicina de familia", "CS Lérez");

    [Fact]
    public void Buscar_exige_dos_caracteres_y_encuentra_por_fragmento_normalizado_CA_004()
    {
        var (servicio, _, conexion) = Montar();
        using var c = conexion;
        servicio.Crear(Vidal, null);

        // FR-032: menos de dos caracteres no es una búsqueda.
        Assert.Empty(servicio.Buscar("v"));
        Assert.Empty(servicio.Buscar(null));
        Assert.Empty(servicio.Buscar("  "));

        // Sin tildes y en minúsculas: nadie debería tener que teclear "Núñez" exacto.
        Assert.Single(servicio.Buscar("nunez"));
        Assert.Single(servicio.Buscar("VIDAL"));
        // FR-032 dice también por centro.
        Assert.Single(servicio.Buscar("lerez"));
        Assert.Empty(servicio.Buscar("zzzz"));
    }

    [Fact]
    public void Crear_avisa_pero_no_bloquea_ante_un_posible_duplicado_FR_034()
    {
        var (servicio, _, conexion) = Montar();
        using var c = conexion;
        servicio.Crear(Vidal, null);

        // Mismo nombre y apellidos: puede ser otra persona o la misma en otro centro. Se crea.
        var mismoNombre = servicio.Crear(Vidal with { Colegiado = "36/99999", Centro = "CS Monteporreiro" }, null);
        Assert.NotNull(mismoNombre.Aviso);
        Assert.Contains("Vidal Núñez", mismoNombre.Aviso);
        Assert.True(mismoNombre.Medico.Id > 0, "El aviso no debe impedir el alta (FR-034).");

        // Mismo colegiado con otro nombre: también avisa.
        var mismoColegiado = servicio.Crear(new DatosMedico("Luis", "Otero", "36/12345"), null);
        Assert.Contains("36/12345", mismoColegiado.Aviso);

        // Un médico sin parecido no genera ruido.
        Assert.Null(servicio.Crear(new DatosMedico("Ana", "Barreiro", "36/00001"), null).Aviso);
    }

    [Fact]
    public void Actualizar_se_refleja_al_releer_un_paciente_que_lo_referencia_CA_006()
    {
        var (servicio, pacientes, conexion) = Montar();
        using var c = conexion;
        var medico = servicio.Crear(Vidal, null).Medico;

        var paciente = pacientes.Crear(
            new DatosAltaPaciente("Ana", "Ríos", null, "12345678Z", null, null, null, null, null, null,
                null, null, null, MedicoId: medico.Id, null, null, null, false, null, null, 1), null);

        servicio.Actualizar(medico.Id, Vidal with { Centro = "CS Campolongo" }, null);

        // Art. IV.1 / FR-035: el paciente guarda la referencia, no una copia; releerlo basta.
        Assert.Equal(medico.Id, pacientes.ObtenerPorId(paciente.Id)!.MedicoId);
        Assert.Equal("CS Campolongo", servicio.ObtenerPorId(medico.Id)!.Centro);
    }

    [Fact]
    public void DarDeBaja_lista_los_pacientes_si_es_medico_de_cabecera_CA_007()
    {
        var (servicio, pacientes, conexion) = Montar();
        using var c = conexion;
        var medico = servicio.Crear(Vidal, null).Medico;

        var paciente = pacientes.Crear(
            new DatosAltaPaciente("Ana", "Ríos", null, "12345678Z", null, null, null, null, null, null,
                null, null, null, MedicoId: medico.Id, null, null, null, false, null, null, 1), null);
        pacientes.CambiarEstado(paciente.Id, EstadoPaciente.Activo, null, null);

        var error = Assert.Throws<ErrorValidacionException>(() => servicio.DarDeBaja(medico.Id, null));
        // El mensaje tiene que decir a quién hay que reasignar, no solo que no se puede.
        Assert.Contains("Ana Ríos", error.Message);
        Assert.Contains(paciente.NumFicha, error.Message);
        Assert.Equal(1, servicio.ContarPacientesDeCabecera(medico.Id));
        Assert.True(servicio.ObtenerPorId(medico.Id)!.Activo);

        // Con el paciente de baja ya no cuenta y el médico se puede retirar del catálogo.
        pacientes.CambiarEstado(paciente.Id, EstadoPaciente.Baja,
            new DatosBaja(DateTime.Today, MotivoBaja.Traslado, null), null);
        Assert.Equal(0, servicio.ContarPacientesDeCabecera(medico.Id));

        servicio.DarDeBaja(medico.Id, null);
        Assert.False(servicio.ObtenerPorId(medico.Id)!.Activo);
        // Art. III.1: de baja, pero sigue existiendo para que lo referencien tratamientos previos.
        Assert.NotNull(servicio.ObtenerPorId(medico.Id));
        Assert.DoesNotContain(servicio.ListarActivos(), m => m.Id == medico.Id);
    }

    [Fact]
    public void Crear_y_dar_de_baja_quedan_en_auditoria_Art_VII_6()
    {
        var (servicio, _, conexion) = Montar();
        using var c = conexion;
        var medico = servicio.Crear(Vidal, usuarioQueEjecutaId: null).Medico;
        servicio.Actualizar(medico.Id, Vidal with { Telefono = "986000000" }, null);
        servicio.DarDeBaja(medico.Id, null);

        var acciones = Dapper.SqlMapper.Query<string>(conexion,
            "SELECT accion FROM Auditoria WHERE entidad = 'Medico' AND entidad_id = @id ORDER BY id",
            new { id = medico.Id });

        Assert.Equal(["CREAR", "MODIFICAR", "BAJA"], acciones);
    }

    [Fact]
    public void Nombre_y_apellidos_son_obligatorios_FR_030()
    {
        var (servicio, _, conexion) = Montar();
        using var c = conexion;

        Assert.Throws<ErrorValidacionException>(() => servicio.Crear(new DatosMedico("", "Vidal"), null));
        Assert.Throws<ErrorValidacionException>(() => servicio.Crear(new DatosMedico("Marta", "   "), null));
    }
}
