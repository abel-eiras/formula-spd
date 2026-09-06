using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace Spd.Presentacion.Tests;

/// <summary>Spec 015 CA-1532: ninguna pantalla pide teclear el identificador numérico de una
/// persona.
///
/// Nadie sabe de memoria que la Dra. Vidal es el 7. Equivocarse de dígito atribuye una comunicación
/// —o la firma de una verificación, que es dato legal (Art. II)— a quien no corresponde, y el error
/// no salta por ningún lado porque el 6 también existe. Este test recorre los `.axaml` de verdad, así
/// que impide que vuelva a colarse un campo así en una pantalla nueva.</summary>
public sealed class SinIdentificadoresEnLaInterfazTests
{
    /// <summary>`*Id` de **personas**. Los identificadores de cosas que el usuario sí conoce por su
    /// número (el CN de un medicamento, por ejemplo) no entran aquí.</summary>
    private static readonly string[] IdentificadoresDePersona =
        ["MedicoId", "VerificadorId", "ElaboradorId", "FarmaceuticoId", "UsuarioId", "PacienteId", "ContactoId"];

    private static IEnumerable<string> Vistas()
    {
        var raiz = LocalizarProyectoDePresentacion();
        return Directory.EnumerateFiles(raiz, "*.axaml", SearchOption.AllDirectories);
    }

    [Fact]
    public void Ninguna_vista_pide_teclear_el_id_de_una_persona_CA_1532()
    {
        var infracciones = new List<string>();

        foreach (var ruta in Vistas())
        {
            var contenido = File.ReadAllText(ruta);

            // Un control de entrada numérica o de texto libre enlazado a un *Id de persona.
            foreach (Match control in Regex.Matches(contenido, @"<(NumericUpDown|TextBox)\b[^>]*>", RegexOptions.Singleline))
            {
                foreach (var identificador in IdentificadoresDePersona)
                {
                    if (!control.Value.Contains($"Binding {identificador}", StringComparison.Ordinal)) continue;
                    infracciones.Add($"{Path.GetFileName(ruta)}: {identificador} se teclea a mano.");
                }
            }
        }

        Assert.True(infracciones.Count == 0,
            "Hay pantallas que piden teclear el identificador de una persona:\n" + string.Join("\n", infracciones));
    }

    /// <summary>Sube desde el directorio de ejecución hasta encontrar el proyecto de presentación:
    /// los `.axaml` no se copian a la salida, así que hay que leerlos del repositorio.</summary>
    internal static string RaizDePresentacion() => LocalizarProyectoDePresentacion();

    private static string LocalizarProyectoDePresentacion()
    {
        var directorio = new DirectoryInfo(AppContext.BaseDirectory);
        while (directorio is not null)
        {
            var candidato = Path.Combine(directorio.FullName, "src", "Spd.Presentacion");
            if (Directory.Exists(candidato)) return candidato;
            directorio = directorio.Parent;
        }

        throw new DirectoryNotFoundException(
            "No se encontró src/Spd.Presentacion subiendo desde " + AppContext.BaseDirectory);
    }
}
