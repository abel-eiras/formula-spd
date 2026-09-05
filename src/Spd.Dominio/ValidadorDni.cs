namespace Spd.Dominio;

/// <summary>Valida el formato y la letra de control de un DNI o NIE español (FR-005). Es un
/// aviso, nunca bloquea el guardado (research.md Decisión 4).</summary>
public static class ValidadorDni
{
    private const string Letras = "TRWAGMYFPDXBNJZSQVHLCKE";
    private const string PrefijosNie = "XYZ";

    public static bool EsValido(string dni)
    {
        if (string.IsNullOrWhiteSpace(dni)) return false;
        var texto = dni.Trim().ToUpperInvariant();
        if (texto.Length != 9) return false;

        string digitos;
        if (char.IsDigit(texto[0]))
        {
            digitos = texto[..8];
        }
        else if (PrefijosNie.Contains(texto[0]))
        {
            digitos = PrefijosNie.IndexOf(texto[0]) + texto[1..8];
        }
        else
        {
            return false;
        }

        if (digitos.Length != 8 || !digitos.All(char.IsDigit)) return false;

        var letraEsperada = Letras[int.Parse(digitos) % 23];
        return texto[8] == letraEsperada;
    }
}
