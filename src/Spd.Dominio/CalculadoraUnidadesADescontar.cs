namespace Spd.Dominio;

/// <summary>Unidades enteras a descontar del envase por semana para un tratamiento (FR-522,
/// research.md Decisión 6 de Spec 005). Función pura: solo lee los campos del propio
/// <see cref="Tratamiento"/>, sin acceso a datos.</summary>
public static class CalculadoraUnidadesADescontar
{
    public static int Calcular(Tratamiento tratamiento)
    {
        if (tratamiento.AjusteUnidadesManual is int ajuste) return ajuste;

        var algunaFraccionaria = DosisPorDia(tratamiento).Any(d => d.Valor() % 1m != 0m);
        var sumaSemanal = SumaSemanalReal(tratamiento);

        return algunaFraccionaria
            ? (int)Math.Floor(sumaSemanal) + 1
            : (int)Math.Round(sumaSemanal, MidpointRounding.AwayFromZero);
    }

    /// <summary>Suma semanal real de dosis (puede ser fraccionaria), sin aplicar la regla de
    /// pérdida por fracción de FR-522 — es la magnitud que usa el listado de retirada (FR-531,
    /// "unidades_blister") para estimar necesidades, no la que se descuenta realmente del envase.</summary>
    public static decimal SumaSemanalReal(Tratamiento tratamiento)
    {
        var sumaDiaria = DosisPorDia(tratamiento).Sum(d => d.Valor());
        var diasActivos = tratamiento.DiasSemana.Count(c => c == '1');
        return sumaDiaria * diasActivos;
    }

    private static FraccionDosis[] DosisPorDia(Tratamiento tratamiento) =>
        new[] { tratamiento.PautaD, tratamiento.PautaA, tratamiento.PautaC, tratamiento.PautaN }
            .Where(p => p is not null)
            .Select(p => p!.Value)
            .ToArray();
}
