using Microsoft.Data.Sqlite;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Xunit;

namespace Spd.Aplicacion.Tests;

/// <summary>Spec 006 FR-691 y Spec 009 FR-950: los cuatro tipos de aviso del panel de inicio.</summary>
public sealed class ServicioAvisosInicioTests
{
    [Fact]
    public void Obtener_devuelve_faltantes_verificados_sin_entregar_sesiones_a_medias_y_ambiental()
    {
        using var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();

        var auditoria = new RegistradorAuditoria(conexion);
        var repositorioFarmacia = new RepositorioFarmacia(conexion);
        repositorioFarmacia.Crear(new Farmacia
        {
            CodigoSanitario = "PO-001", Nombre = "F", TitularOComunidadBienes = "T", Cif = "B0", Direccion = "D", Cp = "36000",
            Poblacion = "P", Telefono = "9", DiasAntelacionListado = 7
        });
        var repositorioPacientes = new RepositorioPacientes(conexion);
        var servicioPacientes = new ServicioPacientes(repositorioPacientes, repositorioFarmacia, auditoria);
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var diaHoy = DiasSemana.Codigos[((int)DateTime.Today.DayOfWeek + 6) % 7];

        // Paciente activo con tratamiento en SPD y sin envases → faltante en el listado de retirada.
        var conFaltantes = servicioPacientes.Crear(new DatosAltaPaciente("Ana", "Faltantes", null, "12345678Z", null, null, null, null, null, null,
            null, null, null, null, null, null, null, false, null, diaHoy, 1), null);
        servicioPacientes.CambiarEstado(conFaltantes.Id, EstadoPaciente.Activo, null, null);
        var medicamentos = new RepositorioMedicamentos(conexion);
        var med = new Medicamento { Cn = "111111", Nombre = "Paracetamol", UnidadesEnvase = 28 };
        med.Id = medicamentos.Crear(med);
        var tratamientos = new RepositorioTratamientos(conexion);
        tratamientos.Crear(new Tratamiento { PacienteId = conFaltantes.Id, MedicamentoId = med.Id, EnSpd = true, PautaD = FraccionDosis.Uno, FechaInicio = hoy.AddDays(-30), FechaPrescripcionInicial = hoy.AddDays(-30) });

        // Otro paciente con un blíster verificado sin entregar y una sesión a medias.
        var otro = servicioPacientes.Crear(new DatosAltaPaciente("Luis", "Sesiones", null, "87654321X", null, null, null, null, null, null,
            null, null, null, null, null, null, null, false, null, null, 2), null);
        var elaborador = new Usuario { Nombre = "Elena", Apellidos = "Ruiz", Login = "elena", HashPassword = "x", Rol = Rol.Elaborador };
        elaborador.Id = new RepositorioUsuarios(conexion).Crear(elaborador);
        var repositorioSpd = new RepositorioSpd(conexion);
        repositorioSpd.Crear(new SPD
        {
            NumRegistro = "F-1", CorrelativoNumRegistro = 1, PacienteId = otro.Id, SesionId = Guid.NewGuid(), ElaboradorId = elaborador.Id,
            ValidezDesde = hoy.AddDays(-2), ValidezHasta = hoy.AddDays(4), Estado = EstadoSpd.Verificado
        });
        var sesion = Guid.NewGuid();
        repositorioSpd.Crear(new SPD
        {
            NumRegistro = "F-2", CorrelativoNumRegistro = 2, PacienteId = otro.Id, SesionId = sesion, ElaboradorId = elaborador.Id,
            ValidezDesde = hoy.AddDays(-10), ValidezHasta = hoy.AddDays(-4), Estado = EstadoSpd.Entregado, FechaEntrega = DateTime.Today.AddDays(-9)
        });
        repositorioSpd.Crear(new SPD
        {
            NumRegistro = "F-3", CorrelativoNumRegistro = 3, PacienteId = otro.Id, SesionId = sesion, ElaboradorId = elaborador.Id,
            ValidezDesde = hoy.AddDays(-3), ValidezHasta = hoy.AddDays(3), Estado = EstadoSpd.Preparado
        });

        var listado = new ServicioListadoRetirada(repositorioPacientes, new RepositorioContactos(conexion), tratamientos, medicamentos,
            new RepositorioEnvases(conexion), repositorioFarmacia, new ComprobadorCoberturaSpdNulo(), auditoria);
        var servicio = new ServicioAvisosInicio(listado, repositorioSpd, repositorioPacientes, new RepositorioRegistrosAmbientales(conexion));

        var avisos = servicio.Obtener(hoy);

        Assert.Contains(avisos, a => a.Tipo == "Faltantes" && a.Texto.Contains("Ana Faltantes") && a.Texto.Contains("Paracetamol"));
        Assert.Contains(avisos, a => a.Tipo == "Sin entregar" && a.Texto.Contains("F-1"));
        Assert.Contains(avisos, a => a.Tipo == "Sesión a medias" && a.Texto.Contains("Luis Sesiones"));
        Assert.Contains(avisos, a => a.Tipo == "Ambiental");
    }
}
