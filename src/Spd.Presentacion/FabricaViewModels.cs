using Spd.Dominio;
using Spd.Presentacion.Ayuda;
using Spd.Presentacion.Navegacion;
using Spd.Presentacion.ViewModels;

namespace Spd.Presentacion;

/// <summary>Construye el ViewModel de cada sección a demanda (Spec 015 H1.4, contrato en
/// `specs/015-rediseno-interfaz/contracts/navegacion-y-tema.md`).
///
/// A demanda y no al arrancar: el Art. IX.4 exige que la aplicación llegue al login en menos de dos
/// segundos, y construir catorce pantallas que nadie ha pedido todavía es justo lo que no hay que
/// hacer.</summary>
public sealed class FabricaViewModels(ServiciosAplicacion servicios, Navegador navegador, Usuario usuario)
{
    public object? Crear(Destino destino) => destino.Seccion switch
    {
        Seccion.Inicio => new InicioViewModel(servicios.Avisos, navegador),
        Seccion.Pacientes => CrearPacientes(destino),
        Seccion.Preparaciones => CrearPreparaciones(destino),
        Seccion.Retirada => CrearRetirada(destino),
        Seccion.Exportar => new ExportarPacientesViewModel(
            servicios.PerfilesImportacion, servicios.ExportacionPacientes, servicios.Pacientes),
        Seccion.Catalogo => CrearCatalogo(destino),
        Seccion.RevisionNomenclator => new RevisionNomenclatorViewModel(servicios.ImportacionNomenclator, usuario.Id),
        Seccion.Calidad => new RegistrosCalidadViewModel(servicios.RegistrosCalidad, usuario.Id),
        Seccion.ControlDocumental => new ControlDocumentalViewModel(servicios.ControlDocumental, usuario.Id),
        Seccion.Farmacia => new FarmaciaViewModel(servicios.Farmacia, servicios.GestorLogo, servicios.Backup, usuario.Id),
        Seccion.Usuarios => new UsuariosViewModel(servicios.Usuarios, usuario.Id),
        Seccion.Actualizaciones => new ActualizacionesViewModel(servicios.Actualizaciones, usuario.Id),
        Seccion.Nomenclator => new NomenclatorViewModel(servicios.Farmacia, servicios.Nomenclator, usuario.Id),
        Seccion.Seguridad => new SeguridadViewModel(servicios.Cifrado, servicios.Purga, usuario.Id),
        Seccion.Perfiles => new PerfilesImportacionViewModel(servicios.PerfilesImportacion, usuario.Id),
        Seccion.Ayuda => CrearAyuda(destino.Detalle),
        _ => null
    };

    /// <summary>FR-1522: llegando desde la búsqueda global, la pantalla se abre con el texto ya
    /// puesto, que es lo que hace que el resultado "lleve" de verdad a lo buscado.</summary>
    private BuscadorPacientesViewModel CrearPacientes(Destino destino)
    {
        var vm = new BuscadorPacientesViewModel(
            servicios.Pacientes, servicios.Tratamientos, servicios.Medicamentos, servicios.Envases,
            servicios.ImportacionTratamiento, servicios.Comunicaciones, servicios.Preparacion,
            servicios.Documentos, servicios.Idoneidad, usuario.Id);
        if (destino.Detalle is { Length: > 0 } nombre) vm.Fragmento = nombre;
        return vm;
    }

    private CatalogoMedicamentosViewModel CrearCatalogo(Destino destino)
    {
        var vm = new CatalogoMedicamentosViewModel(
            servicios.Medicamentos, servicios.ImportacionNomenclator, servicios.ConsultaCima, navegador, usuario.Id);
        if (destino.Detalle is { Length: > 0 } texto) vm.Fragmento = texto;
        return vm;
    }

    /// <summary>FR-1511: si se llega desde un aviso, la pantalla se abre ya filtrada por ese
    /// paciente en vez de obligar a buscarlo entre todos.</summary>
    private PreparacionesViewModel CrearPreparaciones(Destino destino)
    {
        var vm = new PreparacionesViewModel(
            servicios.Preparacion, servicios.Pacientes, servicios.Usuarios, servicios.Medicamentos,
            servicios.Documentos, servicios.Comunicaciones, servicios.Lote, usuario.Id);
        if (destino.Detalle is { Length: > 0 } nombre)
        {
            vm.FiltroPaciente = nombre;
            vm.SoloPendientes = false;
            vm.CargarCommand.Execute(null);
        }
        return vm;
    }

    private RetiradaEnvasesViewModel CrearRetirada(Destino destino)
    {
        var vm = new RetiradaEnvasesViewModel(servicios.ListadoRetirada, servicios.Envases, usuario.Id);
        if (destino.PacienteId is { } id) vm.CentrarEnPaciente(id, destino.Detalle);
        return vm;
    }

    /// <summary>`Detalle` llega como "seccion:apartado" desde F1 y desde los botones "¿Por qué?".</summary>
    private static AyudaViewModel CrearAyuda(string? detalle)
    {
        var vm = new AyudaViewModel(IndiceAyuda.Global);
        var partes = detalle?.Split(':', 2);
        if (partes is [var seccion, var apartado]) vm.Seleccionar(seccion, apartado);
        return vm;
    }
}
