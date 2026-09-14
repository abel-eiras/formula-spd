namespace Spd.Dominio;

/// <summary>Unidades enteras a descontar del envase por semana para un tratamiento (FR-522,
/// research.md Decisión 6 de Spec 005). Función pura: solo lee los campos del propio
/// <see cref="Tratamiento"/>, sin acceso a datos.
///
/// Todo se calcula en **doceavos** de unidad (ver <see cref="FraccionDosisExtensiones.Doceavos"/>).
/// Con `decimal` la regla fallaba con los tercios: un tercio en tres días suma exactamente un
/// comprimido, pero en decimal daba 0,999…9, así que «entero + 1» descontaba 1 en vez de 2; y dos
/// tercios en tres días daban 2,000…1, cuyo techo en el listado de retirada era 3 — un faltante que no
/// existía y que bloqueaba abrir la sesión (CA-601).</summary>
public static class CalculadoraUnidadesADescontar
{
    public static int Calcular(Tratamiento tratamiento)
    {
        if (tratamiento.AjusteUnidadesManual is int ajuste) return ajuste;

        var algunaFraccionaria = DosisPorDia(tratamiento).Any(d => d.Doceavos() % 12 != 0);
        var enteros = DoceavosSemanales(tratamiento) / 12;

        // Decisión 6: si alguna dosis es fraccionaria, entero + 1 siempre, incluso cuando la suma ya
        // es entera (CA-507b). Sin fracciones la suma es múltiplo de 12 y la división es exacta.
        return algunaFraccionaria ? enteros + 1 : enteros;
    }

    /// <summary>Suma semanal real de dosis (puede ser fraccionaria), sin aplicar la regla de
    /// pérdida por fracción de FR-522 — es la magnitud que usa el listado de retirada (FR-531,
    /// "unidades_blister") para estimar necesidades, no la que se descuenta realmente del envase.
    /// Cuando la suma es entera, el resultado es exactamente ese entero.</summary>
    public static decimal SumaSemanalReal(Tratamiento tratamiento) => DoceavosSemanales(tratamiento) / 12m;

    private static int DoceavosSemanales(Tratamiento tratamiento)
        => DosisPorDia(tratamiento).Sum(d => d.Doceavos()) * tratamiento.DiasSemana.Count(c => c == '1');

    private static FraccionDosis[] DosisPorDia(Tratamiento tratamiento) =>
        new[] { tratamiento.PautaD, tratamiento.PautaA, tratamiento.PautaC, tratamiento.PautaN }
            .Where(p => p is not null)
            .Select(p => p!.Value)
            .ToArray();
}
