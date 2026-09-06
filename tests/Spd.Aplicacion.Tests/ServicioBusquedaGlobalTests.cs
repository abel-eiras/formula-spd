using Microsoft.Data.Sqlite;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Xunit;

namespace Spd.Aplicacion.Tests;

/// <summary>Spec 015 FR-1520..1522 (CA-1513): un solo campo que encuentra pacientes, medicamentos y
/// blísteres sin distinguir tildes ni mayúsculas, y que ante un texto sin coincidencias devuelve
/// una lista vacía, nunca un error.</summary>
public sealed class ServicioBusquedaGlobalTests
{
    private static (IServicioBusquedaGlobal Servicio, int PacienteId) Montar(SqliteConnection conexion)
    {
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
        var servicioPacientes = new ServicioPacientes(repositorioPacientes, repositorioFarmacia, auditoria);
        var paciente = servicioPacientes.Crear(
            new DatosAltaPaciente("María", "López Pérez", null, "12345678Z", null, null, null, null, null, null,
                null, null, null, null, null, null, null, false, null, null, 1), null);

        var repositorioMedicamentos = new RepositorioMedicamentos(conexion);
        var servicioMedicamentos = new ServicioMedicamentos(repositorioMedicamentos, auditoria);
        var medicamento = new Medicamento
        {
            Cn = "654321", Nombre = "Enalapril 20 mg", PrincipioActivo = "Enalapril",
            NombreNormalizado = Normalizador.QuitarTildesYMayusculas("Enalapril 20 mg"), UnidadesEnvase = 28
        };
        medicamento.Id = repositorioMedicamentos.Crear(medicamento);

        var elaborador = new Usuario { Nombre = "Elena", Apellidos = "Ruiz", Login = "elena", HashPassword = "x", Rol = Rol.Elaborador };
        elaborador.Id = new RepositorioUsuarios(conexion).Crear(elaborador);
        var repositorioSpd = new RepositorioSpd(conexion);
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        repositorioSpd.Crear(new SPD
        {
            NumRegistro = "F-000041", CorrelativoNumRegistro = 41, PacienteId = paciente.Id,
            SesionId = Guid.NewGuid(), ElaboradorId = elaborador.Id,
            ValidezDesde = hoy, ValidezHasta = hoy.AddDays(6), Estado = EstadoSpd.Preparado
        });

        return (new ServicioBusquedaGlobal(servicioPacientes, servicioMedicamentos, repositorioSpd, repositorioPacientes),
                paciente.Id);
    }

    [Fact]
    public void Encuentra_paciente_medicamento_y_blister_sin_tildes_ni_mayusculas()
    {
        using var conexion = new SqliteConnection("Data Source=:memory:");
        var (servicio, pacienteId) = Montar(conexion);

        // Sin tilde y en minúsculas: el usuario no debería tener que teclear "López" exacto.
        var porPaciente = servicio.Buscar("lopez");
        var resultadoPaciente = Assert.Single(porPaciente, r => r.Tipo == TipoResultadoBusqueda.Paciente);
        Assert.Equal("María López Pérez", resultadoPaciente.Titulo);
        Assert.Equal(pacienteId, resultadoPaciente.PacienteId);

        var porCn = servicio.Buscar("654321");
        Assert.Contains(porCn, r => r.Tipo == TipoResultadoBusqueda.Medicamento && r.Titulo == "Enalapril 20 mg");

        // El blíster se busca por su número de registro, que es lo que aparece en el papel.
        var porBlister = servicio.Buscar("f-000041");
        var resultadoBlister = Assert.Single(porBlister, r => r.Tipo == TipoResultadoBusqueda.Blister);
        Assert.Equal("F-000041", resultadoBlister.Titulo);
        // Lleva el paciente para poder abrir la pantalla ya centrada en él (FR-1522).
        Assert.Equal(pacienteId, resultadoBlister.PacienteId);
        Assert.Equal("María López Pérez", resultadoBlister.NombrePaciente);
    }

    [Fact]
    public void Sin_coincidencias_o_con_texto_demasiado_corto_devuelve_vacio_y_no_lanza()
    {
        using var conexion = new SqliteConnection("Data Source=:memory:");
        var (servicio, _) = Montar(conexion);

        Assert.Empty(servicio.Buscar("zzzzzzzz"));
        Assert.Empty(servicio.Buscar(null));
        Assert.Empty(servicio.Buscar("   "));
        // Una sola letra encontraría media base de datos: no es una búsqueda, es ruido.
        Assert.Empty(servicio.Buscar("l"));
    }
}
