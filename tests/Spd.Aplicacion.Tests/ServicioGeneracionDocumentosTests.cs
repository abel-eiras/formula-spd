using Dapper;
using Microsoft.Data.Sqlite;
using QuestPDF.Infrastructure;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using UglyToad.PdfPig;
using Xunit;

namespace Spd.Aplicacion.Tests;

/// <summary>Art. IX.3: cada plantilla genera el PDF con datos de ejemplo y se comprueba la
/// presencia de los elementos obligatorios del anexo (Art. I.2). Los elementos son los del
/// cruce campo a campo de docs/analisis-resources.md §2.3 (PNT I, COF A Coruña).</summary>
public sealed class ServicioGeneracionDocumentosTests : IDisposable
{
    private readonly string _carpetaSalida = Path.Combine(Path.GetTempPath(), "spd-tests-documentos-" + Guid.NewGuid());

    public ServicioGeneracionDocumentosTests() => QuestPDF.Settings.License = LicenseType.Community;

    public void Dispose()
    {
        if (Directory.Exists(_carpetaSalida)) Directory.Delete(_carpetaSalida, recursive: true);
    }

    private sealed record Contexto(
        SqliteConnection Conexion, ServicioGeneracionDocumentos Servicio, int PacienteId, int SpdId,
        int ConsentimientoPacienteId, int ConsentimientoRepresentanteId, RepositorioConsentimientos RepositorioConsentimientos,
        int ComunicacionPresentacionId, int ComunicacionIncidenciaId, int ComunicacionTelefonoId);

    private Contexto Crear()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();

        var repositorioFarmacia = new RepositorioFarmacia(conexion);
        repositorioFarmacia.Crear(new Farmacia
        {
            CodigoSanitario = "PO-001", Nombre = "Farmacia de Prueba", TitularOComunidadBienes = "Titular",
            Cif = "B00000000", Direccion = "Calle Falsa 1", Cp = "36000", Poblacion = "Pontevedra",
            Telefono = "986000000", Email = "farmacia@ejemplo.gal", PrefijoNumSpd = "F-",
            RutaDocumentosGenerados = _carpetaSalida, DpoNombre = "Diego DPO Ejemplo", DpoContacto = "981 000 000 / dpo@ejemplo.gal"
        });

        var auditoria = new RegistradorAuditoria(conexion);

        var repositorioMedicos = new RepositorioMedicos(conexion);
        var medico = new Medico { Nombre = "Rosa", Apellidos = "Ferreiro Castro", Centro = "CS Lérez", Telefono = "986111111" };
        medico.Id = repositorioMedicos.Crear(medico);

        var repositorioPacientes = new RepositorioPacientes(conexion);
        var servicioPacientes = new ServicioPacientes(repositorioPacientes, repositorioFarmacia, auditoria);
        var paciente = servicioPacientes.Crear(
            new DatosAltaPaciente("María", "López Vidal", null, "12345678Z", new DateOnly(1940, 5, 3), "281234567890", null,
                "Rúa Nova 5", "36001", "Pontevedra", "600000000", null, "maria@ejemplo.gal", medico.Id,
                "Hipertensión", "Alergia a penicilina", "Vive sola", false, null, null, null),
            usuarioQueEjecutaId: null);

        var repositorioContactos = new RepositorioContactos(conexion);
        repositorioContactos.Crear(new Contacto
        {
            PacienteId = paciente.Id, Tipo = TipoContacto.Cuidador, Nombre = "Xoán", Apellidos = "López Pérez",
            Telefono = "611222333", Email = "xoan@ejemplo.gal", EsPrincipal = true
        });
        var representanteId = repositorioContactos.Crear(new Contacto
        {
            PacienteId = paciente.Id, Tipo = TipoContacto.RepresentanteLegal, Nombre = "Ana", Apellidos = "Vidal Souto", Dni = "87654321X"
        });

        // Spec 002: evaluación vigente APTO y dos consentimientos (paciente, firmado; representante, sin firmar).
        var repositorioEvaluaciones = new RepositorioEvaluacionesIdoneidad(conexion);
        var repositorioConsentimientos = new RepositorioConsentimientos(conexion);

