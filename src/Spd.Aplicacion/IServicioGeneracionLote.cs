namespace Spd.Aplicacion;

/// <summary>Qué documentos generar en un lote (Spec 007 FR-723): solo los aplicables a una
/// preparación; el resto se generan desde su propia pantalla.</summary>
public sealed record TiposDocumentoLote(bool Ficha, bool Etiquetas, bool Instrucciones)
{
    public bool Alguno => Ficha || Etiquetas || Instrucciones;
}

public sealed record LoteGenerado(int PacienteId, string Paciente, IReadOnlyList<string> Ficheros, IReadOnlyList<string> Avisos);
public sealed record LoteExcluido(int PacienteId, string Paciente, string Motivo);

/// <summary>Resumen de FR-724: generados, excluidos (no estaban listos, FR-722) y fallidos (error
/// durante la preparación o la generación). Un fallo en un paciente no detiene al resto.</summary>
public sealed record ResultadoLote(IReadOnlyList<LoteGenerado> Generados, IReadOnlyList<LoteExcluido> Excluidos, IReadOnlyList<LoteExcluido> Fallidos);

/// <summary>Generación en lote (Spec 007 FR-720..725): para cada paciente seleccionado con
/// "envases al día", localiza o crea su sesión, la lleva a PREPARADO y genera los documentos.</summary>
public interface IServicioGeneracionLote
{
    ResultadoLote Generar(IReadOnlyList<int> pacienteIds, TiposDocumentoLote tipos, int usuarioId);
}
