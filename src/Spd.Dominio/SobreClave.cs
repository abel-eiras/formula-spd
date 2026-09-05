namespace Spd.Dominio;

/// <summary>La clave maestra de cifrado (MEK) envuelta con AES-256-GCM bajo una clave derivada de
/// un secreto humano (contraseña o frase de recuperación) — cifrado de sobre, research.md
/// Decisión 3 de Spec 010. Vive dentro de `config.json`, nunca en la base de datos que protege
/// (Art. VI.4/VII.2).</summary>
public sealed record SobreClave(byte[] SalArgon2id, byte[] NonceAesGcm, byte[] TextoCifradoMek, byte[] TagAesGcm);
