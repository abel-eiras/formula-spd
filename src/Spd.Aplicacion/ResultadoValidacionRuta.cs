namespace Spd.Aplicacion;

/// <summary>Resultado de validar una ruta de backup/documentos (FR-030/031/032).</summary>
public sealed record ResultadoValidacionRuta(bool Existe, bool Escribible, bool CoincideConCarpetaInstalacion);
