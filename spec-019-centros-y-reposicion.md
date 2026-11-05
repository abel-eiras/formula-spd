# Spec 019 — Centros, albarán de entrega y lista de reposición

**Estado:** Implementada
**Versión:** 1.0 — 2026-11-05
**Constitución aplicable:** 3.0.0 (Artículos II, III.1, IV.1, V, VII.6)
**Depende de:** Spec 001 (pacientes), Spec 004 (tratamientos), Spec 005 (envases), Spec 006 (estados del SPD)
**Requerida por:** Ninguna

---

## 1. Propósito

Dos herramientas para el trabajo con residencias y para no quedarse sin medicación. Ideas tomadas del análisis de programas de SPD existentes; implementación propia.

1. **Centros y albarán**: agrupar a los pacientes de una residencia o centro de día y entregar sus blísteres con un **albarán** que firma quien los recibe.
2. **Lista de reposición**: qué medicamentos en SPD se agotan en los próximos N días con lo que hay en custodia, para pedir a tiempo la dispensación o una receta nueva. El listado de retirada (Spec 005) solo mira la próxima preparación; esta lista mira más lejos.

## 2. Actores

| Actor | Puede |
|---|---|
| Elaborador / Administrador | Gestionar centros, asignar pacientes, imprimir albaranes y la lista de reposición |

## 3. Requisitos funcionales

### Centros

- **FR-1900** Catálogo de centros (Art. IV.1): nombre (obligatorio), dirección, teléfono y persona de contacto. Baja lógica con motivo (Art. III.1); un centro con pacientes asignados no se da de baja.
- **FR-1901** Un paciente pertenece como mucho a un centro. Se asigna y se quita desde la pantalla del centro, buscando al paciente.
- **FR-1902** Toda alta, cambio, baja y asignación deja traza en auditoría (Art. VII.6).

### Albarán de entrega

- **FR-1903** La pantalla del centro lista los blísteres **verificados** (listos para entregar) de sus pacientes; todos marcados, se pueden desmarcar.
- **FR-1904** «Imprimir albarán» genera un PDF con la cabecera de la farmacia (FR-730), el centro, la fecha, la tabla de blísteres (nº de registro, paciente, ficha, periodo de validez), el total y espacio para la firma de quien entrega y de quien recibe (nombre, DNI, firma). Se firma a mano (Art. II.1) y la generación queda en auditoría con los números de registro (Art. II.2).
- **FR-1905** El albarán **no registra la entrega**: el PNT pide para cada entrega datos propios del paciente (entrega anterior, unidades no administradas, cambios de medicación; Anexo I.G) que una entrega en bloque no recogería. Cada blíster se sigue entregando desde su preparación (Spec 006 FR-661).

### Lista de reposición

- **FR-1910** Para cada paciente activo y cada medicamento en SPD (agrupando marcas del mismo grupo homogéneo, como el listado de retirada): unidades en custodia que no caducan hoy, consumo semanal según la pauta, días de cobertura (unidades ÷ consumo diario, por defecto) y fecha en que se agota. Se listan los que se agotan dentro del horizonte (1 a 120 días; 14 por defecto), por fecha.
- **FR-1911** Un tratamiento sin consumo (pauta a cero) no tiene fecha de agotamiento y no aparece.
- **FR-1912** Filtro por centro e impresión en PDF con paciente, centro, médico, medicamento, CN, unidades, días y fecha; la impresión queda en auditoría.

## 4. Entidades clave

`Centro` (migración 0018) y `Paciente.centro_id`. La lista de reposición se calcula, no se guarda.

## 5. Criterios de aceptación

**CA-1901** Dado un centro con una paciente que tiene un blíster verificado y otro entregado, cuando abro el centro, entonces el albarán propone solo el verificado.
**CA-1902** El PDF del albarán contiene la farmacia, el título, el centro, el número de registro, el paciente, el periodo de validez y los espacios de firma, nombre y DNI de quien recibe.
**CA-1903** Un centro con pacientes no se da de baja; sin pacientes, sí, y deja de aparecer entre los activos.
**CA-1904** Con 10 unidades y una pauta de 2 al día, el medicamento aparece con 5 días de cobertura en un horizonte de 14 días; con 56 unidades solo aparece en un horizonte de 28 o más.
**CA-1905** El filtro por centro solo lista pacientes de ese centro; una caja caducada no cuenta.
**CA-1906** El PDF de la lista contiene paciente, medicamento, CN y fecha de agotamiento.

## 6. Fuera de alcance

- Entrega en bloque de los blísteres de un centro (FR-1905).
- Hoja de administración por planta o habitación: no hay planta ni habitación en el modelo.
- Envío de la lista de reposición al centro de salud: se imprime; no hay conexiones de red para eso (Art. VI.2).
