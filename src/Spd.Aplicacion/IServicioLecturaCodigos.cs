using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Lectura de los códigos de las cajas con la configuración del lector de esta farmacia
/// (spec-012). Reúne en un sitio lo que antes repetía cada pantalla: interpretar el escaneo con el
/// separador sustituto configurado (FR-1211) e identificar el medicamento por CN o GTIN (FR-1209).</summary>
public interface IServicioLecturaCodigos
{
    /// <summary>Separador que el lector manda en lugar del GS, si se ha configurado.</summary>
    string? SeparadorSustituto { get; }

    LecturaGs1 Interpretar(string? cadenaEscaneada);

    /// <summary>Medicamento del catálogo de una caja escaneada: primero por las formas del CN (AI 712,
    /// sin su dígito de control, deducido del GTIN español) y, si ninguna está, por el GTIN guardado.</summary>
    Medicamento? IdentificarMedicamento(DatosEnvaseEscaneado datos);
}
