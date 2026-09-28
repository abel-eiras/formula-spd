using LectorDataMatrix;

Console.WriteLine("Lector de DataMatrix — prototipo aislado (spec-012)");
Console.WriteLine("Escanea un envase (o pega la cadena) y pulsa Enter. Ctrl+D / Ctrl+Z para salir.");
Console.WriteLine();

string? linea;
while ((linea = Console.ReadLine()) is not null)
{
    if (linea.Length == 0) continue;

    var resultado = LectorGs1DataMatrix.Leer(linea);
    if (resultado is null)
    {
        Console.WriteLine("  -> NO RECONOCIDO (introducir a mano, FR-1204)");
    }
    else
    {
        Console.WriteLine($"  GTIN            : {resultado.Gtin}");
        Console.WriteLine($"  Lote            : {resultado.Lote}");
        Console.WriteLine($"  Numero de serie : {resultado.NumeroSerie}");
        var c = resultado.Caducidad;
        var dia = c.DiaSinEspecificar ? "sin especificar" : c.Dia.ToString("00");
        Console.WriteLine($"  Caducidad       : {c.Anio}-{c.Mes:00}-{dia}");
        Console.WriteLine($"  Codigo Nacional : {resultado.CodigoNacional ?? "(no incluido en el codigo)"}");
    }
    Console.WriteLine();
}
