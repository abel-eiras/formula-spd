using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Datos de alta de una alerta de calidad (Spec 018 FR-1801). `Lotes` admite varios lotes
/// separados por comas, punto y coma o saltos de línea: una nota de retirada suele afectar a varios.</summary>
public sealed record DatosAltaAlertaCalidad(string Referencia, DateOnly FechaAlerta, string Cn, string Lotes, string? Descripcion);

/// <summary>Una alerta con el nombre del medicamento al que se refiere, si está en el catálogo.</summary>
public sealed record AlertaCalidadFila(AlertaCalidad Alerta, string? NombreMedicamento);

/// <summary>Alertas de calidad sobre lotes y trazabilidad inversa lote → envases → blísteres →
/// pacientes (Spec 018). Toda escritura deja traza en auditoría (Art. VII.6).</summary>
public interface IServicioAlertasCalidad
{
    /// <summary>Crea una alerta por cada lote indicado y devuelve las creadas.</summary>
    IReadOnlyList<AlertaCalidad> Registrar(DatosAltaAlertaCalidad datos, int? usuarioId);

    /// <summary>Cierra la alerta con motivo; nunca la borra (Art. III.1).</summary>
    void Cerrar(int alertaId, string motivo, DateOnly hoy, int? usuarioId);

    IReadOnlyList<AlertaCalidadFila> Listar(bool soloActivas);

    TrazabilidadAlerta Trazar(int alertaId);
}
