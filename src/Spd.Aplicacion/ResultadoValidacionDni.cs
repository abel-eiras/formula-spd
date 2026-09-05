namespace Spd.Aplicacion;

/// <summary>Resultado de validar un DNI/NIE (FR-005). Siempre es un aviso, nunca bloquea el guardado.</summary>
public sealed record ResultadoValidacionDni(bool EsValido);
