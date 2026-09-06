namespace Spd.Aplicacion;

/// <summary>Qué se ha encontrado (Spec 015 FR-1520). Determina a dónde lleva el resultado, pero se
/// expresa en términos de negocio, no de pantallas: la traducción a secciones es de la interfaz.</summary>
public enum TipoResultadoBusqueda
{
    Paciente,
    Medicamento,
    Blister
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
}
