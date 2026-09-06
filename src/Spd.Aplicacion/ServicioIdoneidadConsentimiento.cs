using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Idoneidad y consentimiento (Spec 002). La aplicación propone el resultado de la
/// evaluación y prerrellena el firmante, pero las decisiones son del farmacéutico (Art. V.1, II):
/// el único automatismo es EVALUACION→ACTIVO cuando ambos requisitos están en regla (FR-213), y
/// pasa por <see cref="IServicioPacientes"/> para que quede auditado como cualquier otro cambio
/// de estado (research.md Decisión 5).</summary>
public sealed class ServicioIdoneidadConsentimiento(
    IRepositorioEvaluacionesIdoneidad evaluaciones,
    IRepositorioConsentimientos consentimientos,
    IRepositorioContactos contactos,
    IRepositorioPacientes pacientes,
    IServicioPacientes servicioPacientes,
    IRegistradorAuditoria auditoria)
    : IServicioIdoneidadConsentimiento
{
    private static readonly TipoContacto[] TiposRepresentante = [TipoContacto.RepresentanteLegal, TipoContacto.PersonaAutorizada];

    public EstadoIdoneidadConsentimiento Consultar(int pacienteId)
    {
        var paciente = ObtenerPaciente(pacienteId);
        return new EstadoIdoneidadConsentimiento(
            paciente,
            evaluaciones.ObtenerVigente(pacienteId),
            consentimientos.ObtenerVigente(pacienteId),
            evaluaciones.ListarDePaciente(pacienteId),
            consentimientos.ListarDePaciente(pacienteId),
            RepresentantesElegibles(pacienteId));
    }

    public ResultadoEvaluacion RegistrarEvaluacion(int pacienteId, DatosEvaluacionIdoneidad datos, int? usuarioId)
    {
        var paciente = ObtenerPaciente(pacienteId);
        var evaluacion = new EvaluacionIdoneidad
        {
            PacienteId = pacienteId, Fecha = DateTime.UtcNow, FarmaceuticoId = datos.FarmaceuticoId ?? usuarioId,
            Criterio1 = datos.Criterio1, Criterio2 = datos.Criterio2, Criterio3 = datos.Criterio3, Criterio4 = datos.Criterio4,
            Criterio5 = datos.Criterio5, Criterio6 = datos.Criterio6, Criterio7 = datos.Criterio7,
            CondicionMotivacion = datos.CondicionMotivacion, CondicionDestreza = datos.CondicionDestreza,
            Observaciones = string.IsNullOrWhiteSpace(datos.Observaciones) ? null : datos.Observaciones.Trim(),
            Resultado = datos.Resultado
        };

        // FR-204 / CA-205: la decisión profesional se justifica cuando es NO_APTO o contradice la propuesta.
        if (evaluacion.RequiereObservaciones && evaluacion.Observaciones is null)
            throw new ErrorValidacionException(
                evaluacion.Resultado == ResultadoIdoneidad.NoApto
                    ? "Un resultado NO APTO exige observaciones que lo justifiquen (FR-204)."
                    : $"El resultado elegido difiere de la propuesta ({evaluacion.ResultadoPropuesto()}); las observaciones son obligatorias (FR-204).");

        evaluacion.Id = evaluaciones.Crear(evaluacion);
        auditoria.Registrar(usuarioId, "EVALUAR_IDONEIDAD", "Paciente", pacienteId,
            $"evaluacion={evaluacion.Id};resultado={evaluacion.Resultado};propuesta={evaluacion.ResultadoPropuesto()}");

        var activado = ActivarSiProcede(paciente, usuarioId);
        var sugerirSuspension = !activado && paciente.Estado == EstadoPaciente.Activo && evaluacion.Resultado == ResultadoIdoneidad.NoApto;
        return new ResultadoEvaluacion(evaluacion, activado, sugerirSuspension);
    }

    public Consentimiento CrearConsentimiento(int pacienteId, TipoConsentimiento tipo, int? contactoId, int? usuarioId)
    {
        ObtenerPaciente(pacienteId);
        if (tipo == TipoConsentimiento.Representante)
        {
            // FR-210 / CA-202: el firmante debe ser un contacto elegible con DNI; si no lo hay,
            // el mensaje remite a crearlo (FR-211) en vez de aceptar un consentimiento sin firmante.
            var elegibles = RepresentantesElegibles(pacienteId);
            if (contactoId is null || elegibles.All(c => c.Id != contactoId))
                throw new ErrorValidacionException(elegibles.Count == 0
                    ? "El paciente no tiene ningún representante legal ni persona autorizada con DNI: cree el contacto antes de continuar (FR-211)."
                    : "Seleccione el representante legal o la persona autorizada que firma (FR-210).");
        }
        else
        {
            contactoId = null;
        }

        var consentimiento = new Consentimiento
        {
            PacienteId = pacienteId, Tipo = tipo, ContactoId = contactoId, FechaCreacion = DateTime.UtcNow
        };
        consentimiento.Id = consentimientos.Crear(consentimiento);
        auditoria.Registrar(usuarioId, "CREAR_CONSENTIMIENTO", "Paciente", pacienteId, $"consentimiento={consentimiento.Id};tipo={tipo}");
        return consentimiento;
    }

    public Contacto CrearRepresentante(int pacienteId, DatosContactoRepresentante datos, int? usuarioId)
    {
        ObtenerPaciente(pacienteId);
        if (!TiposRepresentante.Contains(datos.Tipo))
            throw new ErrorValidacionException("Desde aquí solo se dan de alta representantes legales o personas autorizadas (FR-211).");
        if (string.IsNullOrWhiteSpace(datos.Nombre) || string.IsNullOrWhiteSpace(datos.Apellidos))
            throw new ErrorValidacionException("Nombre y apellidos del representante son obligatorios.");
        if (string.IsNullOrWhiteSpace(datos.Dni))
            throw new ErrorValidacionException("El DNI del representante es obligatorio (Spec 001 FR-022).");

        var contacto = new Contacto
        {
            PacienteId = pacienteId, Tipo = datos.Tipo, Nombre = datos.Nombre.Trim(), Apellidos = datos.Apellidos.Trim(),
            Dni = datos.Dni.Trim().ToUpperInvariant(), Telefono = datos.Telefono, Email = datos.Email
        };
        contacto.Id = contactos.Crear(contacto);
        auditoria.Registrar(usuarioId, "CREAR_CONTACTO", "Paciente", pacienteId, $"contacto={contacto.Id};tipo={datos.Tipo}");
        return contacto;
    }

    public ResultadoConsentimiento RegistrarFirma(int consentimientoId, DateOnly fechaFirma, int? usuarioId)
    {
        var consentimiento = ObtenerConsentimiento(consentimientoId);
        if (consentimiento.FechaFirma is not null)
            throw new ErrorValidacionException("Este consentimiento ya consta como firmado; para renovarlo cree uno nuevo (FR-215).");
        if (fechaFirma > DateOnly.FromDateTime(DateTime.Today))
            throw new ErrorValidacionException("La fecha de firma no puede ser futura: se registra un hecho ya ocurrido (Art. II).");

        consentimiento.FechaFirma = fechaFirma;
        consentimientos.Actualizar(consentimiento);
        auditoria.Registrar(usuarioId, "FIRMAR_CONSENTIMIENTO", "Paciente", consentimiento.PacienteId,
            $"consentimiento={consentimiento.Id};fecha_firma={fechaFirma:yyyy-MM-dd}");

        var paciente = ObtenerPaciente(consentimiento.PacienteId);
        return new ResultadoConsentimiento(consentimiento, ActivarSiProcede(paciente, usuarioId), false);
    }

    public ResultadoConsentimiento Revocar(int consentimientoId, DateOnly fecha, string motivo, int? usuarioId)
    {
        var consentimiento = ObtenerConsentimiento(consentimientoId);
        if (consentimiento.FechaFirma is null)
            throw new ErrorValidacionException("Solo se revoca un consentimiento firmado.");
        if (consentimiento.FechaRevocacion is not null)
            throw new ErrorValidacionException("Este consentimiento ya está revocado.");
        if (string.IsNullOrWhiteSpace(motivo))
            throw new ErrorValidacionException("La revocación exige un motivo (FR-214).");
        if (fecha < consentimiento.FechaFirma)
            throw new ErrorValidacionException("La fecha de revocación no puede ser anterior a la de firma.");

        consentimiento.FechaRevocacion = fecha;
        consentimiento.MotivoRevocacion = motivo.Trim();
        consentimientos.Actualizar(consentimiento);
        auditoria.Registrar(usuarioId, "REVOCAR_CONSENTIMIENTO", "Paciente", consentimiento.PacienteId,
            $"consentimiento={consentimiento.Id};fecha={fecha:yyyy-MM-dd};motivo={consentimiento.MotivoRevocacion}");

        // FR-214: si no queda otro vigente, se avisa y se ofrece SUSPENDIDO; nunca se suspende aquí.
        var paciente = ObtenerPaciente(consentimiento.PacienteId);
        var sugerir = paciente.Estado == EstadoPaciente.Activo && consentimientos.ObtenerVigente(paciente.Id) is null;
        return new ResultadoConsentimiento(consentimiento, false, sugerir);
    }

    public void Suspender(int pacienteId, int? usuarioId)
        => servicioPacientes.CambiarEstado(pacienteId, EstadoPaciente.Suspendido, null, usuarioId);

    /// <summary>FR-213: EVALUACION → ACTIVO cuando hay evaluación vigente APTO y consentimiento vigente.</summary>
    private bool ActivarSiProcede(Paciente paciente, int? usuarioId)
    {
        if (paciente.Estado != EstadoPaciente.Evaluacion) return false;
        var cumple = evaluaciones.ObtenerVigente(paciente.Id)?.Resultado == ResultadoIdoneidad.Apto
                     && consentimientos.ObtenerVigente(paciente.Id) is not null;
        if (!cumple) return false;
        servicioPacientes.CambiarEstado(paciente.Id, EstadoPaciente.Activo, null, usuarioId);
        paciente.Estado = EstadoPaciente.Activo;
        return true;
    }

    private IReadOnlyList<Contacto> RepresentantesElegibles(int pacienteId)
        => contactos.ListarDePaciente(pacienteId, incluirBaja: false)
            .Where(c => TiposRepresentante.Contains(c.Tipo) && !string.IsNullOrWhiteSpace(c.Dni))
            .ToList();

    private Paciente ObtenerPaciente(int pacienteId)
        => pacientes.ObtenerPorId(pacienteId) ?? throw new ErrorValidacionException($"No existe el paciente {pacienteId}.");

    private Consentimiento ObtenerConsentimiento(int id)
        => consentimientos.ObtenerPorId(id) ?? throw new ErrorValidacionException($"No existe el consentimiento {id}.");
}
