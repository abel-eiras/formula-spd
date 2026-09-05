using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Dominio;

namespace Spd.Infraestructura;

/// <summary>Acceso a la fila única de Farmacia vía Dapper/SQLite (Art. VIII.4: SQL explícito).</summary>
public sealed class RepositorioFarmacia(SqliteConnection conexion) : IRepositorioFarmacia
{
    public Farmacia? Obtener()
        => conexion.QuerySingleOrDefault<Farmacia>("SELECT * FROM Farmacia WHERE id = 1");

    public void Crear(Farmacia farmacia)
        => conexion.Execute(
            """
            INSERT INTO Farmacia (
                id, codigo_sanitario, logo, nombre, titular_o_comunidad_bienes, cif,
                titular_colegiado, direccion, cp, poblacion, provincia, telefono, fax, email,
                whatsapp, responsable_datos, direccion_derechos, email_derechos,
                prefijo_num_ficha, prefijo_num_spd, ruta_backup, ruta_documentos_generados,
                url_nomenclator, umbral_reutilizacion_lectura_ambiental_horas,
                temp_min, temp_max, hr_min, hr_max, dia_retirada_defecto, n_blisteres_defecto,
                dias_antelacion_listado
            ) VALUES (
                1, @CodigoSanitario, @Logo, @Nombre, @TitularOComunidadBienes, @Cif,
                @TitularColegiado, @Direccion, @Cp, @Poblacion, @Provincia, @Telefono, @Fax, @Email,
                @Whatsapp, @ResponsableDatos, @DireccionDerechos, @EmailDerechos,
                @PrefijoNumFicha, @PrefijoNumSpd, @RutaBackup, @RutaDocumentosGenerados,
                @UrlNomenclator, @UmbralReutilizacionLecturaAmbientalHoras,
                @TempMin, @TempMax, @HrMin, @HrMax, @DiaRetiradaDefecto, @NBlisteresDefecto,
                @DiasAntelacionListado
            )
            """,
            farmacia);

    public void Actualizar(Farmacia farmacia)
        => conexion.Execute(
            """
            UPDATE Farmacia SET
                codigo_sanitario = @CodigoSanitario, logo = @Logo, nombre = @Nombre,
                titular_o_comunidad_bienes = @TitularOComunidadBienes, cif = @Cif,
                titular_colegiado = @TitularColegiado, direccion = @Direccion, cp = @Cp,
                poblacion = @Poblacion, provincia = @Provincia, telefono = @Telefono, fax = @Fax,
                email = @Email, whatsapp = @Whatsapp, responsable_datos = @ResponsableDatos,
                direccion_derechos = @DireccionDerechos, email_derechos = @EmailDerechos,
                prefijo_num_ficha = @PrefijoNumFicha, prefijo_num_spd = @PrefijoNumSpd,
                ruta_backup = @RutaBackup, ruta_documentos_generados = @RutaDocumentosGenerados,
                url_nomenclator = @UrlNomenclator,
                umbral_reutilizacion_lectura_ambiental_horas = @UmbralReutilizacionLecturaAmbientalHoras,
                temp_min = @TempMin, temp_max = @TempMax, hr_min = @HrMin, hr_max = @HrMax,
                dia_retirada_defecto = @DiaRetiradaDefecto, n_blisteres_defecto = @NBlisteresDefecto,
                dias_antelacion_listado = @DiasAntelacionListado,
                modificado_en = @ModificadoEn
            WHERE id = 1
            """,
            new
            {
                farmacia.CodigoSanitario, farmacia.Logo, farmacia.Nombre,
                farmacia.TitularOComunidadBienes, farmacia.Cif, farmacia.TitularColegiado,
                farmacia.Direccion, farmacia.Cp, farmacia.Poblacion, farmacia.Provincia,
                farmacia.Telefono, farmacia.Fax, farmacia.Email, farmacia.Whatsapp,
                farmacia.ResponsableDatos, farmacia.DireccionDerechos, farmacia.EmailDerechos,
                farmacia.PrefijoNumFicha, farmacia.PrefijoNumSpd, farmacia.RutaBackup,
                farmacia.RutaDocumentosGenerados, farmacia.UrlNomenclator,
                farmacia.UmbralReutilizacionLecturaAmbientalHoras, farmacia.TempMin,
                farmacia.TempMax, farmacia.HrMin, farmacia.HrMax, farmacia.DiaRetiradaDefecto,
                farmacia.NBlisteresDefecto, farmacia.DiasAntelacionListado,
                ModificadoEn = DateTime.UtcNow.ToString("o")
            });
}
