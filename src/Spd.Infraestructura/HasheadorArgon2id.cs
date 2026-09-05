using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using Spd.Dominio;

namespace Spd.Infraestructura;

/// <summary>Hash de contraseñas con Argon2id (Art. VII.1), vía Konscious.Security.Cryptography (research.md Decisión 2).</summary>
public sealed class HasheadorArgon2id : IHasheadorPassword
{
    private const int TamanoSal = 16;
    private const int TamanoHash = 32;
    private const int Iteraciones = 4;
    private const int MemoriaKb = 65536;
    private const int Paralelismo = 2;

    public string Hashear(string passwordEnClaro)
    {
        var sal = RandomNumberGenerator.GetBytes(TamanoSal);
        var hash = CalcularHash(passwordEnClaro, sal);
        return $"{Convert.ToBase64String(sal)}:{Convert.ToBase64String(hash)}";
    }

    public bool Verificar(string passwordEnClaro, string hash)
    {
        var partes = hash.Split(':');
        if (partes.Length != 2) return false;

        var sal = Convert.FromBase64String(partes[0]);
        var hashEsperado = Convert.FromBase64String(partes[1]);
        var hashCalculado = CalcularHash(passwordEnClaro, sal);
        return CryptographicOperations.FixedTimeEquals(hashCalculado, hashEsperado);
    }

    private static byte[] CalcularHash(string passwordEnClaro, byte[] sal)
    {
        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(passwordEnClaro))
        {
            Salt = sal,
            DegreeOfParallelism = Paralelismo,
            Iterations = Iteraciones,
            MemorySize = MemoriaKb
        };
        return argon2.GetBytes(TamanoHash);
    }
}
