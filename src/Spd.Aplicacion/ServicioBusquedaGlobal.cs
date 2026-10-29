using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Búsqueda global de la cabecera (Spec 015 FR-1520..1522). Un solo campo que responde a
/// "¿dónde está esto?" sin obligar a acertar primero la pantalla.
///
/// Reutiliza las búsquedas que ya existen para pacientes y medicamentos (Spec 001 FR-010, Spec 003
/// FR-305) en vez de escribir otra consulta: el Art. V prohíbe duplicar lo que ya está en el
/// catálogo, y además así la búsqueda global encuentra exactamente lo mismo que la pantalla
/// correspondiente, que es lo que el usuario espera.
///
/// Los blísteres no tienen búsqueda propia, así que se filtran aquí por número de registro; el
/// listado completo de SPD es el que ya carga la pantalla de preparaciones.</summary>
public sealed class ServicioBusquedaGlobal(
    IServicioPacientes servicioPacientes,
    IServicioMedicamentos servicioMedicamentos,
    IRepositorioSpd repositorioSpd,
    IRepositorioPacientes repositorioPacientes,
    IServicioListadoRetirada listadoRetirada)
    : IServicioBusquedaGlobal
{
    public IReadOnlyList<ResultadoBusqueda> BuscarEnvaseEscaneado(DatosEnvaseEscaneado datos, DateOnly hoy)
    {
        // Mismo orden que el selector de medicamento: el CN del propio código (AI 712) manda sobre el GTIN.
        var medicamento = (datos.CodigoNacional is { } cn ? servicioMedicamentos.ObtenerPorCn(cn) : null)
                          ?? servicioMedicamentos.ObtenerPorGtin(datos.Gtin);
        if (medicamento is null) return [];

        var pendientes = listadoRetirada.ObtenerListado(hoy, new FiltrosListadoRetirada(SoloConFaltantes: true))
            .Where(f => f.MedicamentoId == medicamento.Id)
            .Select(f => new ResultadoBusqueda(
                TipoResultadoBusqueda.RetiradaPendiente,
                $"{f.Nombre} {f.Apellidos}",
                $"{medicamento.Nombre} · faltan {f.Faltan} ud. · retirada {f.ProximaRetirada:dd/MM}",
                medicamento.Id, f.PacienteId, $"{f.Nombre} {f.Apellidos}"))
            .ToList();
        if (pendientes.Count > 0) return pendientes;

        return [new ResultadoBusqueda(
            TipoResultadoBusqueda.Medicamento, medicamento.Nombre,
            $"CN {medicamento.Cn} · ningún paciente tiene pendiente retirarlo", medicamento.Id, null)];
    }

    /// <summary>Por debajo de dos caracteres cualquier texto encuentra media base de datos: no es
    /// una búsqueda, es ruido.</summary>
    public const int MinimoCaracteres = 2;

    public IReadOnlyList<ResultadoBusqueda> Buscar(string? texto, int limitePorTipo = 5)
    {
        var fragmento = texto?.Trim() ?? string.Empty;
        if (fragmento.Length < MinimoCaracteres) return [];

        var normalizado = Normalizador.QuitarTildesYMayusculas(fragmento);
        var resultados = new List<ResultadoBusqueda>();

        foreach (var p in servicioPacientes.Buscar(fragmento, null, null).Take(limitePorTipo))
        {
            resultados.Add(new ResultadoBusqueda(
                TipoResultadoBusqueda.Paciente,
                $"{p.Nombre} {p.Apellidos}",
                $"Ficha {p.NumFicha}" + (p.Dni is { Length: > 0 } dni ? $" · {dni}" : string.Empty),
                p.Id, p.Id, $"{p.Nombre} {p.Apellidos}"));
        }

        foreach (var m in servicioMedicamentos.Buscar(fragmento, limitePorTipo))
        {
            resultados.Add(new ResultadoBusqueda(
                TipoResultadoBusqueda.Medicamento,
                m.Nombre,
                $"CN {m.Cn}" + (m.PrincipioActivo is { Length: > 0 } pa ? $" · {pa}" : string.Empty),
                m.Id, null));
        }

        foreach (var spd in repositorioSpd.Listar(null, null, null)
                     .Where(s => Normalizador.QuitarTildesYMayusculas(s.NumRegistro).Contains(normalizado))
                     .OrderByDescending(s => s.Id)
                     .Take(limitePorTipo))
        {
            var paciente = repositorioPacientes.ObtenerPorId(spd.PacienteId);
            var nombre = paciente is null ? null : $"{paciente.Nombre} {paciente.Apellidos}";
            resultados.Add(new ResultadoBusqueda(
                TipoResultadoBusqueda.Blister,
                spd.NumRegistro,
                $"{spd.Estado} · validez {spd.ValidezDesde:dd/MM} a {spd.ValidezHasta:dd/MM}"
                + (nombre is null ? string.Empty : $" · {nombre}"),
                spd.Id, spd.PacienteId, nombre));
        }

        return resultados;
    }
}
