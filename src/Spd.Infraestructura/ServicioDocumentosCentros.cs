using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Spd.Aplicacion;
using Spd.Dominio;

namespace Spd.Infraestructura;

/// <summary>Albarán de entrega a un centro y lista de reposición (Spec 019). Misma cabecera
/// institucional que el resto de documentos (FR-730); cada impresión queda en auditoría (Art. II.2).</summary>
public sealed class ServicioDocumentosCentros(
    IRepositorioCentros repositorioCentros, IServicioCentros servicioCentros, IRepositorioFarmacia repositorioFarmacia,
    IRegistradorAuditoria auditoria) : IServicioDocumentosCentros
{
    public ResultadoGeneracionDocumento GenerarAlbaran(int centroId, IReadOnlyList<int> spdIds, int? usuarioId)
    {
        var centro = repositorioCentros.ObtenerPorId(centroId) ?? throw new ErrorValidacionException("El centro no existe.");
        var blisteres = servicioCentros.BlisteresParaAlbaran(centroId).Where(b => spdIds.Contains(b.SpdId)).ToList();
        if (blisteres.Count == 0) throw new ErrorValidacionException("Elige al menos un blíster verificado de este centro.");
        var farmacia = ObtenerFarmacia();
        var hoy = DateOnly.FromDateTime(DateTime.Today);

        var documento = Document.Create(c => c.Page(pagina =>
        {
            ServicioGeneracionDocumentos.ConfigurarPagina(pagina, PageSizes.A4, 9);
            pagina.Header().Element(e => ServicioGeneracionDocumentos.Cabecera(e, farmacia, "ALBARÁN DE ENTREGA DE SPD A CENTRO"));
            pagina.Content().PaddingTop(8).Column(col =>
            {
                col.Spacing(6);
                col.Item().Text(t => { t.Span("Centro: ").Bold(); t.Span(centro.Nombre); });
                if (centro.Direccion is { } direccion) col.Item().Text(t => { t.Span("Dirección: ").Bold(); t.Span(direccion); });
                if (centro.PersonaContacto is { } contacto) col.Item().Text(t => { t.Span("Persona de contacto: ").Bold(); t.Span(contacto); });
                col.Item().Text(t => { t.Span("Fecha: ").Bold(); t.Span(hoy.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)); });
                col.Item().Table(tabla =>
                {
                    tabla.ColumnsDefinition(cd => { cd.ConstantColumn(24); cd.RelativeColumn(1.2f); cd.RelativeColumn(3); cd.RelativeColumn(1); cd.RelativeColumn(2); });
                    foreach (var cab in new[] { "#", "Nº registro", "Paciente", "Ficha", "Periodo de validez" })
                        ServicioGeneracionDocumentos.Celda(tabla, cab, negrita: true);
                    var n = 1;
                    foreach (var b in blisteres)
                    {
                        ServicioGeneracionDocumentos.Celda(tabla, (n++).ToString(CultureInfo.InvariantCulture));
                        ServicioGeneracionDocumentos.Celda(tabla, b.NumRegistro);
                        ServicioGeneracionDocumentos.Celda(tabla, b.Paciente);
                        ServicioGeneracionDocumentos.Celda(tabla, b.NumFicha);
                        ServicioGeneracionDocumentos.Celda(tabla, $"{b.ValidezDesde:dd/MM/yyyy} — {b.ValidezHasta:dd/MM/yyyy}");
                    }
                });
                col.Item().Text($"Total: {blisteres.Count} blíster(es).").Bold();
                col.Item().PaddingTop(4).Text(
                    "Cada blíster va con su etiqueta y su hoja de instrucciones. La entrega de cada blíster se registra en la aplicación " +
                    "con sus datos (Anexo I.G del PNT); este albarán acredita qué blísteres recibió el centro.").Italic();
                col.Item().PaddingTop(24).Row(fila =>
                {
                    fila.RelativeItem().Column(cc =>
                    {
                        cc.Item().Text("Entregado por (farmacia)").Bold();
                        cc.Item().PaddingTop(30).Text("Nombre y firma: ______________________________");
                    });
                    fila.RelativeItem().Column(cc =>
                    {
                        cc.Item().Text("Recibido por (centro)").Bold();
                        cc.Item().PaddingTop(12).Text("Nombre: ______________________________");
                        cc.Item().PaddingTop(6).Text("DNI: _________________");
                        cc.Item().PaddingTop(6).Text("Firma: ______________________________");
                    });
                });
            });
        }));

        return Guardar(documento, "ALBARAN", $"C{centroId}-{DateTime.Now:HHmmss}", farmacia, "Centro", centroId,
            $"blisteres={string.Join(',', blisteres.Select(b => b.NumRegistro))}", usuarioId);
    }

    public ResultadoGeneracionDocumento GenerarListaReposicion(IReadOnlyList<FilaReposicion> filas, int horizonteDias, DateOnly hoy, int? usuarioId)
    {
        if (filas.Count == 0) throw new ErrorValidacionException("No hay nada que reponer en ese plazo.");
        var farmacia = ObtenerFarmacia();
        var documento = Document.Create(c => c.Page(pagina =>
        {
            ServicioGeneracionDocumentos.ConfigurarPagina(pagina, PageSizes.A4.Landscape(), 8);
            pagina.Header().Element(e => ServicioGeneracionDocumentos.Cabecera(e, farmacia,
                $"LISTA DE REPOSICIÓN — medicamentos que se agotan en los próximos {horizonteDias} días ({hoy:dd/MM/yyyy})"));
            pagina.Content().PaddingTop(8).Table(tabla =>
            {
                tabla.ColumnsDefinition(cd =>
                {
                    cd.RelativeColumn(2.2f); cd.RelativeColumn(1.4f); cd.RelativeColumn(2); cd.RelativeColumn(2.6f);
                    cd.ConstantColumn(48); cd.ConstantColumn(44); cd.ConstantColumn(40); cd.ConstantColumn(56);
                });
                foreach (var cab in new[] { "Paciente", "Centro", "Médico", "Medicamento", "CN", "Quedan", "Días", "Se agota" })
                    ServicioGeneracionDocumentos.Celda(tabla, cab, negrita: true);
                foreach (var f in filas)
                {
                    ServicioGeneracionDocumentos.Celda(tabla, f.Paciente);
                    ServicioGeneracionDocumentos.Celda(tabla, f.Centro ?? "");
                    ServicioGeneracionDocumentos.Celda(tabla, f.Medico ?? "");
                    ServicioGeneracionDocumentos.Celda(tabla, f.Medicamento);
                    ServicioGeneracionDocumentos.Celda(tabla, f.Cn);
                    ServicioGeneracionDocumentos.Celda(tabla, f.UnidadesDisponibles.ToString(CultureInfo.InvariantCulture));
                    ServicioGeneracionDocumentos.Celda(tabla, f.DiasCobertura.ToString(CultureInfo.InvariantCulture));
                    ServicioGeneracionDocumentos.Celda(tabla, f.FechaAgotamiento.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture));
                }
            });
        }));
        return Guardar(documento, "REPOSICION", $"{horizonteDias}d-{DateTime.Now:HHmmss}", farmacia, "Reposicion", null,
            $"filas={filas.Count}", usuarioId);
    }

    private Farmacia ObtenerFarmacia() => repositorioFarmacia.Obtener() ?? throw new ErrorValidacionException("No hay configuración de farmacia.");

    private ResultadoGeneracionDocumento Guardar(IDocument documento, string codigo, string identificador, Farmacia farmacia,
        string entidad, int? entidadId, string detalle, int? usuarioId)
    {
        var fecha = DateOnly.FromDateTime(DateTime.Today);
        var nombreFichero = GeneradorNombreFichero.NombreCorto(codigo, identificador, fecha);
        var rutaBase = string.IsNullOrWhiteSpace(farmacia.RutaDocumentosGenerados)
            ? Path.Combine(AppContext.BaseDirectory, "documentos-generados")
            : farmacia.RutaDocumentosGenerados;
        var carpeta = Path.Combine(rutaBase, fecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        Directory.CreateDirectory(carpeta);
        var rutaCompleta = Path.Combine(carpeta, nombreFichero);
        documento.GeneratePdf(rutaCompleta);
        auditoria.Registrar(usuarioId, "GENERAR_DOCUMENTO", entidad, entidadId, $"tipo={codigo};ruta={rutaCompleta};{detalle}");
        return new ResultadoGeneracionDocumento(rutaCompleta, nombreFichero);
    }
}
