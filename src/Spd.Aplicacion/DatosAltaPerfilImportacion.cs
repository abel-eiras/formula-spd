using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Datos de entrada para crear o editar un perfil de importación/exportación (FR-1100).</summary>
public sealed record DatosAltaPerfilImportacion(
    string Nombre, TipoPerfilImportacion Tipo, string Separador, string Codificacion,
    bool TieneCabecera, IReadOnlyList<ParCampoColumna> Mapeo, string? RegexUnidadesEnvase);
