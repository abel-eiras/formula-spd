using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Datos de la farmacia, prefijos de numeración y rutas (FR-010..FR-013, FR-030..FR-032).
/// Todo método de escritura registra en auditoría (Art. VII.6).</summary>
public sealed class ServicioConfiguracionFarmacia(
    IRepositorioFarmacia repositorio, IRegistradorAuditoria auditoria) : IServicioConfiguracionFarmacia
{
    public Farmacia ObtenerConfiguracion()
        => repositorio.Obtener()
           ?? throw new InvalidOperationException("Todavía no se ha completado el asistente de primer arranque.");

    public void ActualizarDatosFarmacia(Farmacia datos, int? administradorQueEjecutaId)
    {
        repositorio.Actualizar(datos);
        auditoria.Registrar(administradorQueEjecutaId, "MODIFICACION", "Farmacia", datos.Id, "datos");
    }

    public void ActualizarPrefijos(string prefijoNumFicha, string prefijoNumSpd, int? administradorQueEjecutaId)
    {
        // Solo toca estas dos columnas: los números ya asignados (Paciente.num_ficha, SPD.num_registro
        // en las specs que los usan) no se reescriben porque nadie vuelve a escribirlos (CA-001).
        var farmacia = ObtenerConfiguracion();
        farmacia.PrefijoNumFicha = prefijoNumFicha;
        farmacia.PrefijoNumSpd = prefijoNumSpd;
        repositorio.Actualizar(farmacia);
        auditoria.Registrar(administradorQueEjecutaId, "MODIFICACION", "Farmacia", farmacia.Id, "prefijos");
    }

    public ResultadoValidacionRuta ValidarRuta(string ruta)
    {
        var existe = Directory.Exists(ruta);
        var escribible = existe && SePuedeEscribirEn(ruta);
        var coincideConInstalacion = EstaDentroDeLaCarpetaDeInstalacion(ruta);
        return new ResultadoValidacionRuta(existe, escribible, coincideConInstalacion);
    }

    private static bool SePuedeEscribirEn(string ruta)
    {
        var ficheroDePrueba = Path.Combine(ruta, $".spd_prueba_escritura_{Guid.NewGuid():N}");
        try
        {
            File.WriteAllText(ficheroDePrueba, string.Empty);
            return true;
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
        finally
        {
            if (File.Exists(ficheroDePrueba))
            {
                File.Delete(ficheroDePrueba);
            }
        }
    }

    private static bool EstaDentroDeLaCarpetaDeInstalacion(string ruta)
    {
        var rutaCompleta = Path.GetFullPath(ruta).TrimEnd(Path.DirectorySeparatorChar);
        var carpetaInstalacion = Path.GetFullPath(AppContext.BaseDirectory).TrimEnd(Path.DirectorySeparatorChar);
        return rutaCompleta.StartsWith(carpetaInstalacion, StringComparison.OrdinalIgnoreCase);
    }
}
