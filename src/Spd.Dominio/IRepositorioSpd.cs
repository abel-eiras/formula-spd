namespace Spd.Dominio;

public interface IRepositorioSpd
{
    int Crear(SPD spd);
    void Actualizar(SPD spd);
    SPD? ObtenerPorId(int id);
    int ObtenerSiguienteCorrelativo();
    IReadOnlyList<SPD> ListarPorSesion(Guid sesionId);
    IReadOnlyList<SPD> ListarUltimaSesionDePaciente(int pacienteId);
    IReadOnlyList<SPD> ListarPendientesDeEntrega(int pacienteId);
    IReadOnlyList<SPD> Listar(EstadoSpd? filtroEstado, int? filtroPacienteId, int? filtroElaboradorId);
}
