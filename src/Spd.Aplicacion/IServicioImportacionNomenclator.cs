using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Revisión y aplicación fila a fila del nomenclátor sobre el catálogo (FR-320..FR-322).
/// Nunca sobrescribe descripción física ni aptitud SPD (FR-321).</summary>
public interface IServicioImportacionNomenclator
{
    ResultadoComparacionNomenclator CompararConNomenclator(string rutaFicheroDescargado);
    Medicamento AplicarAltaDesdeNomenclator(string cn, string nombre, int? usuarioQueEjecutaId);
    void AplicarNombreDesdeNomenclator(int medicamentoId, string nombreNuevo, int? usuarioQueEjecutaId);
}
