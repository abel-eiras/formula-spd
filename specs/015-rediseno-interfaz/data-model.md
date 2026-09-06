# Fase 1 — Modelo de datos: Rediseño de la interfaz

**Sin migración y sin entidades de negocio nuevas** (spec.md §5, FR-1540). Esta spec no crea, modifica
ni elimina ninguna tabla. Lo que sigue son tipos de presentación, todos en `Spd.Presentacion` salvo los
dos marcados.

## Navegación (`Spd.Presentacion/Navegacion`)

| Tipo | Contenido | Notas |
|---|---|---|
| `Seccion` (enum) | `Inicio, Pacientes, Paciente, Preparaciones, Retirada, Exportar, Catalogo, RevisionNomenclator, Calidad, ControlDocumental, Farmacia, Usuarios, Actualizaciones, Nomenclator, Seguridad, Perfiles, Ayuda` | Cada valor tiene su entrada en la navegación y su apartado de ayuda |
| `Destino` (record) | `Seccion Seccion, int? PacienteId, string? Pestana` | Un aviso o un resultado de búsqueda apunta a un destino |
| `Navegador` | `Actual`, `Navegar(Destino)`, `Atras()`, `Historial` | Notifica al shell; el `ViewLocator` resuelve la vista del ViewModel |
| `EntradaNavegacion` (record) | `Seccion Seccion, string Titulo, string Grupo, bool SoloAdministrador, int? Contador` | Lo que pinta el menú lateral |

## Estado de la interfaz

| Tipo | Contenido | Notas |
|---|---|---|
| `PacienteContexto` | `Paciente Actual`, `Recargar()`, evento de cambio | research.md Decisión 5; una carga por paciente abierto, muchos suscriptores |
| `EstadoPestana` (record) | `string Titulo, bool Pendiente, string? MotivoPendiente` | FR-1522 |
| `PasoPreparacion` (record) | `int Numero, string Titulo, string Detalle, EstadoPaso Estado, string? MotivoBloqueo` | FR-1530; se deriva del `SPD`, no se persiste |
| `EstadoPaso` (enum) | `Completado, Actual, Bloqueado` | |
| `Alveolo` (record) | `int Dia, Toma Toma, IReadOnlyList<ContenidoAlveolo> Contenido` | FR-1531; se calcula de `SpdLinea`, no se persiste |
| `ContenidoAlveolo` (record) | `string Inicial, string Fraccion, string MedicamentoNombre` | La fracción se toma de `FraccionDosis.Texto()` (FR-740: nunca decimal) |

## Ampliaciones de lectura en `Spd.Aplicacion` (sin reglas nuevas)

| Tipo | Cambio | Fase |
|---|---|---|
| `AvisoInicio` (record existente) | Gana `Destino` (sección + paciente + pestaña) para que el aviso sea accionable | 2 (H2.2) |
| `IServicioBusquedaGlobal` (nuevo) | `Buscar(string)` → pacientes, medicamentos y blísteres coincidentes; consulta pura sobre repositorios existentes, con `Normalizador` de Spec 001 | 2 (H2.4) |

## Recursos del tema (`Spd.Presentacion/Estilos`)

Claves de color, todas definidas en la variante clara y en la oscura (CA-1502):

`Fondo`, `Superficie`, `Tinta`, `TintaSuave`, `Linea`, `Acento`, `AcentoSuave`, `Apto`, `AptoSuave`,
`Aviso`, `AvisoSuave`, `Bloqueo`, `BloqueoSuave`, `Sombra`.

Tipografías: `FuenteInterfaz` (IBM Plex Sans) y `FuenteDatos` (IBM Plex Mono), embebidas en
`Assets/Fuentes/` (research.md Decisión 3).
