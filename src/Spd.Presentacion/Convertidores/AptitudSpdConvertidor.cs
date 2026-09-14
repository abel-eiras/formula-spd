using Avalonia.Data.Converters;

namespace Spd.Presentacion.Convertidores;

/// <summary>Aptitud SPD de tres estados para pantalla (Spec 003 FR-301, revisado el 2026-09-14).
/// «Sin confirmar» no es «No»: es que nadie lo ha decidido todavía.</summary>
public static class AptitudSpdConvertidor
{
    public static readonly IValueConverter Texto = new FuncValueConverter<bool?, string>(apto => apto switch
    {
        true => "Sí",
        false => "No",
        null => "Sin confirmar"
    });
}
