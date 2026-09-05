namespace Spd.Aplicacion;

/// <summary>Listado de retirada (FR-530..537): calculado, no almacenado.</summary>
public interface IServicioListadoRetirada
{
    IReadOnlyList<FilaListadoRetirada> ObtenerListado(DateOnly fechaReferencia, FiltrosListadoRetirada filtros);

    void RegistrarImpresion(int? usuarioQueEjecutaId);
}
