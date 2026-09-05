using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Xunit;

namespace Spd.Aplicacion.Tests;

public sealed class ServicioRegistrosCalidadTests
{
    private static (ServicioRegistrosCalidad Servicio, SqliteConnection Conexion, int UsuarioId) CrearServicio()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();

        var repositorioFarmacia = new RepositorioFarmacia(conexion);
        repositorioFarmacia.Crear(new Farmacia
        {
            CodigoSanitario = "PO-001", Nombre = "Farmacia de Prueba", TitularOComunidadBienes = "Titular",
            Cif = "B00000000", Direccion = "Calle Falsa 1", Cp = "36000", Poblacion = "Pontevedra",
            Telefono = "986000000", TempMin = 15, TempMax = 25, HrMin = 40, HrMax = 60
        });

        var usuarioId = new RepositorioUsuarios(conexion).Crear(new Usuario
        {
            Nombre = "Eva", Apellidos = "Elaboradora", Login = "eva.elab",
            HashPassword = "hash-de-prueba", Rol = Rol.Elaborador
        });

        var servicio = new ServicioRegistrosCalidad(
            new RepositorioRegistrosCalidad(conexion), repositorioFarmacia, new RegistradorAuditoria(conexion));
        return (servicio, conexion, usuarioId);
    }

    [Fact]
    public void RegistrarAmbiental_marca_fuera_de_rango_una_lectura_fuera_del_rango_configurado_CA_900()
    {
        var (servicio, _, usuarioId) = CrearServicio();

        var registro = servicio.RegistrarAmbiental(new DatosRegistroAmbiental(27, 50, null), usuarioId);

        Assert.True(registro.FueraDeRango);
    }

    [Fact]
    public void RegistrarAmbiental_conserva_fuera_de_rango_aunque_cambie_la_configuracion_despues_CA_900()
    {
        var (servicio, conexion, usuarioId) = CrearServicio();
        var registro = servicio.RegistrarAmbiental(new DatosRegistroAmbiental(27, 50, null), usuarioId);

        var farmacia = new RepositorioFarmacia(conexion).Obtener()!;
        farmacia.TempMax = 30; // ahora 27 entraría en rango
        new RepositorioFarmacia(conexion).Actualizar(farmacia);

        var recargado = servicio.ListarAmbiental().Single(r => r.Id == registro.Id);
        Assert.True(recargado.FueraDeRango); // Art. IV: instantánea, no se recalcula
    }

    [Fact]
    public void RegistrarLimpieza_guarda_con_fecha_y_usuario_actuales_sin_pasos_adicionales_CA_901()
    {
        var (servicio, _, usuarioId) = CrearServicio();

        var registro = servicio.RegistrarLimpieza(TipoLimpieza.PrePreparacion, null, usuarioId);

        Assert.Equal(usuarioId, registro.UsuarioId);
        Assert.True((DateTime.UtcNow - registro.Fecha).TotalMinutes < 1);
    }

    [Fact]
    public void RegistrarAmbiental_y_RegistrarLimpieza_registran_en_auditoria()
    {
        var (servicio, conexion, usuarioId) = CrearServicio();

        servicio.RegistrarAmbiental(new DatosRegistroAmbiental(20, 50, null), usuarioId);
        servicio.RegistrarLimpieza(TipoLimpieza.Rutinaria, null, usuarioId);

        var acciones = conexion.Query<string>(
            "SELECT entidad FROM Auditoria WHERE entidad IN ('RegistroAmbiental', 'RegistroLimpieza') ORDER BY id").ToList();
        Assert.Contains("RegistroAmbiental", acciones);
        Assert.Contains("RegistroLimpieza", acciones);
    }

    [Fact]
    public void RegistrarFormacion_tres_veces_deja_las_tres_consultables_CA_902()
    {
        var (servicio, _, usuarioId) = CrearServicio();
        var hoy = DateOnly.FromDateTime(DateTime.Today);

        servicio.RegistrarFormacion(new DatosFormacion(usuarioId, "Curso 1", "Colegio", hoy, true), usuarioId);
        servicio.RegistrarFormacion(new DatosFormacion(usuarioId, "Curso 2", "Colegio", hoy, false), usuarioId);
        servicio.RegistrarFormacion(new DatosFormacion(usuarioId, "Curso 3", "Colegio", hoy, true), usuarioId);

        var formaciones = servicio.ListarFormacion(usuarioId);

        Assert.Equal(3, formaciones.Count);
        Assert.Contains(formaciones, f => f.NombreCurso == "Curso 1");
        Assert.Contains(formaciones, f => f.NombreCurso == "Curso 2");
        Assert.Contains(formaciones, f => f.NombreCurso == "Curso 3");
    }

    [Fact]
    public void RegistrarRecogidaResiduos_guarda_fecha_empresa_y_usuario()
    {
        var (servicio, _, usuarioId) = CrearServicio();

        var recogida = servicio.RegistrarRecogidaResiduos(
            new DatosRecogidaResiduos(DateOnly.FromDateTime(DateTime.Today), "Gestora S.L.", null), usuarioId);

        Assert.Equal("Gestora S.L.", recogida.EmpresaGestora);
        Assert.Equal(usuarioId, recogida.UsuarioId);
    }

    [Fact]
    public void RegistrarFormacion_y_RegistrarRecogidaResiduos_registran_en_auditoria()
    {
        var (servicio, conexion, usuarioId) = CrearServicio();

        servicio.RegistrarFormacion(
            new DatosFormacion(usuarioId, "Curso 1", null, DateOnly.FromDateTime(DateTime.Today), false), usuarioId);
        servicio.RegistrarRecogidaResiduos(
            new DatosRecogidaResiduos(DateOnly.FromDateTime(DateTime.Today), "Gestora S.L.", null), usuarioId);

        var acciones = conexion.Query<string>(
            "SELECT entidad FROM Auditoria WHERE entidad IN ('FormacionPersonal', 'RecogidaResiduos') ORDER BY id").ToList();
        Assert.Contains("FormacionPersonal", acciones);
        Assert.Contains("RecogidaResiduos", acciones);
    }
}
