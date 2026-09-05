using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Xunit;

namespace Spd.Aplicacion.Tests;

/// <summary>Smoke tests de la Fase 1 (Foundational) de Spec 001: migración 0002, entidades y
/// repositorios de Paciente/Contacto/Medico funcionan de extremo a extremo antes de construir las
/// user stories encima (Art. IX.1). T017/T018 de tasks.md.</summary>
public sealed class InfraestructuraPacientesFundamentosTests
{
    private static SqliteConnection AbrirBaseDeDatosDePrueba()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();
        return conexion;
    }

    private static Paciente PacienteDePrueba(string numFicha, int correlativo, string apellidos = "Núñez") => new()
    {
        NumFicha = numFicha,
        CorrelativoNumFicha = correlativo,
        FechaAltaFicha = DateTime.UtcNow,
        Nombre = "José",
        Apellidos = apellidos,
        DiaRetirada = "LU",
        BusquedaNormalizada = Normalizador.QuitarTildesYMayusculas($"{apellidos} José {numFicha}")
    };

    [Fact]
    public void AplicadorMigraciones_aplica_0002_de_forma_idempotente()
    {
        using var conexion = AbrirBaseDeDatosDePrueba();

        new AplicadorMigraciones(conexion).Aplicar();

        var version = conexion.ExecuteScalar<int>("SELECT MAX(version) FROM schema_version");
        Assert.Equal(2, version);
    }

    [Fact]
    public void RepositorioMedicos_crea_y_obtiene()
    {
        using var conexion = AbrirBaseDeDatosDePrueba();
        var repositorio = new RepositorioMedicos(conexion);
        var medico = new Medico
        {
            Nombre = "Ana",
            Apellidos = "Fernández Souto",
            BusquedaNormalizada = Normalizador.QuitarTildesYMayusculas("Fernández Souto Ana")
        };

        var id = repositorio.Crear(medico);
        var obtenido = repositorio.ObtenerPorId(id);

        Assert.NotNull(obtenido);
        Assert.Equal("Fernández Souto", obtenido!.Apellidos);
        Assert.True(obtenido.Activo);
    }

    [Fact]
    public void RepositorioPacientes_crea_y_obtiene_con_estado_evaluacion_por_defecto()
    {
        using var conexion = AbrirBaseDeDatosDePrueba();
        var repositorio = new RepositorioPacientes(conexion);

        var id = repositorio.Crear(PacienteDePrueba("F-000001", 1));
        var obtenido = repositorio.ObtenerPorId(id);

        Assert.NotNull(obtenido);
        Assert.Equal(EstadoPaciente.Evaluacion, obtenido!.Estado);
        Assert.Equal(1, repositorio.ObtenerSiguienteCorrelativo() - 1);
    }

    [Fact]
    public void RepositorioContactos_crea_y_lista_del_paciente()
    {
        using var conexion = AbrirBaseDeDatosDePrueba();
        var pacientes = new RepositorioPacientes(conexion);
        var pacienteId = pacientes.Crear(PacienteDePrueba("F-000002", 2, "Pérez"));
        var contactos = new RepositorioContactos(conexion);

        contactos.Crear(new Contacto { PacienteId = pacienteId, Tipo = TipoContacto.Familiar, Nombre = "Luis", Apellidos = "Pérez" });
        var lista = contactos.ListarDePaciente(pacienteId, incluirBaja: false);

        Assert.Single(lista);
        Assert.Equal(TipoContacto.Familiar, lista[0].Tipo);
    }
}
