namespace Spd.Dominio;

/// <summary>Se lanza cuando una operación dejaría al sistema sin ningún Administrador activo (FR-042).</summary>
public sealed class UltimoAdministradorException(string mensaje) : Exception(mensaje);
