using System.Resources;

namespace Spd.Presentacion.Recursos;

/// <summary>Textos de interfaz centralizados en Textos.resx (Art. IX.5).</summary>
public static class Textos
{
    private static readonly ResourceManager Recursos = new("Spd.Presentacion.Recursos.Textos", typeof(Textos).Assembly);
    public static string RecordatorioActualizaciones => Recursos.GetString(nameof(RecordatorioActualizaciones))!;
    public static string AbrirActualizaciones => Recursos.GetString(nameof(AbrirActualizaciones))!;
    public static string GrupoHomogeneo => Recursos.GetString(nameof(GrupoHomogeneo))!;
    public static string AyudaGrupoHomogeneo => Recursos.GetString(nameof(AyudaGrupoHomogeneo))!;
    public static string OtraCaja => Recursos.GetString(nameof(OtraCaja))!;
    public static string Medicamento => Recursos.GetString(nameof(Medicamento))!;
    public static string AltaConsecutiva => Recursos.GetString(nameof(AltaConsecutiva))!;
}
