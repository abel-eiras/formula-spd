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
        Assert.Null(actual.AptoSpd); // no tocado: sigue sin confirmar
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

    /// <summary>FR-320..FR-322 revisados el 2026-09-14: al descargar el nomenclátor se da de alta entero.
    /// Los nombres son copia literal del fichero de septiembre de 2026.</summary>
    [Fact]
    public void ImportarCompleto_da_de_alta_los_medicamentos_nuevos_y_no_toca_nada_de_lo_existente()
    {
        var (servicio, medicamentos, conexion) = CrearServicio();
        // Un medicamento que ya estaba, con decisiones clínicas tomadas: nada de esto debe cambiar.
        var existente = medicamentos.Crear(new DatosAltaMedicamento("654321", "Paracetamol 1g"), null);
        existente = medicamentos.ObtenerPorId(existente.Id)!;
        existente.AptoSpd = false;
        existente.MotivoNoApto = "Decisión del farmacéutico.";
        medicamentos.ActualizarDatos(existente, null);

        var ruta = EscribirCsvTemporal(
            "Código Nacional,Nombre del producto farmacéutico,Tipo de fármaco,Nombre genérico efecto y accesorio,Estado\n" +
            "650004,\"DEPAKINE 500 mg comprimidos gastrorresistentes, 20 comprimidos\",Medicamento Etica,,ALTA\n" +
            "700001,LAMOTRIGINA KERN PHARMA 100MG 56 COMPR DISPER EFG,Medicamento Generico,,BAJA GENERAL\n" +
            "700002,AMOXICILINA NORMON 250MG/5ML 120ML SUSP EXTEMP EFG,Medicamento Generico,,SUSPENSION TEMPORAL GENERAL\n" +
            "400011,MODERMA FLEX ABIERTA PLANA MINI OPACA 15-55MM 30U,,BOLSAS ILEOST RES SINT MIC FIL,ALTA\n" +
            "654321,PARACETAMOL CON OTRO NOMBRE EN EL NOMENCLATOR,Medicamento Generico,,ALTA\n");

        var resultado = servicio.ImportarCompleto(ruta, usuarioQueEjecutaId: 7);

        Assert.True(resultado.Exito);
        Assert.Equal(2, resultado.AltasActivas);      // alta y suspensión temporal
        Assert.Equal(1, resultado.AltasDeBaja);
        Assert.Equal(1, resultado.YaExistian);
        Assert.Equal(1, resultado.NoSonMedicamentos); // la bolsa de ostomía

        // Todo lo nuevo entra sin aptitud confirmada: el nomenclátor no dice si es apto.
        var depakine = medicamentos.ObtenerPorCn("650004")!;
        Assert.Null(depakine.AptoSpd);
        Assert.True(depakine.Activo);
        Assert.False(medicamentos.ObtenerPorCn("700001")!.Activo);   // de baja: entra inactivo
        Assert.True(medicamentos.ObtenerPorCn("700002")!.Activo);    // suspensión temporal: activo
        Assert.Null(medicamentos.ObtenerPorCn("400011"));            // accesorio: fuera
        Assert.Equal(7, conexion.ExecuteScalar<int>("SELECT creado_por FROM Medicamento WHERE cn = '650004'"));

        // FR-321: lo existente, intacto — ni el nombre, ni la aptitud, ni su motivo.
        var trasImportar = medicamentos.ObtenerPorCn("654321")!;
        Assert.Equal("Paracetamol 1g", trasImportar.Nombre);
        Assert.False(trasImportar.AptoSpd);
        Assert.Equal("Decisión del farmacéutico.", trasImportar.MotivoNoApto);

        // Art. VII.6: la importación queda trazada con su recuento.
        var traza = Assert.Single(conexion.Query<string>(
            "SELECT detalle FROM Auditoria WHERE accion = 'IMPORTACION_NOMENCLATOR'"));
        Assert.Contains("2 altas activas", traza);
        Assert.Contains("1 de baja", traza);

        // El cambio de nombre del existente sigue en la revisión, para decidirlo aparte.
        var comparacion = servicio.CompararConNomenclator(ruta);
        Assert.Empty(comparacion.Nuevos);   // el accesorio ya no se propone como alta
        Assert.Contains(comparacion.ConNombreDistinto, c => c.Existente.Cn == "654321");
    }

    [Fact]
    public void ImportarCompleto_dos_veces_no_duplica_nada()
    {
        var (servicio, _, conexion) = CrearServicio();
        var ruta = EscribirCsvTemporal(
            "Código Nacional,Nombre del producto farmacéutico,Tipo de fármaco,Estado\n" +
            "650004,DEPAKINE 500 mg,Medicamento Etica,ALTA\n700001,LAMOTRIGINA 100MG,Medicamento Generico,BAJA GENERAL\n");

        servicio.ImportarCompleto(ruta, null);
        var segunda = servicio.ImportarCompleto(ruta, null);

        Assert.Equal(0, segunda.TotalAltas);
        Assert.Equal(2, segunda.YaExistian);
        Assert.Equal(2, conexion.ExecuteScalar<int>("SELECT COUNT(*) FROM Medicamento"));
    }
}
