using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Catálogo de medicamentos: alta mínima, edición, descripción física versionada,
/// unidades por envase y búsqueda (FR-300..FR-311). Toda escritura registra en auditoría (Art. VII.6).</summary>
public sealed class ServicioMedicamentos(
    IRepositorioMedicamentos repositorio, IRegistradorAuditoria auditoria) : IServicioMedicamentos
{
    public Medicamento? ObtenerPorId(int id) => repositorio.ObtenerPorId(id);

    public Medicamento? ObtenerPorCn(string cn) => repositorio.ObtenerPorCn(cn);

    public IReadOnlyList<Medicamento> Buscar(string fragmento)
        => repositorio.Buscar(fragmento, Normalizador.QuitarTildesYMayusculas(fragmento));

    public Medicamento Crear(DatosAltaMedicamento datos, int? usuarioQueEjecutaId)
    {
        if (string.IsNullOrWhiteSpace(datos.Cn) || string.IsNullOrWhiteSpace(datos.Nombre))
        {
            throw new ErrorValidacionException("CN y nombre son obligatorios (FR-300, FR-302).");
        }

        var existente = repositorio.ObtenerPorCn(datos.Cn);
        if (existente is not null)
        {
            if (existente.Activo)
            {
                throw new MedicamentoDuplicadoException(existente,
                    $"Ya existe el medicamento con CN {existente.Cn}: {existente.Nombre}.");
            }

            // CN dado de baja: se reactiva la fila existente, nunca se duplica (FR-306, CA-305).
            existente.Activo = true;
            repositorio.Actualizar(existente);
            auditoria.Registrar(usuarioQueEjecutaId, "REACTIVACION", "Medicamento", existente.Id, null);
            return existente;
        }

        var medicamento = new Medicamento
        {
            Cn = datos.Cn,
            Nombre = datos.Nombre,
            NombreNormalizado = Normalizador.QuitarTildesYMayusculas(datos.Nombre)
        };
        medicamento.Id = repositorio.Crear(medicamento);

        auditoria.Registrar(usuarioQueEjecutaId, "ALTA", "Medicamento", medicamento.Id, null);
        return medicamento;
    }

    public void ActualizarDatos(Medicamento medicamento, int? usuarioQueEjecutaId)
    {
        _ = repositorio.ObtenerPorId(medicamento.Id)
            ?? throw new ErrorValidacionException($"No existe el medicamento {medicamento.Id}.");

        var aptoDerivado = DerivarAptoPorDefecto(medicamento.FormaFarmaceutica);
        if (medicamento.AptoSpd != aptoDerivado && string.IsNullOrWhiteSpace(medicamento.MotivoNoApto))
        {
            throw new ErrorValidacionException(
                "Fijar la aptitud SPD distinta de la derivada de la forma farmacéutica exige un motivo (FR-301, CA-302).");
        }

        medicamento.NombreNormalizado = Normalizador.QuitarTildesYMayusculas(medicamento.Nombre);
        repositorio.Actualizar(medicamento);

        auditoria.Registrar(usuarioQueEjecutaId, "EDITAR", "Medicamento", medicamento.Id, null);
    }

    public string ProponerDescripcionTexto(DatosDescripcionFisica datos)
    {
        var base_ = string.Join(" ", new[] { datos.DescForma, datos.DescColor }.Where(s => !string.IsNullOrWhiteSpace(s)));
        var extras = new List<string>();
        if (!string.IsNullOrWhiteSpace(datos.DescRanura)) extras.Add($"ranura {datos.DescRanura}");
        if (!string.IsNullOrWhiteSpace(datos.DescSerigrafia)) extras.Add($"serigrafía {datos.DescSerigrafia}");
        if (!string.IsNullOrWhiteSpace(datos.DescTamano)) extras.Add(datos.DescTamano!);

        return extras.Count == 0 ? base_ : $"{base_}, {string.Join(", ", extras)}";
    }

    public void ActualizarDescripcionFisica(int medicamentoId, DatosDescripcionFisica datos, int? usuarioQueEjecutaId)
    {
        var medicamento = repositorio.ObtenerPorId(medicamentoId)
            ?? throw new ErrorValidacionException($"No existe el medicamento {medicamentoId}.");

        if (!DescripcionCambio(medicamento, datos)) return;

        var ahora = DateTime.UtcNow;
        repositorio.AgregarVersionHistorica(medicamentoId, new VersionDescripcionFisica(
            medicamento.DescForma, medicamento.DescColor, medicamento.DescRanura, medicamento.DescSerigrafia,
            medicamento.DescTamano, medicamento.DescTexto, medicamento.DescVigenteDesde, ahora));

        medicamento.DescForma = datos.DescForma;
        medicamento.DescColor = datos.DescColor;
        medicamento.DescRanura = datos.DescRanura;
        medicamento.DescSerigrafia = datos.DescSerigrafia;
        medicamento.DescTamano = datos.DescTamano;
        medicamento.DescTexto = datos.DescTexto;
        medicamento.DescVigenteDesde = ahora;
        repositorio.Actualizar(medicamento);

        auditoria.Registrar(usuarioQueEjecutaId, "EDITAR_DESCRIPCION", "Medicamento", medicamentoId, null);
    }

    public IReadOnlyList<VersionDescripcionFisica> ListarHistorialDescripcion(int medicamentoId)
        => repositorio.ListarHistorial(medicamentoId);

    public void ActualizarUnidadesEnvase(int medicamentoId, int unidadesEnvase, int? usuarioQueEjecutaId)
    {
        var medicamento = repositorio.ObtenerPorId(medicamentoId)
            ?? throw new ErrorValidacionException($"No existe el medicamento {medicamentoId}.");

        medicamento.UnidadesEnvase = unidadesEnvase;
        medicamento.UnidadesEnvaseOrigen = OrigenUnidadesEnvase.Manual; // FR-311: siempre manual desde Presentación
        repositorio.Actualizar(medicamento);

        auditoria.Registrar(usuarioQueEjecutaId, "EDITAR_UNIDADES_ENVASE", "Medicamento", medicamentoId, null);
    }

    public void DarDeBaja(int medicamentoId, int? usuarioQueEjecutaId)
    {
        var medicamento = repositorio.ObtenerPorId(medicamentoId)
            ?? throw new ErrorValidacionException($"No existe el medicamento {medicamentoId}.");

        medicamento.Activo = false;
        repositorio.Actualizar(medicamento);

        auditoria.Registrar(usuarioQueEjecutaId, "BAJA", "Medicamento", medicamentoId, null);
    }

    private static bool DerivarAptoPorDefecto(FormaFarmaceutica? forma)
        => forma is null || ReglaAptitudSpd.PorDefecto(forma.Value);

    private static bool DescripcionCambio(Medicamento medicamento, DatosDescripcionFisica datos)
        => medicamento.DescForma != datos.DescForma
           || medicamento.DescColor != datos.DescColor
           || medicamento.DescRanura != datos.DescRanura
           || medicamento.DescSerigrafia != datos.DescSerigrafia
           || medicamento.DescTamano != datos.DescTamano
           || medicamento.DescTexto != datos.DescTexto;
}
