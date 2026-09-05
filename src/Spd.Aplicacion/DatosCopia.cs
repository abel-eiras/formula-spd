namespace Spd.Aplicacion;

/// <summary>Datos de entrada para un control de copias del PNT (FR-941). Exclusivo de
/// Administrador (FR-942).</summary>
public sealed record DatosCopia(string Documento, int NumCopia, int UsuarioId, DateOnly Fecha);
