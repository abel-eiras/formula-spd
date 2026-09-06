# Plan y progreso — Spec 006: Preparación, verificación y entrega del SPD

Este documento se actualiza al completar cada fase, cada test y cada prueba. No se reordena
retroactivamente lo ya marcado como hecho; solo se añade.

**Nota de contexto**: esta rama se bifurca de `main` tras fusionar Specs 001, 003, 009, 010, 004,
005, 008 y 011 (2026-09-06). Migración nueva `0009_preparacion.sql` (7 tablas), sin colisión,
siguiendo directamente a la 0008 de Spec 011. Es la spec de mayor superficie del proyecto hasta
ahora.

- **2026-09-06** — Ciclo completo `/speckit-specify` → `/speckit-plan` → `/speckit-tasks` →
  `/speckit-implement`, ejecutado de forma autónoma tras petición explícita del usuario de
  completar el ciclo alta→tratamiento→preparación→verificación→entrega→documentación sin esperar
  a Spec 002. FR-602 (precondición de idoneidad/consentimiento, Art. I.3) se implementa contra
  `IComprobadorIdoneidadYConsentimiento`, con implementación nula por defecto — mismo patrón que
  Spec 005 dejó documentado hacia esta misma spec con `IComprobadorCoberturaSpd`.

- **Hallazgo notable, resuelto durante la implementación**: el nombre natural de la entidad
  principal, `Spd`, colisiona con el espacio de nombres raíz del proyecto (`Spd.Dominio`,
  `Spd.Aplicacion`...) y produce errores de resolución de nombres en C#. Se renombró a `SPD`
  (mayúsculas, como ya se escribe en toda la documentación) — un identificador distinto de `Spd`
  al ser C# sensible a mayúsculas, sin este problema.

- **Extensión completada, no solo diferida**: al construir `SPD` se pudo completar de verdad el
  punto de extensión que Spec 005 había dejado (`IComprobadorCoberturaSpd`, research.md Decisión 3
  de esa spec): `ComprobadorCoberturaSpdReal` sustituye a la implementación nula en
  `App.axaml.cs`, con tests propios. FR-530/CA-502 del listado de retirada ya funciona de verdad.

- **User Story 1 (P1, MVP) — Sesión, líneas y multi-envase**: `CrearSesion` crea 1 o 2 SPD en
  `Borrador` con sus líneas (instantánea + `unidades_dosis`/`unidades_envase`, reutilizando
  `CalculadoraUnidadesADescontar` de Spec 005), validando FR-602 completo. El envase no se consume
  hasta `PasarAPreparado` (research.md Decisión 2): se hace una pasada de validación sin mutar
  nada, y solo si todas las líneas tienen saldo se descuenta línea a línea vía
  `IServicioAsignacionEnvases.Descontar` (Spec 005, sin cambios), reutilizando su reparto
  multi-envase (CA-602).

- **User Story 2 (P2) — Alta de envase desde la preparación**: `RegistrarEnvaseDesdeLinea` delega
  en `IServicioEnvases.RegistrarEnvase` (Spec 005); tras el alta, `PasarAPreparado` funciona sin
  recargar nada (CA-603/604). Con control propio en la pantalla de sesión (botón por línea).

- **User Story 3/4 (P1) — Verificación y entrega**: `Verificar` exige motivo (≥10 caracteres) si
  verificador = elaborador (Art. I.3/VII.5); el resultado NoApto no avanza el estado. `RegistrarEntrega`
  acepta una lista de SPD (uno o dos), con los mismos datos si se entregan juntos (CA-606/607).

- **User Story 5 (P2) — Continuidad**: `PrepararSiguiente` copia la hoja anterior por medicamento
  (no por `tratamiento_id`, que cambia si la pauta se modificó — Spec 004 cierra/abre fila); marca
  `EnvasePendiente` de forma persistente (única marca real en `estado_linea`) y devuelve
  `ResultadoContinuidad` con modificadas/nuevas/eliminadas como comparación calculada, no
  persistida (research.md Decisión 5). CA-608/608b/609/610 cubiertas.

- **User Story 6 (P3) — Reelaboración**: `Reelaborar` conserva `num_registro`, sube `version`,
  limpia la verificación anterior y exige una nueva. El recálculo de envases es diferencial
  (aumenta = solo la diferencia se descuenta; reduce/elimina = la diferencia vuelve a
  `unidades_restantes`, empezando por la fila de envase más reciente de esa línea) — nunca
  "devolver todo y volver a descontar entero", que habría exigido borrar filas históricas
  (Art. III). `SPD_Modificacion` guarda copia íntegra de líneas y filas de envase anteriores.
  CA-6120..6126 cubiertas.

  `dotnet build` sin errores; **55 (Dominio) + 17 (Presentación) + 190 (Aplicación) = 262 tests en
  verde**.

## Pendiente (documentado, no fabricado)

- **T053 — Pantalla "Preparaciones" global** (FR-690, listado con filtro por sesión/paciente/
  elaborador/estado, cruzando pacientes): no construida. `ServicioPreparacion.ListarPorFiltro` ya
  existe y está probado; falta la vista. La pantalla por paciente (`PreparacionWindow`, accesible
  desde la ficha del paciente) ya cubre el ciclo completo pedido por el usuario para un paciente
  concreto — el listado global es una vista de conveniencia sobre datos ya expuestos, no una
  funcionalidad nueva.
- FR-680/681 reales: Spec 007 debe generar la ficha/etiquetas/instrucciones de verdad;
  `RegistrarImpresion` ya registra el timestamp y audita.
- FR-663 (aviso automático de cambio referido → tratamiento pendiente de revisión): el campo y el
  bloqueo de continuidad (FR-674) ya funcionan; falta el enganche automático desde la pantalla de
  entrega cuando se responde "sí" a "cambios de medicación referidos" (hoy es una acción manual del
  farmacéutico sobre Spec 004, no automatizada desde esta pantalla).
- FR-691 (avisos de inicio: faltantes previos a sesión, verificados sin entregar, sesiones a
  medias): no implementado; es una vista de panel de inicio, no bloquea el ciclo pedido.
- Prueba manual real (`dotnet run`) por el usuario — el ciclo completo de un paciente nunca se ha
  ejecutado en una app en ejecución real, solo en tests headless.

## 2026-09-06 (noche) — Pantalla "Preparaciones" global, avisos de inicio y enganches pendientes

- **FR-690** `PreparacionesWindow` desde la pantalla principal: todos los blísteres cruzando pacientes,
  filtro por paciente/estado y "solo pendientes", elaborador y verificador por fila, acceso a la
  preparación del paciente. Con la columna "envases al día" y el lote de Spec 007.
- **FR-691** Avisos de inicio en `MainWindow` (`ServicioAvisosInicio`): faltantes en el listado de
  retirada, verificados sin entregar con validez iniciada, sesiones a medias (>3 días) y días sin
  lectura ambiental (Spec 009 FR-950, umbral 7). Informativos; "Actualizar avisos" recalcula.
- **FR-663** real: la entrega tiene ahora sus campos (primera entrega, SPD anterior recogido,
  unidades no administradas, observaciones, "refiere cambios de medicación"); con cambios referidos
  los tratamientos en SPD pasan a pendiente de revisión (auditado) y se abre la comunicación al
  médico prerrellenada (Spec 008 FR-805).
- **FR-682** resuelto por el PNT I §4.4.1 (la hoja es "en cada entrega"): botón "Imprimir
  instrucciones de la sesión" — una hoja para los blísteres de la sesión si su contenido es idéntico,
  con el periodo de validez completo; si difieren, se imprime una por blíster.
