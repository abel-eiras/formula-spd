namespace Spd.Dominio;

/// <summary>Acceso al control documental del PNT, exclusivo de Administrador (FR-942). Solo
/// alta (Art. III.1).</summary>
public interface IRepositorioControlDocumental
{
    int Crear(ControlCambiosPNT cambio);
    IReadOnlyList<ControlCambiosPNT> ListarCambiosPnt();

    int Crear(ControlCopias copia);
    IReadOnlyList<ControlCopias> ListarCopias();
}
