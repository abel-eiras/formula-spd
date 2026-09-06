using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Dominio;

namespace Spd.Infraestructura;

/// <summary>Borrado físico en cascada de un paciente (Spec 010 FR-1022) en orden de dependencias,
/// dentro de una transacción. Es el único DELETE de datos de paciente de toda la aplicación
/// (Art. III); la fila de auditoría que documenta la purga la escribe el servicio antes de llamar
/// aquí, y no se toca.</summary>
public sealed class RepositorioPurga(SqliteConnection conexion) : IRepositorioPurga
{
    private static readonly string[] Sentencias =
    [
        "DELETE FROM SPD_Linea_Envase WHERE spd_linea_id IN (SELECT id FROM SPD_Linea WHERE spd_id IN (SELECT id FROM SPD WHERE paciente_id = @p))",
        "DELETE FROM SPD_Verificacion WHERE spd_id IN (SELECT id FROM SPD WHERE paciente_id = @p)",
        "DELETE FROM SPD_Modificacion WHERE spd_id IN (SELECT id FROM SPD WHERE paciente_id = @p)",
        "DELETE FROM SPD_Linea WHERE spd_id IN (SELECT id FROM SPD WHERE paciente_id = @p)",
        "DELETE FROM SPD WHERE paciente_id = @p",
        "DELETE FROM Envase WHERE paciente_id = @p",
        "DELETE FROM Tratamiento WHERE paciente_id = @p",
        "DELETE FROM ComunicacionMedico WHERE paciente_id = @p",
        "DELETE FROM Consentimiento WHERE paciente_id = @p",
        "DELETE FROM EvaluacionIdoneidad WHERE paciente_id = @p",
        "DELETE FROM Contacto WHERE paciente_id = @p",
        "DELETE FROM Paciente WHERE id = @p",
    ];

    public int PurgarPaciente(int pacienteId)
    {
        using var transaccion = conexion.BeginTransaction();
        var total = 0;
        foreach (var sql in Sentencias)
            total += conexion.Execute(sql, new { p = pacienteId }, transaccion);
        transaccion.Commit();
        return total;
    }
}
