using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Dominio;

namespace Spd.Infraestructura;

/// <summary>Acceso a SPD_Verificacion vía Dapper/SQLite. Solo alta: se conservan todos los
/// intentos (FR-651, Art. III). Ocho ítems desde la migración 0010 (Anexo I.G del PNT I).</summary>
public sealed class RepositorioSpdVerificaciones(SqliteConnection conexion) : IRepositorioSpdVerificaciones
{
    private const string Columnas = """
        id, spd_id, verificador_id, fecha, verif_aspecto, verif_etiqueta_datos, verif_etiqueta_validez,
        verif_instrucciones, verif_contenido, verif_fabricante_pnt, verif_etiqueta_ficha_paciente,
        verif_trazabilidad, resultado, excepcion_motivo
        """;

    public int Crear(SpdVerificacion v)
        => conexion.ExecuteScalar<int>(
            $"""
            INSERT INTO SPD_Verificacion (
                spd_id, verificador_id, fecha, verif_aspecto, verif_etiqueta_datos, verif_etiqueta_validez,
                verif_instrucciones, verif_contenido, verif_fabricante_pnt, verif_etiqueta_ficha_paciente,
                verif_trazabilidad, resultado, excepcion_motivo
            ) VALUES (
                @SpdId, @VerificadorId, @Fecha, @VerifAspecto, @VerifEtiquetaDatos, @VerifEtiquetaValidez,
                @VerifInstrucciones, @VerifContenido, @VerifFabricantePnt, @VerifEtiquetaFichaPaciente,
                @VerifTrazabilidad, @Resultado, @ExcepcionMotivo
            ) RETURNING id
            """,
            new
            {
                v.SpdId, v.VerificadorId, Fecha = v.Fecha.ToString("o"),
                VerifAspecto = v.VerifAspecto ? 1 : 0, VerifEtiquetaDatos = v.VerifEtiquetaDatos ? 1 : 0,
                VerifEtiquetaValidez = v.VerifEtiquetaValidez ? 1 : 0, VerifInstrucciones = v.VerifInstrucciones ? 1 : 0,
                VerifContenido = v.VerifContenido ? 1 : 0, VerifFabricantePnt = v.VerifFabricantePnt ? 1 : 0,
                VerifEtiquetaFichaPaciente = v.VerifEtiquetaFichaPaciente ? 1 : 0, VerifTrazabilidad = v.VerifTrazabilidad ? 1 : 0,
                Resultado = v.Resultado.ToString(), v.ExcepcionMotivo
            });

    public IReadOnlyList<SpdVerificacion> ListarPorSpd(int spdId)
        => conexion.Query<Fila>($"SELECT {Columnas} FROM SPD_Verificacion WHERE spd_id = @spdId ORDER BY id", new { spdId })
            .Select(f => f.AVerificacion())
            .ToList();

    private sealed record Fila(
        long Id, long SpdId, long VerificadorId, string Fecha, long VerifAspecto, long VerifEtiquetaDatos,
        long VerifEtiquetaValidez, long VerifInstrucciones, long VerifContenido, long VerifFabricantePnt,
        long VerifEtiquetaFichaPaciente, long VerifTrazabilidad, string Resultado, string? ExcepcionMotivo)
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
            VerifFabricantePnt = VerifFabricantePnt == 1,
            VerifEtiquetaFichaPaciente = VerifEtiquetaFichaPaciente == 1,
            VerifTrazabilidad = VerifTrazabilidad == 1,
            Resultado = Enum.Parse<ResultadoVerificacion>(Resultado),
            ExcepcionMotivo = ExcepcionMotivo
        };
    }
}
