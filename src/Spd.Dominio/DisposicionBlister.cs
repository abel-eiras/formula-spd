namespace Spd.Dominio;

/// <summary>Qué día ocupa la primera columna del blíster (Spec 016 FR-1601).</summary>
public enum InicioBlister
{
    /// <summary>La primera columna es el primer día de validez y cada columna el día siguiente.</summary>
    Cronologico,
    /// <summary>Las columnas son los días de la semana fijos, de lunes a domingo, como vienen impresos
    /// en muchos blísteres. Un blíster que empieza en jueves llena las columnas de jueves a domingo con
    /// esa semana y las de lunes a miércoles con la semana siguiente.</summary>
    Convencional
}

/// <summary>Cómo se ve el blíster físico que usa esta farmacia (Spec 016). Los fabricantes difieren
/// en si imprimen los días fijos o por orden, en qué lado está el primer día y en qué toma queda
/// arriba; la rejilla de preparación debe enseñar el blíster tal como se tiene delante, porque la
/// traducción mental de una disposición a otra es justo donde se equivoca quien llena (PNT: el
/// llenado se comprueba alvéolo a alvéolo). Un blíster girado 180° es días de derecha a izquierda
/// y noche arriba a la vez.</summary>
public sealed record DisposicionBlister(InicioBlister Inicio, bool DiasDeDerechaAIzquierda, bool NocheArriba)
{
    /// <summary>La disposición que la aplicación usaba antes de poder configurarse.</summary>
    public static readonly DisposicionBlister PorDefecto = new(InicioBlister.Cronologico, false, false);

    public const int Dias = 7;
    public const int Tomas = 4;

    /// <summary>Fecha de cada columna física, de izquierda a derecha.</summary>
    public IReadOnlyList<DateOnly> FechasDeColumnas(DateOnly validezDesde)
    {
        var fechas = new DateOnly[Dias];
        for (var columna = 0; columna < Dias; columna++)
        {
            var orden = DiasDeDerechaAIzquierda ? Dias - 1 - columna : columna;
            fechas[columna] = Inicio == InicioBlister.Cronologico
                ? validezDesde.AddDays(orden)
                : ProximaFechaDelDia(validezDesde, (DayOfWeek)((orden + 1) % 7));
        }
        return fechas;
    }

    /// <summary>Toma (0 desayuno … 3 noche) de cada fila física, de arriba abajo.</summary>
    public int TomaDeFila(int fila) => NocheArriba ? Tomas - 1 - fila : fila;

    // En la disposición convencional cada columna es un día de la semana fijo: le corresponde la
    // primera fecha con ese día a partir del inicio de validez (nunca una anterior, que estaría fuera
    // del periodo de validez del SPD, Art. I.3).
    private static DateOnly ProximaFechaDelDia(DateOnly desde, DayOfWeek dia)
        => desde.AddDays(((int)dia - (int)desde.DayOfWeek + 7) % 7);
}
