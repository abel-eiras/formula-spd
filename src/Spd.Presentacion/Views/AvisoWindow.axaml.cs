using Avalonia.Controls;
using System.Threading.Tasks;

namespace Spd.Presentacion.Views;

/// <summary>Aviso modal simple, reutilizable donde haga falta bloquear hasta que el usuario lo vea
/// (CA-1001: la app no puede terminar de cerrarse sin que el usuario haya visto el aviso).</summary>
public partial class AvisoWindow : Window
{
    public AvisoWindow()
    {
        InitializeComponent();
    }

    public AvisoWindow(string mensaje) : this()
    {
        this.FindControl<TextBlock>("TextoMensaje")!.Text = mensaje;
        this.FindControl<Button>("BotonAceptar")!.Click += (_, _) => Close();
    }

    public static Task MostrarAsync(Window propietaria, string mensaje)
        => new AvisoWindow(mensaje).ShowDialog(propietaria);
}
