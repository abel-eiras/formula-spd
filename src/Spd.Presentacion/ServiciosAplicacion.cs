using Spd.Aplicacion;
using Spd.Infraestructura;

namespace Spd.Presentacion;

/// <summary>Todos los servicios de aplicación, reunidos una sola vez en el arranque (Spec 015 H1.4).
///
/// Antes del marco único, cada servicio nuevo había que enhebrarlo por la cascada de constructores
/// de ventanas: `App` → `MainViewModel` → `BuscadorPacientesWindow` → `FichaPacienteWindow` → …
/// Seis ficheros por servicio. Con esto, `FabricaViewModels` construye cada pantalla a demanda y la
/// cascada desaparece (Art. XI: legibilidad).</summary>
public sealed record ServiciosAplicacion(
    IServicioUsuarios Usuarios,
    IServicioConfiguracionFarmacia Farmacia,
    GestorLogoFarmacia GestorLogo,
    IServicioActualizaciones Actualizaciones,
    IServicioInstalacionActualizaciones InstalacionActualizaciones,
    IServicioNomenclator Nomenclator,
    IServicioPacientes Pacientes,
    IServicioTratamientos Tratamientos,
    IServicioMedicamentos Medicamentos,
    IServicioImportacionNomenclator ImportacionNomenclator,
    IServicioConsultaCima ConsultaCima,
    IServicioRegistrosCalidad RegistrosCalidad,
    IServicioControlDocumental ControlDocumental,
    IServicioBackup Backup,
    IServicioCifrado Cifrado,
    IServicioEnvases Envases,
    IServicioListadoRetirada ListadoRetirada,
    IServicioImportacionTratamientoEnvase ImportacionTratamiento,
    IServicioComunicacionesMedico Comunicaciones,
    IServicioPerfilesImportacion PerfilesImportacion,
    IServicioExportacionPacientes ExportacionPacientes,
    IServicioPreparacion Preparacion,
    IServicioGeneracionDocumentos Documentos,
    IServicioIdoneidadConsentimiento Idoneidad,
    IServicioAvisosInicio Avisos,
    IServicioBusquedaGlobal BusquedaGlobal,
    IServicioMedicos Medicos,
    IServicioContactos Contactos,
    IServicioGeneracionLote Lote,
    IServicioPurga Purga,
    // Al final y con valor por defecto: las pruebas que montan los servicios a mano no necesitan
    // conocerlo, y las pantallas caen en una lectura sin separador sustituto (spec-012 FR-1211).
    IServicioLecturaCodigos? LecturaCodigos = null,
    IServicioIntegridadAuditoria? IntegridadAuditoria = null,
    IServicioAlertasCalidad? AlertasCalidad = null,
    IServicioCentros? Centros = null,
    IServicioReposicion? Reposicion = null,
    IServicioDocumentosCentros? DocumentosCentros = null);
