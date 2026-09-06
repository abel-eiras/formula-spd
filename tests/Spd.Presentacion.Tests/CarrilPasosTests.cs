using System;
using System.Linq;
using Spd.Dominio;
using Spd.Presentacion.Preparacion;
using Xunit;

namespace Spd.Presentacion.Tests;

/// <summary>Spec 015 CA-1530: el carril refleja el estado real del blíster y, cuando algo está
/// bloqueado, dice por qué (Art. XI). No introduce reglas: cada bloqueo corresponde a una validación
/// que el servicio ya aplica.</summary>
public sealed class CarrilPasosTests
{
    private static SPD Blister(EstadoSpd estado, DateTime? etiquetas = null, DateTime? instrucciones = null) => new()
    {
        NumRegistro = "F-1", CorrelativoNumRegistro = 1, PacienteId = 1, SesionId = Guid.NewGuid(),
        ElaboradorId = 1, Estado = estado,
        ValidezDesde = new DateOnly(2026, 9, 7), ValidezHasta = new DateOnly(2026, 9, 13),
        ImpresoEtiquetasEn = etiquetas, ImpresoInstruccionesEn = instrucciones
    };

    private static PasoPreparacion Paso(SPD spd, ClavePaso clave, bool hayMaterial = true)
        => PasoPreparacion.Derivar(spd, hayMaterial).Single(p => p.Clave == clave);

    [Fact]
    public void El_paso_actual_corresponde_al_estado_del_spd_CA_1530()
    {
        // En BORRADOR se está llenando; nada posterior está disponible todavía.
        var borrador = Blister(EstadoSpd.Borrador);
        Assert.Equal(EstadoPaso.Actual, Paso(borrador, ClavePaso.Llenado).Estado);
        Assert.Equal(EstadoPaso.Bloqueado, Paso(borrador, ClavePaso.Verificacion).Estado);
        Assert.Equal(EstadoPaso.Bloqueado, Paso(borrador, ClavePaso.Entrega).Estado);

        // En PREPARADO el llenado está hecho y toca verificar.
        var preparado = Blister(EstadoSpd.Preparado);
        Assert.Equal(EstadoPaso.Completado, Paso(preparado, ClavePaso.Llenado).Estado);
        Assert.Equal(EstadoPaso.Actual, Paso(preparado, ClavePaso.Verificacion).Estado);
        Assert.Equal(EstadoPaso.Bloqueado, Paso(preparado, ClavePaso.Entrega).Estado);

        // En VERIFICADO toca entregar.
        var verificado = Blister(EstadoSpd.Verificado);
        Assert.Equal(EstadoPaso.Completado, Paso(verificado, ClavePaso.Verificacion).Estado);
        Assert.Equal(EstadoPaso.Actual, Paso(verificado, ClavePaso.Entrega).Estado);

        // ENTREGADO: los cinco pasos hechos, ninguno actual.
        var entregado = Blister(EstadoSpd.Entregado, DateTime.Now, DateTime.Now);
        Assert.All(PasoPreparacion.Derivar(entregado, true), p => Assert.Equal(EstadoPaso.Completado, p.Estado));

        // ANULADO: nada se puede seguir haciendo, y se dice.
        var anulado = Blister(EstadoSpd.Anulado);
        Assert.All(PasoPreparacion.Derivar(anulado, true), p =>
        {
            Assert.Equal(EstadoPaso.Bloqueado, p.Estado);
            Assert.Contains("ANULADO", p.MotivoBloqueo);
        });
    }

    [Fact]
    public void Los_pasos_bloqueados_dan_su_motivo_CA_1530()
    {
        var borrador = Blister(EstadoSpd.Borrador);

        // Sin material de acondicionamiento no se puede cerrar el llenado: falta su lote para trazar.
        var llenadoSinMaterial = Paso(borrador, ClavePaso.Llenado, hayMaterial: false);
        Assert.Equal(EstadoPaso.Bloqueado, llenadoSinMaterial.Estado);
        Assert.Contains("material de acondicionamiento", llenadoSinMaterial.MotivoBloqueo);

        // La entrega bloqueada cita la razón real y el requisito que la impone.
        var entrega = Paso(borrador, ClavePaso.Entrega);
        Assert.Contains("verificación", entrega.MotivoBloqueo);
        Assert.Contains("FR-660", entrega.MotivoBloqueo);

        // La verificación bloqueada cita FR-650.
        Assert.Contains("FR-650", Paso(borrador, ClavePaso.Verificacion).MotivoBloqueo);

        // Un paso que no está bloqueado no arrastra motivo: un texto huérfano confundiría.
        Assert.Null(Paso(Blister(EstadoSpd.Preparado), ClavePaso.Verificacion).MotivoBloqueo);
    }

    [Fact]
    public void Etiquetado_e_instrucciones_se_marcan_hechos_cuando_ya_se_imprimieron()
    {
        var preparado = Blister(EstadoSpd.Preparado);
        Assert.Equal(EstadoPaso.Actual, Paso(preparado, ClavePaso.Etiquetado).Estado);
        Assert.Equal(EstadoPaso.Actual, Paso(preparado, ClavePaso.Instrucciones).Estado);

        var impreso = Blister(EstadoSpd.Preparado, etiquetas: DateTime.Now, instrucciones: DateTime.Now);
        Assert.Equal(EstadoPaso.Completado, Paso(impreso, ClavePaso.Etiquetado).Estado);
        Assert.Equal(EstadoPaso.Completado, Paso(impreso, ClavePaso.Instrucciones).Estado);
    }
}
