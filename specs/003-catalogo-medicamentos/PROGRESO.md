# Plan y progreso — Spec 003: Catálogo de medicamentos

Este documento se actualiza al completar cada fase, cada test y cada prueba. No se reordena
retroactivamente lo ya marcado como hecho; solo se añade.

**Nota de contexto**: esta spec se implementa en una rama independiente
(`003-catalogo-medicamentos`, creada desde `main`) mientras la Spec 001
(`001-pacientes-y-medicos`) espera la verificación manual del usuario. Spec 003 no depende
funcionalmente de Spec 001 (ver spec.md, "Depende de"), así que ambas ramas avanzan en paralelo
sin conflicto — Medicamento y Paciente son catálogos/entidades independientes.

## Fases del flujo Spec Kit

| # | Fase | Comando | Estado | Commit |
|---|---|---|---|---|
| 1 | Especificación | `/speckit-specify` | ✅ Hecho — 2026-09-05 | *(pendiente de commit)* |
| 2 | Aclaración | `/speckit-clarify` | ✅ Hecho — 2026-09-05 (Q1 resuelta) | *(pendiente de commit)* |
| 3 | Plan de implementación | `/speckit-plan` | ✅ Hecho — 2026-09-05 | *(pendiente de commit)* |
| 4 | Desglose de tareas | `/speckit-tasks` | ✅ Hecho — 2026-09-05 | *(pendiente de commit)* |
| 5 | Análisis de coherencia | `/speckit-analyze` | ✅ Hecho — 2026-09-05 (2 hallazgos remediados) | *(pendiente de commit)* |
| 6 | Implementación | `/speckit-implement` | ✅ Hecho — 2026-09-05 (pendiente prueba manual del usuario) | *(pendiente de commit)* |

## Criterios de aceptación de la spec y su tipo de test previsto

Formato Dado/Cuando/Entonces (Constitución Art. IX.2). El tipo de test se confirma o ajusta en la
fase de `/speckit-plan`.

| CA | Descripción | Capa / tipo de test previsto |
|---|---|---|
| CA-300 | Alta mínima (solo CN + nombre), marcado "descripción física pendiente" | ✅ `Crear_guarda_con_solo_cn_y_nombre_CA_300` |
| CA-301 | Editar descripción física no reescribe instantáneas de SPD ya entregados | ✅ `ActualizarDescripcionFisica_versiona_la_anterior_en_dos_cambios_sucesivos_CA_301` |
| CA-302 | Marcar no apto SPD exige motivo | ✅ `ActualizarDatos_exige_motivo_solo_si_apto_spd_difiere_del_derivado_CA_302` |
| CA-303 | CN duplicado bloqueado | ✅ `Crear_bloquea_un_cn_ya_activo_CA_303` |
| CA-304 | Importación no sobrescribe descripción física sin confirmación | ✅ `AplicarNombreDesdeNomenclator_cambia_solo_el_nombre_CA_304` |
| CA-305 | Reactivación de CN dado de baja en vez de duplicar | ✅ `Crear_reactiva_un_cn_dado_de_baja_en_vez_de_duplicar_CA_305` |

## Invariantes de constitución que aplican a esta spec

- Art. IV (catálogo vivo, historia congelada): FR-303/FR-304 son el ejemplo canónico del Art. IV —
  editar la descripción física de un medicamento afecta a todo uso futuro, pero `Medicamento_Hist`
  conserva cada versión y las líneas de SPD ya creadas guardan su propia instantánea (a diferencia
  de Spec 001, donde Medico no tiene instantánea porque la referencia del paciente siempre es
  viva — aquí sí existe instantánea porque el consumidor, SPD_Linea, es un documento legal
  reproducible, Art. II.3).
- Art. V (un dato, una entrada): FR-303 (texto autogenerado a partir de los campos de descripción,
  editable pero no re-tecleado desde cero).
