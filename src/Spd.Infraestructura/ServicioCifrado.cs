using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dapper;
using Konscious.Security.Cryptography;
using Microsoft.Data.Sqlite;
using Spd.Aplicacion;
using Spd.Dominio;

namespace Spd.Infraestructura;

/// <summary>Cifrado completo de la base de datos con SQLCipher (Art. VII.2/3, FR-1010..1014,
/// research.md de Spec 010). `conexion` debe ser la conexión única y persistente de la
/// aplicación, abierta con `Pooling=False` (Decisión 1, corolario) — este servicio la cierra y
/// reabre a través de `ActivarCifrado`/`DesactivarCifrado`.</summary>
public sealed class ServicioCifrado(
    SqliteConnection conexion, string rutaBaseDeDatos, string carpetaInstalacion,
    IRegistradorAuditoria auditoria, GeneradorFraseRecuperacion generadorFrase)
    : IServicioCifrado
{
    private const int TamanoMekBytes = 32; // 256 bits
    private const int TamanoSalBytes = 16;
    private const int Argon2Iteraciones = 4;
    private const int Argon2MemoriaKb = 65536;
    private const int Argon2Paralelismo = 2;

    private string RutaConfig => Path.Combine(carpetaInstalacion, "config.json");

    public bool EstaActivo()
    {
        using var prueba = new SqliteConnection($"Data Source={rutaBaseDeDatos};Pooling=False");
        try
        {
            prueba.Open();
            prueba.ExecuteScalar<long>("SELECT count(*) FROM sqlite_master;");
            return false;
        }
        catch (SqliteException)
        {
            return true;
        }
    }

    // FR-1010/CA-1003: la clave de recuperación se genera y se muestra ANTES de tocar la base de
    // datos; `ActivarCifrado` no se llama hasta que el administrador confirma explícitamente
    // haberla impreso o guardado — la Presentación es quien retiene `mek`/`frase` entre medias
    // (en memoria, nunca persistidas hasta la confirmación).
    public (byte[] Mek, string[] FraseRecuperacion) GenerarClaveYFraseNuevas()
        => (RandomNumberGenerator.GetBytes(TamanoMekBytes), generadorFrase.GenerarFrase());

    public ResultadoActivarCifrado ActivarCifrado(
        byte[] mek, string[] frase, string contrasenaMaestra, int administradorQueEjecutaId)
    {
        if (EstaActivo())
        {
            return ResultadoActivarCifrado.Fallido("El cifrado ya está activo.");
        }

        var mekHex = Convert.ToHexString(mek);

        var rutaCifradaTemporal = rutaBaseDeDatos + ".cifrada.tmp";
        BorrarSiExiste(rutaCifradaTemporal);

        conexion.Execute($"ATTACH DATABASE '{EscaparRuta(rutaCifradaTemporal)}' AS cifrada KEY \"x'{mekHex}'\";");
        conexion.Execute("SELECT sqlcipher_export('cifrada');");
        conexion.Execute("DETACH DATABASE cifrada;");
        conexion.Close();

        File.Copy(rutaBaseDeDatos, rutaBaseDeDatos + ".preclave.bak", overwrite: true);
        File.Move(rutaCifradaTemporal, rutaBaseDeDatos, overwrite: true);

        conexion.Open();
        conexion.Execute($"PRAGMA key = \"x'{mekHex}'\";");

        GuardarSobres(EnvolverMek(mek, contrasenaMaestra), EnvolverMek(mek, TextoDeFrase(frase)));

        auditoria.Registrar(administradorQueEjecutaId, "ACTIVAR_CIFRADO", "Sistema", null, null);
        return ResultadoActivarCifrado.Exitoso(frase);
    }

    public ResultadoOperacionCifrado DesactivarCifrado(string contrasenaMaestraActual, int administradorQueEjecutaId)
    {
        var mek = DesenvolverMek(contrasenaMaestraActual, esFraseDeRecuperacion: false);
        if (mek is null)
        {
            return ResultadoOperacionCifrado.Fallido("La contraseña maestra actual no es correcta.");
        }

        var rutaPlanaTemporal = rutaBaseDeDatos + ".plana.tmp";
        BorrarSiExiste(rutaPlanaTemporal);

        conexion.Execute($"ATTACH DATABASE '{EscaparRuta(rutaPlanaTemporal)}' AS plana KEY '';");
        conexion.Execute("SELECT sqlcipher_export('plana');");
        conexion.Execute("DETACH DATABASE plana;");
        conexion.Close();

        File.Copy(rutaBaseDeDatos, rutaBaseDeDatos + ".precifrado.bak", overwrite: true);
        File.Move(rutaPlanaTemporal, rutaBaseDeDatos, overwrite: true);

        conexion.Open();

        BorrarSobres();
        auditoria.Registrar(administradorQueEjecutaId, "DESACTIVAR_CIFRADO", "Sistema", null, null);
        return ResultadoOperacionCifrado.Exitoso();
    }

    // FR-1012/CA-1003 (misma exigencia de confirmación que FR-1010): la MEK y frase nuevas se
    // generan con GenerarClaveYFraseNuevas() y se muestran antes de llamar a este método.
    public ResultadoCambioContrasena CambiarContrasenaMaestra(
        string contrasenaActual, string contrasenaNueva, byte[] mekNueva, string[] fraseNueva, int administradorQueEjecutaId)
    {
        if (DesenvolverMek(contrasenaActual, esFraseDeRecuperacion: false) is null)
        {
            return ResultadoCambioContrasena.Fallido("La contraseña maestra actual no es correcta.");
        }

        // FR-1012: MEK nueva + PRAGMA rekey (en la conexión ya abierta y autenticada), sin
        // exportar/reimportar nada — a diferencia de Activar/DesactivarCifrado, que sí cambian de
        // modo (sin cifrar ↔ cifrado) y por eso necesitan sqlcipher_export (research.md Decisión 1).
        conexion.Execute($"PRAGMA rekey = \"x'{Convert.ToHexString(mekNueva)}'\";");

        GuardarSobres(EnvolverMek(mekNueva, contrasenaNueva), EnvolverMek(mekNueva, TextoDeFrase(fraseNueva)));

        auditoria.Registrar(administradorQueEjecutaId, "CAMBIAR_CONTRASENA_MAESTRA", "Sistema", null, null);
        return ResultadoCambioContrasena.Exitoso(fraseNueva);
    }

    public byte[]? DesenvolverMek(string secreto, bool esFraseDeRecuperacion)
    {
        var sobres = LeerSobres();
        if (sobres is null)
        {
            return null;
        }
        var sobre = esFraseDeRecuperacion ? sobres.Value.Recuperacion : sobres.Value.Contrasena;
        return Desenvolver(sobre, secreto);
    }

    private static string TextoDeFrase(string[] frase) => string.Join(' ', frase);

    private static void BorrarSiExiste(string ruta)
    {
        if (File.Exists(ruta)) File.Delete(ruta);
    }

    private static string EscaparRuta(string ruta) => ruta.Replace("'", "''");

    // --- cifrado de sobre: MEK envuelta con AES-256-GCM bajo una clave derivada por Argon2id de
    // un secreto humano (research.md Decisión 3) ---

    private static SobreClave EnvolverMek(byte[] mek, string secreto)
    {
        var sal = RandomNumberGenerator.GetBytes(TamanoSalBytes);
        var kek = DerivarKek(secreto, sal);
        var nonce = new byte[AesGcm.NonceByteSizes.MaxSize];
        RandomNumberGenerator.Fill(nonce);
        var textoCifrado = new byte[mek.Length];
        var tag = new byte[AesGcm.TagByteSizes.MaxSize];
        using var aesGcm = new AesGcm(kek, tag.Length);
        aesGcm.Encrypt(nonce, mek, textoCifrado, tag);
        return new SobreClave(sal, nonce, textoCifrado, tag);
    }

    private static byte[]? Desenvolver(SobreClave sobre, string secreto)
    {
        try
        {
            var kek = DerivarKek(secreto, sobre.SalArgon2id);
            var mek = new byte[sobre.TextoCifradoMek.Length];
            using var aesGcm = new AesGcm(kek, sobre.TagAesGcm.Length);
            aesGcm.Decrypt(sobre.NonceAesGcm, sobre.TextoCifradoMek, sobre.TagAesGcm, mek);
            return mek;
        }
        catch (CryptographicException)
        {
            // El tag de GCM no valida: secreto incorrecto. Un fallo esperable, no un bug.
            return null;
        }
    }

    // Mismos parámetros que HasheadorArgon2id (Art. X.2: reutilizar el KDF ya elegido para
    // contraseñas de usuario en vez de introducir uno nuevo para este otro uso).
    private static byte[] DerivarKek(string secreto, byte[] sal)
    {
        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(secreto))
        {
            Salt = sal,
            DegreeOfParallelism = Argon2Paralelismo,
            Iterations = Argon2Iteraciones,
            MemorySize = Argon2MemoriaKb
        };
        return argon2.GetBytes(32);
    }

    // --- persistencia de los dos sobres en config.json (Art. VI.4) ---

    private sealed record SobreJson(string Sal, string Nonce, string Texto, string Tag)
    {
        public static SobreJson DeSobre(SobreClave s) => new(
            Convert.ToBase64String(s.SalArgon2id), Convert.ToBase64String(s.NonceAesGcm),
            Convert.ToBase64String(s.TextoCifradoMek), Convert.ToBase64String(s.TagAesGcm));

        public SobreClave ASobre() => new(
            Convert.FromBase64String(Sal), Convert.FromBase64String(Nonce),
            Convert.FromBase64String(Texto), Convert.FromBase64String(Tag));
    }

    private sealed record CifradoJson(
        [property: JsonPropertyName("sobreContrasena")] SobreJson SobreContrasena,
        [property: JsonPropertyName("sobreRecuperacion")] SobreJson SobreRecuperacion);

    private sealed record ConfigJson([property: JsonPropertyName("cifrado")] CifradoJson? Cifrado);

    private (SobreClave Contrasena, SobreClave Recuperacion)? LeerSobres()
    {
        if (!File.Exists(RutaConfig)) return null;
        var config = JsonSerializer.Deserialize<ConfigJson>(File.ReadAllText(RutaConfig));
        if (config?.Cifrado is null) return null;
        return (config.Cifrado.SobreContrasena.ASobre(), config.Cifrado.SobreRecuperacion.ASobre());
    }

    private void GuardarSobres(SobreClave contrasena, SobreClave recuperacion)
    {
        var config = new ConfigJson(new CifradoJson(SobreJson.DeSobre(contrasena), SobreJson.DeSobre(recuperacion)));
        File.WriteAllText(RutaConfig, JsonSerializer.Serialize(config));
    }

    private void BorrarSobres()
    {
        if (File.Exists(RutaConfig)) File.WriteAllText(RutaConfig, JsonSerializer.Serialize(new ConfigJson(null)));
    }
}
