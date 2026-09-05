using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Xunit;

namespace Spd.Aplicacion.Tests;

public sealed class AsistentePrimerArranqueTests
{
    private static (ServicioAsistentePrimerArranque Servicio, SqliteConnection Conexion) CrearServicio()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();

        var servicio = new ServicioAsistentePrimerArranque(
            new RepositorioFarmacia(conexion),
            new RepositorioUsuarios(conexion),
            new HasheadorArgon2id(),
            new RegistradorAuditoria(conexion));
        return (servicio, conexion);
    }

    private static Farmacia FarmaciaValida() => new()
    {
        CodigoSanitario = "PO-001",
        Nombre = "Farmacia de Prueba",
        TitularOComunidadBienes = "Titular de Prueba",
        Cif = "B00000000",
        Direccion = "Calle Falsa 1",
        Cp = "36000",
        Poblacion = "Pontevedra",
        Telefono = "986000000"
    };

    [Fact]
    public void HayConfiguracionInicial_es_falso_sin_Farmacia_y_verdadero_tras_crearla()
    {
        var (servicio, _) = CrearServicio();
        Assert.False(servicio.HayConfiguracionInicial());

        servicio.EjecutarPasoFarmacia(FarmaciaValida());
        servicio.EjecutarPasoPrimerUsuario("Ana", "Administradora", "ana.admin", "contraseña-segura");
        servicio.FinalizarAsistente();

        Assert.True(servicio.HayConfiguracionInicial());
    }

    [Fact]
    public void EjecutarPasoFarmacia_lanza_ErrorValidacion_si_falta_un_campo_obligatorio()
    {
        var (servicio, _) = CrearServicio();
        var farmaciaIncompleta = FarmaciaValida();
        farmaciaIncompleta.Telefono = "";

        Assert.Throws<ErrorValidacionException>(() => servicio.EjecutarPasoFarmacia(farmaciaIncompleta));
    }

    [Fact]
    public void EjecutarPasoPrimerUsuario_lanza_ErrorValidacion_si_falta_el_login()
    {
        var (servicio, _) = CrearServicio();

        Assert.Throws<ErrorValidacionException>(
            () => servicio.EjecutarPasoPrimerUsuario("Ana", "Administradora", login: "", "contraseña"));
    }

    [Fact]
    public void FinalizarAsistente_registra_en_auditoria_la_creacion_de_Farmacia_y_del_primer_Usuario()
    {
        var (servicio, conexion) = CrearServicio();
        servicio.EjecutarPasoFarmacia(FarmaciaValida());
        servicio.EjecutarPasoPrimerUsuario("Ana", "Administradora", "ana.admin", "contraseña-segura");

        servicio.FinalizarAsistente();

        var accionesRegistradas = conexion.Query<string>(
            "SELECT entidad FROM Auditoria ORDER BY entidad").ToList();
        Assert.Equal(new[] { "Farmacia", "Usuario" }, accionesRegistradas);
    }
}
