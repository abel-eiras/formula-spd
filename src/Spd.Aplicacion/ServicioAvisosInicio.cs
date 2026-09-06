using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Avisos de inicio (Spec 006 FR-691; Spec 009 FR-950): faltantes en el listado de
/// retirada, blísteres verificados sin entregar con validez ya iniciada, sesiones a medias (un
/// blíster entregado y el otro no, más de tres días) y días sin lectura ambiental. Todo se
/// calcula al abrir; nada se almacena.</summary>
public sealed class ServicioAvisosInicio(
    IServicioListadoRetirada listadoRetirada,
    IRepositorioSpd repositorioSpd,
    IRepositorioPacientes repositorioPacientes,
    IRepositorioRegistrosAmbientales repositorioAmbiental)
    : IServicioAvisosInicio
{
    /// <summary>FR-950: umbral por defecto de 7 días sin lectura ambiental (la configuración
    /// específica se retiró con la pantalla ambiental de Spec 009).</summary>
    public const int DiasSinLecturaAmbiental = 7;
    public const int DiasSesionAMedias = 3;

    public IReadOnlyList<AvisoInicio> Obtener(DateOnly hoy)
    {
        var avisos = new List<AvisoInicio>();

        foreach (var grupo in listadoRetirada.ObtenerListado(hoy, new FiltrosListadoRetirada(SoloConFaltantes: true)).GroupBy(f => f.PacienteId))
        {
            var primera = grupo.First();
            avisos.Add(new AvisoInicio("Faltantes",
                $"{primera.Nombre} {primera.Apellidos}: faltan envases de {string.Join(", ", grupo.Select(f => f.MedicamentoNombre))} para la retirada del {primera.ProximaRetirada:dd/MM}.",
                grupo.Key));
        }

        var todos = repositorioSpd.Listar(null, null, null);

        foreach (var spd in todos.Where(s => s.Estado == EstadoSpd.Verificado && s.ValidezDesde <= hoy))
            avisos.Add(new AvisoInicio("Sin entregar",
                $"{NombrePaciente(spd.PacienteId)}: blíster {spd.NumRegistro} verificado y sin entregar; su validez empezó el {spd.ValidezDesde:dd/MM}.",
                spd.PacienteId));

        foreach (var sesion in todos.GroupBy(s => s.SesionId))
        {
            var entregados = sesion.Where(s => s.Estado == EstadoSpd.Entregado).ToList();
            var pendientes = sesion.Where(s => s.Estado is EstadoSpd.Borrador or EstadoSpd.Preparado or EstadoSpd.Verificado).ToList();
            if (entregados.Count == 0 || pendientes.Count == 0) continue;
            var ultimaEntrega = entregados.Max(s => s.FechaEntrega ?? DateTime.MinValue);
            if ((hoy.ToDateTime(TimeOnly.MinValue) - ultimaEntrega).TotalDays <= DiasSesionAMedias) continue;
            avisos.Add(new AvisoInicio("Sesión a medias",
                $"{NombrePaciente(sesion.First().PacienteId)}: {entregados.Count} blíster(es) entregado(s) el {ultimaEntrega:dd/MM} y {pendientes.Count} todavía en {pendientes[0].Estado}.",
                sesion.First().PacienteId));
        }

        var ultima = repositorioAmbiental.ObtenerUltimo();
        var diasSinLectura = ultima is null ? int.MaxValue : (int)(DateTime.UtcNow - ultima.Fecha).TotalDays;
        if (diasSinLectura >= DiasSinLecturaAmbiental)
            avisos.Add(new AvisoInicio("Ambiental",
                ultima is null ? "No hay ninguna lectura de temperatura y humedad registrada." : $"Hace {diasSinLectura} días de la última lectura de temperatura y humedad.",
                null));

        return avisos;
    }

    private string NombrePaciente(int pacienteId)
    {
        var p = repositorioPacientes.ObtenerPorId(pacienteId);
        return p is null ? $"paciente {pacienteId}" : $"{p.Nombre} {p.Apellidos}";
    }
}
