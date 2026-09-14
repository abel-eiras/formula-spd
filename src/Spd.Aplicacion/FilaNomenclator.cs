namespace Spd.Aplicacion;

/// <summary>Una fila del nomenclátor de facturación.
///
/// `TipoFarmaco` y `Estado` son <c>null</c> cuando el fichero **no tiene** esa columna (la cabecera
/// mínima de pruebas `CN,Nombre`) y cadena vacía cuando la tiene pero la celda está vacía. La
/// distinción importa: en el fichero real, un tipo vacío es un efecto o accesorio, no un
/// medicamento.</summary>
public sealed record FilaNomenclator(
    string Cn,
    string Nombre,
    string? PrincipioActivo = null,
    string? Laboratorio = null,
    string? TipoFarmaco = null,
    string? Estado = null)
{
    /// <summary>El nomenclátor de facturación mezcla medicamentos con **efectos y accesorios** (bolsas
    /// de ostomía, apósitos) y con **códigos de facturación** («VISADOS SCP», «FORMULAS MAGISTRALES»).
    /// Solo lo que el propio fichero declara medicamento entra en el catálogo de medicamentos. Sin
    /// columna de tipo no hay forma de distinguirlo, y se toma todo.</summary>
    public bool EsMedicamento
        => TipoFarmaco is null || TipoFarmaco.Trim().StartsWith("Medicamento", StringComparison.OrdinalIgnoreCase);

    /// <summary>Cualquier baja («BAJA GENERAL», «BAJA POR NO COMERCIALIZAR»…). La suspensión temporal
    /// no es una baja: el medicamento sigue existiendo y un paciente puede estar tomándolo.</summary>
    public bool EstaDeBaja
        => Estado is not null && Estado.Trim().StartsWith("BAJA", StringComparison.OrdinalIgnoreCase);
}
