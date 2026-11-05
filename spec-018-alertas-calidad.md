# Spec 018 — Alertas de calidad y trazabilidad inversa por lote

**Estado:** Implementada
**Versión:** 1.0 — 2026-11-05
**Constitución aplicable:** 3.0.0 (Artículos I, III.1, IV.3, VII.6)
**Depende de:** Spec 005 (envases con lote), Spec 006 (líneas de SPD con sus envases y su instantánea), Spec 006 FR-691 (avisos de inicio)
**Requerida por:** Ninguna

---

## 1. Propósito

Cuando la AEMPS, el laboratorio o el Colegio comunican la retirada de un lote, saber **en segundos** a quién alcanza: qué cajas hay en el depósito, qué blísteres sin entregar hay que rehacer y a qué pacientes se les entregó un blíster con unidades de ese lote. Es la razón de registrar el lote de cada caja (PNT, trazabilidad; Spec 005 FR-510) y de guardarlo en la instantánea de cada línea (Art. IV.3). Idea tomada del análisis de programas de SPD existentes; implementación propia.

## 2. Actores

| Actor | Puede |
|---|---|
| Elaborador / Administrador | Registrar y cerrar alertas y ver su trazabilidad (una retirada de lote no espera al administrador) |

## 3. Requisitos funcionales

- **FR-1801** Alta de alerta: referencia (nota de la AEMPS, laboratorio o Colegio), fecha, CN del medicamento (seis dígitos, o siete con el dígito de control, que se quita), uno o varios lotes (separados por comas, punto y coma o saltos de línea) y descripción opcional. Se guarda una alerta por lote.
- **FR-1802** Coincidencia de lote: sin distinguir mayúsculas ni tener en cuenta espacios, guiones ni otros signos («AB-123» = «ab 123» = «AB123»). Una coincidencia de más se descarta mirando la caja; una de menos deja un lote retirado en un blíster.
- **FR-1803** Trazabilidad de cajas: todos los envases (en cualquier estado) de un medicamento con ese CN y ese lote, con paciente, serie, estado y unidades restantes.
- **FR-1804** Trazabilidad de blísteres: todos los SPD con alguna línea que tomó unidades de un envase de ese CN y lote, según la **instantánea** de la línea (`SPD_Linea_Envase.snap_lote`/`snap_cn`, o el envase si la instantánea es anterior a esos campos), separados en pendientes (borrador, preparado, verificado) y entregados.
- **FR-1805** Cierre con motivo obligatorio; nunca se borra (Art. III.1). Las cerradas siguen consultables.
- **FR-1806** Mientras una alerta activa alcance a alguna caja en custodia o a algún blíster pendiente o entregado, el panel de inicio lo muestra el primero, con severidad máxima, y lleva a la pantalla de alertas.
- **FR-1807** Alta y cierre dejan traza en auditoría (Art. VII.6).

## 4. Entidades clave

`AlertaCalidad` (migración 0017): referencia, fecha_alerta, cn, lote, lote_normalizado, descripcion, activa, fecha_cierre, motivo_cierre.

## 5. Criterios de aceptación

**CA-1801** Dados los lotes «AB-123, CD9; ab123», cuando registro la alerta, entonces se crean dos alertas (AB-123 y CD9).
**CA-1802** Dado un lote en cajas de dos pacientes, uno con un blíster entregado y otro con uno verificado, cuando consulto la alerta, entonces veo las dos cajas, el blíster entregado y el pendiente, y no veo las cajas de otros lotes.
**CA-1803** Dada una alerta activa que alcanza a cajas en custodia, cuando abro el inicio, entonces veo el aviso; al cerrarla, desaparece.
**CA-1804** Sin referencia, con un CN que no tiene seis dígitos o sin ningún lote, el alta se rechaza.
**CA-1805** Cerrar sin motivo se rechaza.

## 6. Fuera de alcance

- Descarga automática de alertas de la AEMPS: abriría una tercera conexión de red (Art. VI.2).
- Aviso al registrar una caja de un lote con alerta activa: la caja aparece igualmente en el aviso de inicio en cuanto se registra.
- Comunicación a los pacientes afectados: la hace la farmacia; la pantalla dice a quién.
