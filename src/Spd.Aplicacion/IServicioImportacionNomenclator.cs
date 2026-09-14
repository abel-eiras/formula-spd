using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Importación del nomenclátor sobre el catálogo (FR-320..FR-322, revisados el 2026-09-14): alta
/// completa de los medicamentos nuevos al descargarlo, y revisión aparte de los nombres que han cambiado.
/// Nunca sobrescribe nada de un medicamento que ya exista (FR-321).</summary>
public interface IServicioImportacionNomenclator
{
    /// <summary>Da de alta **todos** los medicamentos del fichero que no estén ya en el catálogo: los de
    /// alta o suspensión temporal como activos y los de baja como inactivos (se reactivan solos si un
    /// paciente los necesita, CA-305). Descarta efectos y accesorios. Los que ya existen no se tocan.
    /// Todos entran con la aptitud SPD sin confirmar.</summary>
    ResultadoImportacionNomenclator ImportarCompleto(string rutaFicheroDescargado, int? usuarioQueEjecutaId);

    ResultadoComparacionNomenclator CompararConNomenclator(string rutaFicheroDescargado);
    Medicamento AplicarAltaDesdeNomenclator(FilaNomenclator fila, int? usuarioQueEjecutaId);
    void AplicarNombreDesdeNomenclator(int medicamentoId, string nombreNuevo, int? usuarioQueEjecutaId);
}
