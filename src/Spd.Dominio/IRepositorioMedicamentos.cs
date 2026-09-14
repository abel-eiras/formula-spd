namespace Spd.Dominio;

/// <summary>Acceso al catálogo de medicamentos (Art. IV.1).</summary>
public interface IRepositorioMedicamentos
{
    Medicamento? ObtenerPorId(int id);
    Medicamento? ObtenerPorCn(string cn);

    /// <summary>Busca por CN exacto o por fragmento de nombre ya normalizado (FR-305), con límite.</summary>
    IReadOnlyList<Medicamento> Buscar(string fragmento, string fragmentoNormalizado, int limite);

    IReadOnlySet<string> ListarCns();

    /// <summary>Alta masiva atómica, con autor en cada fila (importación del nomenclátor).</summary>
    void CrearEnLote(IReadOnlyList<Medicamento> medicamentos, int? creadoPor);

    /// <summary>Marca aptos los que estén sin confirmar y devuelve cuáles; nunca toca un «no apto».</summary>
    IReadOnlyList<int> ConfirmarAptitud(IReadOnlyCollection<int> medicamentoIds);

    int Crear(Medicamento medicamento);
    void Actualizar(Medicamento medicamento);
    void AgregarVersionHistorica(int medicamentoId, VersionDescripcionFisica version);
    IReadOnlyList<VersionDescripcionFisica> ListarHistorial(int medicamentoId);
}
