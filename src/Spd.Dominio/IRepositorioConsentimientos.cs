namespace Spd.Dominio;

/// <summary>Acceso a Consentimiento. Alta y actualización de firma/revocación/impresión; nunca
/// borrado (Art. III).</summary>
public interface IRepositorioConsentimientos
{
    int Crear(Consentimiento consentimiento);
    void Actualizar(Consentimiento consentimiento);
    Consentimiento? ObtenerPorId(int id);
    IReadOnlyList<Consentimiento> ListarDePaciente(int pacienteId);
    /// <summary>El firmado y no revocado más reciente del paciente, o null (FR-213/215).</summary>
    Consentimiento? ObtenerVigente(int pacienteId);
}
