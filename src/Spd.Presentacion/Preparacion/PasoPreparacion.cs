using System.Collections.Generic;
using Spd.Dominio;

namespace Spd.Presentacion.Preparacion;

/// <summary>Los cinco pasos del ciclo de un blíster (Spec 015 FR-1540).</summary>
public enum ClavePaso
{
    Llenado,
    Etiquetado,
    Instrucciones,
    Verificacion,
    Entrega
}

public enum EstadoPaso
{
    Completado,
    Actual,
    Bloqueado
}

/// <summary>Un paso del carril: qué es, en qué estado está y, si está bloqueado, **por qué**.
///
/// El motivo no es un adorno: el Art. XI exige que un bloqueo se explique. Hoy el usuario descubre
/// que no puede entregar cuando pulsa "Entregar" y le sale un error; con el carril lo sabe antes de
/// intentarlo y sabe qué le falta.</summary>
public sealed record PasoPreparacion(ClavePaso Clave, string Titulo, EstadoPaso Estado, string? MotivoBloqueo = null)
{
    public bool EstaBloqueado => Estado == EstadoPaso.Bloqueado;
    public bool EsActual => Estado == EstadoPaso.Actual;
    public bool EstaCompletado => Estado == EstadoPaso.Completado;

    /// <summary>Deriva el carril del estado real del blíster y de sus datos. **No introduce ninguna
    /// regla nueva**: cada bloqueo corresponde a una validación que el servicio ya aplica
    /// (FR-650 verificar solo un PREPARADO, FR-660 entregar solo un VERIFICADO). Si esto y el
    /// servicio discreparan, mandaría el servicio; por eso aquí no se decide nada, solo se cuenta.</summary>
    public static IReadOnlyList<PasoPreparacion> Derivar(SPD spd, bool hayMaterialDisponible)
    {
        if (spd.Estado == EstadoSpd.Anulado)
        {
            const string anulado = "El blíster está ANULADO; no se puede seguir trabajando sobre él.";
            return
            [
                new(ClavePaso.Llenado, "Llenado", EstadoPaso.Bloqueado, anulado),
                new(ClavePaso.Etiquetado, "Etiquetado", EstadoPaso.Bloqueado, anulado),
                new(ClavePaso.Instrucciones, "Instrucciones", EstadoPaso.Bloqueado, anulado),
                new(ClavePaso.Verificacion, "Verificación", EstadoPaso.Bloqueado, anulado),
                new(ClavePaso.Entrega, "Entrega", EstadoPaso.Bloqueado, anulado)
            ];
        }

        var yaPreparado = spd.Estado is EstadoSpd.Preparado or EstadoSpd.Verificado or EstadoSpd.Entregado;
        var yaVerificado = spd.Estado is EstadoSpd.Verificado or EstadoSpd.Entregado;
        var yaEntregado = spd.Estado == EstadoSpd.Entregado;

        return
        [
            new(ClavePaso.Llenado, "Llenado",
                yaPreparado ? EstadoPaso.Completado
                : hayMaterialDisponible ? EstadoPaso.Actual : EstadoPaso.Bloqueado,
                yaPreparado || hayMaterialDisponible
                    ? null
                    : "No hay material de acondicionamiento registrado; se necesita su lote para poder trazar el blíster."),

            new(ClavePaso.Etiquetado, "Etiquetado",
                spd.ImpresoEtiquetasEn is not null ? EstadoPaso.Completado
                : yaPreparado ? EstadoPaso.Actual : EstadoPaso.Bloqueado,
                spd.ImpresoEtiquetasEn is not null || yaPreparado
                    ? null
                    : "Requiere terminar el llenado: la etiqueta lleva la validez del blíster ya cerrado."),

            new(ClavePaso.Instrucciones, "Instrucciones",
                spd.ImpresoInstruccionesEn is not null ? EstadoPaso.Completado
                : yaPreparado ? EstadoPaso.Actual : EstadoPaso.Bloqueado,
                spd.ImpresoInstruccionesEn is not null || yaPreparado
                    ? null
                    : "Requiere terminar el llenado."),

            new(ClavePaso.Verificacion, "Verificación",
                yaVerificado ? EstadoPaso.Completado
                : spd.Estado == EstadoSpd.Preparado ? EstadoPaso.Actual : EstadoPaso.Bloqueado,
                yaVerificado || spd.Estado == EstadoSpd.Preparado
                    ? null
                    : "Solo se puede verificar un blíster en PREPARADO (FR-650)."),

            new(ClavePaso.Entrega, "Entrega",
                yaEntregado ? EstadoPaso.Completado
                : spd.Estado == EstadoSpd.Verificado ? EstadoPaso.Actual : EstadoPaso.Bloqueado,
                yaEntregado || spd.Estado == EstadoSpd.Verificado
                    ? null
                    : "Requiere verificación: solo se entrega un blíster VERIFICADO (FR-660).")
        ];
    }
}