- Art. VI (aislamiento de red): FR-320 reutiliza la única conexión de red ya prevista para el
  nomenclátor (Spec 000); esta spec no abre ninguna conexión nueva.
- Art. IX (calidad): CA-300..CA-305 se traducen en tests antes de darse por completado.
- Art. X (simplicidad): FR-306 reactiva un CN existente en vez de crear un catálogo paralelo de
  "medicamentos retirados".

## Registro de avance

- **2026-09-05** — Fase 1 completada. `spec.md` trasladado literalmente desde
  `spec-003-catalogo-medicamentos.md`, con Q1 (lista de formas farmacéuticas) marcada
  `[NEEDS CLARIFICATION]`. Checklist de calidad generado; todo pasa salvo "sin NEEDS
  CLARIFICATION", bloqueado a propósito hasta la fase 2.
- **2026-09-05** — Fase 2 completada. Q1 planteada; el propietario del producto eligió la opción
  recomendada: mantener la lista de formas farmacéuticas de FR-300 tal cual, sin añadir valores.
  `spec.md` actualizado con sección "Clarifications" y §9 sin marcadores pendientes. Checklist de
  calidad al 100 %.
- **2026-09-05** — Fase 3 completada. `plan.md` (Technical Context + Constitution Check con 8
  artículos), `research.md` (5 decisiones: enum cerrado de forma farmacéutica, derivación de
  `apto_spd` sin tabla de excepciones, `desc_texto` como propuesta que nunca sobrescribe una
  edición manual, `desc_vigente_desde` para poder versionar en `Medicamento_Hist`, lector de
  nomenclátor CSV mínimo a la espera de Spec 011). `data-model.md` (recorte de
  Medicamento/Medicamento_Hist), `contracts/servicios-aplicacion.md` (2 interfaces),
  `quickstart.md`. **Nota importante de coordinación entre ramas**: esta rama parte de `main`
  (solo Spec 000), así que su migración se numera `0002` aquí, la misma que usa
  `001-pacientes-y-medicos` para su propia migración (que también parte de `main`). La que se
  mergee en segundo lugar de las dos deberá renumerar su migración a `0003` antes de mergear —
  anotado en `plan.md` §Project Structure para no olvidarlo cuando llegue el momento.
- **2026-09-05** — Fase 4 completada. `tasks.md` generado: Foundational (T001-T009: migración
  0002, entidad, `ReglaAptitudSpd`, repositorio, tests de humo) y 2 user stories — US1 Alta,
  edición y búsqueda P1/MVP (T010-T024), US2 Importación del nomenclátor P2 (T025-T033) — más
  Polish (T034-T036). 36 tareas en total. El test de auditoría (Art. VII.6) y el test
  Avalonia.Headless de regresión (mismo patrón que `UsuariosWindowTests`/`BuscadorPacientesViewTests`)
  se incluyeron desde el diseño, aplicando la misma lección ya aprendida en la remediación de
  `/speckit-analyze` de Spec 001.
- **2026-09-05** — Fase 5 completada. 2 hallazgos, ambos MEDIA, ninguno crítico: **F1** —
  ningún test verificaba que `DarDeBaja` conserva la fila (Art. III.1), a diferencia de Spec 001 →
  nuevo T020. **F2** — `ActualizarUnidadesEnvase` no estaba en la lista de escrituras auditadas de
  T019 → ampliado. Renumerado de 36 a 37 tareas (seguro, sin código aún).
- **2026-09-05** — Fase 6 (Implementación) iniciada. Foundational (T001-T009) completada:
  migración `0002_catalogo_medicamentos.sql` (Medicamento, Medicamento_Hist), enums de Dominio
  (`FormaFarmaceutica`, `OrigenUnidadesEnvase`), `ReglaAptitudSpd`, entidad `Medicamento`,
  `VersionDescripcionFisica`, repositorio con `Fila` explícita (mismo patrón que Spec 001).
  **Nota de coordinación entre ramas**: se añadió `Spd.Dominio/Normalizador.cs` idéntico al de la
  rama `001-pacientes-y-medicos` (sin mergear) y una columna `nombre_normalizado` en Medicamento,
  análoga a `busqueda_normalizada` de esa spec — ambas ramas necesitan la misma utilidad de
  normalización de búsqueda; quedará un solo fichero al mergear. También se corrigió, igual que en
  Spec 001, el test viejo de Spec 000 que asumía un número fijo de migraciones. `dotnet build` sin
  errores; 7+2+32 = 41 tests en verde.
