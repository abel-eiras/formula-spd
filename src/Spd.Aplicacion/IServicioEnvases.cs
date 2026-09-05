using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Depósito de envases en custodia (FR-510..544). Toda escritura registra en auditoría
/// (Art. VII.6). Ningún envase se elimina ni se reasigna a otro paciente (Art. III, FR-544).</summary>
public interface IServicioEnvases
{
    Envase RegistrarEnvase(DatosAltaEnvase datos, int? usuarioQueEjecutaId);

    Envase RegistrarEntregaFueraBlister(DatosEntregaFueraBlister datos, int? usuarioQueEjecutaId);

    IReadOnlyList<Envase> ListarEnCustodiaDePaciente(int pacienteId);

    IReadOnlyList<Envase> ListarHistoricoDePaciente(int pacienteId);

    Envase DarSalidaSigre(int envaseId, MotivoSalidaEnvase motivo, string? motivoDetalle, int? usuarioQueEjecutaId);

    IReadOnlyList<Envase> ProponerSalidaSigrePorFinDeTratamiento(int pacienteId, int medicamentoId);

    IReadOnlyList<Envase> ProponerSalidaSigreMasivaPorBaja(int pacienteId);

    IReadOnlyList<Envase> DarSalidaSigreMasiva(int pacienteId, MotivoSalidaEnvase motivo, int? usuarioQueEjecutaId);
}
