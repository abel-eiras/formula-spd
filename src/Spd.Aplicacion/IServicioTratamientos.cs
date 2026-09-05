using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Tratamiento del paciente (FR-400..430). Cerrar/abrir fila es la única forma de
/// cambiar algo clínicamente relevante (Art. IV.3/IV.4); los campos no clínicos se editan en el
/// sitio (FR-411).</summary>
public interface IServicioTratamientos
{
    Tratamiento Crear(int pacienteId, DatosAltaTratamiento datos, int? usuarioQueEjecutaId);

    Tratamiento CambiarPauta(int tratamientoId, DatosAltaTratamiento datosNuevos, int? usuarioQueEjecutaId);

    void ActualizarCamposNoClinicos(int tratamientoId, DatosNoClinicos datos, int? usuarioQueEjecutaId);

    void CambiarEstado(int tratamientoId, EstadoTratamiento nuevoEstado, int? usuarioQueEjecutaId);

    IReadOnlyList<Tratamiento> ListarVigentesDePaciente(int pacienteId);

    IReadOnlyList<Tratamiento> ListarHistorialDeMedicamento(int pacienteId, int medicamentoId);
}
