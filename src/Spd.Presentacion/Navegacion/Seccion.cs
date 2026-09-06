using System.Collections.Generic;
namespace Spd.Presentacion.Navegacion;

/// <summary>Secciones del marco único (Spec 015 FR-1501). Cada una tiene su entrada en la
/// navegación, su ViewModel en <see cref="FabricaViewModels"/> y su apartado de ayuda.</summary>
public enum Seccion
{
    Inicio,
    Pacientes,
    Preparaciones,
    Retirada,
    Exportar,
    Catalogo,
    RevisionNomenclator,
    Calidad,
    ControlDocumental,
    Farmacia,
    Usuarios,
    Actualizaciones,
    Nomenclator,
    Seguridad,
    Perfiles,
    Ayuda
}

/// <summary>A dónde se navega. `Detalle` es el sub-destino: el apartado de ayuda en
/// <see cref="Seccion.Ayuda"/> y, desde la fase 3, la pestaña del paciente.</summary>
public sealed record Destino(Seccion Seccion, int? PacienteId = null, string? Detalle = null);

/// <summary>Una entrada del menú lateral.</summary>
public sealed record EntradaNavegacion(Seccion Seccion, string Titulo, string Grupo, bool SoloAdministrador)
{
    public const string GrupoTrabajo = "Trabajo diario";
    public const string GrupoAdministracion = "Administración";

    /// <summary>Las entradas visibles del menú, en orden. `RevisionNomenclator` y `Ayuda` son
    /// secciones navegables pero no entradas del menú: se llega a ellas desde el catálogo y desde
    /// F1 respectivamente.</summary>
    public static readonly IReadOnlyList<EntradaNavegacion> Todas =
    [
        new(Seccion.Inicio, "Inicio", GrupoTrabajo, false),
        new(Seccion.Pacientes, "Pacientes", GrupoTrabajo, false),
        new(Seccion.Preparaciones, "Preparaciones", GrupoTrabajo, false),
        new(Seccion.Retirada, "Retirada de envases", GrupoTrabajo, false),
        new(Seccion.Catalogo, "Catálogo de medicamentos", GrupoTrabajo, false),
        new(Seccion.Calidad, "Registros de calidad", GrupoTrabajo, false),
        new(Seccion.Exportar, "Exportar pacientes", GrupoTrabajo, false),
        new(Seccion.Farmacia, "Farmacia", GrupoAdministracion, true),
        new(Seccion.Usuarios, "Usuarios", GrupoAdministracion, true),
        new(Seccion.Seguridad, "Seguridad", GrupoAdministracion, true),
        new(Seccion.ControlDocumental, "Control documental", GrupoAdministracion, true),
        new(Seccion.Nomenclator, "Nomenclátor", GrupoAdministracion, true),
        new(Seccion.Perfiles, "Perfiles de importación", GrupoAdministracion, true),
        new(Seccion.Actualizaciones, "Actualizaciones", GrupoAdministracion, true),
    ];
}
