using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Xunit;

namespace Spd.Aplicacion.Tests;

public sealed class ServicioRegistrosCalidadTests
{
    private static (ServicioRegistrosCalidad Servicio, SqliteConnection Conexion, int UsuarioId) CrearServicio()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();

        var usuarioId = new RepositorioUsuarios(conexion).Crear(new Usuario
        {
            Nombre = "Eva", Apellidos = "Elaboradora", Login = "eva.elab",
            HashPassword = "hash-de-prueba", Rol = Rol.Elaborador
        });

        var servicio = new ServicioRegistrosCalidad(new RepositorioRegistrosCalidad(conexion), new RegistradorAuditoria(conexion));
        return (servicio, conexion, usuarioId);
    }

    [Fact]
    public void RegistrarFormacion_tres_veces_deja_las_tres_consultables_CA_902()
    {
        var (servicio, _, usuarioId) = CrearServicio();
        var hoy = DateOnly.FromDateTime(DateTime.Today);

        servicio.RegistrarFormacion(new DatosFormacion(usuarioId, "Curso 1", "Colegio", hoy, true), usuarioId);
        servicio.RegistrarFormacion(new DatosFormacion(usuarioId, "Curso 2", "Colegio", hoy, false), usuarioId);
        servicio.RegistrarFormacion(new DatosFormacion(usuarioId, "Curso 3", "Colegio", hoy, true), usuarioId);

        var formaciones = servicio.ListarFormacion(usuarioId);

        Assert.Equal(3, formaciones.Count);
        Assert.Contains(formaciones, f => f.NombreCurso == "Curso 1");
        Assert.Contains(formaciones, f => f.NombreCurso == "Curso 2");
        Assert.Contains(formaciones, f => f.NombreCurso == "Curso 3");
    }

    [Fact]
    public void RegistrarRecogidaResiduos_guarda_fecha_empresa_y_usuario()
    {
        var (servicio, _, usuarioId) = CrearServicio();

        var recogida = servicio.RegistrarRecogidaResiduos(
            new DatosRecogidaResiduos(DateOnly.FromDateTime(DateTime.Today), "Gestora S.L.", null), usuarioId);

        Assert.Equal("Gestora S.L.", recogida.EmpresaGestora);
        Assert.Equal(usuarioId, recogida.UsuarioId);
    }

    [Fact]
    public void RegistrarFormacion_y_RegistrarRecogidaResiduos_registran_en_auditoria()
    {
        var (servicio, conexion, usuarioId) = CrearServicio();

        servicio.RegistrarFormacion(
            new DatosFormacion(usuarioId, "Curso 1", null, DateOnly.FromDateTime(DateTime.Today), false), usuarioId);
        servicio.RegistrarRecogidaResiduos(
            new DatosRecogidaResiduos(DateOnly.FromDateTime(DateTime.Today), "Gestora S.L.", null), usuarioId);

        var acciones = conexion.Query<string>(
            "SELECT entidad FROM Auditoria WHERE entidad IN ('FormacionPersonal', 'RecogidaResiduos') ORDER BY id").ToList();
        Assert.Contains("FormacionPersonal", acciones);
        Assert.Contains("RecogidaResiduos", acciones);
    }
}
