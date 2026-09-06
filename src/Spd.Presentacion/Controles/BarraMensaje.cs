using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace Spd.Presentacion.Controles;

/// <summary>La barra donde la aplicación contesta (Spec 015 FR-1503, Spec 014 FR-1402).
///
/// Unifica dos cosas que hasta ahora se repetían a mano: el recuadro del mensaje y el botón
/// «¿Por qué?» que abre la explicación normativa del bloqueo. Que estuviera copiado en cada pantalla
/// tenía un coste real: en la mayoría **no estaba**, así que un rechazo con base legal —«el
/// representante necesita DNI», «la firma no puede ser futura»— se leía como un capricho de la
/// aplicación. El Art. XI exige que un bloqueo se explique; con el control, explicarlo es poner un
/// atributo.
///
/// Se oculta sola cuando no hay nada que decir: una barra vacía ocupando sitio es ruido.</summary>
public class BarraMensaje : TemplatedControl
{
    /// <summary>Arranca oculta: mientras nadie le haya dado un texto no hay nada que decir, y el
    /// caso normal de una pantalla es no tener ningún mensaje pendiente.</summary>
    static BarraMensaje() => IsVisibleProperty.OverrideDefaultValue<BarraMensaje>(false);

    public static readonly StyledProperty<string?> TextoProperty =
        AvaloniaProperty.Register<BarraMensaje, string?>(nameof(Texto));

    /// <summary>Apartado de ayuda que abre «¿Por qué?», como `"seccion:apartado"` (por ejemplo
    /// `"procedimiento:porque-de-los-bloqueos"`). Sin él, el botón no aparece: ofrecer una
    /// explicación que no existe es peor que no ofrecerla.</summary>
    public static readonly StyledProperty<string?> ApartadoProperty =
        AvaloniaProperty.Register<BarraMensaje, string?>(nameof(Apartado));

    public string? Texto
    {
        get => GetValue(TextoProperty);
        set => SetValue(TextoProperty, value);
    }

    public string? Apartado
    {
        get => GetValue(ApartadoProperty);
        set => SetValue(ApartadoProperty, value);
    }

    /// <summary>Derivada de <see cref="Apartado"/>. Es una propiedad de estilo y no una calculada
    /// porque la plantilla la consulta con `TemplateBinding`, y eso exige que sea accesible desde el
    /// XAML compilado.</summary>
    public static readonly StyledProperty<bool> HayApartadoProperty =
        AvaloniaProperty.Register<BarraMensaje, bool>(nameof(HayApartado));

    public bool HayApartado
    {
        get => GetValue(HayApartadoProperty);
        private set => SetValue(HayApartadoProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (e.NameScope.Find<Button>("BotonPorque") is { } boton)
        {
            boton.Click -= AbrirAyuda;
            boton.Click += AbrirAyuda;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == TextoProperty)
            IsVisible = !string.IsNullOrWhiteSpace(Texto);
        else if (change.Property == ApartadoProperty)
            HayApartado = !string.IsNullOrWhiteSpace(Apartado);
    }

    private void AbrirAyuda(object? sender, RoutedEventArgs e)
    {
        if (Apartado is not { Length: > 0 } apartado) return;

        var partes = apartado.Split(':', 2);
        if (partes is [var seccion, var id]) AyudaContextual.Abrir(seccion, id);
    }
}
