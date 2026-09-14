using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Importación del nomenclátor sobre el catálogo (FR-320..FR-322, revisados el 2026-09-14).
/// Toda escritura registra en auditoría (Art. VII.6).</summary>
public sealed class ServicioImportacionNomenclator(
    ILectorNomenclator lector, IRepositorioMedicamentos repositorio, IRegistradorAuditoria auditoria)
    : IServicioImportacionNomenclator
{
    public ResultadoImportacionNomenclator ImportarCompleto(string rutaFicheroDescargado, int? usuarioQueEjecutaId)
    {
        var lectura = lector.Leer(rutaFicheroDescargado);
        if (!lectura.Exito) return ResultadoImportacionNomenclator.Fallido(lectura.Error!);

        // Los CN existentes se cargan una vez: 15.000 consultas sueltas serían 15.000 idas a la base.
        var existentes = repositorio.ListarCns();
        var vistos = new HashSet<string>(StringComparer.Ordinal);
        var nuevos = new List<Medicamento>();
        int yaExistian = 0, noSonMedicamentos = 0, deBaja = 0;

        foreach (var fila in lectura.Filas)
        {
            if (!fila.EsMedicamento) { noSonMedicamentos++; continue; }
            if (!vistos.Add(fila.Cn)) continue;

            // FR-321: lo que ya está en el catálogo no se toca — ni nombre, ni descripción, ni aptitud.
            if (existentes.Contains(fila.Cn)) { yaExistian++; continue; }

            if (fila.EstaDeBaja) deBaja++;
            nuevos.Add(new Medicamento
            {
                Cn = fila.Cn,
                Nombre = fila.Nombre,
                NombreNormalizado = Normalizador.QuitarTildesYMayusculas(fila.Nombre),
                PrincipioActivo = fila.PrincipioActivo,
                Laboratorio = fila.Laboratorio,
                // El nomenclátor no dice si es apto para SPD: queda sin confirmar (FR-301 revisado).
                AptoSpd = null,
                Activo = !fila.EstaDeBaja
            });
        }

        repositorio.CrearEnLote(nuevos, usuarioQueEjecutaId);

        // Art. VII.6: la importación deja traza con su recuento. Cada medicamento creado lleva además su
        // autor en creado_por; una traza por fila no cabe en la transacción del alta en bloque.
        var resultado = new ResultadoImportacionNomenclator(
            true, null, nuevos.Count - deBaja, deBaja, yaExistian, noSonMedicamentos);
        auditoria.Registrar(usuarioQueEjecutaId, "IMPORTACION_NOMENCLATOR", "Medicamento", null,
            $"{Path.GetFileName(rutaFicheroDescargado)}: {resultado.AltasActivas} altas activas, " +
            $"{resultado.AltasDeBaja} de baja (inactivas), {resultado.YaExistian} ya existían y no se han tocado, " +
            $"{resultado.NoSonMedicamentos} filas descartadas por no ser medicamentos.");
        return resultado;
    }

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
            // Los efectos y accesorios no son medicamentos: no se proponen como altas.
            if (!fila.EsMedicamento) continue;

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

    public Medicamento AplicarAltaDesdeNomenclator(FilaNomenclator fila, int? usuarioQueEjecutaId)
    {
        var medicamento = new Medicamento
        {
            Cn = fila.Cn,
            Nombre = fila.Nombre,
            NombreNormalizado = Normalizador.QuitarTildesYMayusculas(fila.Nombre),
            PrincipioActivo = fila.PrincipioActivo,
            Laboratorio = fila.Laboratorio,
            Activo = !fila.EstaDeBaja
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
