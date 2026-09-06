using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Contactos del paciente (Spec 001 US3). Todas las reglas de exclusividad se aplican aquí
/// y no en la interfaz: son invariantes de dominio (Art. I.3), y una pantalla no es sitio donde
/// dejarlas.</summary>
public sealed class ServicioContactos(
    IRepositorioContactos repositorio,
    IRepositorioPacientes repositorioPacientes,
    IRegistradorAuditoria auditoria)
    : IServicioContactos
{
    public IReadOnlyList<Contacto> ListarDePaciente(int pacienteId, bool incluirBaja = false)
        => repositorio.ListarDePaciente(pacienteId, incluirBaja);

    public Contacto Crear(int pacienteId, DatosContacto datos, int? usuarioQueEjecutaId)
    {
        _ = repositorioPacientes.ObtenerPorId(pacienteId)
            ?? throw new ErrorValidacionException($"No existe el paciente {pacienteId}.");

        Validar(datos);

        var contacto = new Contacto
        {
            PacienteId = pacienteId,
            Tipo = datos.Tipo,
            Nombre = datos.Nombre.Trim(),
            Apellidos = datos.Apellidos.Trim(),
            Dni = Limpiar(datos.Dni),
            Telefono = Limpiar(datos.Telefono),
            Email = Limpiar(datos.Email),
            EsPrincipal = datos.EsPrincipal,
            RetiraMedicacion = datos.RetiraMedicacion,
            Activo = true
        };

        contacto.Id = repositorio.Crear(contacto);
        AplicarExclusividad(contacto);

        auditoria.Registrar(usuarioQueEjecutaId, "CREAR", "Contacto", contacto.Id,
            $"Paciente {pacienteId}: {contacto.Tipo} {contacto.Apellidos}, {contacto.Nombre}");

        return contacto;
    }

    public Contacto Actualizar(int contactoId, DatosContacto datos, int? usuarioQueEjecutaId)
    {
        var contacto = repositorio.ObtenerPorId(contactoId)
            ?? throw new ErrorValidacionException($"No existe el contacto {contactoId}.");

        Validar(datos);

        contacto.Tipo = datos.Tipo;
        contacto.Nombre = datos.Nombre.Trim();
        contacto.Apellidos = datos.Apellidos.Trim();
        contacto.Dni = Limpiar(datos.Dni);
        contacto.Telefono = Limpiar(datos.Telefono);
        contacto.Email = Limpiar(datos.Email);
        contacto.EsPrincipal = datos.EsPrincipal;
        contacto.RetiraMedicacion = datos.RetiraMedicacion;

        repositorio.Actualizar(contacto);
        AplicarExclusividad(contacto);

        auditoria.Registrar(usuarioQueEjecutaId, "MODIFICAR", "Contacto", contacto.Id,
            $"Paciente {contacto.PacienteId}: {contacto.Apellidos}, {contacto.Nombre}");

        return contacto;
    }

    public void DarDeBaja(int contactoId, int? usuarioQueEjecutaId)
    {
        var contacto = repositorio.ObtenerPorId(contactoId)
            ?? throw new ErrorValidacionException($"No existe el contacto {contactoId}.");

        contacto.Activo = false;
        contacto.FechaBaja = DateTime.UtcNow;
        // Un contacto de baja no puede seguir siendo el principal ni el que retira: si lo siguiera
        // siendo, el Anexo imprimiría a alguien que ya no está y el listado de retirada tomaría su
        // DNI. Se le quitan las dos marcas al darlo de baja.
        contacto.EsPrincipal = false;
        contacto.RetiraMedicacion = false;
        repositorio.Actualizar(contacto);

        auditoria.Registrar(usuarioQueEjecutaId, "BAJA", "Contacto", contacto.Id,
            $"Paciente {contacto.PacienteId}: {contacto.Apellidos}, {contacto.Nombre}");
    }

    /// <summary>FR-021 y FR-021b: como mucho un principal y como mucho uno que retire, por paciente.
    /// Se resuelve desmarcando a los demás en vez de rechazar la operación: marcar a otro es
    /// exactamente la forma de cambiar quién lo es, y hacer que falle obligaría a dos pasos.</summary>
    private void AplicarExclusividad(Contacto contacto)
    {
        if (!contacto.EsPrincipal && !contacto.RetiraMedicacion) return;

        foreach (var otro in repositorio.ListarDePaciente(contacto.PacienteId, incluirBaja: false))
        {
            if (otro.Id == contacto.Id) continue;

            var cambiado = false;
            if (contacto.EsPrincipal && otro.EsPrincipal) { otro.EsPrincipal = false; cambiado = true; }
            if (contacto.RetiraMedicacion && otro.RetiraMedicacion) { otro.RetiraMedicacion = false; cambiado = true; }
            if (cambiado) repositorio.Actualizar(otro);
        }
    }

    private static void Validar(DatosContacto datos)
    {
        if (string.IsNullOrWhiteSpace(datos.Nombre) || string.IsNullOrWhiteSpace(datos.Apellidos))
            throw new ErrorValidacionException("Nombre y apellidos del contacto son obligatorios (FR-020).");

        var sinDni = string.IsNullOrWhiteSpace(datos.Dni);

        // FR-022: el DNI del representante legal o de la persona autorizada va en el Anexo 1b.
        if (sinDni && datos.Tipo is TipoContacto.RepresentanteLegal or TipoContacto.PersonaAutorizada)
            throw new ErrorValidacionException(
                "Un representante legal o una persona autorizada necesita DNI (FR-022): es el dato que se imprime en el consentimiento.");

        // FR-021c: quien retira la medicación se identifica con DNI en el listado de retirada.
        if (sinDni && datos.RetiraMedicacion)
            throw new ErrorValidacionException(
                "Quien retira la medicación necesita DNI (FR-021c): es el que aparece en el listado de retirada.");
    }

    private static string? Limpiar(string? valor)
        => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
