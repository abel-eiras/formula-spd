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
    IServicioPurga Purga);
