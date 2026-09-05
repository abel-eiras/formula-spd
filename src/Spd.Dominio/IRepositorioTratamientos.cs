namespace Spd.Dominio;

public interface IRepositorioTratamientos
{
    int Crear(Tratamiento tratamiento);
    void Actualizar(Tratamiento tratamiento);
    Tratamiento? ObtenerPorId(int id);
    IReadOnlyList<Tratamiento> ListarVigentesDePaciente(int pacienteId);
    IReadOnlyList<Tratamiento> ListarHistorialDeMedicamento(int pacienteId, int medicamentoId);
}
