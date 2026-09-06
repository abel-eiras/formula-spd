using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Spd.Aplicacion;
using Spd.Dominio;

namespace Spd.Infraestructura;

/// <summary>Motor único de generación de documentos PDF (FR-700..713). Un método privado común
/// (`GuardarDocumento`) aplica el nombrado, la carpeta de salida y la auditoría para los cinco
/// generadores públicos, evitando duplicar esa lógica (research.md Decisión 2 de Spec 007).</summary>
public sealed class ServicioGeneracionDocumentos(
    IRepositorioSpd repositorioSpd, IRepositorioSpdLineas repositorioLineas, IRepositorioSpdLineaEnvases repositorioLineaEnvases,
    IRepositorioPacientes repositorioPacientes, IRepositorioFarmacia repositorioFarmacia, IRegistradorAuditoria auditoria)
    : IServicioGeneracionDocumentos
{
    public ResultadoGeneracionDocumento GenerarFichaSpd(int spdId, int? usuarioQueEjecutaId)
    {
        var (spd, paciente, lineas) = ObtenerDatosSpd(spdId);
        var farmacia = ObtenerFarmacia();

        var documento = Document.Create(contenedor => contenedor.Page(pagina =>
        {
            ConfigurarPagina(pagina, PageSizes.A4);
            pagina.Header().Column(col =>
            {
                col.Item().Text(farmacia.Nombre).FontSize(14).Bold();
                col.Item().Text($"Ficha de preparación, control y entrega — {spd.NumRegistro}").FontSize(12);
            });
            pagina.Content().Column(col =>
            {
                col.Item().PaddingTop(10).Text($"Paciente: {paciente.Nombre} {paciente.Apellidos} (ficha {paciente.NumFicha})");
                col.Item().Text($"Validez: {spd.ValidezDesde:dd/MM/yyyy} — {spd.ValidezHasta:dd/MM/yyyy}");
                col.Item().Text($"Estado: {spd.Estado}");
                col.Item().PaddingTop(10).Table(tabla =>
                {
                    tabla.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(3); c.RelativeColumn(1); c.RelativeColumn(1); c.RelativeColumn(1); c.RelativeColumn(1); c.RelativeColumn(2);
                    });
                    foreach (var cabecera in new[] { "Medicamento", "D", "A", "C", "N", "Vía / momento" })
                        tabla.Cell().Border(1).Padding(2).Text(cabecera).Bold();

                    foreach (var linea in lineas.Where(l => l.EstadoLinea != EstadoLinea.Excluida))
                    {
                        tabla.Cell().Border(1).Padding(2).Text(linea.SnapNombre);
                        tabla.Cell().Border(1).Padding(2).Text(FraccionTexto(linea.SnapPautaD));
                        tabla.Cell().Border(1).Padding(2).Text(FraccionTexto(linea.SnapPautaA));
                        tabla.Cell().Border(1).Padding(2).Text(FraccionTexto(linea.SnapPautaC));
                        tabla.Cell().Border(1).Padding(2).Text(FraccionTexto(linea.SnapPautaN));
                        tabla.Cell().Border(1).Padding(2).Text($"{linea.SnapMomento} — {string.Join(", ", ListarEnvasesDeLinea(linea.Id))}");
                    }
                });
                col.Item().PaddingTop(30).Text("Elaborador: ____________________    Verificador: ____________________");
            });
        }));

        return GuardarDocumento(
            documento, "Ficha de preparación", "FICHA", paciente, spd.NumRegistro, farmacia,
            "SPD", spdId, usuarioQueEjecutaId);
    }

    public ResultadoGeneracionDocumento GenerarEtiquetaAnverso(int spdId, int? usuarioQueEjecutaId)
    {
        var (spd, paciente, _) = ObtenerDatosSpd(spdId);
        var farmacia = ObtenerFarmacia();

        var documento = Document.Create(contenedor => contenedor.Page(pagina =>
        {
            ConfigurarPagina(pagina, new PageSize(400, 250));
            pagina.Content().Column(col =>
            {
                col.Item().Text(farmacia.Nombre).FontSize(10).Bold();
                col.Item().Text($"{paciente.Nombre} {paciente.Apellidos}").FontSize(12).Bold();
                col.Item().Text($"Ficha: {paciente.NumFicha} — Blíster {spd.NumRegistro}");
                col.Item().Text($"Validez: {spd.ValidezDesde:dd/MM/yyyy} — {spd.ValidezHasta:dd/MM/yyyy}");
            });
        }));

        return GuardarDocumento(documento, "Etiqueta anverso", "ETQ-A", paciente, spd.NumRegistro, farmacia, "SPD", spdId, usuarioQueEjecutaId);
    }

    public ResultadoGeneracionDocumento GenerarEtiquetaReverso(int spdId, int? usuarioQueEjecutaId)
    {
        var (spd, paciente, lineas) = ObtenerDatosSpd(spdId);
        var farmacia = ObtenerFarmacia();

        var documento = Document.Create(contenedor => contenedor.Page(pagina =>
        {
            ConfigurarPagina(pagina, new PageSize(400, 250));
            pagina.Content().Column(col =>
            {
                col.Item().Text($"Blíster {spd.NumRegistro} — {paciente.Nombre} {paciente.Apellidos}").FontSize(10).Bold();
                foreach (var linea in lineas.Where(l => l.EstadoLinea != EstadoLinea.Excluida))
                    foreach (var fila in repositorioLineaEnvases.ListarPorLinea(linea.Id))
                        col.Item().Text($"{linea.SnapNombre} — serie {fila.SnapSerie}, lote {fila.SnapLote}, cad. {fila.SnapCaducidad:MM/yyyy}").FontSize(8);
            });
        }));

        return GuardarDocumento(documento, "Etiqueta reverso", "ETQ-R", paciente, spd.NumRegistro, farmacia, "SPD", spdId, usuarioQueEjecutaId);
    }

    public ResultadoGeneracionDocumento GenerarInstrucciones(int spdId, int? usuarioQueEjecutaId)
    {
        var (spd, paciente, lineas) = ObtenerDatosSpd(spdId);
        var farmacia = ObtenerFarmacia();

        var documento = Document.Create(contenedor => contenedor.Page(pagina =>
        {
            ConfigurarPagina(pagina, PageSizes.A4);
            pagina.Header().Text($"Hoja de instrucciones — {paciente.Nombre} {paciente.Apellidos}").FontSize(14).Bold();
            pagina.Content().Column(col =>
            {
                col.Item().Text($"Validez: {spd.ValidezDesde:dd/MM/yyyy} — {spd.ValidezHasta:dd/MM/yyyy}");
                foreach (var linea in lineas.Where(l => l.EstadoLinea != EstadoLinea.Excluida))
                    col.Item().PaddingTop(6).Text(
                        $"{linea.SnapNombre}: {FraccionTexto(linea.SnapPautaD)} - {FraccionTexto(linea.SnapPautaA)} - " +
                        $"{FraccionTexto(linea.SnapPautaC)} - {FraccionTexto(linea.SnapPautaN)}" +
                        (string.IsNullOrWhiteSpace(linea.SnapMomento) ? "" : $" ({linea.SnapMomento})"));
            });
        }));

        return GuardarDocumento(documento, "Hoja de instrucciones", "INSTR", paciente, spd.NumRegistro, farmacia, "SPD", spdId, usuarioQueEjecutaId);
    }

    public ResultadoGeneracionDocumento GenerarFichaPaciente(int pacienteId, int? usuarioQueEjecutaId)
    {
        var paciente = repositorioPacientes.ObtenerPorId(pacienteId)
            ?? throw new ErrorValidacionException($"No existe el paciente {pacienteId}.");
        var farmacia = ObtenerFarmacia();

        var documento = Document.Create(contenedor => contenedor.Page(pagina =>
        {
            ConfigurarPagina(pagina, PageSizes.A4);
            pagina.Header().Column(col =>
            {
                col.Item().Text(farmacia.Nombre).FontSize(14).Bold();
                col.Item().Text("Ficha del paciente").FontSize(12);
            });
            pagina.Content().Column(col =>
            {
                col.Item().PaddingTop(10).Text($"Ficha: {paciente.NumFicha}");
                col.Item().Text($"Nombre: {paciente.Nombre} {paciente.Apellidos}");
                if (!string.IsNullOrWhiteSpace(paciente.Dni)) col.Item().Text($"DNI: {paciente.Dni}");
                if (!string.IsNullOrWhiteSpace(paciente.Cip)) col.Item().Text($"CIP: {paciente.Cip}");
                if (paciente.FechaNacimiento is not null) col.Item().Text($"Fecha de nacimiento: {paciente.FechaNacimiento:dd/MM/yyyy}");
                if (!string.IsNullOrWhiteSpace(paciente.Direccion)) col.Item().Text($"Dirección: {paciente.Direccion}, {paciente.Cp} {paciente.Poblacion}");
                if (!string.IsNullOrWhiteSpace(paciente.Telefono1)) col.Item().Text($"Teléfono: {paciente.Telefono1}");
                if (!string.IsNullOrWhiteSpace(paciente.EnfermedadesCronicas)) col.Item().Text($"Enfermedades crónicas: {paciente.EnfermedadesCronicas}");
                if (!string.IsNullOrWhiteSpace(paciente.Alergias)) col.Item().Text($"Alergias: {paciente.Alergias}").FontColor(QuestPDF.Helpers.Colors.Red.Medium);
                col.Item().Text($"Día de retirada: {paciente.DiaRetirada} — Nº blísteres: {paciente.NBlisteres}");
            });
        }));

        return GuardarDocumento(documento, "Ficha del paciente", "FICHA-PAC", paciente, paciente.NumFicha, farmacia, "Paciente", pacienteId, usuarioQueEjecutaId);
    }

    private (SPD Spd, Paciente Paciente, IReadOnlyList<SpdLinea> Lineas) ObtenerDatosSpd(int spdId)
    {
        var spd = repositorioSpd.ObtenerPorId(spdId) ?? throw new ErrorValidacionException($"No existe el SPD {spdId}.");
        var paciente = repositorioPacientes.ObtenerPorId(spd.PacienteId)
            ?? throw new ErrorValidacionException($"No existe el paciente {spd.PacienteId}.");
        return (spd, paciente, repositorioLineas.ListarPorSpd(spdId));
    }

    private Farmacia ObtenerFarmacia() => repositorioFarmacia.Obtener() ?? throw new ErrorValidacionException("No hay configuración de farmacia.");

    private IEnumerable<string> ListarEnvasesDeLinea(int spdLineaId)
        => repositorioLineaEnvases.ListarPorLinea(spdLineaId).Select(f => $"S:{f.SnapSerie}");

    // FR-740/743: lo impreso siempre en fracción, nunca en decimal.
    private static string FraccionTexto(FraccionDosis? dosis) => dosis?.Texto() ?? "0";

    private static void ConfigurarPagina(PageDescriptor pagina, PageSize tamano)
    {
        pagina.Size(tamano);
        pagina.Margin(1, Unit.Centimetre);
        pagina.DefaultTextStyle(estilo => estilo.FontSize(10));
    }

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
