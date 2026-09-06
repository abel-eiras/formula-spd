using Avalonia;
using Avalonia.Controls.Primitives;

namespace Spd.Presentacion.Controles;

/// <summary>Variantes de <see cref="Pastilla"/>. El color es información, no decoración: Apto,
/// Aviso y Bloqueo se reservan para estado (Spec 015 FR-1504).</summary>
public enum VariantePastilla
{
    Neutra,
    Acento,
    Apto,
    Aviso,
    Bloqueo
}

/// <summary>Etiqueta de estado (estado de paciente, de blíster, de tratamiento, envases al día…).
/// Su tema vive en `Estilos/Controles.axaml` para que reaccione al cambio de variante clara/oscura.</summary>
public class Pastilla : TemplatedControl
{
    public static readonly StyledProperty<string?> TextoProperty =
        AvaloniaProperty.Register<Pastilla, string?>(nameof(Texto));

    public static readonly StyledProperty<VariantePastilla> VarianteProperty =
        AvaloniaProperty.Register<Pastilla, VariantePastilla>(nameof(Variante));

    public string? Texto
    {
        get => GetValue(TextoProperty);
        set => SetValue(TextoProperty, value);
    }

    public VariantePastilla Variante
    {
        get => GetValue(VarianteProperty);
        set => SetValue(VarianteProperty, value);
    }
}
