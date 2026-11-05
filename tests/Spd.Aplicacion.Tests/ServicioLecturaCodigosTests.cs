using Microsoft.Data.Sqlite;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Xunit;

namespace Spd.Aplicacion.Tests;

/// <summary>spec-012 v0.5: identificación del medicamento por el CN deducido del GTIN o por el AI 712
/// sin su dígito de control (FR-1209) y separador sustituto configurado en Farmacia (FR-1211).</summary>
public sealed class ServicioLecturaCodigosTests
{
    private static (ServicioLecturaCodigos Lectura, ServicioConfiguracionFarmacia Farmacia, RepositorioMedicamentos Medicamentos) Montar(SqliteConnection c)
    {
        c.Open();
        new AplicadorMigraciones(c).Aplicar();
        var auditoria = new RegistradorAuditoria(c);
        var repositorioFarmacia = new RepositorioFarmacia(c);
        repositorioFarmacia.Crear(new Farmacia
        {
            CodigoSanitario = "PO-001", Nombre = "F", TitularOComunidadBienes = "T", Cif = "B0",
            Direccion = "D", Cp = "36000", Poblacion = "P", Telefono = "9"
        });
        var repositorioMedicamentos = new RepositorioMedicamentos(c);
        var servicioMedicamentos = new ServicioMedicamentos(repositorioMedicamentos, auditoria);
        return (new ServicioLecturaCodigos(servicioMedicamentos, repositorioFarmacia),
                new ServicioConfiguracionFarmacia(repositorioFarmacia, auditoria), repositorioMedicamentos);
    }

    [Fact]
    public void Caja_sin_AI712_se_identifica_por_el_CN_del_GTIN_aunque_el_catalogo_no_tenga_GTIN()
    {
        using var c = new SqliteConnection("Data Source=:memory:");
        var (lectura, _, medicamentos) = Montar(c);
        var id = medicamentos.Crear(new Medicamento { Cn = "695064", Nombre = "Enalapril 20 mg" });

        var datos = lectura.Interpretar("010847000695064721172342612146291728073110RA0462").Datos!;

        Assert.Equal(id, lectura.IdentificarMedicamento(datos)!.Id);
    }

    [Fact]
    public void AI712_con_digito_de_control_encuentra_el_CN_de_seis_digitos()
    {
        using var c = new SqliteConnection("Data Source=:memory:");
        var (lectura, _, medicamentos) = Montar(c);
        var id = medicamentos.Crear(new Medicamento { Cn = "870261", Nombre = "Importado" });

        var datos = lectura.Interpretar("010541378799931617301031103012801214000LQG554DTM17128702619").Datos!;

        Assert.Equal(id, lectura.IdentificarMedicamento(datos)!.Id);
    }

    [Fact]
    public void El_separador_guardado_en_Farmacia_se_aplica_al_siguiente_escaneo()
    {
        using var c = new SqliteConnection("Data Source=:memory:");
        var (lectura, farmacia, _) = Montar(c);
        const string codigo = "01084700069506471728073110AB21CD~21EF";
        Assert.Equal(EstadoLecturaGs1.Ambiguo, lectura.Interpretar(codigo.Replace("~", "")).Estado);

        farmacia.ActualizarSeparadorLector("~", null);

        var leida = lectura.Interpretar(codigo);
        Assert.Equal(EstadoLecturaGs1.Reconocido, leida.Estado);
        Assert.Equal("AB21CD", leida.Datos!.Lote);
    }

    [Theory]
    [InlineData("A")]
    [InlineData("1")]
    [InlineData(" ")]
    [InlineData("~~~~")]
    public void Separador_con_letras_digitos_o_espacios_se_rechaza(string separador)
    {
        using var c = new SqliteConnection("Data Source=:memory:");
        var (_, farmacia, _) = Montar(c);
        Assert.Throws<ErrorValidacionException>(() => farmacia.ActualizarSeparadorLector(separador, null));
    }

    [Fact]
    public void Disposicion_del_blister_se_guarda_y_se_lee()
    {
        using var c = new SqliteConnection("Data Source=:memory:");
        var (_, farmacia, _) = Montar(c);
        Assert.Equal(DisposicionBlister.PorDefecto, farmacia.ObtenerConfiguracion().Disposicion());

        var nueva = new DisposicionBlister(InicioBlister.Convencional, true, true);
        farmacia.ActualizarDisposicionBlister(nueva, null);

        Assert.Equal(nueva, farmacia.ObtenerConfiguracion().Disposicion());
    }
}
