using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Asistente obligatorio de primer arranque (FR-000/FR-001).</summary>
public interface IServicioAsistentePrimerArranque
{
    bool HayConfiguracionInicial();
    void EjecutarPasoFarmacia(Farmacia datosFarmacia);
    void EjecutarPasoPrimerUsuario(string nombre, string apellidos, string login, string password);
    void EjecutarPasoValoresDefecto(string diaRetiradaDefecto, int nBlisteresDefecto, int diasAntelacionListado);
    void EjecutarPasoRutas(string? rutaBackup, string? rutaDocumentosGenerados);
    void FinalizarAsistente();
}
