using System.Globalization;
using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Dominio;

namespace Spd.Infraestructura;

/// <summary>Acceso a SPD vía Dapper/SQLite. Nunca DELETE (Art. III); ANULADO es una transición de
/// estado, no una eliminación.</summary>
public sealed class RepositorioSpd(SqliteConnection conexion) : IRepositorioSpd
{
    private const string FormatoFecha = "yyyy-MM-dd";

    private const string Columnas = """
        id, num_registro, correlativo_num_registro, paciente_id, version, sesion_id,
        validez_desde, validez_hasta, fecha_preparacion, material_id, registro_ambiental_id,
        elaborador_id, verificador_id, fecha_verificacion, excepcion_verificador_motivo,
        resultado_verificacion, entregador_id, fecha_entrega, entregado_a, primera_entrega,
        spd_anterior_recogido, unidades_no_administradas, observaciones_adherencia,
        cambios_medicacion_preguntado, observaciones_etiqueta, estado, motivo_anulacion,
        impreso_ficha_en, impreso_etiquetas_en, impreso_instrucciones_en
        """;

    public int Crear(SPD s)
        => conexion.ExecuteScalar<int>(
            $"""
            INSERT INTO SPD (
                num_registro, correlativo_num_registro, paciente_id, version, sesion_id,
                validez_desde, validez_hasta, fecha_preparacion, material_id, registro_ambiental_id,
                elaborador_id, verificador_id, fecha_verificacion, excepcion_verificador_motivo,
                resultado_verificacion, entregador_id, fecha_entrega, entregado_a, primera_entrega,
                spd_anterior_recogido, unidades_no_administradas, observaciones_adherencia,
                cambios_medicacion_preguntado, observaciones_etiqueta, estado, motivo_anulacion,
                impreso_ficha_en, impreso_etiquetas_en, impreso_instrucciones_en
            ) VALUES (
                @NumRegistro, @CorrelativoNumRegistro, @PacienteId, @Version, @SesionId,
                @ValidezDesde, @ValidezHasta, @FechaPreparacion, @MaterialId, @RegistroAmbientalId,
                @ElaboradorId, @VerificadorId, @FechaVerificacion, @ExcepcionVerificadorMotivo,
                @ResultadoVerificacion, @EntregadorId, @FechaEntrega, @EntregadoA, @PrimeraEntrega,
                @SpdAnteriorRecogido, @UnidadesNoAdministradas, @ObservacionesAdherencia,
                @CambiosMedicacionPreguntado, @ObservacionesEtiqueta, @Estado, @MotivoAnulacion,
                @ImpresoFichaEn, @ImpresoEtiquetasEn, @ImpresoInstruccionesEn
            ) RETURNING id
            """,
            AParametros(s));

    public void Actualizar(SPD s)
        => conexion.Execute(
            """
            UPDATE SPD SET
                version = @Version, fecha_preparacion = @FechaPreparacion, material_id = @MaterialId,
                registro_ambiental_id = @RegistroAmbientalId, verificador_id = @VerificadorId,
                fecha_verificacion = @FechaVerificacion, excepcion_verificador_motivo = @ExcepcionVerificadorMotivo,
                resultado_verificacion = @ResultadoVerificacion, entregador_id = @EntregadorId,
                fecha_entrega = @FechaEntrega, entregado_a = @EntregadoA, primera_entrega = @PrimeraEntrega,
                spd_anterior_recogido = @SpdAnteriorRecogido, unidades_no_administradas = @UnidadesNoAdministradas,
                observaciones_adherencia = @ObservacionesAdherencia,
                cambios_medicacion_preguntado = @CambiosMedicacionPreguntado,
                observaciones_etiqueta = @ObservacionesEtiqueta, estado = @Estado, motivo_anulacion = @MotivoAnulacion,
                impreso_ficha_en = @ImpresoFichaEn, impreso_etiquetas_en = @ImpresoEtiquetasEn,
                impreso_instrucciones_en = @ImpresoInstruccionesEn, modificado_en = @ModificadoEn
            WHERE id = @Id
            """,
            AParametros(s));

