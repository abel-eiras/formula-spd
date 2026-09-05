using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Resultado de consultar un CN en el CIMA REST API público de la AEMPS. `Encontrado`
/// distingue "no está en CIMA" (p. ej. una fórmula magistral: CIMA solo indexa medicamentos
/// registrados) de un fallo de red (`Error` no nulo). `EnvaseIndicado` es informativo, tomado del
/// nombre de la presentación cuando CIMA lo incluye entre paréntesis (p. ej. "(Blister)",
/// "(Frasco)") — nunca se guarda en `Medicamento`, es solo una pista para el profesional.</summary>
public sealed record ResultadoConsultaCima(
    bool Encontrado,
    string? Nombre,
    string? PrincipioActivo,
    string? Laboratorio,
    FormaFarmaceutica? FormaFarmaceutica,
    string? EnvaseIndicado,
    string? Error)
{
    public static ResultadoConsultaCima NoEncontrado() => new(false, null, null, null, null, null, null);

    public static ResultadoConsultaCima Fallido(string error) => new(false, null, null, null, null, null, error);

    public static ResultadoConsultaCima Exitoso(
        string nombre, string? principioActivo, string? laboratorio, FormaFarmaceutica? formaFarmaceutica, string? envaseIndicado)
        => new(true, nombre, principioActivo, laboratorio, formaFarmaceutica, envaseIndicado, null);
}
