namespace Spd.Aplicacion;

/// <summary>Lee el fichero de nomenclátor ya descargado (research.md Decisión 5 de Spec 003):
/// un lector mínimo hoy, sustituible por un `PerfilImportacion` configurable en Spec 011 sin
/// cambiar este contrato.</summary>
public interface ILectorNomenclator
{
    ResultadoLecturaNomenclator Leer(string rutaFichero);
}
