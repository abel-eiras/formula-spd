using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using Spd.Presentacion.Views;

namespace Spd.Presentacion;

/// <summary>F1 contextual (Spec 014 FR-1401, research.md Decisión 3): cada ventana se registra con
/// una línea en su constructor y F1 abre el apartado de Procedimiento que le corresponde. Las
/// ventanas sin entrada abren el índice.</summary>
public static class AyudaContextual
{
    /// <summary>Nombre del tipo de ventana → id de Procedimiento (contrato en specs/014/contracts).</summary>
    public static readonly IReadOnlyDictionary<string, string> SeccionPorVentana = new Dictionary<string, string>
    {
        ["MainWindow"] = "servicio-spd",
        ["BuscadorPacientesWindow"] = "ficha-y-tratamiento",
        ["FichaPacienteWindow"] = "ficha-y-tratamiento",
        ["IdoneidadConsentimientoWindow"] = "idoneidad",
        ["TratamientoWindow"] = "ficha-y-tratamiento",
        ["DepositoWindow"] = "deposito-y-retirada",
        ["ImportarTratamientoWindow"] = "deposito-y-retirada",
        ["RetiradaEnvasesWindow"] = "deposito-y-retirada",
        ["PreparacionWindow"] = "preparacion",
        ["PreparacionesWindow"] = "preparacion",
        ["ComunicacionesMedicoWindow"] = "comunicacion-medico",
        ["RegistrosCalidadWindow"] = "personal-higiene-limpieza",
        ["ControlDocumentalWindow"] = "documentacion-y-conservacion",
        ["FarmaciaWindow"] = "documentacion-y-conservacion",
        ["UsuariosWindow"] = "personal-higiene-limpieza",
        ["SeguridadWindow"] = "documentacion-y-conservacion",
        ["CatalogoMedicamentosWindow"] = "ficha-y-tratamiento",
        ["RevisionNomenclatorWindow"] = "ficha-y-tratamiento",
        ["NomenclatorWindow"] = "ficha-y-tratamiento",
        ["ActualizacionesWindow"] = "servicio-spd",
        ["PerfilesImportacionWindow"] = "deposito-y-retirada",
        ["ExportarPacientesWindow"] = "documentacion-y-conservacion",
    };

    /// <summary>Apartado de Uso que documenta cada ventana (FR-1412; lo comprueba un test).</summary>
    public static readonly IReadOnlyDictionary<string, string> UsoPorVentana = new Dictionary<string, string>
    {
        ["MainWindow"] = "inicio",
        ["BuscadorPacientesWindow"] = "pacientes",
        ["FichaPacienteWindow"] = "ficha-paciente",
        ["IdoneidadConsentimientoWindow"] = "idoneidad-consentimiento",
        ["TratamientoWindow"] = "tratamientos",
        ["DepositoWindow"] = "deposito",
        ["ImportarTratamientoWindow"] = "deposito",
        ["RetiradaEnvasesWindow"] = "retirada-envases",
        ["PreparacionWindow"] = "preparacion",
        ["PreparacionesWindow"] = "preparacion",
        ["ComunicacionesMedicoWindow"] = "comunicaciones-medico",
        ["RegistrosCalidadWindow"] = "registros-calidad",
        ["ControlDocumentalWindow"] = "registros-calidad",
        ["FarmaciaWindow"] = "configuracion",
        ["UsuariosWindow"] = "configuracion",
        ["SeguridadWindow"] = "configuracion",
        ["CatalogoMedicamentosWindow"] = "catalogo-medicamentos",
        ["RevisionNomenclatorWindow"] = "catalogo-medicamentos",
        ["NomenclatorWindow"] = "configuracion",
        ["ActualizacionesWindow"] = "configuracion",
        ["PerfilesImportacionWindow"] = "configuracion",
        ["ExportarPacientesWindow"] = "configuracion",
    };

    public static void Registrar(Window ventana)
    {
        var nombre = ventana.GetType().Name;
        ventana.KeyDown += (_, e) =>
        {
            if (e.Key != Key.F1) return;
            e.Handled = true;
            AyudaWindow.Abrir("procedimiento", SeccionPorVentana.TryGetValue(nombre, out var id) ? id : null);
        };
    }
}
