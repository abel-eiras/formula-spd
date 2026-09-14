using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Microsoft.Data.Sqlite;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Spd.Presentacion.ViewModels;
using Spd.Presentacion.Views;
using Spd.Presentacion.Views.Pacientes;
using Xunit;

namespace Spd.Presentacion.Tests;

/// <summary>Cambios del 2026-09-14 pedidos por el propietario: el medicamento del tratamiento se busca y,
/// si no está, se da de alta desde la propia ficha; el nomenclátor entra entero en el catálogo; y la tabla
/// de retirada enseña el CIP.</summary>
public sealed class SelectorMedicamentoTests
{
    private static (ServiciosAplicacion Servicios, SqliteConnection Conexion) Montar()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();
        new RepositorioFarmacia(conexion).Crear(new Farmacia
        {
            CodigoSanitario = "PO-001", Nombre = "F", TitularOComunidadBienes = "T", Cif = "B0",
            Direccion = "D", Cp = "36000", Poblacion = "P", Telefono = "9"
        });
        return (FabricaServiciosTest.Todos(conexion), conexion);
    }

    [Fact]
    public void Busca_por_nombre_sin_tildes_o_por_cn_y_no_con_una_sola_letra()
    {
        var (servicios, conexion) = Montar();
        using var c = conexion;
        servicios.Medicamentos.Crear(new DatosAltaMedicamento("654321", "Enalapril 20 mg"), null);
        var selector = new SelectorMedicamentoViewModel(servicios.Medicamentos, null, null);

        selector.Texto = "e";
        Assert.Empty(selector.Resultados);
        selector.Texto = "ENALÁPRIL";
        Assert.Single(selector.Resultados);
        selector.Texto = "654321";
        Assert.Single(selector.Resultados);
    }

    /// <summary>Con el nomenclátor entero, los de baja también están. Elegir uno lo reactiva (CA-305).</summary>
    [Fact]
    public void Elegir_un_medicamento_de_baja_lo_reactiva_sin_duplicarlo_CA_305()
    {
        var (servicios, conexion) = Montar();
        using var c = conexion;
        var deBaja = new Medicamento { Cn = "700001", Nombre = "Lamotrigina 100 mg", NombreNormalizado = "LAMOTRIGINA 100 MG", Activo = false };
        deBaja.Id = new RepositorioMedicamentos(conexion).Crear(deBaja);
        var selector = new SelectorMedicamentoViewModel(servicios.Medicamentos, null, null);

        selector.Texto = "lamotri";
        selector.ElegirCommand.Execute(Assert.Single(selector.Resultados));

        Assert.Equal(deBaja.Id, selector.Seleccionado!.Id);
        Assert.True(servicios.Medicamentos.ObtenerPorId(deBaja.Id)!.Activo);
        Assert.Contains("reactivado", selector.Mensaje);
    }

    [Fact]
    public void Un_medicamento_que_no_esta_se_da_de_alta_aqui_y_queda_elegido_sin_aptitud_confirmada()
    {
        var (servicios, conexion) = Montar();
        using var c = conexion;
        var selector = new SelectorMedicamentoViewModel(servicios.Medicamentos, null, null);

        selector.Texto = "999999";
        Assert.Empty(selector.Resultados);
        selector.AbrirNuevoCommand.Execute(null);
        Assert.Equal("999999", selector.NuevoCn);   // lo tecleado eran cifras: es el CN

        selector.NuevoNombre = "Fórmula magistral omeprazol 10 mg";
        selector.CrearNuevoCommand.Execute(null);

        Assert.False(selector.PanelNuevoAbierto);
        Assert.Equal("999999", selector.Seleccionado!.Cn);
        Assert.Null(servicios.Medicamentos.ObtenerPorCn("999999")!.AptoSpd);
    }

    [Fact]
    public void Dar_de_alta_un_cn_que_ya_estaba_elige_el_existente_en_vez_de_fallar()
    {
        var (servicios, conexion) = Montar();
        using var c = conexion;
        var existente = servicios.Medicamentos.Crear(new DatosAltaMedicamento("654321", "Enalapril 20 mg"), null);
        var selector = new SelectorMedicamentoViewModel(servicios.Medicamentos, null, null);

        selector.AbrirNuevoCommand.Execute(null);
        selector.NuevoCn = "654321";
        selector.NuevoNombre = "Otro nombre";
        selector.CrearNuevoCommand.Execute(null);

        Assert.Equal(existente.Id, selector.Seleccionado!.Id);
        Assert.Contains("ya estaba", selector.Mensaje);
    }

    /// <summary>El flujo completo en la pestaña: alta del medicamento sin salir y tratamiento guardado.</summary>
    [AvaloniaFact]
    public void El_tratamiento_se_guarda_con_un_medicamento_dado_de_alta_desde_la_propia_ficha()
    {
        var (servicios, conexion) = Montar();
        using var c = conexion;
        var paciente = servicios.Pacientes.Crear(
            new DatosAltaPaciente("Ana", "Ríos", null, "12345678Z", null, null, null, null, null, null,
                null, null, null, null, null, null, null, false, null, null, 1), null);
        var vm = new TratamientoViewModel(servicios.Tratamientos, servicios.Medicamentos, servicios.Comunicaciones,
            servicios.Documentos, servicios.Medicos, paciente.Id, usuarioActualId: null, servicioConsultaCima: servicios.ConsultaCima);
        var ventana = AnfitrionDeVista.Anfitrion(new TratamientoView { DataContext = vm });
        ventana.Show();

        vm.GuardarCommand.Execute(null);
        Assert.Contains("Elige el medicamento", vm.Mensaje);

        vm.SelectorMedicamento.Texto = "Fórmula";
        vm.SelectorMedicamento.AbrirNuevoCommand.Execute(null);
        vm.SelectorMedicamento.NuevoCn = "999999";
        vm.SelectorMedicamento.CrearNuevoCommand.Execute(null);
        ventana.UpdateLayout();
        vm.PautaD = "1";
        vm.GuardarCommand.Execute(null);

        var tratamiento = Assert.Single(vm.Vigentes);
        Assert.Equal("Fórmula", tratamiento.NombreMedicamento);
    }

    /// <summary>Spec 005 FR-532: el CIP es una de las columnas del listado de retirada.</summary>
    [AvaloniaFact]
    public void La_tabla_de_retirada_de_envases_muestra_el_cip_FR_532()
    {
        var (servicios, conexion) = Montar();
        using var c = conexion;
        var ventana = AnfitrionDeVista.Anfitrion(new RetiradaEnvasesView
        {
            DataContext = new RetiradaEnvasesViewModel(servicios.ListadoRetirada, servicios.Envases, null)
        });
        ventana.Show();

        var tabla = ventana.GetVisualDescendants().OfType<DataGrid>().Single();
        Assert.Contains("CIP", tabla.Columns.Select(col => col.Header as string));
    }

    [Fact]
    public void Desde_la_pantalla_del_nomenclator_se_importa_el_fichero_descargado()
    {
        var (servicios, conexion) = Montar();
        using var c = conexion;
        var ruta = Path.Combine(Path.GetTempPath(), $"nomenclator-{System.Guid.NewGuid():N}.csv");
        File.WriteAllText(ruta,
            "Código Nacional,Nombre del producto farmacéutico,Tipo de fármaco,Estado\n" +
            "650004,DEPAKINE 500 mg,Medicamento Etica,ALTA\n" +
            "700001,LAMOTRIGINA 100MG,Medicamento Generico,BAJA GENERAL\n");
        try
        {
            var vm = new NomenclatorViewModel(servicios.Farmacia, servicios.Nomenclator, null, servicios.ImportacionNomenclator)
            {
                RutaFichero = ruta
            };

            vm.ImportarDescargadoCommand.Execute(null);

            Assert.Contains("Medicamentos nuevos: 1", vm.Mensaje);
            Assert.Contains("de baja (quedan inactivos): 1", vm.Mensaje);
            Assert.NotNull(servicios.Medicamentos.ObtenerPorCn("650004"));
        }
        finally
        {
            File.Delete(ruta);
        }
    }
}
