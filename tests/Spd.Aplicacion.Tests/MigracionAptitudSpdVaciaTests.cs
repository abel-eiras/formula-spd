using System.Text.RegularExpressions;
using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Xunit;

namespace Spd.Aplicacion.Tests;

/// <summary>Migración 0013 (Spec 003 FR-301, revisado el 2026-09-14): la aptitud SPD pasa a admitir vacío.
///
/// SQLite no permite quitar un NOT NULL, así que la tabla se reconstruye. De una reconstrucción solo
/// importa una cosa, y es lo que se prueba aquí sobre una base en el esquema anterior: **que no se pierde
/// ni cambia nada de lo que ya había** (Art. III).</summary>
public sealed class MigracionAptitudSpdVaciaTests
{
    private const int VersionAnterior = 12;

    /// <summary>Una base tal como estaba antes de la 0013: se aplican a mano solo las migraciones hasta la
    /// 12, con el mismo formato de <c>schema_version</c> que usa el aplicador.</summary>
    private static SqliteConnection BaseEnLaVersionAnterior()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        conexion.Execute("CREATE TABLE schema_version (version INTEGER PRIMARY KEY, aplicado_en TEXT NOT NULL)");

        var ensamblado = typeof(AplicadorMigraciones).Assembly;
        var patron = new Regex(@"(\d{4})_[^.]+\.sql$");
        var migraciones = ensamblado.GetManifestResourceNames()
            .Select(nombre => (Coincidencia: patron.Match(nombre), Nombre: nombre))
            .Where(x => x.Coincidencia.Success)
            .Select(x => (Version: int.Parse(x.Coincidencia.Groups[1].Value), x.Nombre))
            .Where(x => x.Version <= VersionAnterior)
            .OrderBy(x => x.Version);

        foreach (var (version, recurso) in migraciones)
        {
            using var flujo = ensamblado.GetManifestResourceStream(recurso)!;
            using var lector = new StreamReader(flujo);
            using var transaccion = conexion.BeginTransaction();
            conexion.Execute(lector.ReadToEnd(), transaction: transaccion);
            conexion.Execute("INSERT INTO schema_version (version, aplicado_en) VALUES (@version, 'prueba')",
                new { version }, transaccion);
            transaccion.Commit();
        }
        return conexion;
    }

    private static int ObligatoriaAptitud(SqliteConnection conexion)
        => conexion.ExecuteScalar<int>("SELECT \"notnull\" FROM pragma_table_info('Medicamento') WHERE name = 'apto_spd'");

    [Fact]
    public void La_reconstruccion_conserva_filas_ids_y_valores_y_admite_la_aptitud_vacia()
    {
        using var conexion = BaseEnLaVersionAnterior();
        // Garantiza que la prueba parte del esquema viejo, donde la aptitud era obligatoria.
        Assert.Equal(1, ObligatoriaAptitud(conexion));

        var repositorio = new RepositorioMedicamentos(conexion);
        var apto = new Medicamento { Cn = "111111", Nombre = "Enalapril 20 mg", NombreNormalizado = "enalapril 20 mg", AptoSpd = true };
        apto.Id = repositorio.Crear(apto);
        var noApto = new Medicamento
        {
            Cn = "222222", Nombre = "Jarabe tos", NombreNormalizado = "jarabe tos",
            AptoSpd = false, MotivoNoApto = "Forma líquida.", DescTexto = "jarabe marrón"
        };
        noApto.Id = repositorio.Crear(noApto);
        var deBaja = new Medicamento { Cn = "333333", Nombre = "Omeprazol 20 mg", NombreNormalizado = "omeprazol 20 mg", AptoSpd = true, Activo = false };
        deBaja.Id = repositorio.Crear(deBaja);
        conexion.Execute(
            "INSERT INTO Medicamento_Hist (medicamento_id, desc_texto, vigente_desde, vigente_hasta) " +
            "VALUES (@id, 'jarabe rojo', '2026-01-01', '2026-06-01')", new { id = noApto.Id });

        // Las claves foráneas están activas: la reconstrucción no puede dejar huérfano nada que cuelgue
        // del medicamento.
        Assert.Equal(1, conexion.ExecuteScalar<int>("PRAGMA foreign_keys"));

        new AplicadorMigraciones(conexion).Aplicar();

        Assert.Empty(conexion.Query("PRAGMA foreign_key_check"));
        Assert.Equal(0, conexion.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM sqlite_master WHERE name = 'Medicamento_copia'"));

        Assert.True(conexion.ExecuteScalar<int>("SELECT MAX(version) FROM schema_version") > VersionAnterior);
        Assert.Equal(0, ObligatoriaAptitud(conexion));
        Assert.Equal(3, conexion.ExecuteScalar<int>("SELECT COUNT(*) FROM Medicamento"));

        // Mismos id, mismos valores: nada se reescribe.
        var aptoTras = repositorio.ObtenerPorId(apto.Id)!;
        var noAptoTras = repositorio.ObtenerPorId(noApto.Id)!;
        var deBajaTras = repositorio.ObtenerPorId(deBaja.Id)!;
        Assert.Equal("111111", aptoTras.Cn);
        Assert.True(aptoTras.AptoSpd);
        Assert.Equal("222222", noAptoTras.Cn);
        Assert.False(noAptoTras.AptoSpd);
        Assert.Equal("Forma líquida.", noAptoTras.MotivoNoApto);
        Assert.Equal("jarabe marrón", noAptoTras.DescTexto);
        Assert.False(deBajaTras.Activo);

        // La historia de la descripción física sigue colgando del mismo medicamento.
        Assert.Equal("jarabe rojo", conexion.ExecuteScalar<string>(
            "SELECT h.desc_texto FROM Medicamento_Hist h JOIN Medicamento m ON m.id = h.medicamento_id WHERE m.cn = '222222'"));

        // Lo nuevo nace sin confirmar, y el autoincremento no reutiliza ningún id.
        var nuevo = new Medicamento { Cn = "444444", Nombre = "Nuevo", NombreNormalizado = "nuevo" };
        nuevo.Id = repositorio.Crear(nuevo);
        Assert.True(nuevo.Id > deBaja.Id);
        Assert.Null(repositorio.ObtenerPorId(nuevo.Id)!.AptoSpd);

        // El CHECK sigue impidiendo cualquier valor que no sea 0, 1 o vacío.
        Assert.ThrowsAny<SqliteException>(() =>
            conexion.Execute("UPDATE Medicamento SET apto_spd = 2 WHERE id = @id", new { id = nuevo.Id }));
    }
}
