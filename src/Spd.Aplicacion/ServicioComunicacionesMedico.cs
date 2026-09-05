using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Comunicaciones al médico (FR-800..807).</summary>
public sealed class ServicioComunicacionesMedico(
    IRepositorioComunicacionesMedico repositorio, IRepositorioPacientes repositorioPacientes,
    IRepositorioTratamientos repositorioTratamientos, IRegistradorAuditoria auditoria)
    : IServicioComunicacionesMedico
{
    public ComunicacionMedico Crear(DatosAltaComunicacionMedico datos, int? usuarioQueEjecutaId)
    {
        if (datos.Tipo == TipoComunicacionMedico.Incidencia
            && (string.IsNullOrWhiteSpace(datos.IncidenciasDetectadas) || string.IsNullOrWhiteSpace(datos.Propuesta)))
            throw new ErrorValidacionException("Una incidencia exige incidencias detectadas y propuesta (FR-802).");

        var medicoId = datos.MedicoId
            ?? repositorioPacientes.ObtenerPorId(datos.PacienteId)?.MedicoId
            ?? throw new ErrorValidacionException("El paciente no tiene médico de cabecera; indique uno (FR-801).");

        var comunicacion = new ComunicacionMedico
        {
            PacienteId = datos.PacienteId,
            MedicoId = medicoId,
            Tipo = datos.Tipo,
            Fecha = DateOnly.FromDateTime(DateTime.Today),
            IncidenciasDetectadas = datos.IncidenciasDetectadas,
            Propuesta = datos.Propuesta,
            FarmaceuticoId = usuarioQueEjecutaId
        };
        comunicacion.Id = repositorio.Crear(comunicacion);

        auditoria.Registrar(usuarioQueEjecutaId, "ALTA_COMUNICACION_MEDICO", "ComunicacionMedico", comunicacion.Id, $"tipo={datos.Tipo}");
        return comunicacion;
    }

    // FR-804: prerrellena paciente y médico prescriptor del tratamiento. No guarda nada (FR-807).
    public DatosAltaComunicacionMedico PrepararDesdeTratamiento(int tratamientoId)
    {
        var tratamiento = repositorioTratamientos.ObtenerPorId(tratamientoId)
            ?? throw new ErrorValidacionException($"No existe el tratamiento {tratamientoId}.");
        var medicoId = tratamiento.MedicoId
            ?? repositorioPacientes.ObtenerPorId(tratamiento.PacienteId)?.MedicoId
            ?? throw new ErrorValidacionException("Ni el tratamiento ni el paciente tienen médico asignado (FR-801).");

        return new DatosAltaComunicacionMedico(tratamiento.PacienteId, medicoId, TipoComunicacionMedico.Incidencia, null, null);
    }

    // FR-805, research.md Decisión 2: punto de extensión hacia Spec 006, sin el aviso real todavía.
    public DatosAltaComunicacionMedico PrepararDesdeAvisoCambioReferido(int pacienteId, int medicoId)
        => new(pacienteId, medicoId, TipoComunicacionMedico.Incidencia, null, null);

    public void RegistrarRespuesta(int comunicacionId, string respuesta, DateOnly fechaRespuesta, int? usuarioQueEjecutaId)
    {
        var comunicacion = repositorio.ObtenerPorId(comunicacionId)
            ?? throw new ErrorValidacionException($"No existe la comunicación {comunicacionId}.");

        comunicacion.Respuesta = respuesta;
        comunicacion.FechaRespuesta = fechaRespuesta;
        repositorio.ActualizarRespuesta(comunicacion);

        auditoria.Registrar(usuarioQueEjecutaId, "RESPUESTA_COMUNICACION_MEDICO", "ComunicacionMedico", comunicacionId, null);
    }

    public IReadOnlyList<ComunicacionMedico> ListarDePaciente(int pacienteId) => repositorio.ListarDePaciente(pacienteId);
}
