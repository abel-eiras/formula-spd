using System.Linq;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spd.Aplicacion;
using Spd.Dominio;

namespace Spd.Presentacion.ViewModels;

/// <summary>Configuración / Lector de códigos (spec-012 FR-1211, FR-1212): prueba del lector con una
/// caja real y separador sustituto. Permite ajustar cualquier lector sin conocer su modelo: se escanea,
/// se ve qué ha llegado carácter a carácter y qué ha entendido la aplicación.</summary>
public sealed partial class LectorCodigosViewModel : ViewModelBase
{
    private const char SeparadorGs = '\u001D';

    private readonly IServicioConfiguracionFarmacia _servicioFarmacia;
    private readonly int? _administradorActualId;

    [ObservableProperty] private string _codigoPrueba = string.Empty;
    [ObservableProperty] private string? _caracteresRecibidos;
    [ObservableProperty] private string? _diagnostico;
    [ObservableProperty] private string? _interpretacion;
    [ObservableProperty] private string? _separadorSustituto;
    [ObservableProperty] private string? _mensaje;

    public LectorCodigosViewModel(IServicioConfiguracionFarmacia servicioFarmacia, int? administradorActualId)
    {
        _servicioFarmacia = servicioFarmacia;
        _administradorActualId = administradorActualId;
        _separadorSustituto = servicioFarmacia.ObtenerConfiguracion().SeparadorLector;
    }

    /// <summary>Intro en el campo de prueba: lo que el lector ha tecleado, visible, y lo que se entiende
    /// con el separador escrito ahora mismo (aún sin guardar, para poder probar antes de guardar).</summary>
    [RelayCommand]
    private void Probar()
    {
        var recibido = CodigoPrueba;
        CodigoPrueba = string.Empty;
        CaracteresRecibidos = HacerVisible(recibido);
        Diagnostico = Diagnosticar(recibido, SeparadorSustituto);
        Interpretacion = Describir(LectorGs1DataMatrix.Interpretar(recibido, SeparadorSustituto));
    }

    [RelayCommand]
    private void GuardarSeparador()
    {
        try
        {
            _servicioFarmacia.ActualizarSeparadorLector(SeparadorSustituto, _administradorActualId);
            Mensaje = string.IsNullOrEmpty(SeparadorSustituto)
                ? "Guardado: sin separador sustituto."
                : $"Guardado: «{SeparadorSustituto}» se tratará como separador GS en todas las pantallas.";
        }
        catch (ErrorValidacionException ex)
        {
            Mensaje = ex.Message;
        }
    }

    /// <summary>Los caracteres de control no se ven en pantalla: se escriben entre ⟨ ⟩ para poder saber
    /// si el lector manda el GS, un retorno o nada.</summary>
    public static string HacerVisible(string texto)
    {
        var resultado = new StringBuilder();
        foreach (var c in texto)
        {
            resultado.Append(c switch
            {
                SeparadorGs => "⟨GS⟩",
                '\r' => "⟨CR⟩",
                '\n' => "⟨LF⟩",
                '\t' => "⟨TAB⟩",
                _ when char.IsControl(c) => $"⟨{(int)c:X2}⟩",
                _ => c.ToString()
            });
        }
        return resultado.ToString();
    }

    public static string Diagnosticar(string recibido, string? separadorSustituto)
    {
        if (recibido.Length == 0) return "No ha llegado nada: comprueba que el cursor está en el campo al escanear.";
        if (recibido.Contains(SeparadorGs))
            return $"Han llegado {recibido.Length} caracteres y el separador GS llega bien: no hace falta separador sustituto.";
        if (!string.IsNullOrEmpty(separadorSustituto) && recibido.Contains(separadorSustituto))
            return $"Han llegado {recibido.Length} caracteres y el separador sustituto «{separadorSustituto}» aparece: se trata como GS.";
        return $"Han llegado {recibido.Length} caracteres sin separador GS. Si alguna caja sale ambigua, programa el lector " +
               "para que mande el GS como un símbolo que no aparezca en lotes (por ejemplo «~») y escríbelo abajo.";
    }

    public static string Describir(LecturaGs1 lectura) => lectura.Estado switch
    {
        EstadoLecturaGs1.Reconocido => DescribirDatos(lectura.Datos!),
        EstadoLecturaGs1.Ambiguo =>
            $"Lectura ambigua: {lectura.Candidatas.Count} interpretaciones posibles. En las pantallas se pedirá elegir el lote mirando la caja.\n"
            + string.Join("\n", lectura.Candidatas.Select(c => $"  · Lote {c.Lote} · Serie {c.NumeroSerie}")),
        _ => "No es un DataMatrix de medicamento reconocible (faltan GTIN, lote, caducidad o serie, o algún dato es imposible)."
    };

    private static string DescribirDatos(DatosEnvaseEscaneado d)
        => $"GTIN {d.Gtin}\nCN {d.CnIdentificado ?? "(no viene en el código: caja extranjera)"}" +
           (d.CodigoNacional is null && d.CnIdentificado is not null ? " (deducido del GTIN)" : string.Empty) +
           $"\nLote {d.Lote}\nCaducidad {d.Caducidad.ComoFecha():dd/MM/yyyy}\nSerie {d.NumeroSerie}";
}
