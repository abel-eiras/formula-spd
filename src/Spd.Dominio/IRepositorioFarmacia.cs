namespace Spd.Dominio;

/// <summary>Acceso a la fila única de configuración de la farmacia (Art. IV.1).</summary>
public interface IRepositorioFarmacia
{
    Farmacia? Obtener();
    void Crear(Farmacia farmacia);
    void Actualizar(Farmacia farmacia);
}
