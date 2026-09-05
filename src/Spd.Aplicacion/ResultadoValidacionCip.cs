namespace Spd.Aplicacion;

/// <summary>Resultado de comprobar la correspondencia de un CIP con los datos del paciente
/// (FR-005). Siempre es un aviso, nunca bloquea el guardado (CA-015).</summary>
public sealed record ResultadoValidacionCip(bool Corresponde);
