using Spd.Aplicacion;

namespace Spd.Infraestructura;

/// <summary>Lee un CSV mínimo de nomenclátor con columnas exactas `CN` y `Nombre` (research.md
/// Decisión 5 de Spec 003). Simplificación deliberada: sin perfiles de mapeo columna a columna,
/// sin soporte de comas dentro de un valor. Spec 011 lo sustituirá por un `PerfilImportacion`
/// configurable sin cambiar `ILectorNomenclator`.</summary>
public sealed class LectorNomenclatorCsv : ILectorNomenclator
{
    public ResultadoLecturaNomenclator Leer(string rutaFichero)
    {
        var lineas = File.ReadAllLines(rutaFichero);
        if (lineas.Length == 0)
        {
            return ResultadoLecturaNomenclator.Fallido("El fichero de nomenclátor está vacío.");
        }

        var cabecera = lineas[0].Split(',');
        var indiceCn = Array.IndexOf(cabecera, "CN");
        var indiceNombre = Array.IndexOf(cabecera, "Nombre");
        if (indiceCn < 0 || indiceNombre < 0)
        {
            return ResultadoLecturaNomenclator.Fallido(
                "El fichero no tiene las columnas 'CN' y 'Nombre' esperadas (research.md Decisión 5).");
        }

        var filas = lineas.Skip(1)
            .Where(linea => !string.IsNullOrWhiteSpace(linea))
            .Select(linea => linea.Split(','))
            .Where(campos => campos.Length > Math.Max(indiceCn, indiceNombre))
            .Select(campos => new FilaNomenclator(campos[indiceCn].Trim(), campos[indiceNombre].Trim()))
            .ToList();

        return ResultadoLecturaNomenclator.Exitoso(filas);
    }
}
