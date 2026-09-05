namespace Spd.Dominio;

/// <summary>Deriva la aptitud SPD por defecto a partir de la forma farmacéutica (FR-301). Todas
/// las formas del catálogo cerrado son aptas por defecto salvo <see cref="FormaFarmaceutica.OtraNoApta"/>
/// (research.md Decisión 2): las formas realmente no aptas (líquidas, efervescentes,
/// bucodispersables, parenterales, tópicas) no forman parte de este enum cerrado.</summary>
public static class ReglaAptitudSpd
{
    public static bool PorDefecto(FormaFarmaceutica forma) => forma != FormaFarmaceutica.OtraNoApta;
}
