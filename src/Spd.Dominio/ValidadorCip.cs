using System.Globalization;

namespace Spd.Dominio;

/// <summary>Construye y valida el CIP gallego de 14 caracteres (FR-005/FR-005b). Las posiciones
/// 12-14 no son calculables (asignadas por el sistema sanitario) y nunca se validan. Es un aviso,
/// nunca bloquea el guardado (research.md Decisión 5).</summary>
public static class ValidadorCip
{
    private const char DigitoMujer = '0';
    private const char DigitoHombre = '1';

    /// <summary>Propone las posiciones 1-11 a partir de fecha de nacimiento, apellidos y sexo
    /// (FR-005b); las posiciones 12-14 quedan en blanco para completar a mano.</summary>
    public static string Autocompletar(DateOnly fechaNacimiento, string apellidos, string sexo)
    {
        var fecha = fechaNacimiento.ToString("yyMMdd", CultureInfo.InvariantCulture);
        var (primerApellido, segundoApellido) = SepararApellidos(apellidos);
        var iniciales = $"{PrimeraLetra(primerApellido)}{PrimeraLetra(segundoApellido)}";
        var segundasLetras = $"{SegundaLetra(primerApellido)}{SegundaLetra(segundoApellido)}";
        var digitoSexo = EsHombre(sexo) ? DigitoHombre : DigitoMujer;
        return $"{fecha}{iniciales}{segundasLetras}{digitoSexo}".ToUpperInvariant();
    }

    /// <summary>Compara las posiciones 1-11 del CIP informado con las que le corresponderían al
    /// paciente (CA-014/CA-015). Formato o correspondencia inválidos = aviso, no bloqueo: apellidos
    /// compuestos, con partícula o de un solo apellido pueden no ajustarse a la regla estándar.</summary>
    public static bool Corresponde(string cip, DateOnly fechaNacimiento, string apellidos, string sexo)
    {
        if (string.IsNullOrWhiteSpace(cip) || cip.Length != 14) return false;
        var esperado = Autocompletar(fechaNacimiento, apellidos, sexo);
        return cip[..11].Equals(esperado, StringComparison.OrdinalIgnoreCase);
    }

    private static (string Primero, string Segundo) SepararApellidos(string apellidos)
    {
        var partes = Normalizador.QuitarTildesYMayusculas(apellidos)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return (partes.Length > 0 ? partes[0] : string.Empty, partes.Length > 1 ? partes[1] : string.Empty);
    }

    private static char PrimeraLetra(string palabra) => palabra.Length > 0 ? palabra[0] : 'X';

    private static char SegundaLetra(string palabra) => palabra.Length > 1 ? palabra[1] : 'X';

    private static bool EsHombre(string sexo) => sexo.Equals("H", StringComparison.OrdinalIgnoreCase);
}
