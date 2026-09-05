namespace Spd.Dominio;

/// <summary>Acceso al catálogo de medicamentos (Art. IV.1).</summary>
public interface IRepositorioMedicamentos
{
    Medicamento? ObtenerPorId(int id);
    Medicamento? ObtenerPorCn(string cn);

    /// <summary>Busca por CN exacto o por fragmento de nombre ya normalizado (FR-305).</summary>
    IReadOnlyList<Medicamento> Buscar(string fragmento, string fragmentoNormalizado);

    int Crear(Medicamento medicamento);
    void Actualizar(Medicamento medicamento);
    void AgregarVersionHistorica(int medicamentoId, VersionDescripcionFisica version);
    IReadOnlyList<VersionDescripcionFisica> ListarHistorial(int medicamentoId);
}
