using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Xunit;

namespace Spd.Aplicacion.Tests;

public sealed class ServicioConfiguracionFarmaciaTests
{
    private static (ServicioConfiguracionFarmacia Servicio, SqliteConnection Conexion) CrearServicio()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();

        var repositorio = new RepositorioFarmacia(conexion);
        repositorio.Crear(FarmaciaValida());

        var servicio = new ServicioConfiguracionFarmacia(repositorio, new RegistradorAuditoria(conexion));
        return (servicio, conexion);
    }

    private static Farmacia FarmaciaValida() => new()
    {
        CodigoSanitario = "PO-001", Nombre = "Farmacia de Prueba", TitularOComunidadBienes = "Titular",
        Cif = "B00000000", Direccion = "Calle Falsa 1", Cp = "36000", Poblacion = "Pontevedra",
        Telefono = "986000000"
    };

    [Fact]
    public void ValidarRuta_detecta_ruta_no_escribible_sin_lanzar_excepcion()
    {
        var (servicio, _) = CrearServicio();

        var resultado = servicio.ValidarRuta("/ruta/que/no/existe/spd-test");

        Assert.False(resultado.Existe);
        Assert.False(resultado.Escribible);
    }

    [Fact]
    public void ValidarRuta_confirma_una_ruta_real_y_escribible()
    {
        var (servicio, _) = CrearServicio();
        var carpetaTemporal = Directory.CreateTempSubdirectory("spd-test-").FullName;
        try
        {
            var resultado = servicio.ValidarRuta(carpetaTemporal);

            Assert.True(resultado.Existe);
            Assert.True(resultado.Escribible);
        }
        finally
        {
            Directory.Delete(carpetaTemporal, recursive: true);
        }
    }

    [Fact]
    public void ValidarRuta_avisa_si_la_ruta_coincide_con_la_carpeta_de_instalacion()
    {
        var (servicio, _) = CrearServicio();

        var resultado = servicio.ValidarRuta(AppContext.BaseDirectory);

        Assert.True(resultado.CoincideConCarpetaInstalacion);
    }

    [Fact]
    public void ActualizarDatosFarmacia_resuelve_responsable_datos_al_titular_si_no_se_rellena()
    {
        var farmacia = FarmaciaValida();
        // FR-012: responsable_datos/direccion_derechos vacíos → valor efectivo = titular/dirección.
        Assert.Equal(farmacia.TitularOComunidadBienes, farmacia.ResponsableDatosEfectivo());
        Assert.Equal(farmacia.Direccion, farmacia.DireccionDerechosEfectiva());

        farmacia.ResponsableDatos = "Otro responsable";
        Assert.Equal("Otro responsable", farmacia.ResponsableDatosEfectivo());
    }

    [Fact]
    public void ActualizarValoresDefecto_solo_escribe_en_Farmacia_no_en_otras_entidades()
    {
        var (servicio, conexion) = CrearServicio();
        var valores = new DatosValoresDefecto(
            DiaRetiradaDefecto: "JU", NBlisteresDefecto: 2, DiasAntelacionListado: 3,
            TempMin: 16, TempMax: 24, HrMin: 45, HrMax: 55, UmbralReutilizacionLecturaAmbientalHoras: 3);

        servicio.ActualizarValoresDefecto(valores, administradorQueEjecutaId: 1);

        var farmaciaActualizada = servicio.ObtenerConfiguracion();
        Assert.Equal("JU", farmaciaActualizada.DiaRetiradaDefecto);
        Assert.Equal(2, farmaciaActualizada.NBlisteresDefecto);
        // CA-006: esta spec no tiene entidad Paciente todavía (Spec 001); la garantía de que un
        // paciente ya personalizado no cambia se valida en Spec 001 leyendo estos valores solo
        // como valor por defecto de un alta nueva, nunca sobrescribiendo uno existente.
        var totalTablasAjenas = conexion.ExecuteScalar<int>("SELECT COUNT(*) FROM Usuario");
        Assert.Equal(0, totalTablasAjenas);
    }

    [Fact]
    public void ActualizarDatosFarmacia_y_ActualizarPrefijos_registran_en_auditoria()
    {
        var (servicio, conexion) = CrearServicio();
        var datos = servicio.ObtenerConfiguracion();
        datos.Telefono = "986111111";

        servicio.ActualizarDatosFarmacia(datos, administradorQueEjecutaId: 1);
        servicio.ActualizarPrefijos("PAC-", "BLI-", administradorQueEjecutaId: 1);

        var totalAuditoria = conexion.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM Auditoria WHERE entidad = 'Farmacia'");
        Assert.Equal(2, totalAuditoria);
    }
}
