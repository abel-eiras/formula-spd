using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Asistente obligatorio de primer arranque (FR-000/FR-001): pide farmacia, primer
/// usuario y valores por defecto antes de permitir usar el resto de la aplicación. Nada se
/// escribe en base de datos hasta <see cref="FinalizarAsistente"/> (CA-000).</summary>
public sealed class ServicioAsistentePrimerArranque(
    IRepositorioFarmacia repositorioFarmacia,
    IRepositorioUsuarios repositorioUsuarios,
    IHasheadorPassword hasheador,
    IRegistradorAuditoria auditoria) : IServicioAsistentePrimerArranque
{
    private Farmacia? _farmaciaEnProgreso;
    private (string Nombre, string Apellidos, string Login, string Password)? _primerUsuarioEnProgreso;

    public bool HayConfiguracionInicial() => repositorioFarmacia.Obtener() is not null;

    public void EjecutarPasoFarmacia(Farmacia datosFarmacia)
    {
        ValidarObligatoriosFarmacia(datosFarmacia);
        _farmaciaEnProgreso = datosFarmacia;
    }

    public void EjecutarPasoPrimerUsuario(string nombre, string apellidos, string login, string password)
    {
        if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(apellidos)
            || string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
        {
            throw new ErrorValidacionException("Faltan campos obligatorios del primer usuario.");
        }
        _primerUsuarioEnProgreso = (nombre, apellidos, login, password);
    }

    public void EjecutarPasoValoresDefecto(string diaRetiradaDefecto, int nBlisteresDefecto, int diasAntelacionListado)
    {
        if (string.IsNullOrWhiteSpace(diaRetiradaDefecto) || nBlisteresDefecto <= 0)
        {
            throw new ErrorValidacionException("Faltan valores por defecto obligatorios (FR-020).");
        }

        var farmacia = ExigirFarmaciaEnProgreso();
        farmacia.DiaRetiradaDefecto = diaRetiradaDefecto;
        farmacia.NBlisteresDefecto = nBlisteresDefecto;
        farmacia.DiasAntelacionListado = diasAntelacionListado;
    }

    public void EjecutarPasoRutas(string? rutaBackup, string? rutaDocumentosGenerados)
    {
        var farmacia = ExigirFarmaciaEnProgreso();
        farmacia.RutaBackup = rutaBackup;
        farmacia.RutaDocumentosGenerados = rutaDocumentosGenerados;
    }

    public void FinalizarAsistente()
    {
        var farmacia = ExigirFarmaciaEnProgreso();
        if (_primerUsuarioEnProgreso is null)
        {
            throw new ErrorValidacionException("Falta completar el paso del primer usuario.");
        }

        repositorioFarmacia.Crear(farmacia);
        auditoria.Registrar(null, "ALTA", "Farmacia", farmacia.Id, null);

        var (nombre, apellidos, login, password) = _primerUsuarioEnProgreso.Value;
        var usuario = new Usuario
        {
            Nombre = nombre,
            Apellidos = apellidos,
            Login = login,
            HashPassword = hasheador.Hashear(password),
            Rol = Rol.Administrador
        };
        var idUsuario = repositorioUsuarios.Crear(usuario);
        auditoria.Registrar(idUsuario, "ALTA", "Usuario", idUsuario, null);
    }

    private static void ValidarObligatoriosFarmacia(Farmacia f)
    {
        if (string.IsNullOrWhiteSpace(f.CodigoSanitario) || string.IsNullOrWhiteSpace(f.Nombre)
            || string.IsNullOrWhiteSpace(f.TitularOComunidadBienes) || string.IsNullOrWhiteSpace(f.Cif)
            || string.IsNullOrWhiteSpace(f.Direccion) || string.IsNullOrWhiteSpace(f.Cp)
            || string.IsNullOrWhiteSpace(f.Poblacion) || string.IsNullOrWhiteSpace(f.Telefono))
        {
            throw new ErrorValidacionException("Faltan campos obligatorios de la farmacia (FR-010).");
        }
    }

    private Farmacia ExigirFarmaciaEnProgreso()
        => _farmaciaEnProgreso
           ?? throw new InvalidOperationException("El paso de datos de la farmacia todavía no se ha completado.");
}
