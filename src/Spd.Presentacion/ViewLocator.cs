using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Spd.Presentacion.ViewModels;

namespace Spd.Presentacion;

/// <summary>Dado un ViewModel, construye su vista por convención de nombre (`XViewModel` → `XView`).
/// Es lo que permite que la región de contenido del marco único (Spec 015) reciba un ViewModel y
/// pinte la pantalla correspondiente.
///
/// La implementación de la plantilla de Avalonia resolvía el nombre completo con un `Replace`, lo que
/// falla en cuanto la vista vive en un subespacio de nombres (`Views.Pacientes.FichaPacienteView`
/// frente a `ViewModels.FichaPacienteViewModel`). Aquí se indexa una vez el ensamblado por nombre
/// simple, que es lo que la convención garantiza.</summary>
[RequiresUnreferencedCode("El localizador de vistas usa reflexión, que el recortado puede eliminar.")]
public class ViewLocator : IDataTemplate
{
    private static readonly Lazy<IReadOnlyDictionary<string, Type>> VistasPorNombre = new(() =>
        typeof(ViewLocator).Assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsClass: true }
                        && t.Name.EndsWith("View", StringComparison.Ordinal)
                        && typeof(Control).IsAssignableFrom(t)
                        && t.GetConstructor(Type.EmptyTypes) is not null)
            .GroupBy(t => t.Name)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal));

    public Control? Build(object? param)
    {
        if (param is null) return null;

        var nombreVista = param.GetType().Name.Replace("ViewModel", "View", StringComparison.Ordinal);
        return VistasPorNombre.Value.TryGetValue(nombreVista, out var tipo)
            ? (Control)Activator.CreateInstance(tipo)!
            : new TextBlock { Text = $"No se encontró la vista {nombreVista}" };
    }

    public bool Match(object? data) => data is ViewModelBase;
}
