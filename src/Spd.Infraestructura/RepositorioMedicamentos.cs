using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Dominio;

namespace Spd.Infraestructura;

/// <summary>Acceso al catálogo de medicamentos vía Dapper/SQLite (Art. IV.1).</summary>
public sealed class RepositorioMedicamentos(SqliteConnection conexion) : IRepositorioMedicamentos
{
    private const string Columnas = """
        id, cn, nombre, nombre_normalizado, principio_activo, laboratorio, forma_farmaceutica,
        apto_spd, motivo_no_apto, fraccionable, unidades_envase, unidades_envase_origen,
        desc_forma, desc_color, desc_ranura, desc_serigrafia, desc_tamano, desc_texto,
        desc_vigente_desde, gtin, activo
        """;

    public Medicamento? ObtenerPorId(int id)
        => conexion.QuerySingleOrDefault<MedicamentoFila>($"SELECT {Columnas} FROM Medicamento WHERE id = @id", new { id })
            ?.AMedicamento();

    public Medicamento? ObtenerPorCn(string cn)
        => conexion.QuerySingleOrDefault<MedicamentoFila>($"SELECT {Columnas} FROM Medicamento WHERE cn = @cn", new { cn })
            ?.AMedicamento();

    public IReadOnlyList<Medicamento> Buscar(string fragmento, string fragmentoNormalizado)
        => conexion.Query<MedicamentoFila>(
                $"""
                SELECT {Columnas} FROM Medicamento
                WHERE cn = @fragmento OR nombre_normalizado LIKE @patron
                ORDER BY nombre
                """,
                new { fragmento, patron = $"%{fragmentoNormalizado}%" })
            .Select(f => f.AMedicamento())
            .ToList();

    public int Crear(Medicamento medicamento)
        => conexion.ExecuteScalar<int>(
            """
            INSERT INTO Medicamento (
                cn, nombre, nombre_normalizado, principio_activo, laboratorio, forma_farmaceutica,
                apto_spd, motivo_no_apto, fraccionable, unidades_envase, unidades_envase_origen,
                desc_forma, desc_color, desc_ranura, desc_serigrafia, desc_tamano, desc_texto,
                desc_vigente_desde, gtin, activo
            ) VALUES (
                @Cn, @Nombre, @NombreNormalizado, @PrincipioActivo, @Laboratorio, @FormaFarmaceutica,
                @AptoSpd, @MotivoNoApto, @Fraccionable, @UnidadesEnvase, @UnidadesEnvaseOrigen,
                @DescForma, @DescColor, @DescRanura, @DescSerigrafia, @DescTamano, @DescTexto,
                @DescVigenteDesde, @Gtin, @Activo
            ) RETURNING id
            """,
            AParametros(medicamento));

    public void Actualizar(Medicamento medicamento)
        => conexion.Execute(
            """
            UPDATE Medicamento SET
                nombre = @Nombre, nombre_normalizado = @NombreNormalizado,
                principio_activo = @PrincipioActivo, laboratorio = @Laboratorio,
                forma_farmaceutica = @FormaFarmaceutica, apto_spd = @AptoSpd,
                motivo_no_apto = @MotivoNoApto, fraccionable = @Fraccionable,
                unidades_envase = @UnidadesEnvase, unidades_envase_origen = @UnidadesEnvaseOrigen,
                desc_forma = @DescForma, desc_color = @DescColor, desc_ranura = @DescRanura,
                desc_serigrafia = @DescSerigrafia, desc_tamano = @DescTamano, desc_texto = @DescTexto,
                desc_vigente_desde = @DescVigenteDesde, gtin = @Gtin, activo = @Activo,
                modificado_en = @ModificadoEn
            WHERE id = @Id
            """,
            AParametros(medicamento));

    public void AgregarVersionHistorica(int medicamentoId, VersionDescripcionFisica version)
        => conexion.Execute(
            """
            INSERT INTO Medicamento_Hist (
                medicamento_id, desc_forma, desc_color, desc_ranura, desc_serigrafia, desc_tamano,
                desc_texto, vigente_desde, vigente_hasta
            ) VALUES (
                @medicamentoId, @DescForma, @DescColor, @DescRanura, @DescSerigrafia, @DescTamano,
                @DescTexto, @VigenteDesde, @VigenteHasta
            )
            """,
            new
            {
                medicamentoId, version.DescForma, version.DescColor, version.DescRanura,
                version.DescSerigrafia, version.DescTamano, version.DescTexto,
                VigenteDesde = version.VigenteDesde.ToString("o"), VigenteHasta = version.VigenteHasta.ToString("o")
            });

    public IReadOnlyList<VersionDescripcionFisica> ListarHistorial(int medicamentoId)
        => conexion.Query<VersionHistFila>(
                """
                SELECT desc_forma, desc_color, desc_ranura, desc_serigrafia, desc_tamano, desc_texto,
                       vigente_desde, vigente_hasta
                FROM Medicamento_Hist WHERE medicamento_id = @medicamentoId ORDER BY vigente_desde
                """,
                new { medicamentoId })
            .Select(f => f.AVersion())
            .ToList();