        var repositorioMedicamentos = new RepositorioMedicamentos(conexion);
        var paracetamol = new Medicamento { Cn = "654321", Nombre = "Paracetamol 1g", DescTexto = "Comprimido blanco oblongo" };
        paracetamol.Id = repositorioMedicamentos.Crear(paracetamol);
        var jarabe = new Medicamento { Cn = "111222", Nombre = "Jarabe Tos 100ml", AptoSpd = false };
        jarabe.Id = repositorioMedicamentos.Crear(jarabe);

        var repositorioUsuarios = new RepositorioUsuarios(conexion);
        var elaborador = new Usuario { Nombre = "Elena", Apellidos = "Ruiz", Login = "elena", HashPassword = "x", Rol = Rol.Elaborador };
        elaborador.Id = repositorioUsuarios.Crear(elaborador);
        var verificador = new Usuario { Nombre = "Carlos", Apellidos = "Souto", Login = "carlos", HashPassword = "x", Rol = Rol.Elaborador };
        verificador.Id = repositorioUsuarios.Crear(verificador);

        var repositorioMaterial = new RepositorioMaterialAcondicionamiento(conexion);
        var materialId = repositorioMaterial.Crear(new MaterialAcondicionamiento { Descripcion = "Blíster semanal Venalink", Lote = "MAT-778", FechaEntrada = new DateOnly(2026, 8, 1) });

        var repositorioAmbiental = new RepositorioRegistrosAmbientales(conexion);
        var ambientalId = repositorioAmbiental.Crear(new RegistroAmbiental { Fecha = DateTime.UtcNow, Temperatura = 21.5, Humedad = 48, FueraRango = false, UsuarioId = elaborador.Id });

        var repositorioSpd = new RepositorioSpd(conexion);
        var spd = new SPD
        {
            NumRegistro = "F-000001", CorrelativoNumRegistro = 1, PacienteId = paciente.Id, SesionId = Guid.NewGuid(),
            ValidezDesde = new DateOnly(2026, 9, 7), ValidezHasta = new DateOnly(2026, 9, 13), ElaboradorId = elaborador.Id,
            FechaPreparacion = new DateTime(2026, 9, 6), MaterialId = materialId, RegistroAmbientalId = ambientalId,
            VerificadorId = verificador.Id, FechaVerificacion = new DateTime(2026, 9, 6, 12, 0, 0),
            Estado = EstadoSpd.Entregado, EntregadorId = elaborador.Id, FechaEntrega = new DateTime(2026, 9, 7),
            EntregadoA = "Xoán López Pérez", PrimeraEntrega = false, SpdAnteriorRecogido = true, UnidadesNoAdministradas = "Paracetamol: 2"
        };
        spd.Id = repositorioSpd.Crear(spd);

        var repositorioTratamientos = new RepositorioTratamientos(conexion);
        var tratamiento = new Tratamiento
        {
            PacienteId = paciente.Id, MedicamentoId = paracetamol.Id, EnSpd = true, PautaD = FraccionDosis.Media, PautaC = FraccionDosis.Uno,
            ProblemaSalud = "Dolor crónico", MedicoId = medico.Id, Momento = "con las comidas",
            FechaInicio = new DateOnly(2026, 3, 1), FechaPrescripcionInicial = new DateOnly(2026, 1, 1)
        };
        tratamiento.Id = repositorioTratamientos.Crear(tratamiento);
        repositorioTratamientos.Crear(new Tratamiento
        {
            PacienteId = paciente.Id, MedicamentoId = jarabe.Id, EnSpd = false, PautaTexto = "5 ml cada 8 horas", Via = "oral",
            MedicoId = medico.Id, FechaInicio = new DateOnly(2026, 8, 1), FechaPrescripcionInicial = new DateOnly(2026, 8, 1)
        });

        var repositorioEnvases = new RepositorioEnvases(conexion);
        var envase = new Envase
        {
            PacienteId = paciente.Id, MedicamentoId = paracetamol.Id, Serie = "SER1", Lote = "LOT1",
            Caducidad = new DateOnly(2030, 1, 1), UnidadesIniciales = 28, UnidadesRestantes = 24
        };
        envase.Id = repositorioEnvases.Crear(envase);

