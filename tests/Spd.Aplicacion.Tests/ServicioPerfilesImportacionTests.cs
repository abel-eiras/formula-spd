using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Xunit;

namespace Spd.Aplicacion.Tests;

public sealed class ServicioPerfilesImportacionTests
{
    private static (ServicioPerfilesImportacion Servicio, SqliteConnection Conexion) Crear()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();
        var servicio = new ServicioPerfilesImportacion(new RepositorioPerfilesImportacion(conexion), new RegistradorAuditoria(conexion));
        return (servicio, conexion);
    }

    private static readonly IReadOnlyList<ParCampoColumna> MapeoBasico =
        [new ParCampoColumna("Nombre", "0"), new ParCampoColumna("Apellidos", "1")];

    [Fact]
    public void Crear_y_ObtenerPorNombre_devuelve_el_mismo_mapeo_CA_1100()
    {
        var (servicio, conexion) = Crear();
        using var c = conexion;

        servicio.Crear(new DatosAltaPerfilImportacion("Mi perfil", TipoPerfilImportacion.Pacientes, ",", "UTF-8", true, MapeoBasico, null), null);

        var obtenido = servicio.ObtenerPorNombre("Mi perfil");
        Assert.NotNull(obtenido);
        Assert.Equal(MapeoBasico, obtenido!.Mapeo);
    }

    [Fact]
    public void Actualizar_conserva_el_id_y_el_nombre_tras_cambiar_el_mapeo_CA_1102()
    {
        var (servicio, conexion) = Crear();
        using var c = conexion;
        var creado = servicio.Crear(new DatosAltaPerfilImportacion("Mi perfil", TipoPerfilImportacion.Pacientes, ",", "UTF-8", true, MapeoBasico, null), null);

        var nuevoMapeo = new List<ParCampoColumna> { new("Nombre", "2"), new("Telefono1", "3") };
        servicio.Actualizar(creado.Id, new DatosAltaPerfilImportacion("Mi perfil", TipoPerfilImportacion.Pacientes, ";", "UTF-8", false, nuevoMapeo, null), null);

        var actualizado = servicio.ObtenerPorNombre("Mi perfil");
        Assert.Equal(creado.Id, actualizado!.Id);
        Assert.Equal(nuevoMapeo, actualizado.Mapeo);
        Assert.Equal(";", actualizado.Separador);
    }

    [Fact]
    public void ListarPorTipo_filtra_por_tipo_de_perfil()
    {
        var (servicio, conexion) = Crear();
        using var c = conexion;
        servicio.Crear(new DatosAltaPerfilImportacion("Perfil pacientes", TipoPerfilImportacion.Pacientes, ",", "UTF-8", true, MapeoBasico, null), null);
        servicio.Crear(new DatosAltaPerfilImportacion("Perfil nomenclator", TipoPerfilImportacion.Nomenclator, ",", "UTF-8", true, MapeoBasico, @"(\d+)\s*UDS"), null);

        var pacientes = servicio.ListarPorTipo(TipoPerfilImportacion.Pacientes);

        Assert.Single(pacientes);
        Assert.Equal("Perfil pacientes", pacientes[0].Nombre);
    }

    [Fact]
    public void Crear_registra_en_auditoria_Art_VII_6()
    {
        var (servicio, conexion) = Crear();
        using var c = conexion;

        servicio.Crear(new DatosAltaPerfilImportacion("Mi perfil", TipoPerfilImportacion.Pacientes, ",", "UTF-8", true, MapeoBasico, null), 7);

        var registros = conexion.Query<(string Accion, int? UsuarioId)>("SELECT accion, usuario_id FROM Auditoria WHERE entidad = 'PerfilImportacion'");
        Assert.Contains(registros, r => r.Accion == "ALTA_PERFIL_IMPORTACION" && r.UsuarioId == 7);
    }
}
