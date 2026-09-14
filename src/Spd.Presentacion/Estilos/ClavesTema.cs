namespace Spd.Presentacion.Estilos;

/// <summary>Claves del tema (Spec 015 H1.1). Son el contrato entre `Paleta.axaml`/`Tipografia.axaml`
/// y el resto de la interfaz: `TemaTests` comprueba que todas resuelven en la variante clara y en la
/// oscura (CA-1502), que es el fallo clásico de un tema a medias.</summary>
public static class ClavesTema
{
    public static readonly string[] Colores =
    [
        "Fondo", "LogoFormulaFarma", "Superficie", "SuperficieAlterna", "Tinta", "TintaSuave", "Linea",
        "Acento", "AcentoSuave", "AcentoTexto",
        "Apto", "AptoSuave", "Aviso", "AvisoSuave", "Bloqueo", "BloqueoSuave",
    ];

    public static readonly string[] Fuentes = ["FuenteInterfaz", "FuenteDatos"];

    public static readonly string[] Tamanos =
    [
        "TamanoEtiqueta", "TamanoBase", "TamanoDestacado", "TamanoSeccion", "TamanoPagina",
    ];
}
