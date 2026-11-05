using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Spd.Dominio;

/// <summary>Una entrada de auditoría tal como se guarda, con su sello (Spec 017).</summary>
public sealed record EntradaAuditoria(
    long Id, string FechaHora, int? UsuarioId, string Accion, string Entidad, int? EntidadId, string? Detalle,
    string? HashPrevio, string? Hash);

/// <summary>Resultado de comprobar la cadena. <see cref="Integra"/> es cierto solo si no hay ningún
/// problema; <see cref="SinSellar"/> cuenta las entradas anteriores a que existiera el sellado, que no
/// son un problema.</summary>
public sealed record ResultadoCadenaAuditoria(
    int Total, int Selladas, int SinSellar, long? PrimeraEntradaConProblema, string? Problema)
{
    public bool Integra => Problema is null;
}

/// <summary>Sellado de la auditoría con una cadena de hash SHA-256 (Spec 017). El Art. III.3 dice que
/// la auditoría es de solo inserción para la aplicación; la cadena permite además **demostrarlo** ante
/// una inspección: cada entrada incluye el hash de la anterior, así que modificar, borrar, reordenar o
/// intercalar entradas directamente en el fichero de la base de datos rompe la cadena.
///
/// Límite conocido: borrar las últimas entradas no deja rastro dentro de la propia tabla; lo cubren las
/// copias de seguridad (Art. VI.6), que conservan la cadena de cada momento.</summary>
public static class CadenaAuditoria
{
    /// <summary>Hash previo de la primera entrada sellada.</summary>
    public static readonly string Genesis = new('0', 64);

    /// <summary>Hash de una entrada. Cada campo va precedido de su longitud para que dos entradas
    /// distintas nunca den el mismo texto (p. ej. «ab|c» frente a «a|bc»); un nulo se distingue de un
    /// texto vacío.</summary>
    public static string CalcularHash(
        string hashPrevio, string fechaHora, int? usuarioId, string accion, string entidad, int? entidadId, string? detalle)
    {
        var texto = new StringBuilder();
        foreach (var campo in new[]
                 {
                     hashPrevio, fechaHora, usuarioId?.ToString(CultureInfo.InvariantCulture), accion, entidad,
                     entidadId?.ToString(CultureInfo.InvariantCulture), detalle
                 })
        {
            texto.Append(campo is null ? "-1:" : $"{campo.Length}:{campo}").Append('\n');
        }
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(texto.ToString()))).ToLowerInvariant();
    }

    /// <summary>Hash que debe llevar una entrada nueva como previo: el de la última sellada, o el
    /// génesis si todavía no hay ninguna.</summary>
    public static string HashPrevioPara(string? hashUltimaSellada) => hashUltimaSellada ?? Genesis;

    /// <summary>Comprueba las entradas en orden de inserción (id ascendente). Las entradas sin sellar
    /// solo son legítimas antes de la primera sellada; una sin sellar después es una inserción hecha
    /// por fuera de la aplicación.</summary>
    public static ResultadoCadenaAuditoria Verificar(IReadOnlyList<EntradaAuditoria> entradas)
    {
        var sinSellar = 0;
        var selladas = 0;
        string? previoEsperado = null;
        foreach (var e in entradas)
        {
            if (e.Hash is null)
            {
                if (previoEsperado is not null)
                    return Fallo(entradas.Count, selladas, sinSellar, e.Id, "entrada sin sellar después de empezar el sellado: se insertó por fuera de la aplicación");
                sinSellar++;
                continue;
            }

            var previo = previoEsperado ?? Genesis;
            if (e.HashPrevio != previo)
                return Fallo(entradas.Count, selladas, sinSellar, e.Id, "no enlaza con la entrada anterior: falta, sobra o se ha reordenado alguna entrada");
            var calculado = CalcularHash(previo, e.FechaHora, e.UsuarioId, e.Accion, e.Entidad, e.EntidadId, e.Detalle);
            if (e.Hash != calculado)
                return Fallo(entradas.Count, selladas, sinSellar, e.Id, "su contenido no coincide con su sello: se ha modificado");
            previoEsperado = e.Hash;
            selladas++;
        }
        return new ResultadoCadenaAuditoria(entradas.Count, selladas, sinSellar, null, null);
    }

    private static ResultadoCadenaAuditoria Fallo(int total, int selladas, int sinSellar, long id, string problema)
        => new(total, selladas, sinSellar, id, problema);
}
