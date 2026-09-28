using Microsoft.Data.Sqlite;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Xunit;

namespace Spd.Aplicacion.Tests;

/// <summary>Spec 000 FR-051 (0.2.0-beta): la URL del nomenclátor viene preinformada. Una farmacia creada
/// con la 0.1.0-beta la tiene vacía en la base de datos; al actualizar debe ver también la URL por
/// defecto, sin migración (Dapper no aplica los NULL sobre los valores iniciales de la entidad).</summary>
public sealed class UrlNomenclatorPorDefectoTests
{
    [Fact]
    public void Una_farmacia_guardada_sin_url_la_lee_con_la_url_por_defecto()
    {
        using var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();
        var repositorio = new RepositorioFarmacia(conexion);
        repositorio.Crear(new Farmacia
        {
            CodigoSanitario = "PO-001", Nombre = "F", TitularOComunidadBienes = "T", Cif = "B0",
            Direccion = "D", Cp = "36000", Poblacion = "P", Telefono = "9", UrlNomenclator = null
        });

        var leida = repositorio.Obtener()!;

        Assert.StartsWith("https://www.sanidad.gob.es/profesionales/nomenclator.do", leida.UrlNomenclator);
    }
}
