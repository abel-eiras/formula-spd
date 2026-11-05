namespace Spd.Dominio;

/// <summary>Centro (residencia, centro de día) al que se entregan los blísteres de varios pacientes
/// (Spec 019). Es un catálogo (Art. IV.1): el paciente lo referencia por clave. Nunca se borra; se da
/// de baja con motivo (Art. III.1).</summary>
public sealed class Centro
{
    public int Id { get; set; }
    public required string Nombre { get; set; }
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? PersonaContacto { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime? FechaBaja { get; set; }
    public string? MotivoBaja { get; set; }
}

public interface IRepositorioCentros
{
    int Crear(Centro centro, int? usuarioId);
    void Actualizar(Centro centro, int? usuarioId);
    Centro? ObtenerPorId(int id);
    IReadOnlyList<Centro> Listar(bool incluirBaja);

    /// <summary>Asigna (o quita, con null) el centro de un paciente.</summary>
    void AsignarPaciente(int pacienteId, int? centroId);
    int? CentroDePaciente(int pacienteId);
    IReadOnlyList<int> PacientesDe(int centroId);
}

/// <summary>Cuántos días cubre lo que hay en custodia de un medicamento (Spec 019 FR-1911): unidades
/// disponibles entre consumo diario, por defecto. Sin consumo (un «si precisa» o una pauta a cero) no
/// hay fecha de agotamiento que calcular.</summary>
public static class CoberturaEnvases
{
    public static int? DiasDeCobertura(int unidadesDisponibles, decimal consumoSemanal)
        => consumoSemanal <= 0 ? null : (int)Math.Floor(Math.Max(0, unidadesDisponibles) / (consumoSemanal / 7m));
}
