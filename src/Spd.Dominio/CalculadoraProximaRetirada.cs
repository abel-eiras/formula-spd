namespace Spd.Dominio;

/// <summary>Fecha de la próxima retirada de un paciente (FR-502): el primer `dia_retirada`
/// posterior a la validez del último SPD, o el primero a partir de hoy si no tiene ninguno. Se
/// calcula, no se almacena.</summary>
public static class CalculadoraProximaRetirada
{
    public static DateOnly Calcular(string diaRetirada, DateOnly? ultimaValidezHasta, DateOnly hoy)
    {
        var diaObjetivo = ACodigoDiaSemana(diaRetirada);

        var candidato = ultimaValidezHasta is DateOnly ultima ? ultima.AddDays(1) : hoy;
        while (candidato.DayOfWeek != diaObjetivo) candidato = candidato.AddDays(1);
        return candidato;
    }

    private static DayOfWeek ACodigoDiaSemana(string codigo) => codigo switch
    {
        "LU" => DayOfWeek.Monday,
        "MA" => DayOfWeek.Tuesday,
        "MI" => DayOfWeek.Wednesday,
        "JU" => DayOfWeek.Thursday,
        "VI" => DayOfWeek.Friday,
        "SA" => DayOfWeek.Saturday,
        "DO" => DayOfWeek.Sunday,
        _ => throw new ArgumentOutOfRangeException(nameof(codigo), codigo, "Código de día de la semana no reconocido.")
    };
}
