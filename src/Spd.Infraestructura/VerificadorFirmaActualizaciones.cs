using System.Security.Cryptography;

namespace Spd.Infraestructura;

/// <summary>Comprueba que un paquete de actualización es el que publicó el proyecto y que llegó entero
/// (FR-053).
///
/// La cadena de confianza tiene dos eslabones: la CI firma `SHA256SUMS.txt` con una clave privada que solo
/// existe como secreto de GitHub, y ese fichero lista la suma SHA-256 de cada paquete. Con la clave pública
/// embebida aquí, la aplicación sabe que las sumas son auténticas; con la suma, que el paquete también. Sin
/// esto, cualquiera capaz de interponerse en la descarga podría sustituir el programa de una farmacia.</summary>
public static class VerificadorFirmaActualizaciones
{
    /// <summary>Pública, ECDSA P-256 (copia de `publicar/clave-publica-actualizaciones.pem`). Cambiarla deja
    /// a todas las instalaciones anteriores sin poder actualizarse solas: la siguiente sería manual.</summary>
    public const string ClavePublicaOficial = """
        -----BEGIN PUBLIC KEY-----
        MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEbwrO0aI8wII8WkqaLH+/RaSHa31C
        2VyqqSOfc2KjxcLPF7/UvqPQJlU877ncuy4vd81teGZ3Qh2xMk43kcEFeg==
        -----END PUBLIC KEY-----
        """;

    /// <summary>La firma es la que genera `openssl dgst -sha256 -sign` (DER), de ahí el formato RFC 3279.
    /// Una firma malformada es una firma inválida, no un error del programa.</summary>
    public static bool FirmaValida(byte[] datos, byte[] firma, string clavePublicaPem)
    {
        using var clave = ECDsa.Create();
        clave.ImportFromPem(clavePublicaPem);
        try
        {
            return clave.VerifyData(datos, firma, HashAlgorithmName.SHA256, DSASignatureFormat.Rfc3279DerSequence);
        }
        catch (CryptographicException)
        {
            return false;
        }
    }

    /// <summary>La suma de <paramref name="nombreFichero"/> en un `SHA256SUMS.txt` con el formato de
    /// `sha256sum` (`suma  nombre`, o `suma *nombre` en modo binario); <c>null</c> si no aparece.</summary>
    public static string? SumaDe(string contenidoSumas, string nombreFichero)
    {
        foreach (var linea in contenidoSumas.Split('\n'))
        {
            var partes = linea.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            if (partes.Length == 2 && partes[1].Trim().TrimStart('*') == nombreFichero) return partes[0];
        }
        return null;
    }

    public static string Sha256DeFichero(string ruta)
    {
        using var fichero = File.OpenRead(ruta);
        return Convert.ToHexString(SHA256.HashData(fichero));
    }
}