    public SPD? ObtenerPorId(int id)
        => conexion.QuerySingleOrDefault<SpdFila>($"SELECT {Columnas} FROM SPD WHERE id = @id", new { id })?.ASpd();

    public int ObtenerSiguienteCorrelativo()
        => conexion.ExecuteScalar<int>("SELECT COALESCE(MAX(correlativo_num_registro), 0) + 1 FROM SPD");

    public IReadOnlyList<SPD> ListarPorSesion(Guid sesionId)
        => conexion.Query<SpdFila>($"SELECT {Columnas} FROM SPD WHERE sesion_id = @sesionId ORDER BY id", new { sesionId = sesionId.ToString() })
            .Select(f => f.ASpd())
            .ToList();

    public IReadOnlyList<SPD> ListarUltimaSesionDePaciente(int pacienteId)
    {
        var ultimaSesion = conexion.ExecuteScalar<string?>(
            "SELECT sesion_id FROM SPD WHERE paciente_id = @pacienteId ORDER BY id DESC LIMIT 1", new { pacienteId });
        return ultimaSesion is null ? [] : ListarPorSesion(Guid.Parse(ultimaSesion));
    }

    public IReadOnlyList<SPD> ListarPendientesDeEntrega(int pacienteId)
        => conexion.Query<SpdFila>(
                $"SELECT {Columnas} FROM SPD WHERE paciente_id = @pacienteId AND estado = 'Verificado' ORDER BY id",
                new { pacienteId })
            .Select(f => f.ASpd())
            .ToList();

    public IReadOnlyList<SPD> Listar(EstadoSpd? filtroEstado, int? filtroPacienteId, int? filtroElaboradorId)
    {
        var condiciones = new List<string>();
        var parametros = new DynamicParameters();
        if (filtroEstado is not null) { condiciones.Add("estado = @estado"); parametros.Add("estado", filtroEstado.ToString()); }
        if (filtroPacienteId is not null) { condiciones.Add("paciente_id = @pacienteId"); parametros.Add("pacienteId", filtroPacienteId); }
        if (filtroElaboradorId is not null) { condiciones.Add("elaborador_id = @elaboradorId"); parametros.Add("elaboradorId", filtroElaboradorId); }

        var clausula = condiciones.Count > 0 ? $"WHERE {string.Join(" AND ", condiciones)}" : "";
        return conexion.Query<SpdFila>($"SELECT {Columnas} FROM SPD {clausula} ORDER BY id DESC", parametros)
            .Select(f => f.ASpd())
            .ToList();
    }

    private static object AParametros(SPD s) => new
    {
        s.Id,
        s.NumRegistro,
        s.CorrelativoNumRegistro,
        s.PacienteId,
        s.Version,
        SesionId = s.SesionId.ToString(),
        ValidezDesde = s.ValidezDesde.ToString(FormatoFecha, CultureInfo.InvariantCulture),
        ValidezHasta = s.ValidezHasta.ToString(FormatoFecha, CultureInfo.InvariantCulture),
        FechaPreparacion = s.FechaPreparacion?.ToString("o"),
        s.MaterialId,
        s.RegistroAmbientalId,
        s.ElaboradorId,
        s.VerificadorId,
        FechaVerificacion = s.FechaVerificacion?.ToString("o"),
        s.ExcepcionVerificadorMotivo,
        ResultadoVerificacion = s.ResultadoVerificacion?.ToString(),
        s.EntregadorId,
        FechaEntrega = s.FechaEntrega?.ToString("o"),
        s.EntregadoA,
        PrimeraEntrega = s.PrimeraEntrega ? 1 : 0,
        SpdAnteriorRecogido = s.SpdAnteriorRecogido.HasValue ? (s.SpdAnteriorRecogido.Value ? 1 : 0) : (int?)null,
        s.UnidadesNoAdministradas,
        s.ObservacionesAdherencia,
        CambiosMedicacionPreguntado = s.CambiosMedicacionPreguntado ? 1 : 0,
        s.ObservacionesEtiqueta,
        Estado = s.Estado.ToString(),
        s.MotivoAnulacion,
        ImpresoFichaEn = s.ImpresoFichaEn?.ToString("o"),
        ImpresoEtiquetasEn = s.ImpresoEtiquetasEn?.ToString("o"),
        ImpresoInstruccionesEn = s.ImpresoInstruccionesEn?.ToString("o"),
        ModificadoEn = DateTime.UtcNow.ToString("o")
    };

