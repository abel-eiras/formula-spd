using System;
using Avalonia.Controls;

namespace Spd.Presentacion.Views;

/// <summary>Se muestra al arrancar si la base de datos está cifrada (FR-1013): pide el secreto
/// una vez por sesión, nunca se guarda. Acepta indistintamente la contraseña maestra o la frase
/// de recuperación — quien construye esta ventana decide cuál de las dos encajó.</summary>
public partial class PeticionContrasenaMaestraWindow : Window
{
    public event Action<string>? SecretoIntroducido;

    public PeticionContrasenaMaestraWindow()
    {
        InitializeComponent();
        this.FindControl<Button>("BotonContinuar")!.Click += (_, _) =>
            SecretoIntroducido?.Invoke(this.FindControl<TextBox>("CampoSecreto")!.Text ?? string.Empty);
    }

    public void MostrarError(string mensaje)
    {
        var texto = this.FindControl<TextBlock>("TextoError")!;
        texto.Text = mensaje;
        texto.IsVisible = true;
    }
}
