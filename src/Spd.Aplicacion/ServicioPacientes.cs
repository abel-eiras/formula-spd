using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Ficha de paciente: alta, edición, estados y búsqueda (FR-001..FR-011). Toda escritura
/// registra en auditoría (Art. VII.6).</summary>
public sealed class ServicioPacientes(
    IRepositorioPacientes repositorio,
    IRepositorioFarmacia repositorioFarmacia,
    IRegistradorAuditoria auditoria) : IServicioPacientes
{
    public Paciente Crear(DatosAltaPaciente datos, int? usuarioQueEjecutaId)
    {
        ValidarMinimos(datos);
        ValidarSexoSiHayCip(datos.Sexo, datos.Cip);

        if (!datos.ConfirmarDuplicado)
        {
            var duplicado = BuscarDuplicadoNoDeBaja(datos.Dni, datos.Cip, excluirId: null);
            if (duplicado is not null)
            {
                throw new PacienteDuplicadoException(duplicado,
                    $"Ya existe un paciente con los mismos datos: {duplicado.Nombre} {duplicado.Apellidos} ({duplicado.NumFicha}).");
            }
        }

        var farmacia = repositorioFarmacia.Obtener()
            ?? throw new ErrorValidacionException("No hay configuración de farmacia (Spec 000).");
        var correlativo = repositorio.ObtenerSiguienteCorrelativo();

        var paciente = new Paciente
        {
            NumFicha = $"{farmacia.PrefijoNumFicha}{correlativo:D6}",
            CorrelativoNumFicha = correlativo,
            FechaAltaFicha = DateTime.UtcNow,
            Nombre = datos.Nombre,
            Apellidos = datos.Apellidos,
            Sexo = datos.Sexo,
            Dni = datos.Dni,
            FechaNacimiento = datos.FechaNacimiento,
            NumSs = datos.NumSs,
            Cip = datos.Cip,
            Direccion = datos.Direccion,
            Cp = datos.Cp,
            Poblacion = datos.Poblacion,
            Telefono1 = datos.Telefono1,
            Telefono2 = datos.Telefono2,
            Email = datos.Email,
            MedicoId = datos.MedicoId,
            EnfermedadesCronicas = datos.EnfermedadesCronicas,
            Alergias = datos.Alergias,
            Observaciones = datos.Observaciones,
            PictogramaComidas = datos.PictogramaComidas,
            IdentificadorVisual = datos.IdentificadorVisual,
            DiaRetirada = datos.DiaRetirada ?? farmacia.DiaRetiradaDefecto,
            NBlisteres = datos.NBlisteres ?? farmacia.NBlisteresDefecto,
            Estado = EstadoPaciente.Evaluacion
        };
        paciente.BusquedaNormalizada = ConstruirBusquedaNormalizada(paciente);
        paciente.Id = repositorio.Crear(paciente);

        auditoria.Registrar(usuarioQueEjecutaId, "ALTA", "Paciente", paciente.Id, null);
        return paciente;
    }

    public void Actualizar(Paciente paciente, int? usuarioQueEjecutaId)
    {
        var antes = repositorio.ObtenerPorId(paciente.Id)
            ?? throw new ErrorValidacionException($"No existe el paciente {paciente.Id}.");
        ValidarSexoSiHayCip(paciente.Sexo, paciente.Cip);

        paciente.BusquedaNormalizada = ConstruirBusquedaNormalizada(paciente);
        repositorio.Actualizar(paciente);

        auditoria.Registrar(usuarioQueEjecutaId, "EDITAR", "Paciente", paciente.Id, ConstruirDetalleCambios(antes, paciente));
    }

    public void CambiarEstado(int pacienteId, EstadoPaciente nuevoEstado, DatosBaja? datosBaja, int? usuarioQueEjecutaId)
    {
        var paciente = repositorio.ObtenerPorId(pacienteId)
            ?? throw new ErrorValidacionException($"No existe el paciente {pacienteId}.");
        if (!paciente.TransicionValida(nuevoEstado))
        {
            throw new ErrorValidacionException($"Transición no permitida: {paciente.Estado} → {nuevoEstado} (FR-006).");
        }

        if (nuevoEstado == EstadoPaciente.Baja)
        {
            if (datosBaja is null)
            {
                throw new ErrorValidacionException("La baja exige fecha y motivo (FR-007).");
            }
            paciente.FechaBaja = datosBaja.Fecha;
            paciente.MotivoBaja = datosBaja.Motivo;
            paciente.MotivoBajaDetalle = datosBaja.Detalle;
        }
        else if (paciente.Estado == EstadoPaciente.Baja)
        {
            // Reactivación (FR-006, CA-010): se limpia la baja; la nueva evaluación y el nuevo
            // consentimiento son responsabilidad de Spec 002, no de esta spec.
            paciente.FechaBaja = null;
            paciente.MotivoBaja = null;
            paciente.MotivoBajaDetalle = null;
        }

        paciente.Estado = nuevoEstado;
        repositorio.Actualizar(paciente);

        auditoria.Registrar(usuarioQueEjecutaId, "CAMBIO_ESTADO", "Paciente", pacienteId,
            $"Estado: {nuevoEstado}" + (datosBaja is null ? "" : $" (motivo: {datosBaja.Motivo})"));
    }

    public IReadOnlyList<Paciente> Buscar(string fragmento, EstadoPaciente[]? filtroEstados, int? filtroMedicoId)
        => repositorio.Buscar(Normalizador.QuitarTildesYMayusculas(fragmento), filtroEstados, filtroMedicoId);

    public ResultadoValidacionDni ValidarDni(string dni) => new(ValidadorDni.EsValido(dni));

    public ResultadoValidacionCip ValidarCip(string cip, DateOnly fechaNacimiento, string apellidos, string sexo)
        => new(ValidadorCip.Corresponde(cip, fechaNacimiento, apellidos, sexo));

    public string AutocompletarCip(DateOnly fechaNacimiento, string apellidos, string sexo)
        => ValidadorCip.Autocompletar(fechaNacimiento, apellidos, sexo);

    private static void ValidarMinimos(DatosAltaPaciente datos)
    {
        if (string.IsNullOrWhiteSpace(datos.Nombre) || string.IsNullOrWhiteSpace(datos.Apellidos))
        {
            throw new ErrorValidacionException("Nombre y apellidos son obligatorios (FR-003).");
        }
        if (string.IsNullOrWhiteSpace(datos.Dni) && string.IsNullOrWhiteSpace(datos.Cip) && datos.FechaNacimiento is null)
        {
            throw new ErrorValidacionException("Falta al menos uno de: DNI, CIP o fecha de nacimiento (FR-003, CA-002).");
        }
    }

    private static void ValidarSexoSiHayCip(string? sexo, string? cip)
    {
        if (!string.IsNullOrWhiteSpace(cip) && string.IsNullOrWhiteSpace(sexo))
        {
            throw new ErrorValidacionException("El sexo es obligatorio cuando se informa el CIP (FR-002b).");
        }
    }

    private Paciente? BuscarDuplicadoNoDeBaja(string? dni, string? cip, int? excluirId)
    {
        var candidatos = new List<Paciente>();
        if (!string.IsNullOrWhiteSpace(dni)) candidatos.AddRange(repositorio.ListarPorDni(dni));
        if (!string.IsNullOrWhiteSpace(cip)) candidatos.AddRange(repositorio.ListarPorCip(cip));
        return candidatos.FirstOrDefault(p => p.Estado != EstadoPaciente.Baja && p.Id != excluirId);
    }

    private static string ConstruirBusquedaNormalizada(Paciente p)
        => Normalizador.QuitarTildesYMayusculas($"{p.Nombre} {p.Apellidos} {p.Dni} {p.Cip} {p.NumFicha}");

    /// <summary>Reflexión limitada a los campos simples de Paciente: única forma de cubrir CA-013
    /// ("detalle que incluye campo: antes → después") sin mantener una lista manual de ~25
    /// propiedades que se desincroniza cada vez que se añade un campo a la ficha.</summary>
    private static string? ConstruirDetalleCambios(Paciente antes, Paciente despues)
    {
        var cambios = new List<string>();
        foreach (var propiedad in typeof(Paciente).GetProperties())
        {
            if (propiedad.Name == nameof(Paciente.BusquedaNormalizada)) continue;
            var valorAntes = propiedad.GetValue(antes);
            var valorDespues = propiedad.GetValue(despues);
            if (!Equals(valorAntes, valorDespues))
            {
                cambios.Add($"{propiedad.Name}: {valorAntes} → {valorDespues}");
            }
        }
        return cambios.Count == 0 ? null : string.Join("; ", cambios);
    }
}
