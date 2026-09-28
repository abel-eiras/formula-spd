namespace Spd.Aplicacion;

/// <summary>Qué se ha encontrado (Spec 015 FR-1520). Determina a dónde lleva el resultado, pero se
/// expresa en términos de negocio, no de pantallas: la traducción a secciones es de la interfaz.</summary>
public enum TipoResultadoBusqueda
{
    Paciente,
    Medicamento,
    Blister,
    /// <summary>Un paciente al que le falta retirar el medicamento de un envase escaneado. `Id` es el
    /// medicamento y `PacienteId` el paciente (spec-012 FR-1207).</summary>
    RetiradaPendiente
}

/// <summary>Un resultado de la búsqueda global. `Titulo` es lo que identifica la cosa (el nombre
/// del paciente, el del medicamento, el número de registro del blíster) y `Detalle` el dato que
/// permite distinguir dos parecidos. `PacienteId` va relleno cuando el resultado pertenece a un
/// paciente, para poder abrir su ficha o filtrar por él.</summary>
public sealed record ResultadoBusqueda(
    TipoResultadoBusqueda Tipo,
    string Titulo,
    string Detalle,
    int Id,
    int? PacienteId,
    string? NombrePaciente = null);

public interface IServicioBusquedaGlobal
{
    /// <summary>Busca en pacientes, medicamentos y blísteres a la vez. Devuelve lista vacía —nunca
    /// excepción— si no hay texto o no hay coincidencias.</summary>
    IReadOnlyList<ResultadoBusqueda> Buscar(string? texto, int limitePorTipo = 5);

    /// <summary>spec-012 FR-1207: para un envase escaneado en el mostrador, los pacientes a los que les
    /// falta retirar ese medicamento. Si no le falta a nadie, el propio medicamento; si no está en el
    /// catálogo, lista vacía.</summary>
    IReadOnlyList<ResultadoBusqueda> BuscarEnvaseEscaneado(Spd.Dominio.DatosEnvaseEscaneado datos, DateOnly hoy);
}
