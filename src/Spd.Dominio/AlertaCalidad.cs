namespace Spd.Dominio;

/// <summary>Alerta de calidad sobre un lote concreto de un medicamento (Spec 018): retirada de lote
/// comunicada por la AEMPS, el laboratorio o el Colegio. Identifica el medicamento por su CN, que es
/// el dato que traen las notas de retirada y el que identifica la marca (un lote es de un fabricante).
/// No se borra nunca: se cierra con fecha y motivo (Art. III.1).</summary>
public sealed class AlertaCalidad
{
    public int Id { get; set; }
    public required string Referencia { get; set; }
    public required DateOnly FechaAlerta { get; set; }
    public required string Cn { get; set; }
    public required string Lote { get; set; }
    public string? Descripcion { get; set; }
    public bool Activa { get; set; } = true;
    public DateOnly? FechaCierre { get; set; }
    public string? MotivoCierre { get; set; }

    /// <summary>El lote con el que se compara: los lotes se teclean o se leen de formas distintas
    /// («ab 123», «AB-123», «AB123») y una alerta de calidad que no encuentra una caja por un guion es
    /// peor que una coincidencia de más, que la persona descarta al mirar la caja.</summary>
    public static string NormalizarLote(string? lote)
        => new((lote ?? string.Empty).Where(char.IsLetterOrDigit).Select(char.ToUpperInvariant).ToArray());

    /// <summary>Un CN se compara por sus seis dígitos: «712729.3», «7127293» y «712729» son el mismo.</summary>
    public static string NormalizarCn(string? cn)
    {
        var digitos = new string((cn ?? string.Empty).Where(char.IsAsciiDigit).ToArray());
        return digitos.Length == 7 ? digitos[..6] : digitos;
    }

    public bool Afecta(string? cn, string? lote)
        => NormalizarCn(cn) == NormalizarCn(Cn) && NormalizarLote(lote) is { Length: > 0 } l && l == NormalizarLote(Lote);
}

/// <summary>Un envase afectado por una alerta (Spec 018 FR-1803), con dónde está.</summary>
public sealed record EnvaseAfectado(int EnvaseId, int PacienteId, string Paciente, string? Serie, string? Lote,
    EstadoEnvase Estado, int? UnidadesRestantes);

/// <summary>Un blíster que contiene unidades de un envase afectado (Spec 018 FR-1804), con su estado:
/// pendiente (borrador, preparado o verificado) o ya entregado.</summary>
public sealed record SpdAfectado(int SpdId, string NumRegistro, int PacienteId, string Paciente, EstadoSpd Estado,
    DateOnly ValidezDesde, DateOnly ValidezHasta, string? Lote);

/// <summary>Todo lo que alcanza una alerta: trazabilidad inversa de lote a paciente (Spec 018).</summary>
public sealed record TrazabilidadAlerta(AlertaCalidad Alerta, IReadOnlyList<EnvaseAfectado> Envases, IReadOnlyList<SpdAfectado> Blisteres)
{
    public IReadOnlyList<EnvaseAfectado> EnCustodia => Envases.Where(e => e.Estado == EstadoEnvase.EnCustodia).ToList();
    public IReadOnlyList<SpdAfectado> BlisteresPendientes =>
        Blisteres.Where(b => b.Estado is EstadoSpd.Borrador or EstadoSpd.Preparado or EstadoSpd.Verificado).ToList();
    public IReadOnlyList<SpdAfectado> BlisteresEntregados => Blisteres.Where(b => b.Estado == EstadoSpd.Entregado).ToList();

    /// <summary>Hay algo que hacer: cajas que retirar del depósito o blísteres que rehacer o recuperar.
    /// Los anulados ya no están en circulación.</summary>
    public bool RequiereAccion => EnCustodia.Count > 0 || BlisteresPendientes.Count > 0 || BlisteresEntregados.Count > 0;
}

public interface IRepositorioAlertasCalidad
{
    int Crear(AlertaCalidad alerta, int? usuarioId);
    void Cerrar(int alertaId, DateOnly fecha, string motivo, int? usuarioId);
    AlertaCalidad? ObtenerPorId(int id);
    IReadOnlyList<AlertaCalidad> Listar(bool soloActivas);

    /// <summary>Envases (en cualquier estado) de un medicamento con ese CN cuyo lote coincide.</summary>
    IReadOnlyList<EnvaseAfectado> EnvasesAfectados(AlertaCalidad alerta);

    /// <summary>Blísteres con alguna línea que tomó unidades de un envase de ese CN y lote, según la
    /// instantánea de la línea (Art. IV.3: lo que se preparó, no el catálogo de hoy).</summary>
    IReadOnlyList<SpdAfectado> BlisteresAfectados(AlertaCalidad alerta);
}
