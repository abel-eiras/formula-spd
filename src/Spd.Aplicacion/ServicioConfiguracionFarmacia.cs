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

    public void ActualizarValoresDefecto(DatosValoresDefecto valores, int? administradorQueEjecutaId)
    {
        // Solo escribe en Farmacia (CA-006): las entidades ya personalizadas (p. ej. un Paciente
        // con su propio dia_retirada, cuando exista Spec 001) no se leen ni se tocan desde aquí.
        var farmacia = ObtenerConfiguracion();
        farmacia.DiaRetiradaDefecto = valores.DiaRetiradaDefecto;
        farmacia.NBlisteresDefecto = valores.NBlisteresDefecto;
        farmacia.DiasAntelacionListado = valores.DiasAntelacionListado;
        farmacia.TempMin = valores.TempMin;
        farmacia.TempMax = valores.TempMax;
        farmacia.HrMin = valores.HrMin;
        farmacia.HrMax = valores.HrMax;
        farmacia.UmbralReutilizacionLecturaAmbientalHoras = valores.UmbralReutilizacionLecturaAmbientalHoras;
        if (valores.AniosRetencionPurga < 5)
            throw new ErrorValidacionException("La retención antes de purgar no puede ser inferior a cinco años (constitución Art. III.2).");
        farmacia.AniosRetencionPurga = valores.AniosRetencionPurga;
        repositorio.Actualizar(farmacia);
        auditoria.Registrar(administradorQueEjecutaId, "MODIFICACION", "Farmacia", farmacia.Id, "valores_defecto");
    }

    public void ActualizarSeparadorLector(string? separador, int? administradorQueEjecutaId)
    {
        var normalizado = string.IsNullOrEmpty(separador) ? null : separador;
        // Un separador con letras o dígitos partiría lotes y series que los contienen: el lector
        // entregaría un código que nunca se podría leer bien.
        if (normalizado is not null && (normalizado.Length > 3 || normalizado.Any(char.IsLetterOrDigit) || normalizado.Any(char.IsWhiteSpace)))
            throw new ErrorValidacionException("El separador debe ser de uno a tres símbolos, sin letras, dígitos ni espacios (por ejemplo «~»).");
        var farmacia = ObtenerConfiguracion();
        farmacia.SeparadorLector = normalizado;
        repositorio.Actualizar(farmacia);
        auditoria.Registrar(administradorQueEjecutaId, "MODIFICACION", "Farmacia", farmacia.Id, "separador_lector");
    }

    public void ActualizarDisposicionBlister(DisposicionBlister disposicion, int? administradorQueEjecutaId)
    {
        var farmacia = ObtenerConfiguracion();
        farmacia.BlisterInicio = disposicion.Inicio;
        farmacia.BlisterDiasDerechaIzquierda = disposicion.DiasDeDerechaAIzquierda;
        farmacia.BlisterNocheArriba = disposicion.NocheArriba;
        repositorio.Actualizar(farmacia);
        auditoria.Registrar(administradorQueEjecutaId, "MODIFICACION", "Farmacia", farmacia.Id, "disposicion_blister");
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
