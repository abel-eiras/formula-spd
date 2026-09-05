using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Xunit;

namespace Spd.Aplicacion.Tests;

public sealed class ServicioUsuariosTests
{
    private static (ServicioUsuarios Servicio, SqliteConnection Conexion) CrearServicio()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();

        var servicio = new ServicioUsuarios(
            new RepositorioUsuarios(conexion), new HasheadorArgon2id(), new RegistradorAuditoria(conexion));
        return (servicio, conexion);
    }

    private static DatosAltaUsuario DatosElaborador(string login = "eva.elab") =>
        new("Eva", "Elaboradora", login, PasswordProvisional: null, Rol.Elaborador, CargoPnt: null, Colegiado: null);

    private static DatosAltaUsuario DatosAdministrador(string login) =>
        new("Ana", "Administradora", login, PasswordProvisional: "contraseña-inicial", Rol.Administrador, CargoPnt: null, Colegiado: null);

    [Fact]
    public void DarDeBaja_no_elimina_la_fila_solo_marca_activo_0_y_fecha_baja()
    {
        var (servicio, conexion) = CrearServicio();
        servicio.CrearUsuario(DatosAdministrador("admin1"), administradorQueEjecutaId: null);
        var otroAdmin = servicio.CrearUsuario(DatosAdministrador("admin2"), administradorQueEjecutaId: null);

        servicio.DarDeBaja(otroAdmin.Id, usuarioQueEjecutaId: null);

        var totalFilas = conexion.ExecuteScalar<int>("SELECT COUNT(*) FROM Usuario WHERE id = @id", new { id = otroAdmin.Id });
        Assert.Equal(1, totalFilas); // la fila sigue existiendo (Art. III.1)

        var fila = conexion.QuerySingle("SELECT activo, fecha_baja FROM Usuario WHERE id = @id", new { id = otroAdmin.Id });
        Assert.Equal(0L, (long)fila.activo);
        Assert.NotNull(fila.fecha_baja);
    }

    [Fact]
    public void DarDeBaja_lanza_si_es_el_unico_administrador_activo()
    {
        var (servicio, _) = CrearServicio();
        var unico = servicio.CrearUsuario(DatosAdministrador("unico.admin"), administradorQueEjecutaId: null);

        Assert.Throws<UltimoAdministradorException>(() => servicio.DarDeBaja(unico.Id, usuarioQueEjecutaId: null));
    }

    [Fact]
    public void IntentarLogin_bloquea_al_quinto_intento_fallido_y_rechaza_el_sexto_aunque_sea_correcto()
    {
        var (servicio, _) = CrearServicio();
        servicio.CrearUsuario(DatosAdministrador("con.bloqueo") with { PasswordProvisional = "contraseña-correcta" }, null);

        for (var intento = 1; intento <= 5; intento++)
        {
            servicio.IntentarLogin("con.bloqueo", "contraseña-incorrecta");
        }
        var sexto = servicio.IntentarLogin("con.bloqueo", "contraseña-correcta");

        Assert.False(sexto.Exito);
        Assert.True(sexto.Bloqueado);
    }

    [Fact]
    public void DesbloquearUsuario_solo_permitido_a_un_Administrador()
    {
        var (servicio, _) = CrearServicio();
        var elaborador = servicio.CrearUsuario(DatosElaborador(), null);
        var administrador = servicio.CrearUsuario(DatosAdministrador("admin.desbloquea"), null);

        Assert.Throws<ErrorValidacionException>(
            () => servicio.DesbloquearUsuario(elaborador.Id, administradorId: elaborador.Id));

        servicio.DesbloquearUsuario(elaborador.Id, administradorId: administrador.Id); // no debe lanzar
    }

    [Fact]
    public void CrearUsuario_genera_password_provisional_y_marca_debe_cambiar_password()
    {
        var (servicio, _) = CrearServicio();

        var usuario = servicio.CrearUsuario(DatosElaborador(), administradorQueEjecutaId: null);

        Assert.True(usuario.DebeCambiarPassword);
        Assert.NotEmpty(usuario.HashPassword);
    }

    [Fact]
    public void ResetearPassword_tambien_marca_debe_cambiar_password()
    {
        var (servicio, conexion) = CrearServicio();
        var administrador = servicio.CrearUsuario(DatosAdministrador("admin.resetea"), null);
        var elaborador = servicio.CrearUsuario(DatosElaborador(), null);
        servicio.CambiarPassword(elaborador.Id, "ya-la-cambie");

        servicio.ResetearPassword(elaborador.Id, administrador.Id);

        var debeCambiar = conexion.ExecuteScalar<long>(
            "SELECT debe_cambiar_password FROM Usuario WHERE id = @id", new { id = elaborador.Id });
        Assert.Equal(1L, debeCambiar);
    }

    [Fact]
    public void CrearUsuario_DarDeBaja_ResetearPassword_y_DesbloquearUsuario_registran_en_auditoria()
    {
        var (servicio, conexion) = CrearServicio();
        var administrador = servicio.CrearUsuario(DatosAdministrador("admin.audita"), null);
        var elaborador = servicio.CrearUsuario(DatosElaborador(), administrador.Id);

        servicio.ResetearPassword(elaborador.Id, administrador.Id);
        servicio.DesbloquearUsuario(elaborador.Id, administrador.Id);
        servicio.DarDeBaja(elaborador.Id, administrador.Id);

        var acciones = conexion.Query<string>("SELECT accion FROM Auditoria ORDER BY id").ToList();
        Assert.Contains("ALTA", acciones);
        Assert.Contains("RESETEO_PASSWORD", acciones);
        Assert.Contains("DESBLOQUEO", acciones);
        Assert.Contains("BAJA", acciones);
    }
}
