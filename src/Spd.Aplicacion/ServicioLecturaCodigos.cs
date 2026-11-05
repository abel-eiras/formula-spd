using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Interpreta escaneos con el separador sustituto configurado en Farmacia e identifica el
/// medicamento de la caja (spec-012 FR-1209/FR-1211). Sin repositorio de farmacia (pantallas
/// construidas en pruebas) funciona igual, solo que sin separador sustituto.</summary>
public sealed class ServicioLecturaCodigos(IServicioMedicamentos medicamentos, IRepositorioFarmacia? farmacia = null)
    : IServicioLecturaCodigos
{
    // Se lee cada vez: es una fila, y así un cambio en «Lector de códigos» vale para el siguiente
    // escaneo sin reiniciar ni reconstruir pantallas.
    public string? SeparadorSustituto => farmacia?.Obtener()?.SeparadorLector is { Length: > 0 } s ? s : null;

    public LecturaGs1 Interpretar(string? cadenaEscaneada)
        => LectorGs1DataMatrix.Interpretar(cadenaEscaneada, SeparadorSustituto);

    public Medicamento? IdentificarMedicamento(DatosEnvaseEscaneado datos)
    {
        foreach (var cn in datos.CodigosNacionalesCandidatos)
        {
            if (medicamentos.ObtenerPorCn(cn) is { } porCn) return porCn;
        }
        return medicamentos.ObtenerPorGtin(datos.Gtin);
    }
}
