using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Centros y asignación de pacientes (Spec 019 FR-1900..1904).</summary>
public sealed class ServicioCentros(
    IRepositorioCentros repositorio, IRepositorioPacientes repositorioPacientes, IRepositorioSpd repositorioSpd,
    IRegistradorAuditoria auditoria) : IServicioCentros
{
    public Centro Crear(DatosCentro datos, int? usuarioId)
    {
        Validar(datos);
        var centro = new Centro
        {
            Nombre = datos.Nombre.Trim(), Direccion = Vacio(datos.Direccion), Telefono = Vacio(datos.Telefono),
            PersonaContacto = Vacio(datos.PersonaContacto)
        };
        centro.Id = repositorio.Crear(centro, usuarioId);
        auditoria.Registrar(usuarioId, "ALTA", "Centro", centro.Id, centro.Nombre);
        return centro;
    }

    public void Actualizar(int centroId, DatosCentro datos, int? usuarioId)
    {
        Validar(datos);
        var centro = Obtener(centroId);
        centro.Nombre = datos.Nombre.Trim();
        centro.Direccion = Vacio(datos.Direccion);
        centro.Telefono = Vacio(datos.Telefono);
        centro.PersonaContacto = Vacio(datos.PersonaContacto);
        repositorio.Actualizar(centro, usuarioId);
        auditoria.Registrar(usuarioId, "MODIFICACION", "Centro", centroId, centro.Nombre);
    }

    public void DarDeBaja(int centroId, string motivo, int? usuarioId)
    {
        if (string.IsNullOrWhiteSpace(motivo)) throw new ErrorValidacionException("Indica el motivo de la baja del centro.");
        var centro = Obtener(centroId);
        // Un centro con pacientes no se da de baja: esos pacientes quedarían apuntando a un centro que
        // ya no aparece en ningún desplegable. Primero se reasignan o se quitan.
        if (repositorio.PacientesDe(centroId).Count > 0)
            throw new ErrorValidacionException("El centro todavía tiene pacientes asignados: quítalos o cámbialos de centro antes de darlo de baja.");
        centro.Activo = false;
        centro.FechaBaja = DateTime.UtcNow;
        centro.MotivoBaja = motivo.Trim();
        repositorio.Actualizar(centro, usuarioId);
        auditoria.Registrar(usuarioId, "BAJA", "Centro", centroId, motivo.Trim());
    }

    public IReadOnlyList<Centro> Listar(bool incluirBaja) => repositorio.Listar(incluirBaja);

    public void AsignarPaciente(int pacienteId, int? centroId, int? usuarioId)
    {
        _ = repositorioPacientes.ObtenerPorId(pacienteId) ?? throw new ErrorValidacionException("El paciente no existe.");
        if (centroId is { } id && Obtener(id) is { Activo: false })
            throw new ErrorValidacionException("Ese centro está dado de baja.");
        repositorio.AsignarPaciente(pacienteId, centroId);
        auditoria.Registrar(usuarioId, "MODIFICACION", "Paciente", pacienteId, centroId is null ? "sin centro" : $"centro={centroId}");
    }

    public IReadOnlyList<Paciente> PacientesDe(int centroId)
        => repositorio.PacientesDe(centroId).Select(repositorioPacientes.ObtenerPorId).OfType<Paciente>().ToList();

    public IReadOnlyList<BlisterParaAlbaran> BlisteresParaAlbaran(int centroId)
        => PacientesDe(centroId)
            .SelectMany(p => repositorioSpd.Listar(EstadoSpd.Verificado, p.Id, null)
                .Select(s => new BlisterParaAlbaran(s.Id, s.NumRegistro, p.Id, p.NumFicha, $"{p.Apellidos}, {p.Nombre}", s.ValidezDesde, s.ValidezHasta)))
            .OrderBy(b => b.Paciente).ThenBy(b => b.ValidezDesde)
            .ToList();

    private Centro Obtener(int centroId) => repositorio.ObtenerPorId(centroId) ?? throw new ErrorValidacionException("El centro no existe.");

    private static void Validar(DatosCentro datos)
    {
        if (string.IsNullOrWhiteSpace(datos.Nombre)) throw new ErrorValidacionException("El nombre del centro es obligatorio.");
    }

    private static string? Vacio(string? texto) => string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
}
