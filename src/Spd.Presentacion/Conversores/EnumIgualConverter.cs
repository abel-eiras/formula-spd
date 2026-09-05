using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Spd.Presentacion.Conversores;

/// <summary>Compara un valor de enum con el nombre pasado como ConverterParameter (p. ej. para
/// mostrar el paso del asistente que corresponde a PasoActual).</summary>
public sealed class EnumIgualConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value?.ToString() == parameter?.ToString();

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
