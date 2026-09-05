namespace Spd.Aplicacion;

/// <summary>Resultado de leer el fichero de nomenclátor (research.md Decisión 5 de Spec 003). Un
/// formato de fichero inesperado es un fallo esperable, no una excepción de programación.</summary>
public sealed record ResultadoLecturaNomenclator(bool Exito, IReadOnlyList<FilaNomenclator> Filas, string? Error)
{
    public static ResultadoLecturaNomenclator Exitoso(IReadOnlyList<FilaNomenclator> filas) => new(true, filas, null);

    public static ResultadoLecturaNomenclator Fallido(string error) => new(false, [], error);
}
