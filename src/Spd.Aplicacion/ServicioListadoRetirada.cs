using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Listado de retirada (FR-530..537). Se recalcula en cada llamada; no se almacena
/// (FR-537).</summary>
public sealed class ServicioListadoRetirada(
    IRepositorioPacientes repositorioPacientes, IRepositorioContactos repositorioContactos,
    IRepositorioTratamientos repositorioTratamientos, IRepositorioMedicamentos repositorioMedicamentos,
    IRepositorioEnvases repositorioEnvases, IRepositorioFarmacia repositorioFarmacia,
    IComprobadorCoberturaSpd comprobadorCoberturaSpd, IRegistradorAuditoria auditoria)
    : IServicioListadoRetirada
{
    public IReadOnlyList<FilaListadoRetirada> ObtenerListado(DateOnly fechaReferencia, FiltrosListadoRetirada filtros)
    {
        var farmacia = repositorioFarmacia.Obtener()
            ?? throw new ErrorValidacionException("No hay configuración de farmacia.");
        var finVentana = fechaReferencia.AddDays(farmacia.DiasAntelacionListado);

        // FR-530: "Buscar" con fragmento vacío es una búsqueda global (research.md); reutiliza el
        // mismo punto de acceso de Spec 001 en vez de añadir un método nuevo de "listar todos".
        var pacientesActivos = repositorioPacientes.Buscar(string.Empty, [EstadoPaciente.Activo], null);

        var filas = new List<FilaListadoRetirada>();
        foreach (var paciente in pacientesActivos)
        {
            if (filtros.PacienteId is { } filtroPaciente && paciente.Id != filtroPaciente) continue;
            if (filtros.DiaRetirada is { } filtroDia && paciente.DiaRetirada != filtroDia) continue;

            var proximaRetirada = CalculadoraProximaRetirada.Calcular(paciente.DiaRetirada, null, fechaReferencia);
            if (proximaRetirada < fechaReferencia || proximaRetirada > finVentana) continue;
            if (comprobadorCoberturaSpd.YaCubierta(paciente.Id, proximaRetirada)) continue;

            var (dniRetirada, avisoSinDni) = ResolverDniDeRetirada(paciente);
            var finValidezPrevista = proximaRetirada.AddDays(7 * paciente.NBlisteres - 1);

            foreach (var tratamiento in repositorioTratamientos.ListarVigentesDePaciente(paciente.Id).Where(t => t.EnSpd))
            {
                var medicamento = repositorioMedicamentos.ObtenerPorId(tratamiento.MedicamentoId);
                if (medicamento is null) continue;

                var necesarias = (int)Math.Ceiling(CalculadoraUnidadesADescontar.SumaSemanalReal(tratamiento)) * paciente.NBlisteres;
                var disponibles = repositorioEnvases.ListarEnCustodiaDePacienteYMedicamento(paciente.Id, medicamento.Id)
                    .Where(e => e.Caducidad is { } caducidad && caducidad >= finValidezPrevista)
                    .Sum(e => e.UnidadesRestantes ?? 0);
                var faltan = Math.Max(0, necesarias - disponibles);

                if (filtros.SoloConFaltantes && faltan == 0) continue;

                int? envasesARetirar = medicamento.UnidadesEnvase is { } unidadesEnvase && unidadesEnvase > 0
                    ? (int)Math.Ceiling(faltan / (decimal)unidadesEnvase)
                    : null;

                filas.Add(new FilaListadoRetirada(
                    paciente.Id, paciente.Apellidos, paciente.Nombre, paciente.Cip, dniRetirada, avisoSinDni,
                    medicamento.Id, medicamento.Nombre, medicamento.Cn,
                    necesarias, disponibles, faltan, envasesARetirar,
                    proximaRetirada, paciente.NBlisteres));
            }
        }

        return filas
            .OrderBy(f => f.ProximaRetirada)
            .ThenBy(f => f.Apellidos)
            .ThenBy(f => f.PacienteId)
            .ToList();
    }

    public void RegistrarImpresion(int? usuarioQueEjecutaId)
        => auditoria.Registrar(usuarioQueEjecutaId, "IMPRIMIR", "ListadoRetirada", null, null);

    private (string? DniRetirada, bool AvisoSinDni) ResolverDniDeRetirada(Paciente paciente)
    {
        var responsable = repositorioContactos.ListarDePaciente(paciente.Id, incluirBaja: false)
            .FirstOrDefault(c => c.RetiraMedicacion);
        var dni = responsable?.Dni ?? paciente.Dni;
        return (dni, string.IsNullOrWhiteSpace(dni));
    }
}
