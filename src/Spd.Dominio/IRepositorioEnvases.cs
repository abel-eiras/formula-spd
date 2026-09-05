namespace Spd.Dominio;

public interface IRepositorioEnvases
{
    int Crear(Envase envase);
    void Actualizar(Envase envase);
    Envase? ObtenerPorId(int id);
    Envase? ObtenerPorSerie(string serie);
    IReadOnlyList<Envase> ListarEnCustodiaDePacienteYMedicamento(int pacienteId, int medicamentoId);
    IReadOnlyList<Envase> ListarEnCustodiaDePaciente(int pacienteId);
    IReadOnlyList<Envase> ListarHistoricoDePaciente(int pacienteId);
    IReadOnlyList<Envase> ListarEnCustodiaDeTodos();
}