- **2026-09-05** — User Story 1 (P1, MVP) completada: T010-T025. `ServicioMedicamentos` con alta
  mínima que reactiva un CN de baja en vez de duplicar (CA-300/303/305), edición que exige motivo
  solo cuando la aptitud SPD difiere de la derivada por `ReglaAptitudSpd` (CA-302), versionado de
  la descripción física en `Medicamento_Hist` en cada cambio real, incluida la primera vez que se
  rellena (CA-301), propuesta de `desc_texto` sin persistir, búsqueda por CN o nombre sin tildes, y
  test dedicado de que `DarDeBaja` conserva la fila (Art. III.1, remediación F1). En Presentación:
  `CatalogoMedicamentosView` (listado + formulario, mismo patrón que `UsuariosView` de Spec 000)
  con botón en `MainWindow` visible para Elaborador y Administrador. Test Avalonia.Headless
  `CatalogoMedicamentosViewTests` fuerza la realización del `ItemTemplate` con un medicamento real.
  Un test descubrió un matiz real del versionado (dos cambios desde vacío generan dos versiones de
  histórico, no una) que corrigió la expectativa del test, no el código — el comportamiento es el
  correcto según FR-304 literal. `dotnet build` sin errores; 15+3+42 = 60 tests en verde. Pendiente
  de prueba manual real por el usuario, igual que en Spec 001.
- **2026-09-05** — User Story 2 (P2) completada: T026-T034. `LectorNomenclatorCsv` (simplificación
  deliberada de research.md Decisión 5) extrae filas `(CN, Nombre)` de un CSV con cabecera exacta;
  `ServicioImportacionNomenclator.CompararConNomenclator` distingue nuevos/con nombre distinto/sin
  cambios; `AplicarNombreDesdeNomenclator` cambia solo el nombre, nunca descripción física ni
  aptitud SPD (CA-304). En Presentación: `RevisionNomenclatorView` (dos listados con confirmación
  fila a fila) accesible desde un botón en `CatalogoMedicamentosView`, leyendo del mismo fichero
  fijo que ya usa `NomenclatorViewModel` de Spec 000 al descargar. Un test propio reveló un uso
  incorrecto de una referencia desactualizada de `Medicamento` en el propio test (no en el
  servicio) — corregido recargando tras cada escritura, mismo matiz ya documentado en Spec 001.
  `dotnet build` sin errores; 15+4+47 = 66 tests en verde.
- **2026-09-05** — Fase Polish completada (T035-T037). Los 6 escenarios de `quickstart.md`
  (CA-300..CA-305) están cubiertos 1:1 por tests automatizados — ver tabla de criterios de
  aceptación arriba, actualizada con el nombre exacto del test que confirma cada uno. Revisión de
  tamaño (Art. XI.6): todas las clases muy por debajo de ~300 líneas
  (`RepositorioMedicamentos.cs` es la mayor, 186); un único método (`CatalogoMedicamentosViewModel.
  Guardar`) ronda ~40 líneas por componer tres llamadas de servicio en un solo flujo de guardado
  cohesivo — no se ha dividido porque fragmentarlo en partes arbitrarias no aportaría legibilidad
  (Art. XI.7). **Spec 003 queda completa en código y tests, pendiente de la prueba manual real
  (`dotnet run`) del usuario** cuando pueda sentarse al ordenador, igual que Spec 001 — no se puede
  verificar una interfaz gráfica de escritorio sin ejecutarla de verdad.
