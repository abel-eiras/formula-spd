using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Purga manual (Spec 010 §4.3). Cada comprobación es una barrera independiente: rol y
/// contraseña del administrador, estado BAJA con la antigüedad configurada (constitución
/// Art. III.2, nunca menos de cinco años), y ningún envase en custodia (FR-1024). La traza de
/// auditoría se escribe antes del borrado y sobrevive a él (FR-1022, CA-1006).</summary>
public sealed class ServicioPurga(
    IRepositorioPacientes pacientes,
    IRepositorioEnvases envases,
    IRepositorioUsuarios usuarios,
    IHasheadorPassword hasheador,
    IRepositorioFarmacia farmacia,
    IRepositorioPurga purga,
    IRegistradorAuditoria auditoria)
    : IServicioPurga
{
    public IReadOnlyList<Paciente> ListarPurgables(DateOnly hoy)
    {
        var limite = FechaLimite(hoy);
        return pacientes.Buscar(string.Empty, [EstadoPaciente.Baja], null)
            .Where(p => p.FechaBaja is DateTime fb && DateOnly.FromDateTime(fb) < limite)
            .OrderBy(p => p.FechaBaja)
            .ToList();
    }

    public ResultadoPurga Purgar(int pacienteId, int administradorId, string passwordAdministrador, DateOnly hoy)
    {
        var administrador = usuarios.ObtenerPorId(administradorId)
            ?? throw new ErrorValidacionException("No existe el administrador que ejecuta la purga.");
        if (administrador.Rol != Rol.Administrador || !administrador.Activo)
            throw new ErrorValidacionException("Solo un Administrador activo puede purgar pacientes (FR-1020).");
        if (string.IsNullOrEmpty(passwordAdministrador) || !hasheador.Verificar(passwordAdministrador, administrador.HashPassword))
            throw new ErrorValidacionException("Contraseña del administrador incorrecta: la purga exige confirmarla (FR-1021, CA-1005).");

        var paciente = pacientes.ObtenerPorId(pacienteId) ?? throw new ErrorValidacionException($"No existe el paciente {pacienteId}.");
        if (paciente.Estado != EstadoPaciente.Baja || paciente.FechaBaja is not DateTime fechaBaja)
            throw new ErrorValidacionException("Solo se purgan pacientes en BAJA (FR-1020).");
        if (DateOnly.FromDateTime(fechaBaja) >= FechaLimite(hoy))
            throw new ErrorValidacionException($"La baja no tiene la antigüedad exigida ({Anios()} años) (FR-1020, Art. III.2).");

        var enCustodia = envases.ListarEnCustodiaDePaciente(pacienteId);
        if (enCustodia.Count > 0)
            throw new ErrorValidacionException(
                $"El paciente conserva {enCustodia.Count} envase(s) en custodia; dé salida a esos envases (SIGRE) antes de purgar (FR-1024, CA-1007).");

        var nombre = $"{paciente.Nombre} {paciente.Apellidos}";
        // FR-1022: el hecho de que existió y fue purgado queda registrado aunque el contenido desaparezca.
        auditoria.Registrar(administradorId, "PURGAR_PACIENTE", "Paciente", pacienteId,
            $"num_ficha={paciente.NumFicha};nombre={nombre};fecha_baja={fechaBaja:yyyy-MM-dd};purgado_en={hoy:yyyy-MM-dd}");

        var filas = purga.PurgarPaciente(pacienteId);
        return new ResultadoPurga(paciente.NumFicha, nombre, filas);
    }

    private int Anios() => Math.Max(5, farmacia.Obtener()?.AniosRetencionPurga ?? 5);

    private DateOnly FechaLimite(DateOnly hoy) => hoy.AddYears(-Anios());
}
