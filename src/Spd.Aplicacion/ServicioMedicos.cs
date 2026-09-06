using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Catálogo de médicos (Spec 001 US2). Cada escritura queda en auditoría (Art. VII.6) y la
/// baja es lógica: un médico nunca se borra (Art. III.1), porque hay tratamientos y comunicaciones
/// que lo referencian y que deben seguir leyéndose.</summary>
public sealed class ServicioMedicos(
    IRepositorioMedicos repositorio,
    IRepositorioPacientes repositorioPacientes,
    IRegistradorAuditoria auditoria)
    : IServicioMedicos
{
    /// <summary>FR-032. Por debajo de dos caracteres cualquier fragmento trae medio catálogo.</summary>
    public const int MinimoCaracteres = 2;

    public IReadOnlyList<Medico> Buscar(string? fragmento)
    {
        var texto = fragmento?.Trim() ?? string.Empty;
        return texto.Length < MinimoCaracteres
            ? []
            : repositorio.Buscar(Normalizador.QuitarTildesYMayusculas(texto));
    }

    public IReadOnlyList<Medico> ListarActivos() => repositorio.ListarActivos();

    public Medico? ObtenerPorId(int id) => repositorio.ObtenerPorId(id);

    public ResultadoAltaMedico Crear(DatosMedico datos, int? usuarioQueEjecutaId)
    {
        Validar(datos);

        var medico = new Medico
        {
            Nombre = datos.Nombre.Trim(),
            Apellidos = datos.Apellidos.Trim(),
            Colegiado = Limpiar(datos.Colegiado),
            Especialidad = Limpiar(datos.Especialidad) ?? "Medicina de familia",
            Centro = Limpiar(datos.Centro),
            Telefono = Limpiar(datos.Telefono),
            Email = Limpiar(datos.Email),
            Direccion = Limpiar(datos.Direccion),
            Activo = true
        };
        medico.BusquedaNormalizada = ConstruirBusqueda(medico);

        var aviso = DetectarPosibleDuplicado(medico);
        medico.Id = repositorio.Crear(medico);

        auditoria.Registrar(usuarioQueEjecutaId, "CREAR", "Medico", medico.Id,
            $"{medico.Apellidos}, {medico.Nombre}" + (medico.Centro is null ? "" : $" — {medico.Centro}"));

        return new ResultadoAltaMedico(medico, aviso);
    }

    public Medico Actualizar(int id, DatosMedico datos, int? usuarioQueEjecutaId)
    {
        Validar(datos);
        var medico = repositorio.ObtenerPorId(id)
            ?? throw new ErrorValidacionException($"No existe el médico {id}.");

        var antes = $"{medico.Apellidos}, {medico.Nombre}";
        medico.Nombre = datos.Nombre.Trim();
        medico.Apellidos = datos.Apellidos.Trim();
        medico.Colegiado = Limpiar(datos.Colegiado);
        medico.Especialidad = Limpiar(datos.Especialidad) ?? medico.Especialidad;
        medico.Centro = Limpiar(datos.Centro);
        medico.Telefono = Limpiar(datos.Telefono);
        medico.Email = Limpiar(datos.Email);
        medico.Direccion = Limpiar(datos.Direccion);
        medico.BusquedaNormalizada = ConstruirBusqueda(medico);

        repositorio.Actualizar(medico);

        // FR-035: no hay copia de estos datos en Paciente ni en Tratamiento, así que editarlos aquí
        // se refleja en toda referencia sin tocar nada más.
        auditoria.Registrar(usuarioQueEjecutaId, "MODIFICAR", "Medico", medico.Id,
            $"{antes} → {medico.Apellidos}, {medico.Nombre}");

        return medico;
    }

    public void DarDeBaja(int id, int? usuarioQueEjecutaId)
    {
        var medico = repositorio.ObtenerPorId(id)
            ?? throw new ErrorValidacionException($"No existe el médico {id}.");

        var deCabecera = PacientesDeCabecera(id);
        if (deCabecera.Count > 0)
        {
            throw new ErrorValidacionException(
                "No se puede dar de baja: es médico de cabecera de " +
                string.Join(", ", deCabecera.Select(p => $"{p.Nombre} {p.Apellidos} (ficha {p.NumFicha})")) +
                ". Asigna otro médico a esos pacientes antes de darlo de baja (FR-036).");
        }

        medico.Activo = false;
        repositorio.Actualizar(medico);
        auditoria.Registrar(usuarioQueEjecutaId, "BAJA", "Medico", medico.Id, $"{medico.Apellidos}, {medico.Nombre}");
    }

    public int ContarPacientesDeCabecera(int medicoId) => PacientesDeCabecera(medicoId).Count;

    /// <summary>Solo cuentan los activos y en evaluación: un médico de cabecera de pacientes que ya
    /// están de baja no impide nada (FR-036).</summary>
    private IReadOnlyList<Paciente> PacientesDeCabecera(int medicoId)
        => repositorioPacientes.Buscar(
            string.Empty,
            [EstadoPaciente.Activo, EstadoPaciente.Evaluacion],
            medicoId);

    /// <summary>FR-034: avisa, no bloquea. Dos personas pueden llamarse igual, y el mismo médico
    /// puede pasar consulta en dos centros; quien lo sabe es el usuario, no la aplicación.</summary>
    private string? DetectarPosibleDuplicado(Medico nuevo)
    {
        var activos = repositorio.ListarActivos();

        var mismoNombre = activos.FirstOrDefault(m =>
            Normalizador.QuitarTildesYMayusculas(m.Apellidos) == Normalizador.QuitarTildesYMayusculas(nuevo.Apellidos) &&
            Normalizador.QuitarTildesYMayusculas(m.Nombre) == Normalizador.QuitarTildesYMayusculas(nuevo.Nombre));
        if (mismoNombre is not null)
        {
            return $"Ya existe un médico activo llamado {mismoNombre.Apellidos}, {mismoNombre.Nombre}" +
                   (mismoNombre.Centro is null ? "" : $" en {mismoNombre.Centro}") +
                   ". Se ha creado igualmente; compruébalo si no era tu intención.";
        }

        if (nuevo.Colegiado is { Length: > 0 } colegiado)
        {
            var mismoColegiado = activos.FirstOrDefault(m => m.Colegiado == colegiado);
            if (mismoColegiado is not null)
            {
                return $"El nº de colegiado {colegiado} ya lo tiene {mismoColegiado.Apellidos}, {mismoColegiado.Nombre}. " +
                       "Se ha creado igualmente; compruébalo si no era tu intención.";
            }
        }

        return null;
    }

    private static void Validar(DatosMedico datos)
    {
        if (string.IsNullOrWhiteSpace(datos.Nombre) || string.IsNullOrWhiteSpace(datos.Apellidos))
            throw new ErrorValidacionException("Nombre y apellidos del médico son obligatorios (FR-030).");
    }

    /// <summary>FR-032: se busca por apellidos, nombre **y centro**, todo en el mismo campo ya
    /// normalizado, para no depender de extensiones de SQLite (research.md Decisión 1 de Spec 001).</summary>
    private static string ConstruirBusqueda(Medico medico)
        => Normalizador.QuitarTildesYMayusculas(
            $"{medico.Apellidos} {medico.Nombre} {medico.Centro} {medico.Colegiado}".Trim());

    private static string? Limpiar(string? valor)
        => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