        var repositorioLineas = new RepositorioSpdLineas(conexion);
        var linea = new SpdLinea
        {
            SpdId = spd.Id, TratamientoId = tratamiento.Id, MedicamentoId = paracetamol.Id, SnapNombre = paracetamol.Nombre,
            SnapCn = paracetamol.Cn, SnapPautaD = FraccionDosis.Media, SnapPautaC = FraccionDosis.Uno, SnapDiasSemana = "1111111",
            SnapDescTexto = paracetamol.DescTexto, SnapMomento = "con las comidas", UnidadesDosis = 10.5m, UnidadesEnvase = 11
        };
        linea.Id = repositorioLineas.Crear(linea);

        var repositorioLineaEnvases = new RepositorioSpdLineaEnvases(conexion);
        repositorioLineaEnvases.Crear(new SpdLineaEnvase
        {
            SpdLineaId = linea.Id, EnvaseId = envase.Id, UnidadesTomadas = 11, SnapSerie = "SER1", SnapLote = "LOT1", SnapCaducidad = new DateOnly(2030, 1, 1)
        });

        var repositorioVerificaciones = new RepositorioSpdVerificaciones(conexion);
        repositorioVerificaciones.Crear(new SpdVerificacion
        {
            SpdId = spd.Id, VerificadorId = verificador.Id, Fecha = DateTime.UtcNow, VerifAspecto = true, VerifEtiquetaDatos = true,
            VerifEtiquetaValidez = true, VerifInstrucciones = true, VerifContenido = true, VerifFabricantePnt = true,
            VerifEtiquetaFichaPaciente = true, VerifTrazabilidad = true, Resultado = ResultadoVerificacion.Apto
        });

        repositorioEvaluaciones.Crear(new EvaluacionIdoneidad
        {
            PacienteId = paciente.Id, Fecha = DateTime.UtcNow, FarmaceuticoId = elaborador.Id, Criterio1 = true, Criterio4 = true,
            CondicionMotivacion = true, CondicionDestreza = true, Observaciones = "Buena disposición", Resultado = ResultadoIdoneidad.Apto
        });
        var consentimientoPacienteId = repositorioConsentimientos.Crear(new Consentimiento
        {
            PacienteId = paciente.Id, Tipo = TipoConsentimiento.Paciente, FechaCreacion = DateTime.UtcNow, FechaFirma = new DateOnly(2026, 9, 1)
        });
        var consentimientoRepresentanteId = repositorioConsentimientos.Crear(new Consentimiento
        {
            PacienteId = paciente.Id, Tipo = TipoConsentimiento.Representante, ContactoId = representanteId, FechaCreacion = DateTime.UtcNow
        });

        // Spec 008: una comunicación de cada tipo (solo presentación e incidencia generan carta).
        var repositorioComunicaciones = new RepositorioComunicacionesMedico(conexion);
        var presentacionId = repositorioComunicaciones.Crear(new ComunicacionMedico
        {
            PacienteId = paciente.Id, MedicoId = medico.Id, Tipo = TipoComunicacionMedico.Presentacion, Fecha = new DateOnly(2026, 9, 2), FarmaceuticoId = elaborador.Id
        });
        var incidenciaId = repositorioComunicaciones.Crear(new ComunicacionMedico
        {
            PacienteId = paciente.Id, MedicoId = medico.Id, Tipo = TipoComunicacionMedico.Incidencia, Fecha = new DateOnly(2026, 9, 5),
            IncidenciasDetectadas = "Duplicidad de paracetamol con otro analgésico", Propuesta = "Retirar uno de los dos", FarmaceuticoId = elaborador.Id
        });
        var telefonoId = repositorioComunicaciones.Crear(new ComunicacionMedico
        {
            PacienteId = paciente.Id, MedicoId = medico.Id, Tipo = TipoComunicacionMedico.Telefono, Fecha = new DateOnly(2026, 9, 5)
        });

        var servicio = new ServicioGeneracionDocumentos(
            repositorioSpd, repositorioLineas, repositorioLineaEnvases, repositorioVerificaciones, repositorioPacientes,
            repositorioContactos, repositorioMedicos, repositorioTratamientos, repositorioMedicamentos, repositorioUsuarios,
            repositorioMaterial, repositorioAmbiental, repositorioEvaluaciones, repositorioConsentimientos, repositorioComunicaciones,
            repositorioFarmacia, auditoria);

