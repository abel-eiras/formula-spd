namespace Spd.Dominio;

/// <summary>Acceso a la ficha de paciente. La baja es lógica, nunca DELETE (Art. III.1).</summary>
public interface IRepositorioPacientes
{
    Paciente? ObtenerPorId(int id);

    /// <summary>Búsqueda global sobre <c>busqueda_normalizada</c> (FR-010/FR-011). El fragmento ya
    /// debe venir normalizado; el orden por estado (activos→evaluación→suspendidos→bajas) lo aplica
    /// el repositorio.</summary>
    IReadOnlyList<Paciente> Buscar(string fragmentoNormalizado, EstadoPaciente[]? filtroEstados, int? filtroMedicoId);

    IReadOnlyList<Paciente> ListarPorDni(string dni);
    IReadOnlyList<Paciente> ListarPorCip(string cip);

    /// <summary>Único punto de verdad para el "siguiente número de ficha" (research.md Decisión 3).</summary>
    int ObtenerSiguienteCorrelativo();

    int Crear(Paciente paciente);
    void Actualizar(Paciente paciente);
}
