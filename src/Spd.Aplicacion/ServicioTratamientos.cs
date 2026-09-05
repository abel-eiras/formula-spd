using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Tratamiento del paciente (FR-400..430). Toda escritura registra en auditoría
/// (Art. VII.6).</summary>
public sealed class ServicioTratamientos(
    IRepositorioTratamientos repositorio, IRepositorioPacientes repositorioPacientes, IRegistradorAuditoria auditoria)
    : IServicioTratamientos
{
    public Tratamiento Crear(int pacienteId, DatosAltaTratamiento datos, int? usuarioQueEjecutaId)
    {
        // CA-400: si no se indica prescriptor, se prerrellena con el médico de cabecera.
        var medicoId = datos.MedicoId ?? repositorioPacientes.ObtenerPorId(pacienteId)?.MedicoId;

        var tratamiento = new Tratamiento
        {
            PacienteId = pacienteId,
            MedicamentoId = datos.MedicamentoId,
            EnSpd = datos.EnSpd,
            ProblemaSalud = datos.ProblemaSalud,
            MedicoId = medicoId,
            PautaD = datos.PautaD,
            PautaA = datos.PautaA,
            PautaC = datos.PautaC,
            PautaN = datos.PautaN,
            PautaTexto = datos.PautaTexto,
            DiasSemana = datos.DiasSemana,
            Via = datos.Via,
            Momento = datos.Momento,
            FechaInicio = datos.FechaInicio,
            FechaPrescripcionInicial = datos.FechaInicio,
            Tipo = datos.Tipo,
            Estado = EstadoTratamiento.Activo
        };
        tratamiento.Id = repositorio.Crear(tratamiento);

        auditoria.Registrar(usuarioQueEjecutaId, "ALTA_TRATAMIENTO", "Tratamiento", tratamiento.Id, null);
        return tratamiento;
    }

    // FR-410: cierra la fila vigente y abre una nueva, heredando la fecha de primera prescripción.
    public Tratamiento CambiarPauta(int tratamientoId, DatosAltaTratamiento datosNuevos, int? usuarioQueEjecutaId)
    {
        var anterior = repositorio.ObtenerPorId(tratamientoId)
            ?? throw new ErrorValidacionException($"No existe el tratamiento {tratamientoId}.");

        anterior.Estado = EstadoTratamiento.Finalizado;
        anterior.FechaFin = datosNuevos.FechaInicio;
        repositorio.Actualizar(anterior);

        var nuevo = new Tratamiento
        {
            PacienteId = anterior.PacienteId,
            MedicamentoId = datosNuevos.MedicamentoId,
            EnSpd = datosNuevos.EnSpd,
            ProblemaSalud = datosNuevos.ProblemaSalud,
            MedicoId = datosNuevos.MedicoId ?? anterior.MedicoId,
            PautaD = datosNuevos.PautaD,
            PautaA = datosNuevos.PautaA,
            PautaC = datosNuevos.PautaC,
            PautaN = datosNuevos.PautaN,
            PautaTexto = datosNuevos.PautaTexto,
            DiasSemana = datosNuevos.DiasSemana,
            Via = datosNuevos.Via,
            Momento = datosNuevos.Momento,
            FechaInicio = datosNuevos.FechaInicio,
            FechaPrescripcionInicial = anterior.FechaPrescripcionInicial,
            Tipo = datosNuevos.Tipo,
            Estado = EstadoTratamiento.Activo
        };
        nuevo.Id = repositorio.Crear(nuevo);

        auditoria.Registrar(usuarioQueEjecutaId, "CAMBIAR_PAUTA_TRATAMIENTO", "Tratamiento", nuevo.Id, $"anterior={tratamientoId}");
        return nuevo;
    }

    public void ActualizarCamposNoClinicos(int tratamientoId, DatosNoClinicos datos, int? usuarioQueEjecutaId)
    {
        var tratamiento = repositorio.ObtenerPorId(tratamientoId)
            ?? throw new ErrorValidacionException($"No existe el tratamiento {tratamientoId}.");

        tratamiento.ConocimientoCumplimiento = datos.ConocimientoCumplimiento;
        tratamiento.Incidencias = datos.Incidencias;
        tratamiento.Intervencion = datos.Intervencion;
        tratamiento.AjusteUnidadesManual = datos.AjusteUnidadesManual;
        repositorio.Actualizar(tratamiento);

        auditoria.Registrar(usuarioQueEjecutaId, "ACTUALIZAR_TRATAMIENTO", "Tratamiento", tratamientoId, null);
    }

    // Transición administrativa (p. ej. Suspendido↔Activo): nunca cierra/abre fila, no hay
    // cambio clínico (caso límite documentado en spec.md §7).
    public void CambiarEstado(int tratamientoId, EstadoTratamiento nuevoEstado, int? usuarioQueEjecutaId)
    {
        var tratamiento = repositorio.ObtenerPorId(tratamientoId)
            ?? throw new ErrorValidacionException($"No existe el tratamiento {tratamientoId}.");

        tratamiento.Estado = nuevoEstado;
        repositorio.Actualizar(tratamiento);

        auditoria.Registrar(usuarioQueEjecutaId, "CAMBIAR_ESTADO_TRATAMIENTO", "Tratamiento", tratamientoId, nuevoEstado.ToString());
    }

    public IReadOnlyList<Tratamiento> ListarVigentesDePaciente(int pacienteId)
        => repositorio.ListarVigentesDePaciente(pacienteId);

    public IReadOnlyList<Tratamiento> ListarHistorialDeMedicamento(int pacienteId, int medicamentoId)
        => repositorio.ListarHistorialDeMedicamento(pacienteId, medicamentoId);
}
