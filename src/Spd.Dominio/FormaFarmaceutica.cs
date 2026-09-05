namespace Spd.Dominio;

/// <summary>Catálogo cerrado de formas farmacéuticas (FR-300). Confirmado sin ampliaciones en
/// `/speckit-clarify` (spec.md, Clarifications).</summary>
public enum FormaFarmaceutica
{
    Comprimido,
    ComprimidoLiberacionProlongada,
    Capsula,
    CapsulaLiberacionProlongada,
    Gragea,
    Pastilla,
    Pildora,
    OtraNoApta
}
