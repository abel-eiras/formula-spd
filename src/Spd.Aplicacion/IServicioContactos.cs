using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Datos de alta o edición de un contacto del paciente (FR-020).</summary>
public sealed record DatosContacto(
    TipoContacto Tipo,
    string Nombre,
    string Apellidos,
    string? Dni = null,
    string? Telefono = null,
    string? Email = null,
    bool EsPrincipal = false,
    bool RetiraMedicacion = false);

/// <summary>Los contactos de un paciente (Spec 001 US3, FR-020..FR-023): familiar próximo, cuidador,
/// representante legal, persona autorizada, y quién retira la medicación.
///
/// No es un adorno de la ficha: el contacto principal es el "Familiar próximo" que se imprime en el
/// Anexo, y el marcado como "retira la medicación" es de quien el listado de retirada toma el DNI
/// (Spec 005 FR-531). Sin esta pantalla esa columna sale siempre en blanco.</summary>
public interface IServicioContactos
{
    IReadOnlyList<Contacto> ListarDePaciente(int pacienteId, bool incluirBaja = false);

    Contacto Crear(int pacienteId, DatosContacto datos, int? usuarioQueEjecutaId);

    Contacto Actualizar(int contactoId, DatosContacto datos, int? usuarioQueEjecutaId);

    /// <summary>FR-023 y Art. III.1: baja lógica. Un contacto no se elimina nunca.</summary>
    void DarDeBaja(int contactoId, int? usuarioQueEjecutaId);
}
