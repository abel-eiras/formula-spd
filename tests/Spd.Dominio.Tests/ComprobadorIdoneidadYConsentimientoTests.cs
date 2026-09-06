using Spd.Dominio;
using Xunit;

namespace Spd.Dominio.Tests;

/// <summary>Documenta el punto de extensión de Art. I.3 ("no se prepara sin consentimiento
/// vigente y evaluación de idoneidad APTO") hacia Spec 002, que no existe en esta rama
/// (research.md Decisión 1 de Spec 006).</summary>
public sealed class ComprobadorIdoneidadYConsentimientoTests
{
    [Fact]
    public void ComprobadorNulo_siempre_aprueba_mientras_Spec_002_no_exista()
    {
        var comprobador = new ComprobadorIdoneidadYConsentimientoNulo();

        Assert.True(comprobador.Aprobado(pacienteId: 1));
        Assert.True(comprobador.Aprobado(pacienteId: 999));
    }
}
