# Contratos — Spec 015

## `Navegador` (Presentación)

- `Destino Actual { get; }` — sección visible en la región de contenido.
- `void Navegar(Destino destino)` — sustituye el contenido; apila el destino anterior. Si la sección es
  de administración y el usuario no lo es, no navega (misma regla de visibilidad que hoy).
- `bool PuedeVolver { get; }` / `void Atras()` — vuelve al destino anterior.
- `event Action<Destino> Navegado` — el shell reacciona pidiendo el ViewModel a la fábrica.

Contrato de resolución: el shell pone en la región un **ViewModel**; el `ViewLocator` existente
construye la vista por convención de nombre (`XViewModel` → `XView`). Toda vista debe conservar su
constructor sin parámetros.

## `FabricaViewModels` (Presentación)

Recibe los servicios de aplicación una sola vez en el arranque y expone un método por sección:
`CrearInicio()`, `CrearPacientes()`, `CrearPaciente(int pacienteId, string? pestana)`,
`CrearPreparaciones()`, `CrearRetirada()`, `CrearCatalogo()`, `CrearCalidad()`, `CrearFarmacia()`,
`CrearUsuarios()`, `CrearSeguridad()`, … Sustituye la cascada actual de constructores entre ventanas.

## `AyudaContextual` (modificado en la fase 1)

- `SeccionPorVista`: **nombre de vista** → apartado de Procedimiento (antes, nombre de ventana).
- `UsoPorVista`: nombre de vista → apartado de Uso.
- El shell resuelve F1 contra la vista activa; las claves de ambas tablas deben coincidir (test de
  Spec 014 FR-1412, actualizado).

## `PacienteContexto` (Presentación, fase 3)

- `Paciente Actual { get; }` — el paciente abierto.
- `void Recargar()` — relee del servicio y notifica.
- `event Action Cambiado` — cabecera y pestañas se suscriben.

Regla: cualquier ViewModel de pestaña que modifique al paciente (idoneidad, datos, estado) llama a
`Recargar()`; ninguno mantiene su propia copia del paciente.

## `IServicioBusquedaGlobal` (Aplicación, fase 2)

- `ResultadoBusquedaGlobal Buscar(string texto)` → listas de pacientes, medicamentos y blísteres
  coincidentes, cada elemento con su `Destino`. Consulta de solo lectura sobre repositorios existentes;
  normalización sin tildes ni mayúsculas con el `Normalizador` de Spec 001.

## Componentes de tema (fase 1)

- `Pastilla`: propiedades `Texto` y `Variante` (`Neutra | Acento | Apto | Aviso | Bloqueo`).
- `FranjaSeveridad`: `Severidad` (`Ninguna | Aviso | Bloqueo | Apto`).
- `BarraMensaje`: `Mensaje`, `ApartadoAyuda` (id del apartado que abre "¿Por qué?"), visible solo con
  mensaje.
- `PanelLateral` (fase 3): `Titulo`, `EstaAbierto`, contenido arbitrario; se cierra al completar la
  acción.
- `CarrilPasos` (fase 4): `Pasos` (lista de `PasoPreparacion`).
- `RejillaAlveolos` (fase 4): `Alveolos`, `TomasPorDia` (4 por defecto), `Dias` (7 por defecto).
