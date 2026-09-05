using Spd.Dominio;
using Xunit;

namespace Spd.Dominio.Tests;

public sealed class PacienteTests
{
    private static Paciente ConEstado(EstadoPaciente estado) => new()
    {
        NumFicha = "F-000001", Nombre = "José", Apellidos = "Núñez", DiaRetirada = "LU", Estado = estado
    };

    [Theory]
    [InlineData(EstadoPaciente.Evaluacion, EstadoPaciente.Activo)]
    [InlineData(EstadoPaciente.Activo, EstadoPaciente.Suspendido)]
    [InlineData(EstadoPaciente.Suspendido, EstadoPaciente.Activo)]
    [InlineData(EstadoPaciente.Baja, EstadoPaciente.Evaluacion)]
    [InlineData(EstadoPaciente.Evaluacion, EstadoPaciente.Baja)]
    [InlineData(EstadoPaciente.Activo, EstadoPaciente.Baja)]
    [InlineData(EstadoPaciente.Suspendido, EstadoPaciente.Baja)]
    public void TransicionValida_permite_las_transiciones_de_FR_006(EstadoPaciente desde, EstadoPaciente hasta)
        => Assert.True(ConEstado(desde).TransicionValida(hasta));

    [Theory]
    [InlineData(EstadoPaciente.Evaluacion, EstadoPaciente.Suspendido)]
    [InlineData(EstadoPaciente.Baja, EstadoPaciente.Activo)]
    [InlineData(EstadoPaciente.Baja, EstadoPaciente.Suspendido)]
    [InlineData(EstadoPaciente.Activo, EstadoPaciente.Evaluacion)]
    public void TransicionValida_rechaza_el_resto(EstadoPaciente desde, EstadoPaciente hasta)
        => Assert.False(ConEstado(desde).TransicionValida(hasta));
}
