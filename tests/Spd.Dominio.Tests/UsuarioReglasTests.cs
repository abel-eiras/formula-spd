using Spd.Dominio;
using Xunit;

namespace Spd.Dominio.Tests;

public sealed class UsuarioReglasTests
{
    private static Usuario Administrador(bool activo = true) => new()
    {
        Nombre = "Ana", Apellidos = "Administradora", Login = "ana.admin",
        HashPassword = "hash", Rol = Rol.Administrador, Activo = activo
    };

    private static Usuario Elaborador(bool activo = true) => new()
    {
        Nombre = "Eva", Apellidos = "Elaboradora", Login = "eva.elab",
        HashPassword = "hash", Rol = Rol.Elaborador, Activo = activo
    };

    [Fact]
    public void ValidarBaja_lanza_si_es_el_unico_administrador_activo()
    {
        var admin = Administrador();

        Assert.Throws<UltimoAdministradorException>(
            () => ReglaUnicoAdministrador.ValidarBaja(admin, administradoresActivos: 1));
    }

    [Fact]
    public void ValidarBaja_permite_si_hay_otro_administrador_activo()
        => ReglaUnicoAdministrador.ValidarBaja(Administrador(), administradoresActivos: 2);

    [Fact]
    public void ValidarBaja_permite_dar_de_baja_a_un_Elaborador_aunque_solo_haya_un_administrador()
        => ReglaUnicoAdministrador.ValidarBaja(Elaborador(), administradoresActivos: 1);

    [Fact]
    public void ValidarDegradacion_lanza_si_degradar_al_unico_administrador_activo()
    {
        var admin = Administrador();

        Assert.Throws<UltimoAdministradorException>(
            () => ReglaUnicoAdministrador.ValidarDegradacion(admin, Rol.Elaborador, administradoresActivos: 1));
    }

    [Fact]
    public void ValidarDegradacion_permite_mantener_el_rol_Administrador()
        => ReglaUnicoAdministrador.ValidarDegradacion(Administrador(), Rol.Administrador, administradoresActivos: 1);
}
