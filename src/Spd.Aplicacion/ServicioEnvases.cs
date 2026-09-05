using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Depósito de envases en custodia (FR-510..544).</summary>
public sealed class ServicioEnvases(
    IRepositorioEnvases repositorio, IRepositorioTratamientos repositorioTratamientos,
    IRepositorioPacientes repositorioPacientes, IRegistradorAuditoria auditoria)
    : IServicioEnvases
{
    public Envase RegistrarEnvase(DatosAltaEnvase datos, int? usuarioQueEjecutaId)
    {
        var tieneTratamientoActivo = repositorioTratamientos.ListarVigentesDePaciente(datos.PacienteId)
            .Any(t => t.MedicamentoId == datos.MedicamentoId && t.EnSpd);
        if (!tieneTratamientoActivo)
            throw new ErrorValidacionException(
                "El paciente no tiene un tratamiento activo en SPD para ese medicamento. Cree el tratamiento antes de registrar el envase (FR-515).");

        var existente = BuscarPorSerie(datos.Serie);
        if (existente is not null)
        {
            var pacienteExistente = repositorioPacientes.ObtenerPorId(existente.PacienteId);
            throw new ErrorValidacionException(
                $"La serie {datos.Serie} ya está en custodia de {pacienteExistente?.NumFicha ?? existente.PacienteId.ToString()} (FR-512, CA-504).");
        }

        var envase = new Envase
        {
            PacienteId = datos.PacienteId,
            MedicamentoId = datos.MedicamentoId,
            Serie = datos.Serie,
            Lote = datos.Lote,
            Caducidad = datos.Caducidad,
            UnidadesIniciales = datos.UnidadesIniciales,
            UnidadesRestantes = datos.UnidadesIniciales,
            FechaEntrada = DateTime.UtcNow,
            Origen = datos.Origen,
            Estado = EstadoEnvase.EnCustodia
        };
        envase.Id = repositorio.Crear(envase);

        auditoria.Registrar(usuarioQueEjecutaId, "ALTA_ENVASE", "Envase", envase.Id, $"serie={datos.Serie}");
        return envase;
    }

    public Envase RegistrarEntregaFueraBlister(DatosEntregaFueraBlister datos, int? usuarioQueEjecutaId)
    {
        var tieneTratamientoFueraDeBlister = repositorioTratamientos.ListarVigentesDePaciente(datos.PacienteId)
            .Any(t => t.MedicamentoId == datos.MedicamentoId && !t.EnSpd);
        if (!tieneTratamientoFueraDeBlister)
            throw new ErrorValidacionException(
                "Solo se puede registrar entrega fuera de blíster de un medicamento con tratamiento activo fuera de SPD (FR-543).");

        if (datos.Serie is not null && BuscarPorSerie(datos.Serie) is not null)
            throw new ErrorValidacionException($"La serie {datos.Serie} ya está en custodia de otro paciente (FR-512).");

        var envase = new Envase
        {
            PacienteId = datos.PacienteId,
            MedicamentoId = datos.MedicamentoId,
            Serie = datos.Serie,
            Lote = datos.Lote,
            FechaEntrada = DateTime.UtcNow,
            Origen = OrigenEnvase.Manual,
            Estado = EstadoEnvase.EntregadoPaciente,
            FechaSalida = DateTime.UtcNow,
            EntregadoA = datos.EntregadoA
        };
        envase.Id = repositorio.Crear(envase);

        auditoria.Registrar(usuarioQueEjecutaId, "ENTREGA_FUERA_BLISTER", "Envase", envase.Id, $"entregadoA={datos.EntregadoA}");
        return envase;
    }

    public IReadOnlyList<Envase> ListarEnCustodiaDePaciente(int pacienteId)
        => repositorio.ListarEnCustodiaDePaciente(pacienteId);

    public IReadOnlyList<Envase> ListarHistoricoDePaciente(int pacienteId)
        => repositorio.ListarHistoricoDePaciente(pacienteId);

    public Envase DarSalidaSigre(int envaseId, MotivoSalidaEnvase motivo, string? motivoDetalle, int? usuarioQueEjecutaId)
    {
        var envase = repositorio.ObtenerPorId(envaseId)
            ?? throw new ErrorValidacionException($"No existe el envase {envaseId}.");
        if (envase.Estado != EstadoEnvase.EnCustodia)
            throw new ErrorValidacionException("Solo se puede dar salida a SIGRE de un envase EN_CUSTODIA (FR-540).");

        envase.Estado = EstadoEnvase.ResiduoSigre;
        envase.FechaSalida = DateTime.UtcNow;
        envase.MotivoSalida = motivo;
        envase.MotivoSalidaDetalle = motivoDetalle;
        repositorio.Actualizar(envase);

        auditoria.Registrar(usuarioQueEjecutaId, "SALIDA_SIGRE", "Envase", envase.Id, $"motivo={motivo}");
        return envase;
    }

    public IReadOnlyList<Envase> ProponerSalidaSigrePorFinDeTratamiento(int pacienteId, int medicamentoId)
        => repositorio.ListarEnCustodiaDePacienteYMedicamento(pacienteId, medicamentoId);

    public IReadOnlyList<Envase> ProponerSalidaSigreMasivaPorBaja(int pacienteId)
        => repositorio.ListarEnCustodiaDePaciente(pacienteId);

    public IReadOnlyList<Envase> DarSalidaSigreMasiva(int pacienteId, MotivoSalidaEnvase motivo, int? usuarioQueEjecutaId)
        => repositorio.ListarEnCustodiaDePaciente(pacienteId)
            .Select(e => DarSalidaSigre(e.Id, motivo, null, usuarioQueEjecutaId))
            .ToList();

    private Envase? BuscarPorSerie(string? serie)
        => serie is null ? null : repositorio.ObtenerPorSerie(serie);
}
