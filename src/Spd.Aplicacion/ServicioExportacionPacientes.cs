using System.Globalization;
using System.Reflection;
using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Exportación de pacientes a CSV (FR-1130). Solo incluye los campos que el perfil mapea
/// explícitamente — nunca "todo el registro" (FR-1131, CA-1103).</summary>
public sealed class ServicioExportacionPacientes : IServicioExportacionPacientes
{
    public string ExportarACsv(PerfilImportacion perfil, IReadOnlyList<Paciente> pacientes)
    {
        var separador = perfil.Separador;
        var lineas = new List<string>();

        if (perfil.TieneCabecera)
            lineas.Add(string.Join(separador, perfil.Mapeo.Select(par => EscaparCsv(par.Columna, separador))));

        foreach (var paciente in pacientes)
        {
            var valores = perfil.Mapeo.Select(par => EscaparCsv(ObtenerValorDeCampo(paciente, par.Campo), separador));
            lineas.Add(string.Join(separador, valores));
        }

        return string.Join("\n", lineas);
    }

    // FR-1131: solo se lee el campo indicado en el mapeo, por reflexión sobre el propio Paciente —
    // nunca se serializa el objeto completo.
    private static string ObtenerValorDeCampo(Paciente paciente, string nombreCampo)
    {
        var propiedad = typeof(Paciente).GetProperty(nombreCampo, BindingFlags.Public | BindingFlags.Instance)
            ?? throw new ErrorValidacionException($"El campo '{nombreCampo}' no existe en Paciente.");
        var valor = propiedad.GetValue(paciente);
        return valor switch
        {
            null => string.Empty,
            DateOnly fecha => fecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            DateTime fecha => fecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            bool booleano => booleano ? "1" : "0",
            _ => valor.ToString() ?? string.Empty
        };
    }

    private static string EscaparCsv(string valor, string separador)
        => valor.Contains(separador) || valor.Contains('"') || valor.Contains('\n')
            ? $"\"{valor.Replace("\"", "\"\"")}\""
            : valor;
}
