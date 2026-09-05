namespace Spd.Aplicacion;

/// <summary>Cifrado completo de la base de datos con SQLCipher (Art. VII.2/3, FR-1010..1014).
/// `EstaActivo` nunca lee un flag guardado — siempre comprueba abriendo la base de verdad
/// (research.md Decisión 2 de Spec 010).</summary>
public interface IServicioCifrado
{
    bool EstaActivo();

    /// <summary>Genera una MEK y una frase de recuperación nuevas, sin escribir nada todavía
    /// (FR-1010/FR-1012, CA-1003: hay que mostrar la frase y confirmar que se ha guardado antes de
    /// que `ActivarCifrado`/`CambiarContrasenaMaestra` toquen la base de datos de verdad).</summary>
    (byte[] Mek, string[] FraseRecuperacion) GenerarClaveYFraseNuevas();

    ResultadoActivarCifrado ActivarCifrado(
        byte[] mek, string[] fraseRecuperacion, string contrasenaMaestra, int administradorQueEjecutaId);

    ResultadoOperacionCifrado DesactivarCifrado(string contrasenaMaestraActual, int administradorQueEjecutaId);

    ResultadoCambioContrasena CambiarContrasenaMaestra(
        string contrasenaActual, string contrasenaNueva, byte[] mekNueva, string[] fraseRecuperacionNueva,
        int administradorQueEjecutaId);

    /// <summary>Desenvuelve la MEK a partir de la contraseña maestra o, si <paramref
    /// name="esFraseDeRecuperacion"/>, de la frase de 24 palabras. Devuelve null si el secreto no
    /// es correcto (FR-1013: se pide una vez por sesión, nunca se persiste el resultado).</summary>
    byte[]? DesenvolverMek(string secreto, bool esFraseDeRecuperacion);
}
