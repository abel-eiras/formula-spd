namespace Spd.Dominio;

/// <summary>Fecha de caducidad tal como viene codificada en el AI (17) de un DataMatrix GS1: AAMMDD
/// (spec-012 FR-1201). <c>Dia</c> puede venir a 0 cuando el fabricante no especifica dia del mes
/// (convencion habitual de GS1 para ese AI); <see cref="ComoFecha"/> interpreta ese caso como el
/// ultimo dia del mes (practica habitual del sector farmaceutico para no descartar existencias
/// validas por falta de precision) — el campo de caducidad en pantalla sigue siendo editable
/// (FR-1202), asi que un error en esta interpretacion se corrige a mano antes de guardar.</summary>
public sealed record FechaCaducidadGs1(int Anio, int Mes, int Dia)
{
    public bool DiaSinEspecificar => Dia == 0;

    /// <summary>Un AAMMDD solo es una fecha si el mes existe y el día cabe en él (00 incluido). Sirve para
    /// descartar descomposiciones que se han comido otro campo (FR-1201).</summary>
    public bool EsValida => Mes is >= 1 and <= 12 && Dia >= 0 && Dia <= DateTime.DaysInMonth(Anio, Mes);

    public DateOnly ComoFecha() =>
        DiaSinEspecificar
            ? new DateOnly(Anio, Mes, DateTime.DaysInMonth(Anio, Mes))
            : new DateOnly(Anio, Mes, Dia);
}

/// <summary>Datos extraidos de un DataMatrix GS1 de un envase de medicamento: GTIN, lote, caducidad,
/// numero de serie y, si el fabricante lo incluye (AI 712), el Codigo Nacional espanol directamente
/// (spec-012). Cuando el AI 712 falta, el CN se deduce del GTIN español (FR-1209).</summary>
public sealed record DatosEnvaseEscaneado(
    string Gtin,
    string Lote,
    FechaCaducidadGs1 Caducidad,
    string NumeroSerie,
    string? CodigoNacional)
{
    /// <summary>CN de seis dígitos que identifica la caja: el del AI 712 (sin su dígito de control, si lo
    /// trae) y, si falta, el deducido del GTIN español. Null solo para cajas extranjeras sin AI 712.</summary>
    public string? CnIdentificado => CodigoNacionalEspanol.SinDigitoDeControl(CodigoNacional)
                                     ?? CodigoNacionalEspanol.DesdeGtin(Gtin);

    /// <summary>Formas de CN con las que buscar en el catálogo, por orden de preferencia (FR-1209).</summary>
    public IReadOnlyList<string> CodigosNacionalesCandidatos => CodigoNacionalEspanol.Candidatos(CodigoNacional, Gtin);
}
