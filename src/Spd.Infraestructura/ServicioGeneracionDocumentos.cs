using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Spd.Aplicacion;
using Spd.Dominio;

namespace Spd.Infraestructura;

/// <summary>Motor único de generación de documentos PDF (FR-700..713). Cada generador
/// reproduce los elementos mínimos del anexo correspondiente del PNT I (COF A Coruña, Decreto
/// 87/2022) — Art. I.2 de la constitución; el cruce campo a campo está en
/// docs/analisis-resources.md §2.3. Un método privado común (`GuardarDocumento`) aplica el
/// nombrado, la carpeta de salida y la auditoría (research.md Decisión 2 de Spec 007).</summary>
public sealed class ServicioGeneracionDocumentos(
    IRepositorioSpd repositorioSpd,
    IRepositorioSpdLineas repositorioLineas,
    IRepositorioSpdLineaEnvases repositorioLineaEnvases,
    IRepositorioSpdVerificaciones repositorioVerificaciones,
    IRepositorioPacientes repositorioPacientes,
    IRepositorioContactos repositorioContactos,
    IRepositorioMedicos repositorioMedicos,
    IRepositorioTratamientos repositorioTratamientos,
    IRepositorioMedicamentos repositorioMedicamentos,
    IRepositorioUsuarios repositorioUsuarios,
    IRepositorioMaterialAcondicionamiento repositorioMaterial,
    IRepositorioRegistrosAmbientales repositorioRegistrosAmbientales,
    IRepositorioEvaluacionesIdoneidad repositorioEvaluaciones,
    IRepositorioConsentimientos repositorioConsentimientos,
    IRepositorioComunicacionesMedico repositorioComunicaciones,
    IRepositorioFarmacia repositorioFarmacia,
    IRegistradorAuditoria auditoria)
    : IServicioGeneracionDocumentos
{
    // Textos fijos de los anexos I.F / I.H (advertencias de uso), literales del PNT I.
    private const string AdvNinos = "MANTÉNGASE FUERA DEL ALCANCE Y VISTA DE LOS NIÑOS";
    private const string AdvValidez = "NO UTILIZAR DESPUÉS DEL PERIODO DE VALIDEZ INDICADO";
    private const string AdvConservacion = "CONDICIONES DE CONSERVACIÓN: conservar en lugar fresco, seco y protegido de la luz";
    private const string AdvCambios = "RECUERDE COMUNICAR A SU FARMACÉUTICO CUANTO ANTES CUALQUIER CAMBIO DE MEDICACIÓN";

    // ------------------------------------------------------------------ Anexo I.G — FICHA

    public ResultadoGeneracionDocumento GenerarFichaSpd(int spdId, int? usuarioQueEjecutaId)
    {
        var (spd, paciente, lineas) = ObtenerDatosSpd(spdId);
        var farmacia = ObtenerFarmacia();
        var material = spd.MaterialId is int mid ? repositorioMaterial.ObtenerPorId(mid) : null;
        var ambiental = spd.RegistroAmbientalId is int rid ? repositorioRegistrosAmbientales.ObtenerPorId(rid) : null;
        var verificacion = repositorioVerificaciones.ListarPorSpd(spdId).LastOrDefault();
        var incluidas = lineas.Where(l => l.EstadoLinea != EstadoLinea.Excluida).ToList();
        var excluidas = lineas.Where(l => l.EstadoLinea == EstadoLinea.Excluida).ToList();

        var documento = Document.Create(contenedor => contenedor.Page(pagina =>
        {
            ConfigurarPagina(pagina, PageSizes.A4, 9);
            pagina.Header().Element(e => Cabecera(e, farmacia, "FICHA DE PREPARACIÓN, CONTROL Y ENTREGA DE SPD"));
            pagina.Content().PaddingTop(8).Column(col =>
            {
                col.Spacing(6);
                col.Item().Row(fila =>
                {
                    fila.RelativeItem().Column(c =>
                    {
                        c.Item().Text(t => { t.Span("Paciente: ").Bold(); t.Span($"{paciente.Nombre} {paciente.Apellidos} (ficha {paciente.NumFicha})"); });
                        c.Item().Text(t => { t.Span("Elaborado por: ").Bold(); t.Span(NombreUsuario(spd.ElaboradorId)); });
                    });
                    fila.RelativeItem().Column(c =>
                    {
                        c.Item().Text(t => { t.Span("Nº REGISTRO DDP: ").Bold(); t.Span($"{spd.NumRegistro} (v{spd.Version})"); });
                        c.Item().Text(t => { t.Span("FECHA PREPARACIÓN: ").Bold(); t.Span(Fecha(spd.FechaPreparacion)); });
                        c.Item().Text(t => { t.Span("FECHA DE ENTREGA: ").Bold(); t.Span(Fecha(spd.FechaEntrega)); });
                        c.Item().Text(t => { t.Span("PERIODO DE VALIDEZ: ").Bold(); t.Span($"{spd.ValidezDesde:dd/MM/yyyy} — {spd.ValidezHasta:dd/MM/yyyy}"); });
                    });
                });

                col.Item().Text("Datos medicamentos").Bold();
                col.Item().Table(tabla =>
                {
                    tabla.ColumnsDefinition(c =>
                    {
                        c.ConstantColumn(48); c.RelativeColumn(3); c.ConstantColumn(22); c.ConstantColumn(22); c.ConstantColumn(22);
                        c.ConstantColumn(22); c.RelativeColumn(1.2f); c.RelativeColumn(1.2f); c.RelativeColumn(1.6f);
                    });
                    foreach (var cab in new[] { "CN", "Medicamento", "D", "A", "C", "N", "Lote", "Caducidad", "Nº serie (SN)" })
                        Celda(tabla, cab, negrita: true);
                    foreach (var linea in incluidas)
                    {
                        var envases = repositorioLineaEnvases.ListarPorLinea(linea.Id);
                        if (envases.Count == 0)
                        {
                            FilaMedicamento(tabla, linea, "—", "—", "—");
                            continue;
                        }
                        // FR-611 / CA-602: una fila por cada envase del que se tomaron unidades.
                        foreach (var e in envases)
                            FilaMedicamento(tabla, linea, e.SnapLote ?? "—", e.SnapCaducidad is DateOnly cad ? cad.ToString("MM/yyyy") : "—", e.SnapSerie ?? "—");
                    }
                });
                if (excluidas.Count > 0)
                    col.Item().Text($"Líneas excluidas de este blíster: {string.Join("; ", excluidas.Select(l => $"{l.SnapNombre} ({l.MotivoExclusion})"))}").Italic();

                col.Item().Text(t =>
                {
                    t.Span("Material de acondicionamiento — Tipo de material: ").Bold();
                    t.Span(material?.Descripcion ?? "________________");
                    t.Span("    Nº de lote: ").Bold();
                    t.Span(material?.Lote ?? "________");
                });
                col.Item().Text(t =>
                {
                    t.Span("Condiciones en el momento de la preparación — Temperatura: ").Bold();
                    t.Span(ambiental is null ? "______ ºC" : $"{ambiental.Temperatura:0.#} ºC");
                    t.Span("    Humedad: ").Bold();
                    t.Span(ambiental is null ? "______ %" : $"{ambiental.Humedad:0.#} %");
                    if (ambiental?.FueraRango == true) t.Span("    (FUERA DE RANGO)").Bold();
                });

                col.Item().Text("Control adherencia al tratamiento").Bold();
                col.Item().Table(tabla =>
                {
                    tabla.ColumnsDefinition(c => { c.RelativeColumn(1.2f); c.RelativeColumn(3); c.RelativeColumn(1); });
                    Celda(tabla, "Entrega SPD anterior (SÍ/NO)", negrita: true);
                    Celda(tabla, "Otra información", negrita: true);
                    Celda(tabla, "Cumple adherencia (SÍ/NO)", negrita: true);
                    var primera = spd.PrimeraEntrega == true;
                    Celda(tabla, primera ? "Primera entrega" : SiNo(spd.SpdAnteriorRecogido == true));
                    Celda(tabla, string.Join(" ", new[]
                    {
                        string.IsNullOrWhiteSpace(spd.UnidadesNoAdministradas) ? null : $"Unidades no administradas: {spd.UnidadesNoAdministradas}.",
                        spd.ObservacionesAdherencia
                    }.Where(s => !string.IsNullOrWhiteSpace(s))));
                    Celda(tabla, spd.FechaEntrega is null ? "" : primera ? "—" : SiNo(string.IsNullOrWhiteSpace(spd.UnidadesNoAdministradas)));
                });

                col.Item().Text("Verificación final del SPD: etiquetado y contenido").Bold();
                col.Item().Table(tabla =>
                {
                    tabla.ColumnsDefinition(c => { c.RelativeColumn(6); c.ConstantColumn(30); c.ConstantColumn(30); });
                    Celda(tabla, "", negrita: true); Celda(tabla, "SÍ", negrita: true); Celda(tabla, "NO", negrita: true);
                    foreach (var (pregunta, valor) in PreguntasVerificacion(verificacion))
                    {
                        Celda(tabla, pregunta);
                        Celda(tabla, valor == true ? "X" : "");
                        Celda(tabla, valor == false ? "X" : "");
                    }
                });
                if (verificacion is not null)
                    col.Item().Text(t =>
                    {
                        t.Span("Resultado: ").Bold(); t.Span(verificacion.Resultado == ResultadoVerificacion.Apto ? "APTO" : "NO APTO");
                        if (!string.IsNullOrWhiteSpace(verificacion.ExcepcionMotivo))
                        { t.Span("    Excepción verificador = elaborador, motivo: ").Bold(); t.Span(verificacion.ExcepcionMotivo); }
                    });

                col.Item().PaddingTop(10).Row(fila =>
                {
                    Firma(fila, "Elaborado por", NombreUsuario(spd.ElaboradorId), spd.FechaPreparacion);
                    Firma(fila, "Verificado por", NombreUsuario(spd.VerificadorId), spd.FechaVerificacion);
                    Firma(fila, "Entregado por", NombreUsuario(spd.EntregadorId), spd.FechaEntrega);
                });
                col.Item().PaddingTop(6).Text("*Posología: D: Desayuno; A: Almuerzo; C: Cena; N: Noche (al acostarse)").FontSize(7).Italic();
            });
        }));

        return GuardarDocumento(documento, "Ficha de preparación", "FICHA", paciente, spd.NumRegistro, farmacia, "SPD", spdId, usuarioQueEjecutaId);
    }

    // ------------------------------------------------------------------ Anexo I.F — ETQ-A

    public ResultadoGeneracionDocumento GenerarEtiquetaAnverso(int spdId, int? usuarioQueEjecutaId)
    {
        var (spd, paciente, lineas) = ObtenerDatosSpd(spdId);
        var farmacia = ObtenerFarmacia();
        var noIncluidos = TratamientosNoIncluidos(paciente.Id, lineas);

        var documento = Document.Create(contenedor => contenedor.Page(pagina =>
        {
            ConfigurarPagina(pagina, new PageSize(400, 280), 8);
            pagina.Content().Column(col =>
            {
                col.Spacing(2);
                col.Item().Row(fila =>
                {
                    fila.RelativeItem().Column(c =>
                    {
                        c.Item().Text("DATOS DE LA FARMACIA").Bold().FontSize(7);
                        c.Item().Text($"FARMACIA: {farmacia.Nombre}");
                        c.Item().Text($"DIRECCIÓN: {farmacia.Direccion}, {farmacia.Cp} {farmacia.Poblacion}");
                        c.Item().Text($"TELÉFONO: {farmacia.Telefono}" + (string.IsNullOrWhiteSpace(farmacia.Email) ? "" : $"  e-mail: {farmacia.Email}"));
                    });
                    fila.ConstantItem(150).Column(c =>
                    {
                        c.Item().Text($"FECHA PREPARACIÓN: {Fecha(spd.FechaPreparacion)}").Bold();
                        c.Item().Text($"Nº REGISTRO DDP: {spd.NumRegistro}").Bold();
                        c.Item().Text($"PERIODO DE VALIDEZ: {spd.ValidezDesde:dd/MM/yyyy} — {spd.ValidezHasta:dd/MM/yyyy}").Bold();
                    });
                });
                col.Item().PaddingTop(3).Text("DATOS DEL PACIENTE").Bold().FontSize(7);
                col.Item().Text($"NOMBRE Y APELLIDOS: {paciente.Nombre} {paciente.Apellidos}").FontSize(10).Bold();
                col.Item().Text($"TELÉFONO DE CONTACTO: {paciente.Telefono1 ?? paciente.Telefono2 ?? "—"}");
                if (!string.IsNullOrWhiteSpace(paciente.IdentificadorVisual))
                    col.Item().Text($"Identificador: {paciente.IdentificadorVisual}");

                col.Item().PaddingTop(3).Text("Recuerde que, además, hay que administrar:").Bold();
                if (noIncluidos.Count == 0)
                    col.Item().Text("• (ningún otro medicamento fuera del DDP)");
                foreach (var (tratamiento, medicamento) in noIncluidos)
                    col.Item().Text($"• {medicamento?.Nombre ?? "?"} — {Posologia(tratamiento)}");

                col.Item().PaddingTop(4).Column(c =>
                {
                    c.Item().Text(AdvNinos).FontSize(6.5f).Bold();
                    c.Item().Text(AdvValidez).FontSize(6.5f).Bold();
                    c.Item().Text(AdvConservacion).FontSize(6.5f);
                    c.Item().Text(AdvCambios).FontSize(6.5f);
                });
            });
        }));

        return GuardarDocumento(documento, "Etiqueta anverso", "ETQ-A", paciente, spd.NumRegistro, farmacia, "SPD", spdId, usuarioQueEjecutaId);
    }

    // ------------------------------------------------------------------ Anexo I.F — ETQ-R

    public ResultadoGeneracionDocumento GenerarEtiquetaReverso(int spdId, int? usuarioQueEjecutaId)
    {
        var (spd, paciente, lineas) = ObtenerDatosSpd(spdId);
        var farmacia = ObtenerFarmacia();

        var documento = Document.Create(contenedor => contenedor.Page(pagina =>
        {
            ConfigurarPagina(pagina, new PageSize(400, 280), 7);
            pagina.Content().Column(col =>
            {
                col.Spacing(2);
                col.Item().Text($"Nº REGISTRO DDP: {spd.NumRegistro} — {paciente.Nombre} {paciente.Apellidos} — VALIDEZ desde {spd.ValidezDesde:dd/MM/yyyy} hasta {spd.ValidezHasta:dd/MM/yyyy}").Bold();
                col.Item().Table(tabla =>
                {
                    tabla.ColumnsDefinition(c =>
                    {
                        c.ConstantColumn(38); c.RelativeColumn(2.2f); c.RelativeColumn(1.6f); c.RelativeColumn(1.2f);
                        c.RelativeColumn(1); c.ConstantColumn(40); c.RelativeColumn(1.6f);
                    });
                    foreach (var cab in new[] { "CN", "Medicamento", "Posología", "Nº serie (SN)", "Lote", "Caducidad", "Aspectos físicos" })
                        Celda(tabla, cab, negrita: true);
                    foreach (var linea in lineas.Where(l => l.EstadoLinea != EstadoLinea.Excluida))
                    {
                        var envases = repositorioLineaEnvases.ListarPorLinea(linea.Id);
                        if (envases.Count == 0) envases = [new SpdLineaEnvase { SpdLineaId = linea.Id, EnvaseId = 0 }];
                        foreach (var e in envases)
                        {
                            Celda(tabla, linea.SnapCn);
                            Celda(tabla, linea.SnapNombre);
                            Celda(tabla, Posologia(linea.SnapPautaD, linea.SnapPautaA, linea.SnapPautaC, linea.SnapPautaN, linea.SnapMomento));
                            Celda(tabla, e.SnapSerie ?? "—");
                            Celda(tabla, e.SnapLote ?? "—");
                            Celda(tabla, e.SnapCaducidad is DateOnly cad ? cad.ToString("MM/yyyy") : "—");
                            Celda(tabla, linea.SnapDescTexto ?? "");
                        }
                    }
                });
                col.Item().PaddingTop(3).Text("ADVERTENCIAS DE USO").Bold().FontSize(6.5f);
                col.Item().Text(AdvNinos).FontSize(6.5f).Bold();
                col.Item().Text(AdvConservacion).FontSize(6.5f);
            });
        }));

        return GuardarDocumento(documento, "Etiqueta reverso", "ETQ-R", paciente, spd.NumRegistro, farmacia, "SPD", spdId, usuarioQueEjecutaId);
    }

    // ------------------------------------------------------------------ Anexo I.H — INSTR

    public ResultadoGeneracionDocumento GenerarInstrucciones(int spdId, int? usuarioQueEjecutaId)
    {
        var (spd, paciente, lineas) = ObtenerDatosSpd(spdId);
        return ConstruirInstrucciones(paciente, lineas, spd.NumRegistro, spd.FechaPreparacion, spd.ValidezDesde, spd.ValidezHasta, spd.NumRegistro, spdId, usuarioQueEjecutaId);
    }

    public ResultadoGeneracionDocumento GenerarInstruccionesSesion(Guid sesionId, int? usuarioQueEjecutaId)
    {
        var spds = repositorioSpd.ListarPorSesion(sesionId).Where(s => s.Estado != EstadoSpd.Anulado).OrderBy(s => s.ValidezDesde).ToList();
        if (spds.Count == 0) throw new ErrorValidacionException("La sesión no tiene blísteres.");
        if (spds.Count == 1) return GenerarInstrucciones(spds[0].Id, usuarioQueEjecutaId);

        var paciente = ObtenerPaciente(spds[0].PacienteId);
        var lineasPorSpd = spds.Select(s => repositorioLineas.ListarPorSpd(s.Id).Where(l => l.EstadoLinea != EstadoLinea.Excluida)
            .Select(l => (l.MedicamentoId, l.SnapPautaD, l.SnapPautaA, l.SnapPautaC, l.SnapPautaN, l.SnapMomento)).OrderBy(x => x.MedicamentoId).ToList()).ToList();
        if (lineasPorSpd.Any(l => !l.SequenceEqual(lineasPorSpd[0])))
            throw new ErrorValidacionException("Los blísteres de la sesión no tienen el mismo contenido: imprima una hoja de instrucciones por blíster (FR-682).");

        var registros = string.Join(" / ", spds.Select(s => s.NumRegistro));
        return ConstruirInstrucciones(paciente, repositorioLineas.ListarPorSpd(spds[0].Id), registros, spds[0].FechaPreparacion,
            spds[0].ValidezDesde, spds[^1].ValidezHasta, spds[0].NumRegistro, spds[0].Id, usuarioQueEjecutaId);
    }

    private ResultadoGeneracionDocumento ConstruirInstrucciones(
        Paciente paciente, IReadOnlyList<SpdLinea> lineas, string numRegistro, DateTime? fechaPreparacion,
        DateOnly validezDesde, DateOnly validezHasta, string identificadorCorto, int spdId, int? usuarioQueEjecutaId)
    {
        var farmacia = ObtenerFarmacia();
        var incluidas = lineas.Where(l => l.EstadoLinea != EstadoLinea.Excluida).ToList();
        var noIncluidos = TratamientosNoIncluidos(paciente.Id, lineas);

        var documento = Document.Create(contenedor => contenedor.Page(pagina =>
        {
            ConfigurarPagina(pagina, PageSizes.A4, 10);
            pagina.Header().Element(e => Cabecera(e, farmacia, "SISTEMA PERSONALIZADO DE DOSIFICACIÓN — HOJA DE INSTRUCCIONES"));
            pagina.Content().PaddingTop(8).Column(col =>
            {
                col.Spacing(6);
                col.Item().Row(fila =>
                {
                    fila.RelativeItem().Text(t => { t.Span("Paciente: ").Bold(); t.Span($"{paciente.Nombre} {paciente.Apellidos}"); });
                    fila.RelativeItem().Column(c =>
                    {
                        c.Item().Text(t => { t.Span("FECHA PREPARACIÓN: ").Bold(); t.Span(Fecha(fechaPreparacion)); });
                        c.Item().Text(t => { t.Span("Nº REGISTRO DDP: ").Bold(); t.Span(numRegistro); });
                        c.Item().Text(t => { t.Span("PERIODO DE VALIDEZ: ").Bold(); t.Span($"{validezDesde:dd/MM/yyyy} — {validezHasta:dd/MM/yyyy}"); });
                    });
                });

                col.Item().Text("• Medicamentos incluidos en el DDP").Bold();
                col.Item().Table(tabla =>
                {
                    DefinirColumnasInstrucciones(tabla);
                    foreach (var linea in incluidas)
                    {
                        var tratamiento = repositorioTratamientos.ObtenerPorId(linea.TratamientoId);
                        Celda(tabla, linea.SnapCn);
                        Celda(tabla, linea.SnapNombre);
                        Celda(tabla, NombreMedico(tratamiento?.MedicoId));
                        Celda(tabla, Posologia(linea.SnapPautaD, linea.SnapPautaA, linea.SnapPautaC, linea.SnapPautaN, linea.SnapMomento));
                        Celda(tabla, FechaPrescripcion(tratamiento));
                    }
                });

                col.Item().Text("• Medicamentos no introducidos en el DDP que forman parte del tratamiento farmacológico").Bold();
                col.Item().Table(tabla =>
                {
                    DefinirColumnasInstrucciones(tabla);
                    if (noIncluidos.Count == 0)
                    {
                        Celda(tabla, "—"); Celda(tabla, "Ninguno"); Celda(tabla, ""); Celda(tabla, ""); Celda(tabla, "");
                    }
                    foreach (var (tratamiento, medicamento) in noIncluidos)
                    {
                        Celda(tabla, medicamento?.Cn ?? "");
                        Celda(tabla, medicamento?.Nombre ?? "?");
                        Celda(tabla, NombreMedico(tratamiento.MedicoId));
                        Celda(tabla, Posologia(tratamiento));
                        Celda(tabla, FechaPrescripcion(tratamiento));
                    }
                });

                col.Item().PaddingTop(6).Text("Advertencias de uso:").Bold();
                col.Item().Text("- Manténgase fuera del alcance de los niños.");
                col.Item().Text("- No utilizar después del periodo de validez indicado anteriormente.");
                col.Item().Text("- Condiciones de conservación: \"conservar en lugar fresco, seco y protegido de la luz\".");
                col.Item().Text("- Recuerde comunicar a su farmacéutico cuanto antes cualquier cambio de medicación.");
                if (paciente.PictogramaComidas)
                    col.Item().Text("- Tome la medicación en relación con las comidas tal y como indica cada casilla del dispositivo.");
            });
        }));

        return GuardarDocumento(documento, "Hoja de instrucciones", "INSTR", paciente, identificadorCorto, farmacia, "SPD", spdId, usuarioQueEjecutaId);
    }

    // ------------------------------------------------------------------ Anexo I.E — FICHA-PAC

    public ResultadoGeneracionDocumento GenerarFichaPaciente(int pacienteId, int? usuarioQueEjecutaId)
    {
        var paciente = ObtenerPaciente(pacienteId);
        var farmacia = ObtenerFarmacia();
        var contactos = repositorioContactos.ListarDePaciente(pacienteId, incluirBaja: false);
        var familiar = contactos.FirstOrDefault(c => c.EsPrincipal) ?? contactos.FirstOrDefault();
        var medicoFamilia = paciente.MedicoId is int mid ? repositorioMedicos.ObtenerPorId(mid) : null;
        var tratamientos = repositorioTratamientos.ListarVigentesDePaciente(pacienteId);
        var entregas = repositorioSpd.Listar(EstadoSpd.Entregado, pacienteId, null).OrderBy(s => s.FechaEntrega).ToList();

        var documento = Document.Create(contenedor => contenedor.Page(pagina =>
        {
            ConfigurarPagina(pagina, PageSizes.A4, 9);
            pagina.Header().Element(e => Cabecera(e, farmacia, "SISTEMA PERSONALIZADO DE DOSIFICACIÓN — FICHA DEL PACIENTE"));
            pagina.Content().PaddingTop(8).Column(col =>
            {
                col.Spacing(4);
                col.Item().Row(fila =>
                {
                    fila.RelativeItem().Text(t => { t.Span("Ficha nº: ").Bold(); t.Span(paciente.NumFicha); });
                    fila.RelativeItem().Text(t => { t.Span("Fecha: ").Bold(); t.Span(paciente.FechaAltaFicha.ToString("dd/MM/yyyy")); });
                });
                col.Item().Text(t => { t.Span("Nombre: ").Bold(); t.Span($"{paciente.Nombre} {paciente.Apellidos}"); t.Span("    Fecha nacimiento: ").Bold(); t.Span(paciente.FechaNacimiento?.ToString("dd/MM/yyyy") ?? "—"); });
                col.Item().Text(t => { t.Span("DNI: ").Bold(); t.Span(paciente.Dni ?? "—"); t.Span("    Nº SS: ").Bold(); t.Span(paciente.NumSs ?? "—"); t.Span("    CIP: ").Bold(); t.Span(paciente.Cip ?? "—"); });
                col.Item().Text(t => { t.Span("Dirección: ").Bold(); t.Span(paciente.Direccion ?? "—"); t.Span("    Población: ").Bold(); t.Span(paciente.Poblacion ?? "—"); t.Span("    Código postal: ").Bold(); t.Span(paciente.Cp ?? "—"); });
                col.Item().Text(t => { t.Span("Teléfonos: ").Bold(); t.Span(string.Join(" / ", new[] { paciente.Telefono1, paciente.Telefono2 }.Where(s => !string.IsNullOrWhiteSpace(s)))); t.Span("    e-mail: ").Bold(); t.Span(paciente.Email ?? "—"); });
                col.Item().Text(t =>
                {
                    t.Span("Familiar o cuidador: ").Bold();
                    t.Span(familiar is null ? "—" : $"{familiar.Nombre} {familiar.Apellidos} ({familiar.Tipo})");
                    t.Span("    Teléfono: ").Bold(); t.Span(familiar?.Telefono ?? "—");
                    t.Span("    e-mail: ").Bold(); t.Span(familiar?.Email ?? "—");
                });
                col.Item().Text(t =>
                {
                    t.Span("Médico de familia: ").Bold();
                    t.Span(medicoFamilia is null ? "—" : $"{medicoFamilia.Nombre} {medicoFamilia.Apellidos}" + (string.IsNullOrWhiteSpace(medicoFamilia.Centro) ? "" : $" ({medicoFamilia.Centro})"));
                    t.Span("    Teléfono: ").Bold(); t.Span(medicoFamilia?.Telefono ?? "—");
                    t.Span("    e-mail: ").Bold(); t.Span(medicoFamilia?.Email ?? "—");
                });
                col.Item().Text(t => { t.Span("Enfermedades crónicas: ").Bold(); t.Span(paciente.EnfermedadesCronicas ?? "—"); });
                col.Item().Text(t => { t.Span("Alergias e intolerancias: ").Bold(); t.Span(paciente.Alergias ?? "—").FontColor(Colors.Red.Medium); });
                col.Item().Text(t => { t.Span("Observaciones: ").Bold(); t.Span(paciente.Observaciones ?? "—"); });
                col.Item().Text($"Día de retirada: {paciente.DiaRetirada} — Nº blísteres: {paciente.NBlisteres} — Estado: {paciente.Estado}").FontSize(8).Italic();

                // Bloque del Anexo I.E. Con evaluación vigente (Spec 002) se imprime; sin ella, en
                // blanco para cubrirlo a mano, que es lo que vale legalmente (Art. II).
                var evaluacion = repositorioEvaluaciones.ObtenerVigente(pacienteId);
                col.Item().PaddingTop(6).Text("EVALUACIÓN IDONEIDAD PARA LA INCLUSIÓN EN EL SERVICIO").Bold();
                col.Item().Border(0.5f).Padding(4).Column(c =>
                {
                    if (evaluacion is null)
                    {
                        c.Item().Text("Criterios de inclusión: ______________________________________________________________");
                        c.Item().Text("Observaciones: _______________________________________________________________________");
                        c.Item().PaddingTop(4).Text("APTO  [   ]        NO APTO  [   ]        Firma farmacéutico/a: ______________________");
                        return;
                    }
                    c.Item().Text("Criterios de inclusión:").Bold();
                    for (var i = 0; i < EvaluacionIdoneidad.TextosCriterios.Length; i++)
                        c.Item().Text($"[{(evaluacion.Criterios[i] ? "X" : "  ")}] {EvaluacionIdoneidad.TextosCriterios[i]}").FontSize(8);
                    c.Item().Text($"[{(evaluacion.CondicionMotivacion ? "X" : "  ")}] {EvaluacionIdoneidad.TextosCondiciones[0]}").FontSize(8);
                    c.Item().Text($"[{(evaluacion.CondicionDestreza ? "X" : "  ")}] {EvaluacionIdoneidad.TextosCondiciones[1]}").FontSize(8);
                    c.Item().Text(t => { t.Span("Observaciones: ").Bold(); t.Span(evaluacion.Observaciones ?? "—"); });
                    var apto = evaluacion.Resultado == ResultadoIdoneidad.Apto;
                    c.Item().PaddingTop(4).Text(t =>
                    {
                        t.Span($"APTO  [{(apto ? "X" : "  ")}]        NO APTO  [{(apto ? "  " : "X")}]        ").Bold();
                        t.Span($"Fecha: {evaluacion.Fecha.ToLocalTime():dd/MM/yyyy}    Farmacéutico/a: {NombreUsuario(evaluacion.FarmaceuticoId)}    Firma: ______________");
                    });
                });

                var incluidos = tratamientos.Where(t => t.EnSpd).ToList();
                var noIncluidos = tratamientos.Where(t => !t.EnSpd).ToList();
                col.Item().PaddingTop(6).Row(fila =>
                {
                    fila.RelativeItem().Text("• Medicamentos incluidos en DDP").Bold();
                    fila.RelativeItem().AlignRight().Text($"FECHA REVISIÓN: {DateTime.Today:dd/MM/yyyy}").Bold();
                });
                col.Item().Element(e => TablaTratamientosFicha(e, incluidos));
                col.Item().PaddingTop(4).Text("• Medicamentos no incluidos en DDP").Bold();
                col.Item().Element(e => TablaTratamientosFicha(e, noIncluidos));

                col.Item().PaddingTop(6).Text("CONTROL ADHERENCIA").Bold();
                col.Item().Table(tabla =>
                {
                    tabla.ColumnsDefinition(c => { c.ConstantColumn(70); c.ConstantColumn(90); c.RelativeColumn(); });
                    Celda(tabla, "Fecha", negrita: true); Celda(tabla, "Cumple adherencia (SI/NO)", negrita: true); Celda(tabla, "Observaciones", negrita: true);
                    if (entregas.Count == 0)
                    {
                        Celda(tabla, ""); Celda(tabla, ""); Celda(tabla, "");
                    }
                    foreach (var e in entregas)
                    {
                        Celda(tabla, Fecha(e.FechaEntrega));
                        Celda(tabla, e.PrimeraEntrega == true ? "Primera entrega" : SiNo(string.IsNullOrWhiteSpace(e.UnidadesNoAdministradas)));
                        Celda(tabla, string.Join(" ", new[]
                        {
                            $"DDP {e.NumRegistro}.",
                            string.IsNullOrWhiteSpace(e.UnidadesNoAdministradas) ? null : $"No administradas: {e.UnidadesNoAdministradas}.",
                            e.ObservacionesAdherencia
                        }.Where(s => !string.IsNullOrWhiteSpace(s))));
                    }
                });
            });
        }));

        return GuardarDocumento(documento, "Ficha del paciente", "FICHA-PAC", paciente, paciente.NumFicha, farmacia, "Paciente", pacienteId, usuarioQueEjecutaId);
    }

    // ------------------------------------------------------------------ Anexo I.D — RGPD

    public ResultadoGeneracionDocumento GenerarInformacionProteccionDatos(int pacienteId, int? usuarioQueEjecutaId)
    {
        var paciente = ObtenerPaciente(pacienteId);
        var farmacia = ObtenerFarmacia();
        var responsable = string.IsNullOrWhiteSpace(farmacia.ResponsableDatos) ? farmacia.Nombre : farmacia.ResponsableDatos;
        var direccionDerechos = string.IsNullOrWhiteSpace(farmacia.DireccionDerechos)
            ? $"{farmacia.Direccion}, {farmacia.Cp} {farmacia.Poblacion}" : farmacia.DireccionDerechos;
        var emailDerechos = string.IsNullOrWhiteSpace(farmacia.EmailDerechos) ? farmacia.Email : farmacia.EmailDerechos;
        var hayDpo = !string.IsNullOrWhiteSpace(farmacia.DpoNombre);

        var documento = Document.Create(contenedor => contenedor.Page(pagina =>
        {
            ConfigurarPagina(pagina, PageSizes.A4, 9.5f);
            pagina.Header().Element(e => Cabecera(e, farmacia, "INFORMACIÓN SOBRE PROTECCIÓN DE DATOS — SERVICIO DE SPD"));
            pagina.Content().PaddingTop(8).Column(col =>
            {
                col.Spacing(5);
                col.Item().Text(t => { t.Span("Paciente: ").Bold(); t.Span($"{paciente.Nombre} {paciente.Apellidos}"); t.Span("    Ficha nº: ").Bold(); t.Span(paciente.NumFicha); if (!string.IsNullOrWhiteSpace(paciente.Dni)) { t.Span("    DNI: ").Bold(); t.Span(paciente.Dni); } });

                col.Item().Text("Por favor lea detenidamente esta política de privacidad. En ella encontrará información importante sobre el tratamiento de sus datos personales en la oficina de farmacia para la elaboración de su sistema personalizado de dosificación y los derechos que tiene con respecto a sus datos. Si tiene dudas o necesita cualquier aclaración respecto a nuestra Política de privacidad o a sus derechos, no dude en contactar con nosotros" + (hayDpo ? " o con el Delegado de Protección de Datos indicado al final." : " o con su Colegio Oficial de Farmacéuticos."));

                Titulo(col, "¿Quién es el responsable del tratamiento de sus datos?");
                col.Item().Text(t => { t.Span("Responsable: ").Bold(); t.Span(responsable); });
                col.Item().Text(t => { t.Span("Dirección postal: ").Bold(); t.Span($"{farmacia.Direccion}, {farmacia.Cp} {farmacia.Poblacion}" + (string.IsNullOrWhiteSpace(farmacia.Provincia) ? "" : $" ({farmacia.Provincia})")); });
                col.Item().Text(t => { t.Span("Teléfono: ").Bold(); t.Span(farmacia.Telefono); });
                col.Item().Text(t => { t.Span("Correo electrónico: ").Bold(); t.Span(farmacia.Email ?? "—"); });

                Titulo(col, "¿Qué información personal obtenemos?");
                col.Item().Text("Los datos que tratamos son los proporcionados por usted como paciente para el seguimiento de tratamientos farmacológicos y atención farmacéutica, para la prestación del servicio de elaboración del sistema personalizado de dosificación de acuerdo con sus necesidades.");
                col.Item().Text("Las categorías de datos que se tratan son: datos identificativos, tratamientos en curso, reacciones adversas a medicamentos, datos de salud relativos a los tratamientos en curso.");

                Titulo(col, "¿Para qué trataremos sus datos?");
                col.Item().Text("Necesitamos sus datos para poder elaborar el sistema personalizado de dosificación.");

                Titulo(col, "¿Cuánto tiempo conservaremos sus datos?");
                // El plazo de borrado es el mismo que aplica la purga (Farmacia.AniosRetencionPurga,
                // constitución Art. III.2): lo impreso y lo ejecutado no pueden discrepar.
                col.Item().Text("Conservamos sus datos para la prestación del servicio de atención farmacéutica y elaboración del sistema personalizado de dosificación. Finalizado el servicio profesional, o cuando la atención prestada se interrumpe o ya no es necesaria, bloqueamos la información transcurrido un año de la inactividad. El bloqueo implica la mera conservación de los datos, sin que se efectúe ningún otro tratamiento, a los meros efectos de posibles responsabilidades. " +
                                $"Eliminamos definitivamente las fichas transcurridos {farmacia.AniosRetencionPurga} años desde la baja en el servicio.");

                Titulo(col, "Base jurídica del tratamiento");
                col.Item().Text("La base jurídica es el cumplimiento de una obligación legal (artículo 6.1.c del RGPD; Ley 41/2002, de 14 de noviembre, básica reguladora de la autonomía del paciente y de derechos y obligaciones en materia de información y documentación clínica, artículo 17; Ley 3/2019, de 2 de julio, de ordenación farmacéutica de Galicia, artículo 8).");

                Titulo(col, "¿A quién podemos comunicar sus datos?");
                col.Item().Text("Sus datos solo serán comunicados a terceros por obligación legal o cuando sea preciso para prestarle el servicio solicitado.");

                Titulo(col, "¿Cuáles son sus derechos?");
                col.Item().Text("Tiene derecho a obtener confirmación de si estamos tratando o no sus datos personales y, en tal caso, a acceder a los mismos. Puede igualmente pedir que sus datos sean rectificados cuando sean inexactos o que se completen los datos que sean incompletos, así como solicitar su supresión cuando, entre otros motivos, los datos ya no sean necesarios para los fines para los que fueron recogidos.");
                col.Item().Text("En determinadas circunstancias, podrá solicitar la limitación del tratamiento de sus datos. En tal caso, solo trataremos los datos afectados para la formulación, el ejercicio o la defensa de reclamaciones o con miras a la protección de los derechos de otras personas.");
                col.Item().Text("En determinadas condiciones y por motivos relacionados con su situación particular, podrá igualmente oponerse al tratamiento de sus datos. En este caso dejaremos de tratar los datos, salvo por motivos legítimos imperiosos que prevalezcan sobre sus intereses, derechos y libertades, o para la formulación, el ejercicio o la defensa de reclamaciones.");
                col.Item().Text(t =>
                {
                    t.Span("Para ejercer sus derechos deberá remitirnos una solicitud acompañada de una copia de su documento nacional de identidad u otro documento válido que le identifique, por correo postal a ");
                    t.Span(direccionDerechos).Bold();
                    if (!string.IsNullOrWhiteSpace(emailDerechos)) { t.Span(" o por correo electrónico a "); t.Span(emailDerechos).Bold(); }
                    t.Span(".");
                });
                col.Item().Text("Podrá obtener más información sobre sus derechos y cómo ejercerlos en la página de la Agencia Española de Protección de Datos: https://www.aepd.es");

                Titulo(col, "Delegado de Protección de Datos");
                col.Item().Text(hayDpo
                    ? $"Si tiene cualquier duda sobre sus derechos o el tratamiento de datos efectuado por la oficina de farmacia puede contactar con {farmacia.DpoNombre}, Delegado de Protección de Datos" + (string.IsNullOrWhiteSpace(farmacia.DpoContacto) ? "." : $", en {farmacia.DpoContacto}.")
                    : "Si tiene cualquier duda sobre sus derechos o el tratamiento de datos efectuado por la oficina de farmacia puede contactar con el Delegado de Protección de Datos del Colegio Oficial de Farmacéuticos de su provincia.");

                col.Item().PaddingTop(14).Text("Recibí una copia de esta información.    Fecha: ____ / ____ / ________    Firma del paciente, representante legal o persona autorizada: ______________________________");
            });
        }));

        return GuardarDocumento(documento, "Información protección de datos", "RGPD", paciente, paciente.NumFicha, farmacia, "Paciente", pacienteId, usuarioQueEjecutaId);
    }

    // ------------------------------------------------------------------ Anexo I.B — CONSENT

    public ResultadoGeneracionDocumento GenerarConsentimiento(int consentimientoId, int? usuarioQueEjecutaId)
    {
        var consentimiento = repositorioConsentimientos.ObtenerPorId(consentimientoId)
            ?? throw new ErrorValidacionException($"No existe el consentimiento {consentimientoId}.");
        var paciente = ObtenerPaciente(consentimiento.PacienteId);
        var farmacia = ObtenerFarmacia();
        var representante = consentimiento.ContactoId is int cid ? repositorioContactos.ObtenerPorId(cid) : null;
        var nombrePaciente = $"{paciente.Nombre} {paciente.Apellidos}";
        var dniPaciente = paciente.Dni ?? "________________";

        // Compromisos literales del Anexo I.B del PNT I.
        string[] compromisos =
        [
            "Conozco el servicio de SPD.",
            "Conozco que el servicio de SPD se ofrece como un acto posterior a la dispensación y que es preparado por la misma oficina de farmacia que realiza la dispensación de mis medicamentos.",
            "Tengo derecho a prescindir del servicio libremente en cualquier momento.",
            "Se me ha facilitado y facilitará toda la información relativa a mi tratamiento de forma actualizada, ordenada y veraz.",
            "En el caso de medicamentos sujetos a prescripción médica, me comprometo a traer siempre con la suficiente antelación las recetas necesarias para efectuar la dispensación previa a la preparación de SPD o, en su caso, a solicitar la renovación de la prescripción en el módulo de receta electrónica en tiempo y forma.",
            "Me comprometo a presentar la tarjeta sanitaria individual para el acceso del/de la farmacéutico/a al módulo de receta electrónica o para la dispensación de recetas en formato papel.",
            "Me comprometo a informar puntualmente al personal farmacéutico de los cambios de tratamiento y a presentar la justificación correspondiente de dichos cambios por escrito.",
            "Presto mi consentimiento para que se reacondicione la medicación como servicio posterior a la dispensación.",
            "Autorizo que la medicación restante quede en depósito en la farmacia.",
            "Cumpliré con las condiciones de conservación y seguridad del SPD.",
            "Facilitaré al personal farmacéutico la información necesaria para comprobar la adherencia a mi tratamiento. En cada acto de entrega de SPD, proporcionaré la información sobre la administración de los medicamentos que me facilitaron en SPD con anterioridad o aportaré los dispositivos de SPD empleados para posibilitar la comprobación del cumplimiento de las pautas posológicas establecidas y para la destrucción de aquellos dispositivos no reutilizables.",
        ];

        var documento = Document.Create(contenedor => contenedor.Page(pagina =>
        {
            ConfigurarPagina(pagina, PageSizes.A4, 9.5f);
            pagina.Header().Element(e => Cabecera(e, farmacia, "CONSENTIMIENTO INFORMADO — SERVICIO DE SISTEMAS PERSONALIZADOS DE DOSIFICACIÓN (SPD)"));
            pagina.Content().PaddingTop(10).Column(col =>
            {
                col.Spacing(6);
                col.Item().Text(t =>
                {
                    if (representante is null)
                    {
                        t.Span("D./Dña. "); t.Span(nombrePaciente).Bold(); t.Span(" con DNI "); t.Span(dniPaciente).Bold();
                        t.Span(", en nombre propio, ");
                    }
                    else
                    {
                        t.Span("D./Dña. "); t.Span($"{representante.Nombre} {representante.Apellidos}").Bold();
                        t.Span(" con DNI "); t.Span(representante.Dni ?? "________________").Bold();
                        t.Span(representante.Tipo == TipoContacto.RepresentanteLegal ? ", como representante legal de D./Dña. " : ", como persona autorizada de D./Dña. ");
                        t.Span(nombrePaciente).Bold(); t.Span(" con DNI "); t.Span(dniPaciente).Bold(); t.Span(", ");
                    }
                    t.Span("AUTORIZO a la farmacia ").Bold(); t.Span(farmacia.Nombre).Bold();
                    t.Span(" para que me preste el servicio de elaboración y entrega de sistemas personalizados de dosificación (SPD).");
                });
                col.Item().Text("Manifiesto que, con carácter previo a prestar mi consentimiento, he sido informado de que:");
                foreach (var c in compromisos)
                    col.Item().PaddingLeft(10).Text($"▪ {c}");

                col.Item().PaddingTop(10).Text(consentimiento.FechaFirma is DateOnly f
                    ? $"{farmacia.Poblacion}, a {f.Day} de {f.ToString("MMMM", new System.Globalization.CultureInfo("es-ES"))} de {f.Year}"
                    : $"{farmacia.Poblacion}, a ______ de ____________________ de ________");
                col.Item().PaddingTop(18).Row(fila =>
                {
                    fila.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Firma del/de la paciente, representante legal o persona autorizada").FontSize(8);
                        c.Item().PaddingTop(24).Text("_________________________________");
                    });
                    fila.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Firma del farmacéutico/a").FontSize(8);
                        c.Item().PaddingTop(24).Text("_________________________________");
                        if (!string.IsNullOrWhiteSpace(farmacia.TitularColegiado))
                            c.Item().Text($"Nº colegiado/a: {farmacia.TitularColegiado}").FontSize(8);
                    });
                });
                col.Item().PaddingTop(8).Text("Una copia de este consentimiento se entrega al paciente y otra se conserva en el archivo de la farmacia (PNT I §4.2).").FontSize(7).Italic();
            });
        }));

        var resultado = GuardarDocumento(documento, "Consentimiento informado", "CONSENT", paciente, paciente.NumFicha, farmacia, "Paciente", paciente.Id, usuarioQueEjecutaId);
        consentimiento.ImpresoEn = DateTime.UtcNow;
        repositorioConsentimientos.Actualizar(consentimiento);
        return resultado;
    }

    // ------------------------------------------------------------------ Anexo I.C — CARTA-PRES / CARTA-INC

    public ResultadoGeneracionDocumento GenerarCartaMedico(int comunicacionId, int? usuarioQueEjecutaId)
    {
        var comunicacion = repositorioComunicaciones.ObtenerPorId(comunicacionId)
            ?? throw new ErrorValidacionException($"No existe la comunicación {comunicacionId}.");
        if (!comunicacion.EsImprimible)
            throw new ErrorValidacionException("Una comunicación telefónica es un registro interno: no genera carta (Spec 008 FR-806).");

        var paciente = ObtenerPaciente(comunicacion.PacienteId);
        var farmacia = ObtenerFarmacia();
        var medico = repositorioMedicos.ObtenerPorId(comunicacion.MedicoId);
        var esPresentacion = comunicacion.Tipo == TipoComunicacionMedico.Presentacion;
        var farmaceutico = comunicacion.FarmaceuticoId is int fid ? repositorioUsuarios.ObtenerPorId(fid) : null;
        var colegiado = farmaceutico?.Colegiado ?? farmacia.TitularColegiado ?? "______________";
        var fecha = comunicacion.Fecha;
        var mes = fecha.ToString("MMMM", new System.Globalization.CultureInfo("es-ES"));

        var documento = Document.Create(contenedor => contenedor.Page(pagina =>
        {
            ConfigurarPagina(pagina, PageSizes.A4, 10);
            pagina.Header().Element(e => Cabecera(e, farmacia, esPresentacion ? "CARTA DE PRESENTACIÓN DEL SERVICIO DE SPD AL MÉDICO" : "COMUNICACIÓN DE INCIDENCIAS AL MÉDICO — SERVICIO DE SPD"));
            pagina.Content().PaddingTop(12).Column(col =>
            {
                col.Spacing(8);
                col.Item().AlignRight().Text($"En {farmacia.Poblacion}, a {fecha.Day} de {mes} de {fecha.Year}");
                col.Item().Text($"Apreciado/a Dr./a. {(medico is null ? "______________________" : $"{medico.Nombre} {medico.Apellidos}")}" +
                                (string.IsNullOrWhiteSpace(medico?.Centro) ? "" : $" ({medico!.Centro})") + ":");

                if (esPresentacion)
                {
                    // Texto literal del Anexo I.C del PNT I.
                    col.Item().Text("Con objeto de mejorar el cumplimiento del tratamiento farmacológico, esta farmacia ofrece a los pacientes que lo necesitan el Sistema Personalizado de Dosificación o SPD. La no observancia del tratamiento puede comportar el fracaso de una terapia bien prescrita y comprometer los resultados esperados de ella.");
                    col.Item().Text("El sistema personalizado de dosificación es el conjunto de actuaciones de atención farmacéutica que, tras la dispensación de los medicamentos y la solicitud del/de la paciente, de la persona autorizada o de su representante legal, consisten en reacondicionar para un período de tiempo determinado todos o parte de los medicamentos dispensados en dispositivos de dosificación personalizados, así como en facilitar al/a la paciente una adecuada información, con el fin de mejorar el cumplimiento del tratamiento farmacoterapéutico y de prevenir y resolver los problemas relacionados con los medicamentos.");
                    col.Item().Text(t =>
                    {
                        t.Span("Con este servicio pretendemos mejorar la organización de los medicamentos e incidir de manera directa en una mejor adherencia terapéutica y uso de los medicamentos con el fin de que el tratamiento que usted ha prescrito al paciente ");
                        t.Span($"{paciente.Nombre} {paciente.Apellidos}").Bold();
                        if (!string.IsNullOrWhiteSpace(paciente.Dni)) { t.Span(" (DNI "); t.Span(paciente.Dni).Bold(); t.Span(")"); }
                        t.Span(" se cumpla de forma correcta, detectando posibles incumplimientos e informándole a usted de los posibles problemas que pudiesen aparecer.");
                    });
                    col.Item().Text("Se adjunta Ficha del Paciente con su tratamiento completo. Si hubiera alguna discrepancia, ruego se ponga en contacto conmigo a la mayor brevedad posible, en el correo o teléfono abajo indicados.");
                }
                else
                {
                    col.Item().Text(t =>
                    {
                        t.Span("En relación con el/la paciente ");
                        t.Span($"{paciente.Nombre} {paciente.Apellidos}").Bold();
                        if (!string.IsNullOrWhiteSpace(paciente.Dni)) { t.Span(" (DNI "); t.Span(paciente.Dni).Bold(); t.Span(")"); }
                        t.Span(", incluido/a en el servicio de Sistemas Personalizados de Dosificación (SPD) de esta farmacia, durante la revisión de su tratamiento farmacoterapéutico hemos detectado las siguientes incidencias:");
                    });
                    col.Item().PaddingLeft(12).Text(comunicacion.IncidenciasDetectadas ?? "—");
                    col.Item().Text("Propuesta del farmacéutico:").Bold();
                    col.Item().PaddingLeft(12).Text(comunicacion.Propuesta ?? "—");
                    col.Item().Text("Le ruego valore esta información y, si lo estima oportuno, me comunique su decisión por escrito para actualizar la ficha del paciente y, en su caso, la preparación del SPD. Hasta entonces se mantiene la pauta prescrita vigente.");
                }

                col.Item().Text("Agradecemos por adelantado su colaboración y le saludamos cordialmente,");
                col.Item().PaddingTop(16).Row(fila =>
                {
                    fila.RelativeItem().Column(c =>
                    {
                        c.Item().Text(t => { t.Span("Farmacéutico/a responsable: ").Bold(); t.Span(farmaceutico is null ? "______________________" : $"{farmaceutico.Nombre} {farmaceutico.Apellidos}"); });
                        c.Item().Text(t => { t.Span("Nº de colegiado/a: ").Bold(); t.Span(colegiado); });
                        c.Item().PaddingTop(20).Text("Firma: _____________________________");
                    });
                });
                col.Item().PaddingTop(14).Text($"P.D.: Para una información más detallada o cualquier sugerencia, sírvase contactar con nosotros en {farmacia.Nombre}, {farmacia.Direccion}, {farmacia.Cp} {farmacia.Poblacion} — teléfono {farmacia.Telefono}" +
                                                (string.IsNullOrWhiteSpace(farmacia.Email) ? "." : $" — {farmacia.Email}.")).FontSize(8.5f);
            });
        }));

        return GuardarDocumento(
            documento, esPresentacion ? "Carta de presentación al médico" : "Carta de incidencias al médico",
            esPresentacion ? "CARTA-PRES" : "CARTA-INC", paciente, $"{paciente.NumFicha}-C{comunicacion.Id}", farmacia,
            "ComunicacionMedico", comunicacion.Id, usuarioQueEjecutaId);
    }

    // ------------------------------------------------------------------ datos

    private (SPD Spd, Paciente Paciente, IReadOnlyList<SpdLinea> Lineas) ObtenerDatosSpd(int spdId)
    {
        var spd = repositorioSpd.ObtenerPorId(spdId) ?? throw new ErrorValidacionException($"No existe el SPD {spdId}.");
        return (spd, ObtenerPaciente(spd.PacienteId), repositorioLineas.ListarPorSpd(spdId));
    }

    private Paciente ObtenerPaciente(int pacienteId)
        => repositorioPacientes.ObtenerPorId(pacienteId) ?? throw new ErrorValidacionException($"No existe el paciente {pacienteId}.");

    private Farmacia ObtenerFarmacia() => repositorioFarmacia.Obtener() ?? throw new ErrorValidacionException("No hay configuración de farmacia.");

    /// <summary>Tratamientos vigentes del paciente que no van en este blíster: los marcados
    /// "no en SPD" (Spec 004) y los que, aun siendo aptos, no tienen línea no excluida en él.
    /// Es lo que el anverso de la etiqueta y la hoja de instrucciones deben listar aparte.</summary>
    private List<(Tratamiento Tratamiento, Medicamento? Medicamento)> TratamientosNoIncluidos(int pacienteId, IReadOnlyList<SpdLinea> lineas)
    {
        var enBlister = lineas.Where(l => l.EstadoLinea != EstadoLinea.Excluida).Select(l => l.TratamientoId).ToHashSet();
        return repositorioTratamientos.ListarVigentesDePaciente(pacienteId)
            .Where(t => !t.EnSpd || !enBlister.Contains(t.Id))
            .Select(t => (t, repositorioMedicamentos.ObtenerPorId(t.MedicamentoId)))
            .ToList();
    }

    private string NombreUsuario(int? usuarioId)
    {
        if (usuarioId is not int id) return "";
        var u = repositorioUsuarios.ObtenerPorId(id);
        return u is null ? "" : $"{u.Nombre} {u.Apellidos}";
    }

    private string NombreMedico(int? medicoId)
    {
        if (medicoId is not int id) return "—";
        var m = repositorioMedicos.ObtenerPorId(id);
        return m is null ? "—" : $"{m.Nombre} {m.Apellidos}";
    }

    private static IEnumerable<(string Pregunta, bool? Valor)> PreguntasVerificacion(SpdVerificacion? v) =>
    [
        ("¿El contenido del DDP es correcto? (comprobar con ficha de paciente)", v?.VerifContenido),
        ("¿Se siguieron instrucciones del fabricante y PNTs durante todo el proceso?", v?.VerifFabricantePnt),
        ("¿Coinciden los datos de DDP de la etiqueta con el de la ficha de preparación, control y entrega?", v?.VerifEtiquetaDatos),
        ("¿Coinciden los datos de etiqueta y ficha de paciente a la fecha actual?", v?.VerifEtiquetaFichaPaciente),
        ("¿Se garantizó trazabilidad entre el envase original y DDP acondicionado?", v?.VerifTrazabilidad),
        ("¿El DDP está identificado con su periodo de validez?", v?.VerifEtiquetaValidez),
        ("¿Se cumplimentó la hoja de instrucciones según la normativa vigente?", v?.VerifInstrucciones),
        // El anexo pregunta "¿Existen alteraciones visibles?"; la app registra "íntegro", se invierte al imprimir.
        ("¿Existen alteraciones visibles en el producto acabado? (rotura, cartón arrugado, fallos en el cierre, etc.)", v is null ? null : !v.VerifAspecto),
    ];

    // ------------------------------------------------------------------ formato

    // FR-740/743: lo impreso siempre en fracción, nunca en decimal.
    private static string FraccionTexto(FraccionDosis? dosis) => dosis?.Texto() ?? "0";

    private static string Posologia(FraccionDosis? d, FraccionDosis? a, FraccionDosis? c, FraccionDosis? n, string? momento)
        => $"{FraccionTexto(d)} - {FraccionTexto(a)} - {FraccionTexto(c)} - {FraccionTexto(n)}" + (string.IsNullOrWhiteSpace(momento) ? "" : $" ({momento})");

    private static string Posologia(Tratamiento t)
        => !string.IsNullOrWhiteSpace(t.PautaTexto)
            ? t.PautaTexto + (string.IsNullOrWhiteSpace(t.Via) ? "" : $" ({t.Via})")
            : Posologia(t.PautaD, t.PautaA, t.PautaC, t.PautaN, t.Momento) + (string.IsNullOrWhiteSpace(t.Via) ? "" : $" ({t.Via})");

    /// <summary>Anexo I.H: fecha de prescripción inicial y, si la fila de tratamiento se abrió
    /// después (Spec 004 cierra y abre fila en cada cambio de pauta), la última modificación.</summary>
    private static string FechaPrescripcion(Tratamiento? t)
    {
        if (t is null) return "—";
        var texto = t.FechaPrescripcionInicial.ToString("dd/MM/yyyy");
        return t.FechaInicio > t.FechaPrescripcionInicial ? $"{texto} / modif. {t.FechaInicio:dd/MM/yyyy}" : texto;
    }

    private static string Fecha(DateTime? f) => f is DateTime d ? d.ToString("dd/MM/yyyy") : "____/____/______";

    private static string SiNo(bool valor) => valor ? "SÍ" : "NO";

    private static void ConfigurarPagina(PageDescriptor pagina, PageSize tamano, float fuente)
    {
        pagina.Size(tamano);
        pagina.Margin(1, Unit.Centimetre);
        pagina.DefaultTextStyle(estilo => estilo.FontSize(fuente));
    }

    /// <summary>Cabecera institucional común (FR-730: logo, nombre, código sanitario, dirección,
    /// teléfono). El logo se incluye si la ruta configurada existe.</summary>
    private static void Cabecera(IContainer contenedor, Farmacia farmacia, string titulo)
        => contenedor.Column(col =>
        {
            col.Item().Row(fila =>
            {
                if (!string.IsNullOrWhiteSpace(farmacia.Logo) && File.Exists(farmacia.Logo))
                    fila.ConstantItem(60).PaddingRight(8).Image(farmacia.Logo);
                fila.RelativeItem().Column(c =>
                {
                    c.Item().Text(farmacia.Nombre).FontSize(13).Bold();
                    c.Item().Text($"{farmacia.Direccion}, {farmacia.Cp} {farmacia.Poblacion} — Tel. {farmacia.Telefono}" + (string.IsNullOrWhiteSpace(farmacia.Email) ? "" : $" — {farmacia.Email}")).FontSize(8);
                    c.Item().Text($"Código sanitario {farmacia.CodigoSanitario} — {farmacia.TitularOComunidadBienes} — CIF {farmacia.Cif}").FontSize(8);
                });
            });
            col.Item().PaddingTop(4).LineHorizontal(0.75f);
            col.Item().PaddingTop(4).Text(titulo).FontSize(11).Bold();
        });

    private static void Titulo(ColumnDescriptor col, string texto) => col.Item().PaddingTop(4).Text(texto).Bold();

    private static void Celda(TableDescriptor tabla, string texto, bool negrita = false)
    {
        var t = tabla.Cell().Border(0.5f).Padding(2).Text(texto ?? "");
        if (negrita) t.Bold();
    }

    private static void FilaMedicamento(TableDescriptor tabla, SpdLinea linea, string lote, string caducidad, string serie)
    {
        Celda(tabla, linea.SnapCn);
        Celda(tabla, linea.SnapNombre);
        Celda(tabla, FraccionTexto(linea.SnapPautaD));
        Celda(tabla, FraccionTexto(linea.SnapPautaA));
        Celda(tabla, FraccionTexto(linea.SnapPautaC));
        Celda(tabla, FraccionTexto(linea.SnapPautaN));
        Celda(tabla, lote);
        Celda(tabla, caducidad);
        Celda(tabla, serie);
    }

    private static void DefinirColumnasInstrucciones(TableDescriptor tabla)
    {
        tabla.ColumnsDefinition(c => { c.ConstantColumn(48); c.RelativeColumn(2.5f); c.RelativeColumn(2); c.RelativeColumn(2); c.RelativeColumn(1.6f); });
        foreach (var cab in new[] { "CN", "Medicamento", "Médico prescriptor", "Posología (D - A - C - N)", "Fecha prescripción / última modificación" })
            Celda(tabla, cab, negrita: true);
    }

    private void TablaTratamientosFicha(IContainer contenedor, IReadOnlyList<Tratamiento> tratamientos)
        => contenedor.Table(tabla =>
        {
            tabla.ColumnsDefinition(c =>
            {
                c.ConstantColumn(42); c.RelativeColumn(2); c.RelativeColumn(1.4f); c.RelativeColumn(1.4f); c.RelativeColumn(1.6f);
                c.ConstantColumn(60); c.ConstantColumn(60); c.RelativeColumn(1.4f); c.RelativeColumn(1.4f);
            });
            foreach (var cab in new[] { "CN", "Medicamento", "Problema de salud", "Médico prescriptor", "Posología y vía", "Inicio tto.", "Fin tto.", "PRM/RNM", "Intervención" })
                Celda(tabla, cab, negrita: true);
            if (tratamientos.Count == 0)
                for (var i = 0; i < 9; i++) Celda(tabla, i == 1 ? "Ninguno" : "");
            foreach (var t in tratamientos)
            {
                var m = repositorioMedicamentos.ObtenerPorId(t.MedicamentoId);
                Celda(tabla, m?.Cn ?? "");
                Celda(tabla, m?.Nombre ?? "?");
                Celda(tabla, t.ProblemaSalud ?? "");
                Celda(tabla, NombreMedico(t.MedicoId));
                Celda(tabla, Posologia(t));
                Celda(tabla, t.FechaInicio.ToString("dd/MM/yyyy"));
                Celda(tabla, t.FechaFin?.ToString("dd/MM/yyyy") ?? "");
                Celda(tabla, t.Incidencias ?? "");
                Celda(tabla, t.Intervencion ?? "");
            }
        });

    private static void Firma(RowDescriptor fila, string etiqueta, string nombre, DateTime? fecha)
        => fila.RelativeItem().Column(c =>
        {
            c.Item().Text($"{etiqueta}:").Bold();
            c.Item().Text(string.IsNullOrWhiteSpace(nombre) ? "______________________" : nombre);
            c.Item().PaddingTop(14).Text("Firma: ______________________");
            c.Item().Text($"Fecha: {Fecha(fecha)}");
        });

    // ------------------------------------------------------------------ salida común

    private ResultadoGeneracionDocumento GuardarDocumento(
        IDocument documento, string nombreDocumento, string codigo, Paciente paciente, string identificadorCorto,
        Farmacia farmacia, string entidad, int entidadId, int? usuarioQueEjecutaId)
    {
        var fecha = DateOnly.FromDateTime(DateTime.Today);
        var nombreFichero = GeneradorNombreFichero.Generar(nombreDocumento, codigo, paciente.Nombre, paciente.Apellidos, identificadorCorto, fecha);

        var rutaBase = string.IsNullOrWhiteSpace(farmacia.RutaDocumentosGenerados)
            ? Path.Combine(AppContext.BaseDirectory, "documentos-generados")
            : farmacia.RutaDocumentosGenerados;
        var carpeta = Path.Combine(rutaBase, fecha.ToString("yyyy-MM-dd"));
        Directory.CreateDirectory(carpeta);

        var rutaCompleta = Path.Combine(carpeta, nombreFichero);
        documento.GeneratePdf(rutaCompleta);

        auditoria.Registrar(usuarioQueEjecutaId, "GENERAR_DOCUMENTO", entidad, entidadId, $"tipo={codigo};ruta={rutaCompleta}");

        return new ResultadoGeneracionDocumento(rutaCompleta, nombreFichero);
    }
}
