using System.Globalization;
using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Dominio;

namespace Spd.Infraestructura;

/// <summary>Acceso a la ficha de paciente vía Dapper/SQLite. La baja es lógica (Art. III.1).</summary>
public sealed class RepositorioPacientes(SqliteConnection conexion) : IRepositorioPacientes
{
    private const string FormatoFecha = "yyyy-MM-dd";

    private const string Columnas = """
        id, num_ficha, correlativo_num_ficha, fecha_alta_ficha, nombre, apellidos, sexo, dni,
        fecha_nacimiento, num_ss, cip, direccion, cp, poblacion, telefono1, telefono2, email,
        medico_id, enfermedades_cronicas, alergias, observaciones, pictograma_comidas,
        identificador_visual, dia_retirada, n_blisteres, estado, fecha_baja, motivo_baja,
        motivo_baja_detalle, busqueda_normalizada
        """;

    public Paciente? ObtenerPorId(int id)
        => conexion.QuerySingleOrDefault<PacienteFila>($"SELECT {Columnas} FROM Paciente WHERE id = @id", new { id })
            ?.APaciente();

    public IReadOnlyList<Paciente> Buscar(string fragmentoNormalizado, EstadoPaciente[]? filtroEstados, int? filtroMedicoId)
    {
        var condiciones = new List<string> { "busqueda_normalizada LIKE @patron" };
        var parametros = new DynamicParameters();
        parametros.Add("patron", $"%{fragmentoNormalizado}%");

        if (filtroEstados is { Length: > 0 })
        {
            condiciones.Add("estado IN @estados");
            parametros.Add("estados", filtroEstados.Select(EstadoATexto).ToArray());
        }
        if (filtroMedicoId is not null)
        {
            condiciones.Add("medico_id = @medicoId");
            parametros.Add("medicoId", filtroMedicoId.Value);
        }

        var sql = $"""
            SELECT {Columnas} FROM Paciente
            WHERE {string.Join(" AND ", condiciones)}
            ORDER BY
                CASE estado WHEN 'ACTIVO' THEN 0 WHEN 'EVALUACION' THEN 1 WHEN 'SUSPENDIDO' THEN 2 ELSE 3 END,
                apellidos, nombre
            """;
        return conexion.Query<PacienteFila>(sql, parametros).Select(f => f.APaciente()).ToList();
    }

    public IReadOnlyList<Paciente> ListarPorDni(string dni)
        => conexion.Query<PacienteFila>($"SELECT {Columnas} FROM Paciente WHERE dni = @dni", new { dni })
            .Select(f => f.APaciente())
            .ToList();

    public IReadOnlyList<Paciente> ListarPorCip(string cip)
        => conexion.Query<PacienteFila>($"SELECT {Columnas} FROM Paciente WHERE cip = @cip", new { cip })
            .Select(f => f.APaciente())
            .ToList();

    public int ObtenerSiguienteCorrelativo()
        => conexion.ExecuteScalar<int>("SELECT COALESCE(MAX(correlativo_num_ficha), 0) + 1 FROM Paciente");

    public int Crear(Paciente paciente)
        => conexion.ExecuteScalar<int>(
            """
            INSERT INTO Paciente (
                num_ficha, correlativo_num_ficha, fecha_alta_ficha, nombre, apellidos, sexo, dni,
                fecha_nacimiento, num_ss, cip, direccion, cp, poblacion, telefono1, telefono2, email,
                medico_id, enfermedades_cronicas, alergias, observaciones, pictograma_comidas,
                identificador_visual, dia_retirada, n_blisteres, estado, busqueda_normalizada
            ) VALUES (
                @NumFicha, @CorrelativoNumFicha, @FechaAltaFicha, @Nombre, @Apellidos, @Sexo, @Dni,
                @FechaNacimiento, @NumSs, @Cip, @Direccion, @Cp, @Poblacion, @Telefono1, @Telefono2, @Email,
                @MedicoId, @EnfermedadesCronicas, @Alergias, @Observaciones, @PictogramaComidas,
                @IdentificadorVisual, @DiaRetirada, @NBlisteres, @Estado, @BusquedaNormalizada
            ) RETURNING id
            """,
            AParametros(paciente));

    public void Actualizar(Paciente paciente)
        => conexion.Execute(
            """
            UPDATE Paciente SET
                nombre = @Nombre, apellidos = @Apellidos, sexo = @Sexo, dni = @Dni,
                fecha_nacimiento = @FechaNacimiento, num_ss = @NumSs, cip = @Cip, direccion = @Direccion,
                cp = @Cp, poblacion = @Poblacion, telefono1 = @Telefono1, telefono2 = @Telefono2,
                email = @Email, medico_id = @MedicoId, enfermedades_cronicas = @EnfermedadesCronicas,
                alergias = @Alergias, observaciones = @Observaciones, pictograma_comidas = @PictogramaComidas,
                identificador_visual = @IdentificadorVisual, dia_retirada = @DiaRetirada,
                n_blisteres = @NBlisteres, estado = @Estado, fecha_baja = @FechaBaja,
                motivo_baja = @MotivoBaja, motivo_baja_detalle = @MotivoBajaDetalle,
                busqueda_normalizada = @BusquedaNormalizada, modificado_en = @ModificadoEn
            WHERE id = @Id
            """,
            AParametros(paciente));

