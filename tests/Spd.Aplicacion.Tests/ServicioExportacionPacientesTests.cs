using Spd.Dominio;
using Xunit;

namespace Spd.Aplicacion.Tests;

public sealed class ServicioExportacionPacientesTests
{
    private static Paciente CrearPaciente(string nombre, string apellidos, string telefono) => new()
    {
        NumFicha = "F-1", Nombre = nombre, Apellidos = apellidos, Telefono1 = telefono, DiaRetirada = "LU",
        Dni = "12345678Z"
    };

    [Fact]
    public void ExportarACsv_incluye_solo_los_campos_mapeados_explicitamente_CA_1103()
    {
        var perfil = new PerfilImportacion
        {
            Nombre = "Solo contacto", Tipo = TipoPerfilImportacion.Pacientes,
            Mapeo = [new ParCampoColumna("Nombre", "Nombre"), new ParCampoColumna("Apellidos", "Apellidos"), new ParCampoColumna("Telefono1", "Teléfono")]
        };
        var servicio = new ServicioExportacionPacientes();

        var csv = servicio.ExportarACsv(perfil, [CrearPaciente("Ana", "Pérez", "986000000")]);
        var lineas = csv.Split('\n');

        Assert.Equal("Nombre,Apellidos,Teléfono", lineas[0]);
        Assert.Equal("Ana,Pérez,986000000", lineas[1]);
        Assert.DoesNotContain("12345678Z", csv);
        Assert.DoesNotContain("F-1", csv);
    }

    [Fact]
    public void ExportarACsv_genera_una_fila_por_paciente_en_el_mismo_orden()
    {
        var perfil = new PerfilImportacion
        {
            Nombre = "Nombres", Tipo = TipoPerfilImportacion.Pacientes,
            Mapeo = [new ParCampoColumna("Nombre", "Nombre")]
        };
        var servicio = new ServicioExportacionPacientes();

        var csv = servicio.ExportarACsv(perfil, [CrearPaciente("Ana", "Pérez", "1"), CrearPaciente("Luis", "Gómez", "2")]);
        var lineas = csv.Split('\n');

        Assert.Equal(3, lineas.Length);
        Assert.Equal("Ana", lineas[1]);
        Assert.Equal("Luis", lineas[2]);
    }

    [Fact]
    public void ExportarACsv_escapa_valores_con_el_separador_o_comillas()
    {
        var perfil = new PerfilImportacion
        {
            Nombre = "Con comas", Tipo = TipoPerfilImportacion.Pacientes,
            Mapeo = [new ParCampoColumna("Apellidos", "Apellidos")]
        };
        var servicio = new ServicioExportacionPacientes();

        var csv = servicio.ExportarACsv(perfil, [CrearPaciente("Ana", "Pérez, García", "1")]);

        Assert.Contains("\"Pérez, García\"", csv);
    }
}
