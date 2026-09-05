# Fase 0 — Investigación: Tratamiento del paciente

## Decisión 1 — Inmutabilidad: cerrar y abrir fila, mismo patrón que Medicamento_Hist (Spec 003)

**Contexto**: FR-410 exige que un cambio clínicamente relevante cierre la fila actual
(`estado = FINALIZADO`, `fecha_fin = hoy`) y cree una fila nueva, en vez de editar en el sitio —
la misma necesidad de "versionar sin sobrescribir" que Spec 003 ya resolvió para la descripción
física de un medicamento (`Medicamento_Hist`, Art. IV.3).

**Decisión**: a diferencia de `Medicamento_Hist` (una tabla de historial separada de la fila viva),
`Tratamiento` versiona **dentro de la misma tabla**: cada fila es una versión completa con su
propio `id`, `fecha_inicio`, `fecha_fin` y `estado`. No hace falta una tabla separada porque no hay
un "tratamiento actual" distinto de sus versiones — el tratamiento vigente de un medicamento para
un paciente es, sencillamente, la fila con ese `medicamento_id`/`paciente_id` sin `fecha_fin` (o
con `fecha_fin` en el futuro) y `estado` distinto de `FINALIZADO`. `fecha_prescripcion_inicial` en
cada fila nueva copia la de la fila que cierra, para no perder cuándo se prescribió por primera vez
ese medicamento a ese paciente aunque la pauta haya cambiado varias veces.

**Alternativas consideradas**: una tabla `Tratamiento` + `Tratamiento_Hist` como Medicamento —
descartada (Art. X.2, menos piezas móviles): aquí no existe un "dato editable en el sitio que
también queda congelado en instantáneas ajenas" (como la descripción física de un medicamento, que
various SPD_Linea de distintos pacientes referencian a la vez) — cada fila de Tratamiento ya
pertenece a un único paciente, así que versionar la propia tabla es suficiente y más simple.

## Decisión 2 — Vocabulario cerrado de fracciones: enum, no tabla ni texto libre

**Contexto**: FR-402 exige que D/A/C/N solo acepten valores de una lista cerrada
(`0, 1/4, 1/3, 1/2, 2/3, 3/4, 1, 1 1/4, 1 1/2`), nunca un decimal libre.

**Decisión**: enum `FraccionDosis` en `Spd.Dominio` con un valor por cada fracción de la lista
(nombres tipo `Cero`, `UnCuarto`, `UnTercio`, `Media`, `DosTercios`, `TresCuartos`, `Uno`,
`UnoYCuarto`, `UnoYMedio`), y una propiedad `Valor` (`decimal`) para los cálculos que lo necesiten
(Spec 005/007) y `Texto` (`string`, p. ej. "1/2") para mostrarlo tal cual en pantalla e impresión —
igual que `FormaFarmaceutica` (Spec 003) es un enum cerrado, no una tabla de catálogo editable.

**Alternativas consideradas**: `decimal` con validación de que el valor esté en una lista permitida
— descartada, un `decimal` en el dominio sugiere que cualquier número es válido hasta que se lee la
validación; un enum lo deja imposible de representar mal por construcción (Art. X.2).

## Decisión 3 — `PENDIENTE_REVISION`/SIGRE/cálculo automático: solo el punto de extensión, no la lógica de Spec 005/006

**Contexto**: FR-421/422/430 mencionan lógica que vive en Spec 005 (Envase, SIGRE, fórmula de
consumo) y Spec 006 (entrega, cambios referidos), ninguna existente en esta rama.

**Decisión**: se implementan los **estados y campos** que esas specs futuras necesitarán enganchar
(`PENDIENTE_REVISION` como estado seleccionable manualmente, `ajuste_unidades_manual` con su botón
"usar cálculo automático" que lo pone a `null`), pero no se inventa el disparador automático ni la
fórmula de cálculo — eso sería adivinar el contrato de una spec no escrita todavía (Art. X.3, un
`[NEEDS CLARIFICATION]` de dependencia no se resuelve con una suposición). Documentado como
"Fuera de alcance" en spec.md, no como pregunta abierta (la respuesta ya se conoce: se completará
cuando existan Spec 005/006).

## Output

Todas las incógnitas de esta iteración resueltas. Ninguna arrastra `NEEDS CLARIFICATION` real a
`tasks.md` — las marcadas en spec.md son dependencias de specs futuras, no ambigüedad de negocio.
