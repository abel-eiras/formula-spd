using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Lista de reposición (Spec 019 FR-1910..1912): qué medicamentos en SPD se agotan dentro del
/// horizonte con lo que hay en custodia, para pedir a tiempo la dispensación o una receta nueva. El
/// listado de retirada (Spec 005) mira solo la próxima preparación; esta lista mira más lejos.
/// Mismo agrupamiento por grupo homogéneo y misma regla de caducidad que el listado de retirada, para
/// que los dos no se contradigan.</summary>
public sealed class ServicioReposicion(
    IRepositorioPacientes repositorioPacientes, IRepositorioTratamientos repositorioTratamientos,
    IRepositorioMedicamentos repositorioMedicamentos, IRepositorioEnvases repositorioEnvases,
    IRepositorioMedicos repositorioMedicos, IRepositorioCentros repositorioCentros) : IServicioReposicion
{
    public const int HorizonteMaximoDias = 120;

    public IReadOnlyList<FilaReposicion> Obtener(DateOnly hoy, int horizonteDias, int? centroId = null)
    {
        if (horizonteDias is < 1 or > HorizonteMaximoDias)
            throw new ErrorValidacionException($"El horizonte debe estar entre 1 y {HorizonteMaximoDias} días.");

        var nombresCentros = repositorioCentros.Listar(incluirBaja: true).ToDictionary(c => c.Id, c => c.Nombre);
        var filas = new List<FilaReposicion>();
        foreach (var paciente in repositorioPacientes.Buscar(string.Empty, [EstadoPaciente.Activo], null))
        {
            var centroPaciente = repositorioCentros.CentroDePaciente(paciente.Id);
            if (centroId is not null && centroPaciente != centroId) continue;
            var medico = paciente.MedicoId is { } mid ? repositorioMedicos.ObtenerPorId(mid) : null;

            var grupos = repositorioTratamientos.ListarVigentesDePaciente(paciente.Id).Where(t => t.EnSpd)
                .Select(t => (Tratamiento: t, Medicamento: repositorioMedicamentos.ObtenerPorId(t.MedicamentoId)))
                .Where(x => x.Medicamento is not null)
                .GroupBy(x => x.Medicamento!.GrupoHomogeneo is { } grupo ? "G:" + grupo : "M:" + x.Medicamento.Id);
            foreach (var grupo in grupos)
            {
                var medicamento = grupo.First().Medicamento!;
                var consumoSemanal = grupo.Sum(x => CalculadoraUnidadesADescontar.SumaSemanalReal(x.Tratamiento));
                // Una caja caducada no cubre nada: se cuenta solo lo que no caduca hoy.
                var disponibles = repositorioEnvases.ListarEquivalentesEnCustodia(paciente.Id, medicamento.Id)
                    .Where(e => e.Caducidad is { } caducidad && caducidad > hoy)
                    .Sum(e => e.UnidadesRestantes ?? 0);
                if (CoberturaEnvases.DiasDeCobertura(disponibles, consumoSemanal) is not { } dias || dias > horizonteDias) continue;

                filas.Add(new FilaReposicion(
                    paciente.Id, $"{paciente.Apellidos}, {paciente.Nombre}", paciente.NumFicha,
                    centroPaciente is { } c ? nombresCentros.GetValueOrDefault(c) : null,
                    medico is null ? null : $"{medico.Nombre} {medico.Apellidos}" + (medico.Centro is { Length: > 0 } cs ? $" ({cs})" : string.Empty),
                    medicamento.Id, medicamento.Nombre, medicamento.Cn, disponibles, consumoSemanal, dias, hoy.AddDays(dias)));
            }
        }
        return filas.OrderBy(f => f.FechaAgotamiento).ThenBy(f => f.Paciente).ToList();
    }
}
