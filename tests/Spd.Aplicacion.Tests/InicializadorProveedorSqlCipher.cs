using System.Runtime.CompilerServices;

namespace Spd.Aplicacion.Tests;

/// <summary>El proceso de tests nunca pasa por `Program.Main` (donde la app real registra el
/// proveedor SQLCipher, research.md Decisión 1 de Spec 010), así que hace falta un punto de
/// entrada equivalente para que `ServicioCifradoTests` pueda usar `ATTACH .. KEY`/`PRAGMA rekey`/
/// `sqlcipher_export` de verdad. Se ejecuta una única vez al cargar este ensamblado, antes de
/// cualquier test.</summary>
internal static class InicializadorProveedorSqlCipher
{
    [ModuleInitializer]
    public static void Inicializar() => SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_e_sqlcipher());
}
