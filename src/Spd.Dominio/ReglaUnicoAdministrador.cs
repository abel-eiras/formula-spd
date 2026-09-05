namespace Spd.Dominio;

/// <summary>Un usuario no puede autodesactivarse ni autodegradarse de Administrador si es el único
/// Administrador activo del sistema (Art. VII, FR-042): la aplicación no puede quedarse sin ningún
/// administrador.</summary>
public static class ReglaUnicoAdministrador
{
    public static void ValidarBaja(Usuario objetivo, int administradoresActivos)
    {
        if (EsElUltimoAdministradorActivo(objetivo, administradoresActivos))
        {
            throw new UltimoAdministradorException(
                "No se puede dar de baja al único Administrador activo del sistema.");
        }
    }

    public static void ValidarDegradacion(Usuario objetivo, Rol rolNuevo, int administradoresActivos)
    {
        if (rolNuevo != Rol.Administrador && EsElUltimoAdministradorActivo(objetivo, administradoresActivos))
        {
            throw new UltimoAdministradorException(
                "No se puede degradar al único Administrador activo del sistema.");
        }
    }

    private static bool EsElUltimoAdministradorActivo(Usuario objetivo, int administradoresActivos)
        => objetivo.Rol == Rol.Administrador && objetivo.Activo && administradoresActivos <= 1;
}
