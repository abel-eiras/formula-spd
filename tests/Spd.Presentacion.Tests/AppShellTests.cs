using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Avalonia.Headless.XUnit;
using Microsoft.Data.Sqlite;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Spd.Presentacion.Navegacion;
using Spd.Presentacion.ViewModels;
using Spd.Presentacion.Views;
using Xunit;

namespace Spd.Presentacion.Tests;

/// <summary>Spec 015 CA-1500/1501/1503: el marco único navega a todas las secciones sin abrir
/// ventanas, respeta el rol y lleva los contadores del menú.</summary>
public sealed class AppShellTests
{
    private static (AppShellViewModel Vm, Navegador Navegador, SqliteConnection Conexion) Crear(Rol rol)
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();
        new RepositorioFarmacia(conexion).Crear(new Farmacia
        {
            CodigoSanitario = "PO-001", Nombre = "Farmacia de Prueba", TitularOComunidadBienes = "Titular",
            Cif = "B00000000", Direccion = "Calle Falsa 1", Cp = "36000", Poblacion = "Pontevedra", Telefono = "986000000"
        });

        var servicios = FabricaServiciosTest.Todos(conexion);
        // El usuario tiene que existir de verdad: servicios como el control documental comprueban el
        // rol contra el repositorio, no contra lo que diga la interfaz (Spec 009 FR-942).
        var usuario = servicios.Usuarios.CrearUsuario(
            new DatosAltaUsuario("Ana", "Ruiz", "ana", "contraseña-inicial", rol, null, null),
            administradorQueEjecutaId: null).Usuario;
        var navegador = new Navegador(rol == Rol.Administrador);
        var vm = new AppShellViewModel(new FabricaViewModels(servicios, navegador, usuario), navegador, servicios.Avisos, servicios.BusquedaGlobal, usuario);
        return (vm, navegador, conexion);
    }

    [AvaloniaFact]
    public void Navega_a_cada_seccion_sin_abrir_ventanas_CA_1500()
    {
        var (vm, navegador, conexion) = Crear(Rol.Administrador);
        using var c = conexion;
        var ventana = AnfitrionDeVista.Anfitrion(new AppShellContenido());
        ventana.Content = null;

        // Arranca en Inicio (el constructor navega) y cada sección construye su propia pantalla.
        Assert.IsType<InicioViewModel>(vm.Contenido);

        foreach (var entrada in EntradaNavegacion.Todas)
        {
            navegador.Navegar(new Destino(entrada.Seccion));
            Assert.True(vm.Contenido is not null, $"La sección {entrada.Seccion} no produjo ninguna pantalla.");
            Assert.Equal(entrada.Titulo, vm.TituloSeccion);
        }

        // Las tres secciones que no están en el menú (se llega desde el buscador de pacientes,
        // desde el catálogo y desde F1).
        navegador.Navegar(new Destino(Seccion.Paciente));
        Assert.IsType<PacienteWorkspaceViewModel>(vm.Contenido);
        navegador.Navegar(new Destino(Seccion.RevisionNomenclator));
        Assert.IsType<RevisionNomenclatorViewModel>(vm.Contenido);
        navegador.Navegar(new Destino(Seccion.Ayuda, Detalle: "procedimiento:verificacion"));
        Assert.Equal("verificacion", Assert.IsType<AyudaViewModel>(vm.Contenido).EntradaSeleccionada!.Id);
    }

    [AvaloniaFact]
    public void La_vista_de_cada_seccion_se_realiza_sin_lanzar()
    {
        var (vm, navegador, conexion) = Crear(Rol.Administrador);
        using var c = conexion;

        foreach (var entrada in EntradaNavegacion.Todas)
        {
            navegador.Navegar(new Destino(entrada.Seccion));
            var vista = new ViewLocator().Build(vm.Contenido);
            Assert.NotNull(vista);
            Assert.False(vista is TextBlock, $"No se encontró la vista de {entrada.Seccion}.");

            // Spec 014 FR-1412 / Spec 015 FR-1505: toda pantalla alcanzable tiene su apartado de ayuda.
            Assert.True(AyudaContextual.SeccionPorVista.ContainsKey(vista!.GetType().Name),
                $"La vista {vista.GetType().Name} ({entrada.Seccion}) no está en la tabla de ayuda contextual.");

            vista.DataContext = vm.Contenido;
            var ventana = AnfitrionDeVista.Anfitrion(vista);
            ventana.Show();
            ventana.Close();
        }
    }

    [AvaloniaFact]
    public void Un_elaborador_no_ve_ni_alcanza_las_secciones_de_administracion_CA_1500()
    {
        var (vm, navegador, conexion) = Crear(Rol.Elaborador);
        using var c = conexion;

        Assert.Empty(vm.ItemsAdministracion);
        Assert.False(vm.EsAdministrador);
        Assert.All(vm.ItemsTrabajo, i => Assert.False(i.Entrada.SoloAdministrador));

        navegador.Navegar(new Destino(Seccion.Seguridad));
        Assert.IsNotType<SeguridadViewModel>(vm.Contenido);
    }

    [AvaloniaFact]
    public void Los_contadores_del_menu_reflejan_los_avisos_CA_1501()
    {
        var (vm, _, conexion) = Crear(Rol.Administrador);
        using var c = conexion;

        // Sin datos no hay faltantes; el contador de retirada queda vacío, no en cero.
        var retirada = Assert.Single(vm.ItemsTrabajo, i => i.Entrada.Seccion == Seccion.Retirada);
        Assert.False(retirada.TieneContador);
        Assert.Equal(string.Empty, retirada.ContadorTexto);
    }

    [AvaloniaFact]
    public void Atras_vuelve_al_destino_anterior_CA_1500()
    {
        var (vm, navegador, conexion) = Crear(Rol.Administrador);
        using var c = conexion;

        navegador.Navegar(new Destino(Seccion.Pacientes));
        navegador.Navegar(new Destino(Seccion.Preparaciones));
        Assert.True(vm.PuedeVolver);

        navegador.Atras();
        Assert.IsType<BuscadorPacientesViewModel>(vm.Contenido);
    }

    /// <summary>Spec 015 CA-1513: la búsqueda de la cabecera encuentra desde cualquier sección y
    /// lleva a la pantalla del resultado, sin abrir ventana ni perder el marco.</summary>
    [AvaloniaFact]
    public void La_busqueda_global_encuentra_un_paciente_y_navega_a_su_pantalla_CA_1513()
    {
        var (vm, navegador, conexion) = Crear(Rol.Administrador);
        using var c = conexion;

        new ServicioPacientes(new RepositorioPacientes(conexion), new RepositorioFarmacia(conexion),
            new RegistradorAuditoria(conexion))
            .Crear(new DatosAltaPaciente("María", "López Pérez", null, "12345678Z", null, null, null, null, null, null,
                null, null, null, null, null, null, null, false, null, null, 1), null);

        // Una sola letra no debe abrir nada: sería ruido, no una búsqueda.
        vm.TextoBusqueda = "l";
        Assert.False(vm.BusquedaAbierta);

        vm.TextoBusqueda = "lopez";
        Assert.True(vm.BusquedaAbierta);
        var resultado = Assert.Single(vm.ResultadosBusqueda, r => r.Tipo == TipoResultadoBusqueda.Paciente);

        vm.AbrirResultadoCommand.Execute(resultado);

        Assert.Equal(Seccion.Pacientes, navegador.Actual!.Seccion);
        // Al elegir un resultado la lista se cierra y el campo queda limpio para la siguiente.
        Assert.False(vm.BusquedaAbierta);
        Assert.Equal(string.Empty, vm.TextoBusqueda);
    }

    /// <summary>Contenido mínimo para tener una ventana viva durante el test.</summary>
    private sealed class AppShellContenido : UserControl;

    /// <summary>spec-012 FR-1207, el flujo del mostrador con teclas de verdad: Ctrl+F, el lector teclea
    /// el código y un Intro, la aplicación va sola a la retirada del único paciente al que le falta
    /// ese medicamento con lote, serie, caducidad y unidades ya puestos, y otro Intro guarda.</summary>
    [AvaloniaFact]
    public void Escanear_en_la_busqueda_global_lleva_a_la_retirada_y_un_Intro_registra_el_envase()
    {
        using var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();
        new RepositorioFarmacia(conexion).Crear(new Farmacia
        {
            CodigoSanitario = "PO-001", Nombre = "Farmacia de Prueba", TitularOComunidadBienes = "Titular",
            Cif = "B00000000", Direccion = "Calle Falsa 1", Cp = "36000", Poblacion = "Pontevedra", Telefono = "986000000"
        });
        var servicios = FabricaServiciosTest.Todos(conexion);
        var usuario = servicios.Usuarios.CrearUsuario(
            new DatosAltaUsuario("Ana", "Ruiz", "ana", "contraseña-inicial", Rol.Elaborador, null, null),
            administradorQueEjecutaId: null).Usuario;

        var diaRetiradaHoy = DiasSemana.Codigos[((int)System.DateTime.Today.DayOfWeek + 6) % 7];
        var paciente = servicios.Pacientes.Crear(
            new DatosAltaPaciente("José", "Núñez", null, "12345678Z", null, null, null, null, null, null,
                null, null, null, null, null, null, null, false, null, diaRetiradaHoy, 1), null);
        servicios.Pacientes.CambiarEstado(paciente.Id, EstadoPaciente.Activo, null, null);
        var medicamento = new Medicamento { Cn = "654321", Nombre = "Enalapril 20 mg", UnidadesEnvase = 28, Gtin = "08470006950647" };
        medicamento.Id = new RepositorioMedicamentos(conexion).Crear(medicamento);
        new RepositorioTratamientos(conexion).Crear(new Tratamiento
        {
            PacienteId = paciente.Id, MedicamentoId = medicamento.Id, EnSpd = true, PautaD = FraccionDosis.Uno,
            FechaInicio = new System.DateOnly(2026, 1, 1), FechaPrescripcionInicial = new System.DateOnly(2026, 1, 1)
        });

        var navegador = new Navegador(esAdministrador: false);
        var vm = new AppShellViewModel(new FabricaViewModels(servicios, navegador, usuario), navegador, servicios.Avisos, servicios.BusquedaGlobal, usuario);
        var ventana = new AppShell { DataContext = vm, Width = 1300, Height = 800 };
        ventana.Show();
        Dispatcher.UIThread.RunJobs();

        ventana.KeyPress(Key.F, RawInputModifiers.Control, PhysicalKey.F, null);
        Assert.True(ventana.FindControl<TextBox>("CampoBusqueda")!.IsFocused);

        // Como lo haría el lector USB: sin separador GS visible, y un Intro al final.
        ventana.KeyTextInput("01084700069506471730123110LOTE921SERIE9");
        ventana.KeyPress(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, null);
        ventana.UpdateLayout();          // la sección nueva se maqueta y engancha al árbol visual...
        Dispatcher.UIThread.RunJobs();   // ...y entonces se ejecuta el cambio de foco diferido.

        var retirada = Assert.IsType<RetiradaEnvasesViewModel>(vm.Contenido);
        Assert.Equal("SERIE9", retirada.Serie);
        Assert.Equal(28, retirada.UnidadesIniciales);
        var unidades = ventana.GetVisualDescendants().OfType<NumericUpDown>().Single(n => n.Name == "CampoUnidades");
        Assert.True(unidades.IsKeyboardFocusWithin);

        ventana.KeyPress(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, null);
        Dispatcher.UIThread.RunJobs();

        var envase = Assert.Single(servicios.Envases.ListarEnCustodiaDePaciente(paciente.Id));
        Assert.Equal("LOTE9", envase.Lote);
        Assert.Equal(OrigenEnvase.Escaneado, envase.Origen);
    }
}
