namespace Spd.Dominio;

/// <summary>Hash de contraseñas con Argon2id (Art. VII.1). Nunca en claro ni reversible.</summary>
public interface IHasheadorPassword
{
    string Hashear(string passwordEnClaro);
    bool Verificar(string passwordEnClaro, string hash);
}
