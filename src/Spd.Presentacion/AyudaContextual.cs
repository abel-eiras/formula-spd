using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace Spd.Presentacion;

/// <summary>F1 contextual (Spec 014 FR-1401; Spec 015 FR-1505). Con el marco único ya no hay una
/// ventana por pantalla, así que la tabla se indexa por **vista**: es lo que sigue identificando a
/// una pantalla cuando además pasa a ser una pestaña (fase 3).
///
/// Quién abre la ayuda lo decide el marco: <see cref="Abridor"/> se configura al arrancar para que
/// navegue a la sección de ayuda, en vez de abrir una ventana.</summary>
public static class AyudaContextual
{
    /// <summary>Nombre de la vista → apartado de "Procedimiento del servicio SPD".</summary>
    public static readonly IReadOnlyDictionary<string, string> SeccionPorVista = new Dictionary<string, string>
    {
        ["InicioView"] = "servicio-spd",
        ["BuscadorPacientesView"] = "ficha-y-tratamiento",
        ["FichaPacienteView"] = "ficha-y-tratamiento",
        ["IdoneidadConsentimientoView"] = "idoneidad",
        ["TratamientoView"] = "ficha-y-tratamiento",
        ["DepositoView"] = "deposito-y-retirada",
        ["ImportarTratamientoView"] = "deposito-y-retirada",
        ["RetiradaEnvasesView"] = "deposito-y-retirada",
        ["PreparacionView"] = "preparacion",
        ["PreparacionesView"] = "preparacion",
        ["ComunicacionesMedicoView"] = "comunicacion-medico",
        ["RegistrosCalidadView"] = "personal-higiene-limpieza",
        ["ControlDocumentalView"] = "documentacion-y-conservacion",
        ["FarmaciaView"] = "documentacion-y-conservacion",
        ["UsuariosView"] = "personal-higiene-limpieza",
        ["SeguridadView"] = "documentacion-y-conservacion",
        ["CatalogoMedicamentosView"] = "ficha-y-tratamiento",
        ["RevisionNomenclatorView"] = "ficha-y-tratamiento",
        ["NomenclatorView"] = "ficha-y-tratamiento",
        ["ActualizacionesView"] = "servicio-spd",
        ["PerfilesImportacionView"] = "deposito-y-retirada",
        ["ExportarPacientesView"] = "documentacion-y-conservacion",
    };

    /// <summary>Apartado de "Uso de la aplicación" que documenta cada vista (Spec 014 FR-1412; un
    /// test cruza esta tabla con la anterior para que ninguna pantalla quede sin documentar).</summary>
    public static readonly IReadOnlyDictionary<string, string> UsoPorVista = new Dictionary<string, string>
    {
        ["InicioView"] = "inicio",
        ["BuscadorPacientesView"] = "pacientes",
        ["FichaPacienteView"] = "ficha-paciente",
        ["IdoneidadConsentimientoView"] = "idoneidad-consentimiento",
        ["TratamientoView"] = "tratamientos",
        ["DepositoView"] = "deposito",
        ["ImportarTratamientoView"] = "deposito",
        ["RetiradaEnvasesView"] = "retirada-envases",
        ["PreparacionView"] = "preparacion",
        ["PreparacionesView"] = "preparacion",
        ["ComunicacionesMedicoView"] = "comunicaciones-medico",
        ["RegistrosCalidadView"] = "registros-calidad",
        ["ControlDocumentalView"] = "registros-calidad",
        ["FarmaciaView"] = "configuracion",
        ["UsuariosView"] = "configuracion",
        ["SeguridadView"] = "configuracion",
        ["CatalogoMedicamentosView"] = "catalogo-medicamentos",
        ["RevisionNomenclatorView"] = "catalogo-medicamentos",
        ["NomenclatorView"] = "configuracion",
        ["ActualizacionesView"] = "configuracion",
        ["PerfilesImportacionView"] = "configuracion",
        ["ExportarPacientesView"] = "configuracion",
    };

    /// <summary>Lo configura el marco al arrancar: (sección, apartado) → navegar a la ayuda.</summary>
    public static Action<string?, string?>? Abridor { get; set; }

    public static void Abrir(string? seccion, string? apartado) => Abridor?.Invoke(seccion, apartado);

    /// <summary>Abre el apartado de procedimiento de una vista; si no está en la tabla, el índice.</summary>
    public static void AbrirParaVista(Control? vista)
    {
        var nombre = vista?.GetType().Name;
        Abrir("procedimiento", nombre is not null && SeccionPorVista.TryGetValue(nombre, out var id) ? id : null);
    }

    /// <summary>F1 en una ventana: resuelve la vista que contiene y abre su apartado. Lo usan tanto
    /// el marco como las ventanas que todavía no se han absorbido (fase 3).</summary>
    public static void Registrar(Window ventana)
        => ventana.KeyDown += (_, e) =>
        {
            if (e.Key != Key.F1) return;
            e.Handled = true;
            AbrirParaVista(VistaDe(ventana));
        };

    /// <summary>Primera vista de la jerarquía visual que esté en la tabla: en una pantalla con
    /// vistas anidadas (registros de calidad, y las pestañas de la fase 3) la de fuera es la que
    /// identifica la pantalla, salvo que no esté documentada.</summary>
    public static Control? VistaDe(Visual? raiz)
        => raiz?.GetVisualDescendants()
            .OfType<UserControl>()
            .FirstOrDefault(v => SeccionPorVista.ContainsKey(v.GetType().Name));
}
