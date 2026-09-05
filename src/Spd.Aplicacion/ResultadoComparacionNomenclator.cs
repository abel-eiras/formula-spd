using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Un medicamento existente cuyo nombre en el nomenclátor difiere del registrado (FR-320).</summary>
public sealed record ComparacionFila(Medicamento Existente, string NombreNomenclator);

/// <summary>Resultado de comparar el nomenclátor con el catálogo (FR-320). Nunca incluye
/// descripción física ni aptitud SPD (FR-321): esos campos no se comparan ni se tocan.</summary>
public sealed record ResultadoComparacionNomenclator(
    bool Exito, string? Error, IReadOnlyList<FilaNomenclator> Nuevos,
    IReadOnlyList<ComparacionFila> ConNombreDistinto, int SinCambios)
{
    public static ResultadoComparacionNomenclator Fallido(string error) => new(false, error, [], [], 0);
}
