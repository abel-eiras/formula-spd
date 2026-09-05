using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Revisión y aplicación fila a fila del nomenclátor sobre el catálogo (FR-320..FR-322).
/// Toda escritura registra en auditoría (Art. VII.6).</summary>
public sealed class ServicioImportacionNomenclator(
    ILectorNomenclator lector, IRepositorioMedicamentos repositorio, IRegistradorAuditoria auditoria)
    : IServicioImportacionNomenclator
{
    public ResultadoComparacionNomenclator CompararConNomenclator(string rutaFicheroDescargado)
    {
        var lectura = lector.Leer(rutaFicheroDescargado);
        if (!lectura.Exito)
        {
            return ResultadoComparacionNomenclator.Fallido(lectura.Error!);
        }

        var nuevos = new List<FilaNomenclator>();
        var conNombreDistinto = new List<ComparacionFila>();
        var sinCambios = 0;

        foreach (var fila in lectura.Filas)
        {
            var existente = repositorio.ObtenerPorCn(fila.Cn);
            if (existente is null)
            {
                nuevos.Add(fila);
            }
            else if (existente.Nombre != fila.Nombre)
            {
                conNombreDistinto.Add(new ComparacionFila(existente, fila.Nombre));
            }
            else
            {
                sinCambios++;
            }
        }

        return new ResultadoComparacionNomenclator(true, null, nuevos, conNombreDistinto, sinCambios);
    }

    public Medicamento AplicarAltaDesdeNomenclator(string cn, string nombre, int? usuarioQueEjecutaId)
    {
        var medicamento = new Medicamento
        {
            Cn = cn,
            Nombre = nombre,
            NombreNormalizado = Normalizador.QuitarTildesYMayusculas(nombre)
        };
        medicamento.Id = repositorio.Crear(medicamento);

        auditoria.Registrar(usuarioQueEjecutaId, "ALTA_DESDE_NOMENCLATOR", "Medicamento", medicamento.Id, null);
        return medicamento;
    }

    public void AplicarNombreDesdeNomenclator(int medicamentoId, string nombreNuevo, int? usuarioQueEjecutaId)
    {
        var medicamento = repositorio.ObtenerPorId(medicamentoId)
            ?? throw new ErrorValidacionException($"No existe el medicamento {medicamentoId}.");

        // FR-321: solo el nombre. Nunca desc_* ni apto_spd, aunque el llamador los tuviera cargados.
        medicamento.Nombre = nombreNuevo;
        medicamento.NombreNormalizado = Normalizador.QuitarTildesYMayusculas(nombreNuevo);
        repositorio.Actualizar(medicamento);

        auditoria.Registrar(usuarioQueEjecutaId, "NOMBRE_DESDE_NOMENCLATOR", "Medicamento", medicamentoId, null);
    }
}
