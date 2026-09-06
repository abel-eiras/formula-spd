namespace Spd.Dominio;

/// <summary>Configuración de la farmacia: fila única de toda la instalación (Art. IV.1).</summary>
public sealed class Farmacia
{
    public int Id { get; set; } = 1;
    public required string CodigoSanitario { get; set; }
    public string? Logo { get; set; }
    public required string Nombre { get; set; }
    public required string TitularOComunidadBienes { get; set; }
    public required string Cif { get; set; }
    public string? TitularColegiado { get; set; }
    public required string Direccion { get; set; }
    public required string Cp { get; set; }
    public required string Poblacion { get; set; }
    public string? Provincia { get; set; }
    public required string Telefono { get; set; }
    public string? Fax { get; set; }
    public string? Email { get; set; }
    public string? Whatsapp { get; set; }
    public string? ResponsableDatos { get; set; }
    public string? DireccionDerechos { get; set; }
    public string? EmailDerechos { get; set; }
    /// <summary>Delegado de Protección de Datos (nombre) y su contacto — Anexo I.D del PNT I.
    /// Habitualmente el DPO del Colegio provincial; opcional, el documento lo omite si está vacío.</summary>
    public string? DpoNombre { get; set; }
    public string? DpoContacto { get; set; }
    public string PrefijoNumFicha { get; set; } = string.Empty;
    public string PrefijoNumSpd { get; set; } = string.Empty;
    public string? RutaBackup { get; set; }
    public string? RutaDocumentosGenerados { get; set; }
    public string? UrlNomenclator { get; set; }
    public int UmbralReutilizacionLecturaAmbientalHoras { get; set; } = 2;
    public double TempMin { get; set; } = 15;
    public double TempMax { get; set; } = 25;
    public double HrMin { get; set; } = 40;
    public double HrMax { get; set; } = 60;
    public string DiaRetiradaDefecto { get; set; } = "LU";
    public int NBlisteresDefecto { get; set; } = 1;
    public int DiasAntelacionListado { get; set; } = 2;

    /// <summary>Valor efectivo de responsable de datos para el consentimiento (Spec 002, FR-012): el titular si no se ha rellenado aparte.</summary>
    public string ResponsableDatosEfectivo() => ResponsableDatos ?? TitularOComunidadBienes;

    /// <summary>Valor efectivo de dirección de derechos ARCO (FR-012): la dirección de la farmacia si no se ha rellenado aparte.</summary>
    public string DireccionDerechosEfectiva() => DireccionDerechos ?? Direccion;
}
