using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Xunit;

namespace Spd.Aplicacion.Tests;

public sealed class ServicioMedicamentosTests
{
    private static (ServicioMedicamentos Servicio, SqliteConnection Conexion) CrearServicio()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();

        var servicio = new ServicioMedicamentos(new RepositorioMedicamentos(conexion), new RegistradorAuditoria(conexion));
        return (servicio, conexion);
    }

    [Fact]
    public void Crear_guarda_con_solo_cn_y_nombre_CA_300()
    {
        var (servicio, _) = CrearServicio();

        var medicamento = servicio.Crear(new DatosAltaMedicamento("654321", "Paracetamol 1g"), null);

        Assert.Equal("654321", medicamento.Cn);
        Assert.Null(medicamento.FormaFarmaceutica);
        Assert.True(medicamento.Activo);
    }

    [Fact]
    public void Crear_bloquea_un_cn_ya_activo_CA_303()
    {
        var (servicio, _) = CrearServicio();
        servicio.Crear(new DatosAltaMedicamento("654321", "Paracetamol 1g"), null);

        var excepcion = Assert.Throws<MedicamentoDuplicadoException>(
            () => servicio.Crear(new DatosAltaMedicamento("654321", "Otro nombre"), null));
        Assert.Equal("654321", excepcion.Existente.Cn);
    }

    [Fact]
    public void Crear_reactiva_un_cn_dado_de_baja_en_vez_de_duplicar_CA_305()
    {
        var (servicio, _) = CrearServicio();
        var original = servicio.Crear(new DatosAltaMedicamento("111111", "Ibuprofeno 600mg"), null);
        servicio.DarDeBaja(original.Id, null);

        var reactivado = servicio.Crear(new DatosAltaMedicamento("111111", "Ibuprofeno 600mg"), null);

        Assert.Equal(original.Id, reactivado.Id);
        Assert.True(reactivado.Activo);
    }

    [Fact]
    public void ActualizarDatos_exige_motivo_solo_si_apto_spd_difiere_del_derivado_CA_302()
    {
        var (servicio, _) = CrearServicio();
        var medicamento = servicio.Crear(new DatosAltaMedicamento("654321", "Paracetamol 1g"), null);
        medicamento.FormaFarmaceutica = FormaFarmaceutica.Comprimido; // derivado: apto = true
        medicamento.AptoSpd = false; // distinto del derivado
        medicamento.MotivoNoApto = null;

        Assert.Throws<ErrorValidacionException>(() => servicio.ActualizarDatos(medicamento, null));

        medicamento.MotivoNoApto = "Informe del laboratorio indica riesgo de fraccionamiento.";
        servicio.ActualizarDatos(medicamento, null); // no debe lanzar
    }

    [Fact]
    public void ActualizarDescripcionFisica_versiona_la_anterior_en_dos_cambios_sucesivos_CA_301()
    {
        var (servicio, _) = CrearServicio();
        var medicamento = servicio.Crear(new DatosAltaMedicamento("654321", "Paracetamol 1g"), null);

        servicio.ActualizarDescripcionFisica(medicamento.Id,
            new DatosDescripcionFisica("comprimido", "blanco", null, null, null, "comprimido blanco"), null);
        servicio.ActualizarDescripcionFisica(medicamento.Id,
            new DatosDescripcionFisica("comprimido", "amarillo", null, null, null, "comprimido amarillo"), null);

        // Dos cambios desde el estado inicial (vacío) generan dos versiones: la vacía y "comprimido
        // blanco"; ambos son "un cambio de descripción física" en el sentido de FR-304.
        var historial = servicio.ListarHistorialDescripcion(medicamento.Id);
        Assert.Equal(2, historial.Count);
        Assert.Contains(historial, v => v.DescTexto == "comprimido blanco");

        var actual = servicio.ObtenerPorId(medicamento.Id)!;
        Assert.Equal("comprimido amarillo", actual.DescTexto); // el SPD entregado seguiría citando su propia instantánea, fuera de esta spec
    }

    [Fact]
    public void ProponerDescripcionTexto_construye_el_texto_sin_persistir_nada()
    {
        var (servicio, _) = CrearServicio();

        var propuesta = servicio.ProponerDescripcionTexto(
            new DatosDescripcionFisica("comprimido", "blanco", "ranurado", null, "grande", null));

        Assert.Equal("comprimido blanco, ranura ranurado, grande", propuesta);
    }

    [Fact]
    public void Buscar_encuentra_por_cn_exacto_y_por_fragmento_de_nombre_sin_tildes_FR_305()
    {
        var (servicio, _) = CrearServicio();
        servicio.Crear(new DatosAltaMedicamento("654321", "Paracetamol 1g"), null);
        servicio.Crear(new DatosAltaMedicamento("111111", "Ácido acetilsalicílico 500mg"), null);

        Assert.Single(servicio.Buscar("654321"));
        Assert.Single(servicio.Buscar("acido acetilsalicilico"));
    }

    [Fact]
    public void ActualizarUnidadesEnvase_fija_origen_manual_FR_310_FR_311()
    {
        var (servicio, conexion) = CrearServicio();
        var medicamento = servicio.Crear(new DatosAltaMedicamento("654321", "Paracetamol 1g"), null);

        servicio.ActualizarUnidadesEnvase(medicamento.Id, 20, null);

        var fila = conexion.QuerySingle(
            "SELECT unidades_envase, unidades_envase_origen FROM Medicamento WHERE id = @id", new { id = medicamento.Id });
        Assert.Equal(20L, (long)fila.unidades_envase);
        Assert.Equal("MANUAL", (string)fila.unidades_envase_origen);
    }

    [Fact]
    public void DarDeBaja_no_elimina_la_fila_solo_marca_activo_0_Art_III_1()
    {
        var (servicio, conexion) = CrearServicio();
        var medicamento = servicio.Crear(new DatosAltaMedicamento("654321", "Paracetamol 1g"), null);

        servicio.DarDeBaja(medicamento.Id, null);

        var totalFilas = conexion.ExecuteScalar<int>("SELECT COUNT(*) FROM Medicamento WHERE id = @id", new { id = medicamento.Id });
        Assert.Equal(1, totalFilas);
        var activo = conexion.ExecuteScalar<long>("SELECT activo FROM Medicamento WHERE id = @id", new { id = medicamento.Id });
        Assert.Equal(0L, activo);
    }

    [Fact]
    public void Crear_ActualizarDatos_ActualizarDescripcionFisica_ActualizarUnidadesEnvase_y_DarDeBaja_registran_en_auditoria()
    {
        var (servicio, conexion) = CrearServicio();
        var medicamento = servicio.Crear(new DatosAltaMedicamento("654321", "Paracetamol 1g"), null);

        medicamento.PrincipioActivo = "Paracetamol";
        servicio.ActualizarDatos(medicamento, null);
        servicio.ActualizarDescripcionFisica(medicamento.Id,
            new DatosDescripcionFisica("comprimido", "blanco", null, null, null, "comprimido blanco"), null);
        servicio.ActualizarUnidadesEnvase(medicamento.Id, 20, null);
        servicio.DarDeBaja(medicamento.Id, null);

        var acciones = conexion.Query<string>("SELECT accion FROM Auditoria WHERE entidad = 'Medicamento' ORDER BY id").ToList();
        Assert.Contains("ALTA", acciones);
        Assert.Contains("EDITAR", acciones);
        Assert.Contains("EDITAR_DESCRIPCION", acciones);
        Assert.Contains("EDITAR_UNIDADES_ENVASE", acciones);
        Assert.Contains("BAJA", acciones);
    }
}
