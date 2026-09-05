namespace Spd.Aplicacion;

/// <summary>Datos de entrada para un control de cambios del PNT (FR-940). Exclusivo de
/// Administrador (FR-942).</summary>
public sealed record DatosCambioPnt(
    string Documento, string Version, string DescripcionCambio, DateOnly Fecha,
    int RedactadoPor, int RevisadoPor, int AprobadoPor);
