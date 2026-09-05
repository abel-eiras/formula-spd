using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Aplicacion;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Xunit;

namespace Spd.Aplicacion.Tests;

public sealed class ServicioImportacionNomenclatorTests
{
    private static (ServicioImportacionNomenclator Servicio, ServicioMedicamentos Medicamentos, SqliteConnection Conexion) CrearServicio()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();

        var repositorio = new RepositorioMedicamentos(conexion);
        var auditoria = new RegistradorAuditoria(conexion);
        var medicamentos = new ServicioMedicamentos(repositorio, auditoria);
        var servicio = new ServicioImportacionNomenclator(new LectorNomenclatorCsv(), repositorio, auditoria);
        return (servicio, medicamentos, conexion);
    }

    private static string EscribirCsvTemporal(string contenido)
    {
        var ruta = Path.GetTempFileName();
        File.WriteAllText(ruta, contenido);
        return ruta;
    }

    [Fact]
    public void CompararConNomenclator_distingue_nuevos_con_nombre_distinto_y_sin_cambios()
    {
        var (servicio, medicamentos, _) = CrearServicio();
        medicamentos.Crear(new DatosAltaMedicamento("111111", "Ibuprofeno 600mg"), null); // sin cambios
        medicamentos.Crear(new DatosAltaMedicamento("222222", "Nombre antiguo"), null); // nombre distinto
        var ruta = EscribirCsvTemporal(
            "CN,Nombre\n111111,Ibuprofeno 600mg\n222222,Nombre nuevo del laboratorio\n333333,Paracetamol 1g\n");

        var resultado = servicio.CompararConNomenclator(ruta);

        Assert.True(resultado.Exito);
        Assert.Equal(1, resultado.SinCambios);
        Assert.Single(resultado.ConNombreDistinto);
        Assert.Equal("Nombre nuevo del laboratorio", resultado.ConNombreDistinto[0].NombreNomenclator);
        Assert.Single(resultado.Nuevos);
        Assert.Equal("333333", resultado.Nuevos[0].Cn);
    }

    [Fact]
    public void AplicarAltaDesdeNomenclator_guarda_principio_activo_y_laboratorio_cuando_vienen_en_la_fila()
    {
        var (servicio, medicamentos, _) = CrearServicio();

        var creado = servicio.AplicarAltaDesdeNomenclator(
            new FilaNomenclator("650004", "Depakine 500 mg", "VALPROATO SODIO", "SANOFI AVENTIS, S.A"), null);

        var actual = medicamentos.ObtenerPorId(creado.Id)!;
        Assert.Equal("VALPROATO SODIO", actual.PrincipioActivo);
        Assert.Equal("SANOFI AVENTIS, S.A", actual.Laboratorio);
    }

    [Fact]
    public void AplicarNombreDesdeNomenclator_cambia_solo_el_nombre_CA_304()
    {
        var (servicio, medicamentos, conexion) = CrearServicio();
        var medicamento = medicamentos.Crear(new DatosAltaMedicamento("654321", "Paracetamol 1g"), null);
        medicamentos.ActualizarDescripcionFisica(medicamento.Id,
            new DatosDescripcionFisica("comprimido", "blanco", null, null, null, "comprimido blanco"), null);
        medicamento = medicamentos.ObtenerPorId(medicamento.Id)!; // recargar: Actualizar* escribe la fila completa
        medicamento.FormaFarmaceutica = Dominio.FormaFarmaceutica.Comprimido;
        medicamentos.ActualizarDatos(medicamento, null);

        servicio.AplicarNombreDesdeNomenclator(medicamento.Id, "Paracetamol 1g (nomenclátor)", null);

        var actual = medicamentos.ObtenerPorId(medicamento.Id)!;
        Assert.Equal("Paracetamol 1g (nomenclátor)", actual.Nombre);
        Assert.Equal("comprimido blanco", actual.DescTexto); // no tocado (FR-321, CA-304)
        Assert.True(actual.AptoSpd); // no tocado
    }

    [Fact]
    public void AplicarAltaDesdeNomenclator_y_AplicarNombreDesdeNomenclator_registran_en_auditoria()
    {
        var (servicio, medicamentos, conexion) = CrearServicio();
        var existente = medicamentos.Crear(new DatosAltaMedicamento("111111", "Nombre antiguo"), null);

        servicio.AplicarAltaDesdeNomenclator(new FilaNomenclator("333333", "Paracetamol 1g"), null);
        servicio.AplicarNombreDesdeNomenclator(existente.Id, "Nombre nuevo", null);

        var acciones = conexion.Query<string>("SELECT accion FROM Auditoria WHERE entidad = 'Medicamento' ORDER BY id").ToList();
        Assert.Contains("ALTA_DESDE_NOMENCLATOR", acciones);
        Assert.Contains("NOMBRE_DESDE_NOMENCLATOR", acciones);
    }
}
