using Spd.Dominio;
using Xunit;

namespace Spd.Dominio.Tests;

/// <summary>Spec 002 FR-201/204 (Art. IX.1): la propuesta de resultado y la exigencia de
/// observaciones son reglas puras de Dominio.</summary>
public sealed class EvaluacionIdoneidadTests
{
    private static EvaluacionIdoneidad Nueva(bool criterio, bool motivacion, bool destreza, ResultadoIdoneidad resultado, string? observaciones = null)
        => new()
        {
            PacienteId = 1, Criterio1 = criterio, CondicionMotivacion = motivacion, CondicionDestreza = destreza,
            Resultado = resultado, Observaciones = observaciones
        };

    [Fact]
    public void Propuesta_es_APTO_con_algun_criterio_y_ambas_condiciones()
        => Assert.Equal(ResultadoIdoneidad.Apto, Nueva(true, true, true, ResultadoIdoneidad.Apto).ResultadoPropuesto());

    [Theory]
    [InlineData(false, true, true)]   // ningún criterio de inclusión
    [InlineData(true, false, true)]   // sin motivación
    [InlineData(true, true, false)]   // sin destreza / agudeza visual
    public void Propuesta_es_NO_APTO_si_falta_un_criterio_o_una_condicion(bool criterio, bool motivacion, bool destreza)
        => Assert.Equal(ResultadoIdoneidad.NoApto, Nueva(criterio, motivacion, destreza, ResultadoIdoneidad.Apto).ResultadoPropuesto());

    [Fact]
    public void NO_APTO_requiere_observaciones_FR_204()
        => Assert.True(Nueva(true, true, true, ResultadoIdoneidad.NoApto).RequiereObservaciones);

    [Fact]
    public void APTO_que_contradice_la_propuesta_requiere_observaciones_FR_204()
        => Assert.True(Nueva(true, false, true, ResultadoIdoneidad.Apto).RequiereObservaciones);

    [Fact]
    public void APTO_coincidente_con_la_propuesta_no_requiere_observaciones()
        => Assert.False(Nueva(true, true, true, ResultadoIdoneidad.Apto).RequiereObservaciones);

    [Fact]
    public void Consentimiento_es_vigente_solo_firmado_y_no_revocado()
    {
        var c = new Consentimiento { PacienteId = 1 };
        Assert.False(c.Vigente);
        c.FechaFirma = new DateOnly(2026, 1, 1);
        Assert.True(c.Vigente);
        c.FechaRevocacion = new DateOnly(2026, 6, 1);
        Assert.False(c.Vigente);
    }

    [Fact]
    public void Textos_de_criterios_y_condiciones_son_los_del_PNT_I()
    {
        Assert.Equal(7, EvaluacionIdoneidad.TextosCriterios.Length);
        Assert.Equal(2, EvaluacionIdoneidad.TextosCondiciones.Length);
        Assert.Contains("polimedicado", EvaluacionIdoneidad.TextosCriterios[0]);
        Assert.Contains("destreza manual", EvaluacionIdoneidad.TextosCondiciones[1]);
    }
}
