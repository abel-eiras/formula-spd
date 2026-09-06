using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Dominio;

namespace Spd.Infraestructura;

/// <summary>Acceso a SPD_Verificacion vía Dapper/SQLite. Solo alta: se conservan todos los
/// intentos (FR-651, Art. III).</summary>
public sealed class RepositorioSpdVerificaciones(SqliteConnection conexion) : IRepositorioSpdVerificaciones
{
    private const string Columnas = """
        id, spd_id, verificador_id, fecha, verif_aspecto, verif_etiqueta_datos, verif_etiqueta_validez,
        verif_instrucciones, verif_contenido, resultado, excepcion_motivo
        """;

    public int Crear(SpdVerificacion v)
        => conexion.ExecuteScalar<int>(
            $"""
            INSERT INTO SPD_Verificacion (
                spd_id, verificador_id, fecha, verif_aspecto, verif_etiqueta_datos, verif_etiqueta_validez,
                verif_instrucciones, verif_contenido, resultado, excepcion_motivo
            ) VALUES (
                @SpdId, @VerificadorId, @Fecha, @VerifAspecto, @VerifEtiquetaDatos, @VerifEtiquetaValidez,
                @VerifInstrucciones, @VerifContenido, @Resultado, @ExcepcionMotivo
            ) RETURNING id
            """,
            new
            {
                v.SpdId, v.VerificadorId, Fecha = v.Fecha.ToString("o"),
                VerifAspecto = v.VerifAspecto ? 1 : 0, VerifEtiquetaDatos = v.VerifEtiquetaDatos ? 1 : 0,
                VerifEtiquetaValidez = v.VerifEtiquetaValidez ? 1 : 0, VerifInstrucciones = v.VerifInstrucciones ? 1 : 0,
                VerifContenido = v.VerifContenido ? 1 : 0, Resultado = v.Resultado.ToString(), v.ExcepcionMotivo
            });

    public IReadOnlyList<SpdVerificacion> ListarPorSpd(int spdId)
        => conexion.Query<Fila>($"SELECT {Columnas} FROM SPD_Verificacion WHERE spd_id = @spdId ORDER BY id", new { spdId })
            .Select(f => f.AVerificacion())
            .ToList();

    private sealed record Fila(
        long Id, long SpdId, long VerificadorId, string Fecha, long VerifAspecto, long VerifEtiquetaDatos,
        long VerifEtiquetaValidez, long VerifInstrucciones, long VerifContenido, string Resultado, string? ExcepcionMotivo)
    {
        public SpdVerificacion AVerificacion() => new()
        {
            Id = (int)Id,
            SpdId = (int)SpdId,
            VerificadorId = (int)VerificadorId,
            Fecha = DateTime.Parse(Fecha, System.Globalization.CultureInfo.InvariantCulture),
            VerifAspecto = VerifAspecto == 1,
            VerifEtiquetaDatos = VerifEtiquetaDatos == 1,
            VerifEtiquetaValidez = VerifEtiquetaValidez == 1,
            VerifInstrucciones = VerifInstrucciones == 1,
            VerifContenido = VerifContenido == 1,
            Resultado = Enum.Parse<ResultadoVerificacion>(Resultado),
            ExcepcionMotivo = ExcepcionMotivo
        };
    }
}