    private sealed record SpdFila(
        long Id, string NumRegistro, long CorrelativoNumRegistro, long PacienteId, long Version, string SesionId,
        string ValidezDesde, string ValidezHasta, string? FechaPreparacion, long? MaterialId, long? RegistroAmbientalId,
        long ElaboradorId, long? VerificadorId, string? FechaVerificacion, string? ExcepcionVerificadorMotivo,
        string? ResultadoVerificacion, long? EntregadorId, string? FechaEntrega, string? EntregadoA, long PrimeraEntrega,
        long? SpdAnteriorRecogido, string? UnidadesNoAdministradas, string? ObservacionesAdherencia,
        long CambiosMedicacionPreguntado, string? ObservacionesEtiqueta, string Estado, string? MotivoAnulacion,
        string? ImpresoFichaEn, string? ImpresoEtiquetasEn, string? ImpresoInstruccionesEn)
    {
        public SPD ASpd() => new()
        {
            Id = (int)Id,
            NumRegistro = NumRegistro,
            CorrelativoNumRegistro = (int)CorrelativoNumRegistro,
            PacienteId = (int)PacienteId,
            Version = (int)Version,
            SesionId = Guid.Parse(SesionId),
            ValidezDesde = DateOnly.ParseExact(ValidezDesde, FormatoFecha, CultureInfo.InvariantCulture),
            ValidezHasta = DateOnly.ParseExact(ValidezHasta, FormatoFecha, CultureInfo.InvariantCulture),
            FechaPreparacion = FechaPreparacion is null ? null : DateTime.Parse(FechaPreparacion, CultureInfo.InvariantCulture),
            MaterialId = MaterialId is null ? null : (int)MaterialId,
            RegistroAmbientalId = RegistroAmbientalId is null ? null : (int)RegistroAmbientalId,
            ElaboradorId = (int)ElaboradorId,
            VerificadorId = VerificadorId is null ? null : (int)VerificadorId,
            FechaVerificacion = FechaVerificacion is null ? null : DateTime.Parse(FechaVerificacion, CultureInfo.InvariantCulture),
            ExcepcionVerificadorMotivo = ExcepcionVerificadorMotivo,
            ResultadoVerificacion = ResultadoVerificacion is null ? null : Enum.Parse<Dominio.ResultadoVerificacion>(ResultadoVerificacion),
            EntregadorId = EntregadorId is null ? null : (int)EntregadorId,
            FechaEntrega = FechaEntrega is null ? null : DateTime.Parse(FechaEntrega, CultureInfo.InvariantCulture),
            EntregadoA = EntregadoA,
            PrimeraEntrega = PrimeraEntrega == 1,
            SpdAnteriorRecogido = SpdAnteriorRecogido is null ? null : SpdAnteriorRecogido == 1,
            UnidadesNoAdministradas = UnidadesNoAdministradas,
            ObservacionesAdherencia = ObservacionesAdherencia,
            CambiosMedicacionPreguntado = CambiosMedicacionPreguntado == 1,
            ObservacionesEtiqueta = ObservacionesEtiqueta,
            Estado = Enum.Parse<EstadoSpd>(Estado),
            MotivoAnulacion = MotivoAnulacion,
            ImpresoFichaEn = ImpresoFichaEn is null ? null : DateTime.Parse(ImpresoFichaEn, CultureInfo.InvariantCulture),
            ImpresoEtiquetasEn = ImpresoEtiquetasEn is null ? null : DateTime.Parse(ImpresoEtiquetasEn, CultureInfo.InvariantCulture),
            ImpresoInstruccionesEn = ImpresoInstruccionesEn is null ? null : DateTime.Parse(ImpresoInstruccionesEn, CultureInfo.InvariantCulture)
        };
    }
}
