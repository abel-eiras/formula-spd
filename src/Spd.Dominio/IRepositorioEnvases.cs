namespace Spd.Dominio;

public interface IRepositorioEnvases
{
    int Crear(Envase envase);
    void Actualizar(Envase envase);
    Envase? ObtenerPorId(int id);
    Envase? ObtenerPorSerie(string serie);
    IReadOnlyList<Envase> ListarEnCustodiaDePacienteYMedicamento(int pacienteId, int medicamentoId);
    /// <summary>Custodia del mismo CN o grupo homogéneo explícito, sin mezclar pacientes ni cajas.</summary>
    IReadOnlyList<Envase> ListarEquivalentesEnCustodia(int pacienteId, int medicamentoId);
    IReadOnlyList<Envase> ListarEnCustodiaDePaciente(int pacienteId);
    IReadOnlyList<Envase> ListarHistoricoDePaciente(int pacienteId);
    IReadOnlyList<Envase> ListarEnCustodiaDeTodos();
}
