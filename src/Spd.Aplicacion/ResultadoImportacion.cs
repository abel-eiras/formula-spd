using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Resumen final de una importación de tratamiento+envase (FR-577).</summary>
public sealed record ResultadoImportacion(
    IReadOnlyList<EnvaseImportado> EnvasesCreados,
    IReadOnlyList<int> TratamientosPendientesCreados,
    IReadOnlyList<FilaImportacionCruda> FilasConCnNoEncontrado,
    IReadOnlyList<(FilaImportacionCruda Fila, int PacienteIdExistente)> FilasConSerieDuplicada);

public sealed record EnvaseImportado(Envase Envase, bool TratamientoPendienteDePosologia);
