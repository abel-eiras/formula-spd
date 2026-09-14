namespace Spd.Aplicacion;

/// <summary>Lo que hizo una importación completa del nomenclátor, en términos que el administrador
/// entiende: cuántos medicamentos se dieron de alta, cuántos entraron ya de baja, cuántos existían y no
/// se tocaron, y cuántas filas se descartaron por no ser medicamentos.</summary>
public sealed record ResultadoImportacionNomenclator(
    bool Exito,
    string? Error,
    int AltasActivas,
    int AltasDeBaja,
    int YaExistian,
    int NoSonMedicamentos)
{
    public int TotalAltas => AltasActivas + AltasDeBaja;

    public static ResultadoImportacionNomenclator Fallido(string error) => new(false, error, 0, 0, 0, 0);
}
