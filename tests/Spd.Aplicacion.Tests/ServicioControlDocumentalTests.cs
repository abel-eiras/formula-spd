using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Xunit;

namespace Spd.Aplicacion.Tests;

public sealed class ServicioControlDocumentalTests
{
    private static (ServicioControlDocumental Servicio, SqliteConnection Conexion, int AdministradorId, int ElaboradorId) CrearServicio()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();

        var repositorioUsuarios = new RepositorioUsuarios(conexion);
        var administradorId = repositorioUsuarios.Crear(new Usuario
        {
            Nombre = "Ana", Apellidos = "Administradora", Login = "ana.admin", HashPassword = "hash", Rol = Rol.Administrador
        });
        var elaboradorId = repositorioUsuarios.Crear(new Usuario
        {
            Nombre = "Eva", Apellidos = "Elaboradora", Login = "eva.elab", HashPassword = "hash", Rol = Rol.Elaborador
        });

        var servicio = new ServicioControlDocumental(
            new RepositorioControlDocumental(conexion), repositorioUsuarios, new RegistradorAuditoria(conexion));
        return (servicio, conexion, administradorId, elaboradorId);
    }

    private static DatosCambioPnt DatosCambio(int usuarioId) => new(
        "PNT-SPD", "1.1", "Actualización anual", DateOnly.FromDateTime(DateTime.Today), usuarioId, usuarioId, usuarioId);

    [Fact]
    public void RegistrarCambioPnt_lanza_si_el_usuario_no_es_administrador_CA_903()
    {
        var (servicio, _, _, elaboradorId) = CrearServicio();

        Assert.Throws<ErrorValidacionException>(() => servicio.RegistrarCambioPnt(DatosCambio(elaboradorId), elaboradorId));
    }

    [Fact]
    public void RegistrarCambioPnt_funciona_para_un_administrador()
    {
        var (servicio, _, administradorId, _) = CrearServicio();

        var cambio = servicio.RegistrarCambioPnt(DatosCambio(administradorId), administradorId);

        Assert.Equal("PNT-SPD", cambio.Documento);
    }

    [Fact]
    public void ListarCambiosPnt_lanza_para_un_elaborador_CA_903()
    {
        var (servicio, _, administradorId, elaboradorId) = CrearServicio();
        servicio.RegistrarCambioPnt(DatosCambio(administradorId), administradorId);

        Assert.Throws<ErrorValidacionException>(() => servicio.ListarCambiosPnt(elaboradorId));
        Assert.Single(servicio.ListarCambiosPnt(administradorId)); // no debe lanzar
    }

    [Fact]
    public void RegistrarCopia_y_ListarCopias_tienen_la_misma_restriccion_CA_903()
    {
        var (servicio, _, administradorId, elaboradorId) = CrearServicio();
        var datos = new DatosCopia("PNT-SPD", 1, administradorId, DateOnly.FromDateTime(DateTime.Today));

        Assert.Throws<ErrorValidacionException>(() => servicio.RegistrarCopia(datos, elaboradorId));

        servicio.RegistrarCopia(datos, administradorId); // no debe lanzar
        Assert.Throws<ErrorValidacionException>(() => servicio.ListarCopias(elaboradorId));
        Assert.Single(servicio.ListarCopias(administradorId));
    }

    [Fact]
    public void RegistrarCambioPnt_y_RegistrarCopia_registran_en_auditoria()
    {
        var (servicio, conexion, administradorId, _) = CrearServicio();

        servicio.RegistrarCambioPnt(DatosCambio(administradorId), administradorId);
        servicio.RegistrarCopia(new DatosCopia("PNT-SPD", 1, administradorId, DateOnly.FromDateTime(DateTime.Today)), administradorId);

        var acciones = conexion.Query<string>(
            "SELECT entidad FROM Auditoria WHERE entidad IN ('ControlCambiosPNT', 'ControlCopias') ORDER BY id").ToList();
        Assert.Contains("ControlCambiosPNT", acciones);
        Assert.Contains("ControlCopias", acciones);
    }
}