    private static object AParametros(Medicamento m) => new
    {
        m.Id, m.Cn, m.Nombre, m.NombreNormalizado, m.PrincipioActivo, m.Laboratorio,
        FormaFarmaceutica = m.FormaFarmaceutica is null ? null : TextoForma(m.FormaFarmaceutica.Value),
        AptoSpd = m.AptoSpd ? 1 : 0, m.MotivoNoApto, Fraccionable = m.Fraccionable ? 1 : 0,
        m.UnidadesEnvase, UnidadesEnvaseOrigen = TextoOrigen(m.UnidadesEnvaseOrigen),
        m.DescForma, m.DescColor, m.DescRanura, m.DescSerigrafia, m.DescTamano, m.DescTexto,
        DescVigenteDesde = m.DescVigenteDesde.ToString("o"), m.Gtin, Activo = m.Activo ? 1 : 0,
        ModificadoEn = DateTime.UtcNow.ToString("o")
    };

    private static string TextoForma(FormaFarmaceutica forma) => forma switch
    {
        Dominio.FormaFarmaceutica.Comprimido => "COMPRIMIDO",
        Dominio.FormaFarmaceutica.ComprimidoLiberacionProlongada => "COMPRIMIDO_LIBERACION_PROLONGADA",
        Dominio.FormaFarmaceutica.Capsula => "CAPSULA",
        Dominio.FormaFarmaceutica.CapsulaLiberacionProlongada => "CAPSULA_LIBERACION_PROLONGADA",
        Dominio.FormaFarmaceutica.Gragea => "GRAGEA",
        Dominio.FormaFarmaceutica.Pastilla => "PASTILLA",
        Dominio.FormaFarmaceutica.Pildora => "PILDORA",
        Dominio.FormaFarmaceutica.OtraNoApta => "OTRA_NO_APTA",
        _ => throw new ArgumentOutOfRangeException(nameof(forma))
    };

    private static FormaFarmaceutica? TextoAForma(string? texto) => texto switch
    {
        null => null,
        "COMPRIMIDO" => Dominio.FormaFarmaceutica.Comprimido,
        "COMPRIMIDO_LIBERACION_PROLONGADA" => Dominio.FormaFarmaceutica.ComprimidoLiberacionProlongada,
        "CAPSULA" => Dominio.FormaFarmaceutica.Capsula,
        "CAPSULA_LIBERACION_PROLONGADA" => Dominio.FormaFarmaceutica.CapsulaLiberacionProlongada,
        "GRAGEA" => Dominio.FormaFarmaceutica.Gragea,
        "PASTILLA" => Dominio.FormaFarmaceutica.Pastilla,
        "PILDORA" => Dominio.FormaFarmaceutica.Pildora,
        "OTRA_NO_APTA" => Dominio.FormaFarmaceutica.OtraNoApta,
        _ => throw new ArgumentOutOfRangeException(nameof(texto))
    };

    private static string TextoOrigen(OrigenUnidadesEnvase origen)
        => origen == OrigenUnidadesEnvase.Manual ? "MANUAL" : "IMPORTADO_REGEX";

    private static OrigenUnidadesEnvase TextoAOrigen(string texto)
        => texto == "MANUAL" ? OrigenUnidadesEnvase.Manual : OrigenUnidadesEnvase.ImportadoRegex;

    /// <summary>Fila 1:1 con las columnas leídas de Medicamento. Evita depender de la conversión
    /// automática de Dapper para los enums y los booleanos (mismo motivo que UsuarioFila en Spec 000).</summary>
    private sealed record MedicamentoFila(
        long Id, string Cn, string Nombre, string NombreNormalizado, string? PrincipioActivo,
        string? Laboratorio, string? FormaFarmaceutica, long AptoSpd, string? MotivoNoApto,
        long Fraccionable, long? UnidadesEnvase, string UnidadesEnvaseOrigen, string? DescForma,
        string? DescColor, string? DescRanura, string? DescSerigrafia, string? DescTamano,
        string? DescTexto, string DescVigenteDesde, string? Gtin, long Activo)
    {
        public Medicamento AMedicamento() => new()
        {
            Id = (int)Id,
            Cn = Cn,
            Nombre = Nombre,
            NombreNormalizado = NombreNormalizado,
            PrincipioActivo = PrincipioActivo,
            Laboratorio = Laboratorio,
            FormaFarmaceutica = TextoAForma(FormaFarmaceutica),
            AptoSpd = AptoSpd == 1,
            MotivoNoApto = MotivoNoApto,
            Fraccionable = Fraccionable == 1,
            UnidadesEnvase = UnidadesEnvase is null ? null : (int)UnidadesEnvase,
            UnidadesEnvaseOrigen = TextoAOrigen(UnidadesEnvaseOrigen),
            DescForma = DescForma,
            DescColor = DescColor,
            DescRanura = DescRanura,
            DescSerigrafia = DescSerigrafia,
            DescTamano = DescTamano,
            DescTexto = DescTexto,
            DescVigenteDesde = DateTime.Parse(DescVigenteDesde),
            Gtin = Gtin,
            Activo = Activo == 1
        };
    }

    private sealed record VersionHistFila(
        string? DescForma, string? DescColor, string? DescRanura, string? DescSerigrafia,
        string? DescTamano, string? DescTexto, string VigenteDesde, string VigenteHasta)
    {
        public VersionDescripcionFisica AVersion() => new(
            DescForma, DescColor, DescRanura, DescSerigrafia, DescTamano, DescTexto,
            DateTime.Parse(VigenteDesde), DateTime.Parse(VigenteHasta));
    }
}
