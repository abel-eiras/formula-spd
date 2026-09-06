namespace Spd.Dominio;

/// <summary>Nombre de fichero de un documento generado (FR-710/711). Función pura: decide entre
/// el nombre largo legible y el corto por código, sin acceso a disco.</summary>
public static class GeneradorNombreFichero
{
    private const int LongitudMaxima = 120;

    public static string Generar(
        string nombreDocumento, string codigo, string? nombrePaciente, string? apellidosPaciente,
        string identificadorCorto, DateOnly fecha, IReadOnlySet<string>? nombresYaUsadosEnLote = null)
    {
        var tieneApellidos = !string.IsNullOrWhiteSpace(apellidosPaciente); // research.md Decisión 3
        if (!tieneApellidos || string.IsNullOrWhiteSpace(nombrePaciente))
            return NombreCorto(codigo, identificadorCorto, fecha);

        var largo = NombreLargo(nombreDocumento, nombrePaciente, apellidosPaciente!, fecha);
        var colisiona = nombresYaUsadosEnLote?.Contains(largo) ?? false;

        return largo.Length <= LongitudMaxima && !colisiona
            ? largo
            : NombreCorto(codigo, identificadorCorto, fecha);
    }

    public static string NombreLargo(string nombreDocumento, string nombre, string apellidos, DateOnly fecha)
        => $"{nombreDocumento}. {nombre} {apellidos}.{fecha:ddMMyyyy}.pdf";

    public static string NombreCorto(string codigo, string identificador, DateOnly fecha)
        => $"{codigo}_{identificador}_{fecha:ddMMyyyy}.pdf";
}