    private static object AParametros(Paciente p) => new
    {
        p.Id,
        p.NumFicha,
        p.CorrelativoNumFicha,
        FechaAltaFicha = p.FechaAltaFicha.ToString("o"),
        p.Nombre,
        p.Apellidos,
        p.Sexo,
        p.Dni,
        FechaNacimiento = p.FechaNacimiento?.ToString(FormatoFecha, CultureInfo.InvariantCulture),
        p.NumSs,
        p.Cip,
        p.Direccion,
        p.Cp,
        p.Poblacion,
        p.Telefono1,
        p.Telefono2,
        p.Email,
        p.MedicoId,
        p.EnfermedadesCronicas,
        p.Alergias,
        p.Observaciones,
        PictogramaComidas = p.PictogramaComidas ? 1 : 0,
        p.IdentificadorVisual,
        p.DiaRetirada,
        p.NBlisteres,
        Estado = EstadoATexto(p.Estado),
        FechaBaja = p.FechaBaja?.ToString("o"),
        MotivoBaja = p.MotivoBaja is null ? null : MotivoBajaATexto(p.MotivoBaja.Value),
        p.MotivoBajaDetalle,
        p.BusquedaNormalizada,
        ModificadoEn = DateTime.UtcNow.ToString("o")
    };

    private static string EstadoATexto(EstadoPaciente estado) => estado switch
    {
        EstadoPaciente.Evaluacion => "EVALUACION",
        EstadoPaciente.Activo => "ACTIVO",
        EstadoPaciente.Suspendido => "SUSPENDIDO",
        EstadoPaciente.Baja => "BAJA",
        _ => throw new ArgumentOutOfRangeException(nameof(estado))
    };

    private static EstadoPaciente TextoAEstado(string texto) => texto switch
    {
        "EVALUACION" => EstadoPaciente.Evaluacion,
        "ACTIVO" => EstadoPaciente.Activo,
        "SUSPENDIDO" => EstadoPaciente.Suspendido,
        "BAJA" => EstadoPaciente.Baja,
        _ => throw new ArgumentOutOfRangeException(nameof(texto))
    };

    private static string MotivoBajaATexto(MotivoBaja motivo) => motivo switch
    {
        Dominio.MotivoBaja.Fallecimiento => "FALLECIMIENTO",
        Dominio.MotivoBaja.Renuncia => "RENUNCIA",
        Dominio.MotivoBaja.Traslado => "TRASLADO",
        Dominio.MotivoBaja.HospitalizacionProlongada => "HOSPITALIZACION_PROLONGADA",
        Dominio.MotivoBaja.CriterioFarmaceutico => "CRITERIO_FARMACEUTICO",
        Dominio.MotivoBaja.Otro => "OTRO",
        _ => throw new ArgumentOutOfRangeException(nameof(motivo))
    };

    private static MotivoBaja? TextoAMotivoBaja(string? texto) => texto switch
    {
        null => null,
        "FALLECIMIENTO" => Dominio.MotivoBaja.Fallecimiento,
        "RENUNCIA" => Dominio.MotivoBaja.Renuncia,
        "TRASLADO" => Dominio.MotivoBaja.Traslado,
        "HOSPITALIZACION_PROLONGADA" => Dominio.MotivoBaja.HospitalizacionProlongada,
        "CRITERIO_FARMACEUTICO" => Dominio.MotivoBaja.CriterioFarmaceutico,
        "OTRO" => Dominio.MotivoBaja.Otro,
        _ => throw new ArgumentOutOfRangeException(nameof(texto))
    };

    /// <summary>Fila 1:1 con las columnas leídas de Paciente. Evita depender de la conversión
    /// automática de Dapper para los enums `estado`/`motivo_baja` y los booleanos (mismo motivo
    /// que UsuarioFila en Spec 000).</summary>
    private sealed record PacienteFila(
        long Id, string NumFicha, long CorrelativoNumFicha, string FechaAltaFicha, string Nombre,
        string Apellidos, string? Sexo, string? Dni, string? FechaNacimiento, string? NumSs, string? Cip,
        string? Direccion, string? Cp, string? Poblacion, string? Telefono1, string? Telefono2,
        string? Email, long? MedicoId, string? EnfermedadesCronicas, string? Alergias, string? Observaciones,
        long PictogramaComidas, string? IdentificadorVisual, string DiaRetirada, long NBlisteres,
        string Estado, string? FechaBaja, string? MotivoBaja, string? MotivoBajaDetalle,
        string BusquedaNormalizada)
    {
        public Paciente APaciente() => new()
        {
            Id = (int)Id,
            NumFicha = NumFicha,
            CorrelativoNumFicha = (int)CorrelativoNumFicha,
            FechaAltaFicha = DateTime.Parse(FechaAltaFicha),
            Nombre = Nombre,
            Apellidos = Apellidos,
            Sexo = Sexo,
            Dni = Dni,
            FechaNacimiento = FechaNacimiento is null
                ? null
                : DateOnly.ParseExact(FechaNacimiento, FormatoFecha, CultureInfo.InvariantCulture),
            NumSs = NumSs,
            Cip = Cip,
            Direccion = Direccion,
            Cp = Cp,
            Poblacion = Poblacion,
            Telefono1 = Telefono1,
            Telefono2 = Telefono2,
            Email = Email,
            MedicoId = MedicoId is null ? null : (int)MedicoId,
            EnfermedadesCronicas = EnfermedadesCronicas,
            Alergias = Alergias,
            Observaciones = Observaciones,
            PictogramaComidas = PictogramaComidas == 1,
            IdentificadorVisual = IdentificadorVisual,
            DiaRetirada = DiaRetirada,
            NBlisteres = (int)NBlisteres,
            Estado = TextoAEstado(Estado),
            FechaBaja = FechaBaja is null ? null : DateTime.Parse(FechaBaja),
            MotivoBaja = TextoAMotivoBaja(MotivoBaja),
            MotivoBajaDetalle = MotivoBajaDetalle,
            BusquedaNormalizada = BusquedaNormalizada
        };
    }
}
