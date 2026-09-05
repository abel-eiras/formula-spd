# Plan y progreso — Spec 005: Depósito de envases y listado de retirada

Este documento se actualiza al completar cada fase, cada test y cada prueba. No se reordena
retroactivamente lo ya marcado como hecho; solo se añade.

**Nota de contexto**: esta rama se bifurca de `main` tras fusionar Specs 001, 003, 009, 010 y 004
(2026-09-06) — todos los campos de parámetros que FR-500/501 pedían (`Farmacia.DiaRetiradaDefecto`,
`NBlisteresDefecto`, `DiasAntelacionListado`, `Paciente.DiaRetirada`, `NBlisteres`) ya existían
desde Specs 000/001: esta spec no necesitó ninguna migración para ellos, solo los lee. La única
migración nueva es `0006_envase.sql` (tablas `Envase` y `PerfilImportacionTratamiento`).

- **2026-09-06** — Ciclo completo `/speckit-specify` → `/speckit-plan` → `/speckit-tasks` →
  `/speckit-implement`, ejecutado de forma autónoma. FR-520/521 (descuento disparado por un SPD
  real) y FR-530 (exclusión por SPD ya preparado/verificado) dependen de la entidad SPD de Spec
  006, que no existe en esta rama — implementados como servicio/interfaz invocable con punto de
  extensión documentado (research.md Decisiones 3 y 5), no como lógica ficticia de preparación.
  Q1-Q3 del documento fuente resueltas con la propuesta única de cada una (spec.md, Clarifications).

- **Foundational**: `Envase` (estados `EnCustodia/Agotado/ResiduoSigre/EntregadoPaciente`, sin
  eliminación — Art. III), `PerfilImportacionTratamiento`, `IComprobadorCoberturaSpd` +
  `ComprobadorCoberturaSpdNulo` (punto de extensión para Spec 006), `CalculadoraUnidadesADescontar`
  y `CalculadoraProximaRetirada` como funciones puras en `Spd.Dominio`, sin acceso a datos.

- **User Story 1 (P1, MVP) — Registrar y consultar envases en custodia**: `ServicioEnvases`
  exige tratamiento activo `en_spd=1` (FR-515), bloquea serie duplicada en cualquier paciente
  (CA-504), avisa sin bloquear en caducidad pasada (FR-514). Pestaña "Depósito" en la ficha de
  paciente (`DepositoView`/`DepositoWindow`), accesible con el mismo patrón que "Tratamientos"
  (Spec 004): botón visible solo con paciente ya guardado (`PuedeAbrirDeposito`).

- **User Story 2 (P1) — Listado de retirada**: `ServicioListadoRetirada.ObtenerListado` calcula
  necesarias/disponibles/faltan/envases_a_retirar por paciente y medicamento (CA-500), respeta la
  ventana de antelación (CA-501), resuelve el DNI de retirada por el contacto marcado o el propio
  paciente (CA-514/515), y muestra "?" cuando `unidades_envase` es desconocido (CA-512). Reutiliza
  `IRepositorioPacientes.Buscar("", [Activo], null)` como "listar todos los activos" en vez de
  añadir un método nuevo al contrato de Spec 001 (Art. V). Pantalla "Retirada de envases" accesible
  desde un nuevo botón en `MainWindow`, con acción "Registrar envase" por fila.

- **User Story 3 (P2) — Sobrante en el descuento**: `CalculadoraUnidadesADescontar` implementa la
  regla de FR-522 ("entero más uno" siempre que haya fracción, incluso con suma entera — CA-507b);
  `ServicioAsignacionEnvases.Descontar` agota primero el envase con menos restantes (CA-505) y
  nunca descarta el sobrante (CA-506), persistido en BD real y probado sin ninguna pantalla de
  preparación (research.md Decisión 5, mismo patrón que Spec 010 probó `ServicioCifrado` antes de
  que existiera el flujo de arranque completo).

- **User Story 4 (P2) — Cese de tratamiento y bajas**: `ServicioEnvases` separa "proponer" (solo
  consulta) de "confirmar" (ejecuta) para SIGRE individual y masiva (FR-541/542, CA-508); ninguna
  operación del servicio devuelve un envase al stock ni lo reasigna (CA-509, verificado también por
  inspección de la interfaz). Diálogo de confirmación con selección de motivo en `DepositoView`.

- **User Story 5 (P3) — Medicación fuera de blíster**: `RegistrarEntregaFueraBlister` exige
  tratamiento activo `en_spd=0`, crea el envase directo en `EntregadoPaciente` sin pasar por
  `EnCustodia` (research.md Decisión 8), serie opcional (CA-510).

- **User Story 6 (P3) — Importación por pegado o fichero**: `ParserLineasImportacion` mapea
  columnas por índice (elegido en la vista previa, no por nombre de cabecera) para texto tabulado o
  CSV; solo CSV en esta iteración, sin dependencia nueva de Excel (research.md Decisión 7).
  `ServicioImportacionTratamientoEnvase` crea un tratamiento `PendienteRevision` cuando el CN no
  tiene uno activo (CA-517, reutilizando el estado ya previsto por Spec 004 en vez de añadir un
  flag nuevo) y separa filas con CN no encontrado (FR-575) o serie duplicada (FR-576, CA-518) sin
  detener el resto. Pantalla "Importar tratamiento" (pegado) accesible desde Depósito.

  `dotnet build` sin errores; **51 (Dominio) + 13 (Presentación) + 141 (Aplicación) = 205 tests en
  verde**. Las 66 tareas de [tasks.md](./tasks.md) están completas.

## Pendiente (documentado, no fabricado)

- FR-520/521 reales: Spec 006 debe invocar `IServicioAsignacionEnvases.Descontar` al pasar un SPD a
  PREPARADO; el servicio ya está listo y probado, solo falta el disparador real.
- FR-530/CA-502 real: Spec 006 debe registrar una implementación real de `IComprobadorCoberturaSpd`
  en el contenedor de DI (hoy usa `ComprobadorCoberturaSpdNulo`, que nunca excluye a nadie).
- FR-535/CA-513 real: Spec 007 debe generar el documento impreso; hoy solo existe el punto de
  extensión y el registro de auditoría `IMPRIMIR`.
- FR-572 perfiles compartidos con Spec 011: esta iteración solo implementa el perfil específico
  `PerfilImportacionTratamiento`, no el motor genérico de importación de Spec 011.
- Soporte real de fichero `.xlsx` (research.md Decisión 7): esta iteración solo soporta CSV.
- Prueba manual real (`dotnet run`) por el usuario — ninguna de las pantallas de esta spec se ha
  abierto en una app en ejecución real, solo en tests headless.
