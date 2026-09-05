using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Xunit;

namespace Spd.Aplicacion.Tests;

public sealed class ServicioPacientesTests
{
    private static (ServicioPacientes Servicio, SqliteConnection Conexion) CrearServicio(string prefijo = "F-")
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();

        var repositorioFarmacia = new RepositorioFarmacia(conexion);
        repositorioFarmacia.Crear(new Farmacia
        {
            CodigoSanitario = "PO-001",
            Nombre = "Farmacia de Prueba",
            TitularOComunidadBienes = "Titular de Prueba",
            Cif = "B00000000",
            Direccion = "Calle Falsa 1",
            Cp = "36000",
            Poblacion = "Pontevedra",
            Telefono = "986000000",
            PrefijoNumFicha = prefijo
        });

        var servicio = new ServicioPacientes(
            new RepositorioPacientes(conexion), repositorioFarmacia, new RegistradorAuditoria(conexion));
        return (servicio, conexion);
    }

    private static DatosAltaPaciente DatosMinimos(string? dni = "12345678Z", string nombre = "José", string apellidos = "Núñez") =>
        new(nombre, apellidos, Sexo: null, Dni: dni, FechaNacimiento: null, NumSs: null, Cip: null,
            Direccion: null, Cp: null, Poblacion: null, Telefono1: null, Telefono2: null, Email: null,
            MedicoId: null, EnfermedadesCronicas: null, Alergias: null, Observaciones: null,
            PictogramaComidas: false, IdentificadorVisual: null, DiaRetirada: null, NBlisteres: null);

    [Fact]
    public void Crear_asigna_num_ficha_correlativo_con_el_prefijo_vigente_y_no_editable()
    {
        var (servicio, _) = CrearServicio("F-");
        servicio.Crear(DatosMinimos(dni: "12345678Z"), null);

        var segundo = servicio.Crear(DatosMinimos(dni: "X1234567L", apellidos: "Otro"), null);

        Assert.Equal("F-000002", segundo.NumFicha);
    }

    [Fact]
    public void Crear_rechaza_si_faltan_los_minimos_de_FR_003()
    {
        var (servicio, _) = CrearServicio();
        var datos = DatosMinimos(dni: null) with { Cip = null, FechaNacimiento = null };

        Assert.Throws<ErrorValidacionException>(() => servicio.Crear(datos, null));
    }

    [Fact]
    public void Crear_avisa_de_duplicado_por_dni_no_de_baja_sin_bloquear_si_se_confirma()
    {
        var (servicio, _) = CrearServicio();
        var existente = servicio.Crear(DatosMinimos(dni: "12345678Z"), null);

        var duplicado = Assert.Throws<PacienteDuplicadoException>(
            () => servicio.Crear(DatosMinimos(dni: "12345678Z", apellidos: "OtroApellido"), null));
        Assert.Equal(existente.Id, duplicado.Existente.Id);

        var confirmado = servicio.Crear(
            DatosMinimos(dni: "12345678Z", apellidos: "OtroApellido") with { ConfirmarDuplicado = true }, null);
        Assert.NotEqual(existente.Id, confirmado.Id);
    }

    [Fact]
    public void Crear_avisa_de_duplicado_por_cip_no_de_baja_FR_004()
    {
        var (servicio, _) = CrearServicio();
        var datosConCip = DatosMinimos(dni: null) with { Cip = "910410EEIS1014", Sexo = "H" };
        servicio.Crear(datosConCip, null);

        Assert.Throws<PacienteDuplicadoException>(
            () => servicio.Crear(datosConCip with { Apellidos = "OtroApellido" }, null));
    }

    [Fact]
    public void CambiarEstado_a_baja_exige_fecha_y_motivo_y_no_borra_nada()
    {
        var (servicio, conexion) = CrearServicio();
        var paciente = servicio.Crear(DatosMinimos(), null);

        Assert.Throws<ErrorValidacionException>(
            () => servicio.CambiarEstado(paciente.Id, EstadoPaciente.Baja, null, null));

        servicio.CambiarEstado(paciente.Id, EstadoPaciente.Baja,
            new DatosBaja(DateTime.UtcNow, MotivoBaja.Renuncia, null), null);

        var totalFilas = conexion.ExecuteScalar<int>("SELECT COUNT(*) FROM Paciente WHERE id = @id", new { id = paciente.Id });
        Assert.Equal(1, totalFilas); // la fila sigue existiendo (Art. III.1)

        var fila = conexion.QuerySingle("SELECT estado, motivo_baja FROM Paciente WHERE id = @id", new { id = paciente.Id });
        Assert.Equal("BAJA", (string)fila.estado);
        Assert.Equal("RENUNCIA", (string)fila.motivo_baja);
    }

    [Fact]
    public void CambiarEstado_de_baja_a_evaluacion_reactiva_CA_010()
    {
        var (servicio, _) = CrearServicio();
        var paciente = servicio.Crear(DatosMinimos(), null);
        servicio.CambiarEstado(paciente.Id, EstadoPaciente.Baja,
            new DatosBaja(DateTime.UtcNow, MotivoBaja.Traslado, null), null);

        servicio.CambiarEstado(paciente.Id, EstadoPaciente.Evaluacion, null, null);

        var reactivado = servicio.Buscar(paciente.Apellidos, null, null).Single(p => p.Id == paciente.Id);
        Assert.Equal(EstadoPaciente.Evaluacion, reactivado.Estado);
        Assert.Null(reactivado.FechaBaja);
    }

    [Fact]
    public void Buscar_encuentra_sin_tildes_y_ordena_activos_antes_que_evaluacion_CA_011()
    {
        var (servicio, _) = CrearServicio();
        var jose = servicio.Crear(DatosMinimos(dni: "12345678Z", nombre: "José", apellidos: "Núñez"), null);
        servicio.CambiarEstado(jose.Id, EstadoPaciente.Activo, null, null);
        servicio.Crear(DatosMinimos(dni: "X1234567L", nombre: "María", apellidos: "Núñez Otro"), null);

        var resultados = servicio.Buscar("nunez", null, null);

        Assert.Equal(2, resultados.Count);
        Assert.Equal(EstadoPaciente.Activo, resultados[0].Estado);
    }

    [Fact]
    public void Crear_Actualizar_y_CambiarEstado_registran_en_auditoria_con_detalle_CA_013()
    {
        var (servicio, conexion) = CrearServicio();
        var paciente = servicio.Crear(DatosMinimos(), usuarioQueEjecutaId: 1);

        paciente.Telefono1 = "600000001";
        servicio.Actualizar(paciente, usuarioQueEjecutaId: 1);
        paciente.Telefono1 = "600000002";
        servicio.Actualizar(paciente, usuarioQueEjecutaId: 1);

        servicio.CambiarEstado(paciente.Id, EstadoPaciente.Baja,
            new DatosBaja(DateTime.UtcNow, MotivoBaja.Renuncia, null), usuarioQueEjecutaId: 1);

        var acciones = conexion.Query<string>("SELECT accion FROM Auditoria WHERE entidad = 'Paciente' ORDER BY id").ToList();
        Assert.Contains("ALTA", acciones);
        Assert.Contains("EDITAR", acciones);
        Assert.Contains("CAMBIO_ESTADO", acciones);

        var detalleEdicion = conexion.ExecuteScalar<string>(
            "SELECT detalle FROM Auditoria WHERE accion = 'EDITAR' ORDER BY id DESC LIMIT 1");
        Assert.Contains("600000001", detalleEdicion);
        Assert.Contains("600000002", detalleEdicion);
    }

    [Fact]
    public void Crear_rechaza_cip_informado_sin_sexo_FR_002b()
    {
        var (servicio, _) = CrearServicio();
        var datos = DatosMinimos(dni: null) with { Cip = "910410EEIS1014", Sexo = null };

        Assert.Throws<ErrorValidacionException>(() => servicio.Crear(datos, null));
    }

    [Fact]
    public void Elaborador_crea_edita_y_cambia_estado_sin_ninguna_restriccion_CA_012()
    {
        var (servicio, conexion) = CrearServicio();
        var servicioUsuarios = new ServicioUsuarios(
            new RepositorioUsuarios(conexion), new HasheadorArgon2id(), new RegistradorAuditoria(conexion));
        var elaborador = servicioUsuarios.CrearUsuario(
            new DatosAltaUsuario("Eva", "Elaboradora", "eva.pacientes", null, Rol.Elaborador, null, null), null).Usuario;

        var paciente = servicio.Crear(DatosMinimos(), elaborador.Id);
        paciente.Observaciones = "Cambiado por un Elaborador";
        servicio.Actualizar(paciente, elaborador.Id); // no debe lanzar
        servicio.CambiarEstado(paciente.Id, EstadoPaciente.Activo, null, elaborador.Id); // no debe lanzar

        var actualizado = servicio.Buscar(paciente.Apellidos, null, null).Single(p => p.Id == paciente.Id);
        Assert.Equal(EstadoPaciente.Activo, actualizado.Estado);
        Assert.Equal("Cambiado por un Elaborador", actualizado.Observaciones);
    }
}
