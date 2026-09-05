using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Alta masiva de tratamiento+envase por copiar/pegar o fichero (FR-570..577). Un CN sin
/// tratamiento activo crea uno pendiente de posología (`EstadoTratamiento.PendienteRevision`), sin
/// bloquear el resto de la importación (FR-573/575/576).</summary>
public sealed class ServicioImportacionTratamientoEnvase(
    IRepositorioMedicamentos repositorioMedicamentos, IRepositorioTratamientos repositorioTratamientos,
    IRepositorioEnvases repositorioEnvases, IRepositorioPerfilesImportacionTratamiento repositorioPerfiles,
    IServicioTratamientos servicioTratamientos, IRegistradorAuditoria auditoria)
    : IServicioImportacionTratamientoEnvase
{
    public ResultadoImportacion ImportarDesdePegado(int pacienteId, PerfilImportacionTratamiento perfil, string textoTabulado, int? usuarioQueEjecutaId)
        => Importar(pacienteId, ParserLineasImportacion.ParsearTexto(textoTabulado, perfil.Mapeo, perfil.TieneCabecera ?? false), usuarioQueEjecutaId);

    public ResultadoImportacion ImportarDesdeFichero(int pacienteId, PerfilImportacionTratamiento perfil, string contenidoCsv, int? usuarioQueEjecutaId)
        => Importar(pacienteId, ParserLineasImportacion.ParsearCsv(contenidoCsv, perfil.Mapeo, perfil.Separador ?? ",", perfil.TieneCabecera ?? true), usuarioQueEjecutaId);

    public PerfilImportacionTratamiento GuardarPerfil(string nombre, OrigenImportacionTratamiento origen, string? separador, bool? tieneCabecera, MapeoColumnasImportacion mapeo)
    {
        var perfil = new PerfilImportacionTratamiento { Nombre = nombre, Origen = origen, Separador = separador, TieneCabecera = tieneCabecera, Mapeo = mapeo };
        perfil.Id = repositorioPerfiles.Crear(perfil);
        return perfil;
    }

    public IReadOnlyList<PerfilImportacionTratamiento> ListarPerfiles() => repositorioPerfiles.Listar();

    private ResultadoImportacion Importar(int pacienteId, IReadOnlyList<FilaImportacionCruda> filas, int? usuarioQueEjecutaId)
    {
        var envasesCreados = new List<EnvaseImportado>();
        var tratamientosPendientes = new List<int>();
        var filasCnNoEncontrado = new List<FilaImportacionCruda>();
        var filasSerieDuplicada = new List<(FilaImportacionCruda Fila, int PacienteIdExistente)>();

        foreach (var fila in filas)
        {
            var medicamento = fila.Cn is null ? null : repositorioMedicamentos.ObtenerPorCn(fila.Cn);
            if (medicamento is null) // FR-575
            {
                filasCnNoEncontrado.Add(fila);
                continue;
            }

            if (fila.NumSerie is not null && repositorioEnvases.ObtenerPorSerie(fila.NumSerie) is { } existente) // FR-576
            {
                filasSerieDuplicada.Add((fila, existente.PacienteId));
                continue;
            }

            var tratamientoActivo = repositorioTratamientos.ListarVigentesDePaciente(pacienteId)
                .FirstOrDefault(t => t.MedicamentoId == medicamento.Id && t.EnSpd);

            var tratamientoPendiente = false;
            if (tratamientoActivo is null) // FR-573: tratamiento nuevo, pendiente de posología
            {
                var nuevo = servicioTratamientos.Crear(
                    pacienteId,
                    new DatosAltaTratamiento(
                        medicamento.Id, EnSpd: true, ProblemaSalud: null, MedicoId: null,
                        PautaD: null, PautaA: null, PautaC: null, PautaN: null, PautaTexto: null,
                        DiasSemana: "1111111", Via: null, Momento: null,
                        FechaInicio: DateOnly.FromDateTime(DateTime.Today), Tipo: TipoTratamiento.Cronico),
                    usuarioQueEjecutaId);
                servicioTratamientos.CambiarEstado(nuevo.Id, EstadoTratamiento.PendienteRevision, usuarioQueEjecutaId);
                tratamientosPendientes.Add(nuevo.Id);
                tratamientoPendiente = true;
            }

            var unidadesIniciales = medicamento.UnidadesEnvase; // FR-574: pendiente de completar si no hay valor en catálogo
            var envase = new Envase
            {
                PacienteId = pacienteId,
                MedicamentoId = medicamento.Id,
                Serie = fila.NumSerie,
                Lote = fila.Lote,
                Caducidad = ParsearCaducidad(fila.Caducidad),
                UnidadesIniciales = unidadesIniciales,
                UnidadesRestantes = unidadesIniciales,
                FechaEntrada = DateTime.UtcNow,
                Origen = OrigenEnvase.Importado,
                Estado = EstadoEnvase.EnCustodia
            };
            envase.Id = repositorioEnvases.Crear(envase);
            envasesCreados.Add(new EnvaseImportado(envase, tratamientoPendiente));
        }

        auditoria.Registrar(usuarioQueEjecutaId, "IMPORTAR_TRATAMIENTO_ENVASE", "Paciente", pacienteId, $"envases={envasesCreados.Count}");
        return new ResultadoImportacion(envasesCreados, tratamientosPendientes, filasCnNoEncontrado, filasSerieDuplicada);
    }

    // FR-512/Q2: fecha completa; mes/año a mano se resuelve al último día del mes.
    private static DateOnly? ParsearCaducidad(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return null;
        if (DateOnly.TryParse(texto, System.Globalization.CultureInfo.InvariantCulture, out var fecha)) return fecha;

        var partes = texto.Split('-', '/');
        if (partes.Length == 2 && int.TryParse(partes[0], out var p1) && int.TryParse(partes[1], out var p2))
        {
            var (anio, mes) = p1 > 999 ? (p1, p2) : (p2, p1);
            return new DateOnly(anio, mes, DateTime.DaysInMonth(anio, mes));
        }
        return null;
    }
}
