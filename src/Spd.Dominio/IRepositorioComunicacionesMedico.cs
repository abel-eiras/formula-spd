namespace Spd.Dominio;

public interface IRepositorioComunicacionesMedico
{
    int Crear(ComunicacionMedico comunicacion);
    void ActualizarRespuesta(ComunicacionMedico comunicacion);
    ComunicacionMedico? ObtenerPorId(int id);
    IReadOnlyList<ComunicacionMedico> ListarDePaciente(int pacienteId);
}
