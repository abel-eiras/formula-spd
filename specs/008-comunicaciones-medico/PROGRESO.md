# Plan y progreso — Spec 008: Comunicaciones al médico

Este documento se actualiza al completar cada fase, cada test y cada prueba. No se reordena
retroactivamente lo ya marcado como hecho; solo se añade.

**Nota de contexto**: esta rama se bifurca de `main` tras fusionar Specs 001, 003, 009, 010, 004 y
005 (2026-09-06). Migración nueva `0007_comunicaciones_medico.sql`, sin colisión, siguiendo
directamente a la 0006 de Spec 005.

- **2026-09-06** — Ciclo completo `/speckit-specify` → `/speckit-plan` → `/speckit-tasks` →
  `/speckit-implement`, ejecutado de forma autónoma. Spec pequeña y sin bloqueos de contenido
  (a diferencia de Spec 002): FR-805 (aviso real desde Spec 006) y FR-806 (documento real de
  Spec 007) se implementan como puntos de extensión invocables, sin fabricar esas pantallas.

- **Corrección de diseño durante la implementación**: el plan inicial de FR-804/805 era que
  `CrearDesdeTratamiento`/`CrearDesdeAvisoCambioReferido` insertaran directamente una comunicación
  "borrador" con incidencias en blanco. Al escribir el servicio se detectó que esto contradice el
  propio FR-807 ("las comunicaciones son un hecho registrado, no un borrador") — se corrigió antes
  de escribir ningún test: ambos métodos pasaron a llamarse `PrepararDesdeTratamiento`/
  `PrepararDesdeAvisoCambioReferido` y devuelven un `DatosAltaComunicacionMedico` sin persistir
  nada; el alta real sigue pasando siempre por `Crear` (que exige incidencias+propuesta si
  `Tipo=Incidencia`, FR-802/CA-801).

- **Foundational**: `ComunicacionMedico` (inmutable salvo `Respuesta`/`FechaRespuesta`, Art. III),
  `EsImprimible` como propiedad calculada (`Tipo != Telefono`, research.md Decisión 3).

- **User Story 1 (P1, MVP) — Presentación prerrellenada**: `ServicioComunicacionesMedico.Crear`
  prerrellena el médico de cabecera si `MedicoId` es null (CA-800).

- **User Story 2 (P1) — Incidencia y respuesta**: `Crear` exige incidencias+propuesta para
  `Tipo=Incidencia` (CA-801); `RegistrarRespuesta` es el único UPDATE permitido, no toca `Fecha`
  de creación (CA-802); `EsImprimible` distingue Telefono de Presentacion/Incidencia (CA-804).

- **User Story 3 (P2) — Creación prerrellenada**: `PrepararDesdeTratamiento`/
  `PrepararDesdeAvisoCambioReferido` resuelven paciente+médico sin persistir nada (FR-804/805,
  CA-803 adaptado). Botón "Comunicar incidencia" en `TratamientoView` (Spec 004) abre
  `ComunicacionesMedicoWindow` ya prerrellena; botón "Comunicaciones" directo en `FichaPacienteView`
  (Spec 001) para presentación/listado sin pasar por un tratamiento concreto.

  `dotnet build` sin errores; **51 (Dominio) + 14 (Presentación) + 150 (Aplicación) = 215 tests en
  verde**. Las 21 tareas de [tasks.md](./tasks.md) están completas.

## Pendiente (documentado, no fabricado)

- FR-805 real: Spec 006 debe invocar `PrepararDesdeAvisoCambioReferido` desde su propio aviso de
  "tratamiento pendiente de revisión".
- FR-806 real: Spec 007 debe generar `CARTA-PRES`/`CARTA-INC`; hoy solo existe `EsImprimible` como
  dato expuesto.
- Prueba manual real (`dotnet run`) por el usuario.

## 2026-09-06 (tarde) — FR-806 real: `CARTA-PRES` / `CARTA-INC`

Con el Anexo I.C del PNT I (COF A Coruña) disponible tras el cribado de `resources/`, el motor de
documentos de Spec 007 genera la carta de presentación con el texto literal del anexo (fecha y
lugar, saludo al médico, definición del servicio, paciente, "se adjunta ficha del paciente",
farmacéutico responsable y nº de colegiado, P.D. con los datos de contacto de la farmacia). La carta
de incidencias no tiene modelo en el PNT: reutiliza encabezado y cierre y lleva las incidencias
detectadas y la propuesta del farmacéutico. Una comunicación telefónica sigue sin generar documento
(`EsImprimible`). Botón "Imprimir carta" en cada comunicación imprimible; la ventana recibe el
servicio de documentos también desde la ficha de tratamiento (FR-804). Tests de presencia de
elementos por extracción de texto del PDF. FR-805 sigue pendiente.


## 2026-09-06 — FR-805 enganchado, y corregido de paso

El circuito existía a medias: al referir cambios en la entrega ya se abría la comunicación, pero
tomaba el médico con `PrepararDesdeTratamiento` sobre **la primera línea que apareciera**. Con dos
tratamientos de médicos distintos, el destinatario podía ser cualquiera de los dos; y en la práctica
`PrepararDesdeAvisoCambioReferido` no lo invocaba nadie.

Ahora el destinatario es el **médico de cabecera del paciente**, que es de quien habla el FR-805: el
paciente refiere un cambio suyo, no de un medicamento concreto. Si no tiene médico de cabecera se
cae al prescriptor del tratamiento, y si tampoco lo hay se abre la pestaña vacía para elegirlo a
mano. Nunca se adivina.

Esto solo es posible desde hoy: hasta que la Spec 001 US2 no construyó el catálogo de médicos y la
ficha no ganó su campo de cabecera, `Paciente.MedicoId` era siempre nulo.

Dos tests nuevos en `AvisoCambioReferidoTests`, uno por rama, con un tratamiento prescrito
deliberadamente por **otro** médico para que un fallo de destinatario salte.