        return new Contexto(conexion, servicio, paciente.Id, spd.Id, consentimientoPacienteId, consentimientoRepresentanteId, repositorioConsentimientos,
            presentacionId, incidenciaId, telefonoId);
    }

    private static string TextoDelPdf(string ruta)
    {
        using var pdf = PdfDocument.Open(ruta);
        return string.Join(" ", pdf.GetPages().SelectMany(p => p.GetWords()).Select(w => w.Text));
    }

    private static void ContieneTodo(string texto, params string[] elementos)
    {
        foreach (var e in elementos)
        {
            if (texto.Contains(e, StringComparison.OrdinalIgnoreCase)) continue;
            // Diagnóstico: el texto completo extraído queda en un fichero, que es más legible que
            // un mensaje de aserción de varios miles de caracteres.
            var volcado = Path.Combine(Path.GetTempPath(), "spd-pdf-texto-extraido.txt");
            File.WriteAllText(volcado, texto);
            Assert.Fail($"Falta en el PDF: «{e}». Texto extraído volcado en {volcado}");
        }
    }

    [Fact]
    public void GenerarFichaSpd_contiene_los_elementos_del_Anexo_I_G_y_audita_CA_709_710()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;

        var resultado = ctx.Servicio.GenerarFichaSpd(ctx.SpdId, usuarioQueEjecutaId: 7);

        Assert.True(File.Exists(resultado.RutaCompleta));
        Assert.StartsWith("Ficha de preparación", resultado.NombreFichero);
        var texto = TextoDelPdf(resultado.RutaCompleta);
        ContieneTodo(texto,
            "María López Vidal", "F-000001", "06/09/2026", "07/09/2026", "13/09/2026",   // paciente, nº registro, fechas prep/entrega, validez
            "654321", "Paracetamol", "1/2", "LOT1", "01/2030", "SER1",                   // CN, medicamento, posología en fracción, lote, caducidad, serie
            "Venalink", "MAT-778",                                                       // material y su lote
            "21,5", "48",                                                                // temperatura y humedad
            "Cumple adherencia", "Paracetamol: 2",                                       // control de adherencia
            "trazabilidad", "alteraciones visibles", "APTO",                             // verificación final (8 preguntas) y resultado
            "Elena Ruiz", "Carlos Souto", "Entregado por",                               // elaborado / verificado / entregado por
            "Desayuno");                                                                 // leyenda D/A/C/N
        Assert.DoesNotContain("0,5", texto);                                             // FR-740: nunca decimal

        var registros = ctx.Conexion.Query<(string Accion, int? UsuarioId)>("SELECT accion, usuario_id FROM Auditoria WHERE entidad = 'SPD'");
        Assert.Contains(registros, r => r.Accion == "GENERAR_DOCUMENTO" && r.UsuarioId == 7);
    }

    [Fact]
    public void GenerarEtiquetaAnverso_contiene_farmacia_paciente_registro_validez_no_incluidos_y_advertencias()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;

        var texto = TextoDelPdf(ctx.Servicio.GenerarEtiquetaAnverso(ctx.SpdId, null).RutaCompleta);

        ContieneTodo(texto,
            "Farmacia de Prueba", "Calle Falsa 1", "986000000",
            "María López Vidal", "600000000",
            "F-000001", "06/09/2026", "07/09/2026", "13/09/2026",
            "hay que administrar", "Jarabe Tos", "5 ml cada 8 horas",
            "ALCANCE", "PERIODO DE VALIDEZ", "protegido de la luz", "CAMBIO DE MEDICACIÓN");
    }

    [Fact]
    public void GenerarEtiquetaReverso_contiene_cn_posologia_serie_lote_caducidad_aspecto_y_advertencias()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;

        var texto = TextoDelPdf(ctx.Servicio.GenerarEtiquetaReverso(ctx.SpdId, null).RutaCompleta);

        ContieneTodo(texto, "654321", "Paracetamol", "1/2 - 0 - 1 - 0", "SER1", "LOT1", "01/2030", "blanco oblongo", "ALCANCE", "protegido de la luz");
        var registros = ctx.Conexion.Query<string>("SELECT accion FROM Auditoria WHERE entidad = 'SPD' AND accion = 'GENERAR_DOCUMENTO'");
        Assert.Single(registros);
    }

    [Fact]
    public void GenerarInstrucciones_contiene_farmacia_incluidos_no_incluidos_medico_fechas_y_advertencias()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;

        var texto = TextoDelPdf(ctx.Servicio.GenerarInstrucciones(ctx.SpdId, null).RutaCompleta);

        ContieneTodo(texto,
            "Farmacia de Prueba", "Calle Falsa 1", "986000000",
            "María López Vidal", "F-000001", "06/09/2026",
            // La extracción lee las tablas fila a fila, así que una celda partida en dos líneas
            // se entremezcla con las vecinas: se comprueban los tokens por separado.
            "654321", "Paracetamol", "Rosa Ferreiro Castro", "01/01/2026", "modif.", "01/03/2026",
            "no introducidos", "Jarabe Tos", "5 ml cada 8 horas",
            "alcance de los niños", "No utilizar después", "cambio de medicación");
    }

    [Fact]
    public void GenerarFichaPaciente_contiene_los_elementos_del_Anexo_I_E_y_audita()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;

        var resultado = ctx.Servicio.GenerarFichaPaciente(ctx.PacienteId, usuarioQueEjecutaId: 3);

        var texto = TextoDelPdf(resultado.RutaCompleta);
        ContieneTodo(texto,
            "María López Vidal", "03/05/1940", "12345678Z", "281234567890", "Rúa Nova 5", "36001", "600000000", "maria@ejemplo.gal",
            "Xoán López Pérez", "611222333",                       // familiar o cuidador (contacto principal)
            "Rosa Ferreiro Castro", "986111111",                   // médico de familia
            "Hipertensión", "penicilina", "Vive sola",
            "IDONEIDAD", "[X] Paciente polimedicado", "Buena disposición", "APTO [X]", "Elena Ruiz",   // evaluación vigente impresa (Spec 002)
            "incluidos en DDP", "Dolor", "crónico", "1/2 - 0 - 1 - 0", "01/03/2026",
            "no incluidos", "Jarabe Tos",
            "CONTROL ADHERENCIA", "07/09/2026", "F-000001");
        var registros = ctx.Conexion.Query<(string Accion, int? UsuarioId)>("SELECT accion, usuario_id FROM Auditoria WHERE entidad = 'Paciente'");
        Assert.Contains(registros, r => r.Accion == "GENERAR_DOCUMENTO" && r.UsuarioId == 3);
    }

    [Fact]
    public void GenerarInformacionProteccionDatos_contiene_responsable_paciente_plazos_derechos_y_dpo()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;

        var resultado = ctx.Servicio.GenerarInformacionProteccionDatos(ctx.PacienteId, usuarioQueEjecutaId: 5);

        Assert.StartsWith("Información protección de datos", resultado.NombreFichero);
        var texto = TextoDelPdf(resultado.RutaCompleta);
        ContieneTodo(texto,
            "María López Vidal", "12345678Z",
            "Responsable", "Farmacia de Prueba", "Calle Falsa 1", "986000000", "farmacia@ejemplo.gal",
            "un año de la inactividad", "5 años desde la baja",
            "6.1.c", "Ley 41/2002", "Ley 3/2019",
            "Diego DPO Ejemplo", "dpo@ejemplo.gal",
            "Recibí una copia");
        var registros = ctx.Conexion.Query<string>("SELECT detalle FROM Auditoria WHERE entidad = 'Paciente' AND accion = 'GENERAR_DOCUMENTO'");
        Assert.Contains(registros, d => d.Contains("tipo=RGPD"));
    }

    [Fact]
    public void GenerarConsentimiento_del_paciente_contiene_los_elementos_del_Anexo_I_B_y_marca_impreso()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;

        var resultado = ctx.Servicio.GenerarConsentimiento(ctx.ConsentimientoPacienteId, usuarioQueEjecutaId: 4);

        Assert.StartsWith("Consentimiento informado", resultado.NombreFichero);
        var texto = TextoDelPdf(resultado.RutaCompleta);
        ContieneTodo(texto,
            "María López Vidal", "12345678Z", "en nombre propio", "AUTORIZO a la farmacia", "Farmacia de Prueba",
            "Conozco el servicio de SPD", "prescindir del servicio libremente", "quede en depósito en la farmacia",
            "comprobar la adherencia", "Pontevedra, a 1 de septiembre de 2026",
            "Firma del/de la paciente", "Firma del farmacéutico");
        Assert.NotNull(ctx.RepositorioConsentimientos.ObtenerPorId(ctx.ConsentimientoPacienteId)!.ImpresoEn);
        var registros = ctx.Conexion.Query<string>("SELECT detalle FROM Auditoria WHERE entidad = 'Paciente' AND accion = 'GENERAR_DOCUMENTO'");
        Assert.Contains(registros, d => d.Contains("tipo=CONSENT"));
    }

    [Fact]
    public void GenerarConsentimiento_por_representante_nombra_al_representante_y_al_paciente()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;

        var texto = TextoDelPdf(ctx.Servicio.GenerarConsentimiento(ctx.ConsentimientoRepresentanteId, null).RutaCompleta);

        ContieneTodo(texto, "Ana Vidal Souto", "87654321X", "como representante legal de", "María López Vidal", "12345678Z",
            "a ______ de");   // sin firmar: fecha en blanco
        Assert.DoesNotContain("en nombre propio", texto);
    }

    [Fact]
    public void GenerarCartaMedico_presentacion_contiene_el_Anexo_I_C_literal_CARTA_PRES()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;

        var resultado = ctx.Servicio.GenerarCartaMedico(ctx.ComunicacionPresentacionId, usuarioQueEjecutaId: 2);

        Assert.StartsWith("Carta de presentación al médico", resultado.NombreFichero);
        var texto = TextoDelPdf(resultado.RutaCompleta);
        ContieneTodo(texto,
            "Pontevedra, a 2 de septiembre de 2026", "Rosa Ferreiro Castro",
            "Sistema Personalizado de Dosificación o SPD", "María López Vidal", "Se adjunta Ficha del Paciente",
            "Farmacéutico/a responsable", "Elena Ruiz", "colegiado", "986000000");
        var registros = ctx.Conexion.Query<string>("SELECT detalle FROM Auditoria WHERE entidad = 'ComunicacionMedico' AND accion = 'GENERAR_DOCUMENTO'");
        Assert.Contains(registros, d => d.Contains("tipo=CARTA-PRES"));
    }

    [Fact]
    public void GenerarCartaMedico_incidencia_lleva_incidencias_y_propuesta_CARTA_INC()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;

        var resultado = ctx.Servicio.GenerarCartaMedico(ctx.ComunicacionIncidenciaId, null);

        Assert.StartsWith("Carta de incidencias al médico", resultado.NombreFichero);
        var texto = TextoDelPdf(resultado.RutaCompleta);
        ContieneTodo(texto, "INCIDENCIAS", "María López Vidal", "Duplicidad de paracetamol", "Propuesta del farmacéutico", "Retirar uno de los dos");
        Assert.DoesNotContain("Se adjunta Ficha del Paciente", texto);
    }

    [Fact]
    public void GenerarCartaMedico_rechaza_una_comunicacion_telefonica_FR_806()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;

        Assert.Throws<ErrorValidacionException>(() => ctx.Servicio.GenerarCartaMedico(ctx.ComunicacionTelefonoId, null));
    }

    [Fact]
    public void GenerarInstruccionesSesion_con_un_solo_blister_equivale_a_la_hoja_del_blister_FR_682()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;
        var sesionId = Guid.Parse(ctx.Conexion.QuerySingle<string>("SELECT sesion_id FROM SPD WHERE id = @id", new { id = ctx.SpdId }));

        var resultado = ctx.Servicio.GenerarInstruccionesSesion(sesionId, null);

        Assert.StartsWith("Hoja de instrucciones", resultado.NombreFichero);
        ContieneTodo(TextoDelPdf(resultado.RutaCompleta), "F-000001", "07/09/2026", "13/09/2026", "Paracetamol");
    }

    [Fact]
    public void GenerarInformacionProteccionDatos_sin_dpo_configurado_remite_al_colegio()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;
        ctx.Conexion.Execute("UPDATE Farmacia SET dpo_nombre = NULL, dpo_contacto = NULL, email = NULL");

        var texto = TextoDelPdf(ctx.Servicio.GenerarInformacionProteccionDatos(ctx.PacienteId, null).RutaCompleta);

        ContieneTodo(texto, "Colegio Oficial de Farmacéuticos");
        Assert.DoesNotContain("Diego DPO", texto);
    }
}
