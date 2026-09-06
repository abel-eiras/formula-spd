# Implementation Plan: Rediseño de la interfaz — una sola ventana

**Branch**: `015-rediseno-interfaz` | **Date**: 2026-09-06 | **Spec**: [spec.md](./spec.md)

**Diseño aprobado**: [`docs/rediseno-interfaz.html`](../../docs/rediseno-interfaz.html) — maquetas de
inicio, espacio del paciente, preparación y listado, con la paleta y las tipografías.

---

## Summary

Cuatro fases independientes y fusionables por separado. Cada una parte de la anterior, deja la
aplicación usable y con todos los tests en verde, y termina con una prueba manual del propietario.

| Fase | Qué entrega | Esfuerzo | Ventanas al terminar |
|---|---|---|---|
| 1 | Marco único, navegación lateral y sistema visual | 1 sesión | 27 → **11** |
| 2 | Inicio accionable, tablas y búsqueda global | 1 sesión | 11 |
| 3 | Espacio del paciente con pestañas y paneles | 2 sesiones | 11 → **4** |
| 4 | Preparación como carril y blíster dibujado | 2 sesiones | 4 |

Las 4 ventanas finales son, por diseño: el marco de trabajo, el asistente de primer arranque, el inicio
de sesión y la petición de contraseña maestra (las tres últimas, previas a la sesión), más los diálogos
modales de aviso y confirmación, que no son ventanas de trabajo.

## Constitution Check

- **Art. II (el papel es la base legal)**: sin efecto. No cambia ningún documento generado ni ninguna
  firma; la interfaz solo cambia dónde se pulsa para generarlos.
- **Art. V.1 (la aplicación propone, el usuario confirma)**: reforzado. El inicio propone el trabajo del
  día y los selectores por nombre sustituyen a la introducción manual de identificadores.
- **Art. VIII.2 (stack fijado: Avalonia 11)**: se respeta. La única incorporación posible es
  `Avalonia.Controls.DataGrid` 11.3.20, del mismo fabricante y versión (research.md Decisión 4), y queda
  **sometida a decisión del propietario** con una alternativa sin dependencia.
- **Art. IX.4 (arranque < 2 s)**: vigilado con una prueba de aceptación propia en la fase 1 (CA-1504).
  Las vistas se construyen al navegar.
- **Art. XI (legibilidad)**: mejora. Desaparece la cascada de constructores de servicios entre ventanas
  (hoy añadir un servicio toca seis ficheros) sustituida por una fábrica explícita.
- **Sin violaciones.** Ninguna regla de negocio, entidad ni migración se toca (FR-1540).

## Technical Context

**Language/Version**: C# / .NET 8. **UI**: Avalonia 11.3.20 (+ `Avalonia.Controls.DataGrid` 11.3.20,
sujeto a Q4). **Storage**: ninguno; sin migración. **Testing**: xUnit + Avalonia.Headless.

**Punto de partida (verificado el 2026-09-06)**: 27 ventanas, 29 vistas ya separadas como `UserControl`,
27 ViewModels, 322 tests en verde (68 Dominio + 29 Presentación + 225 Aplicación), `ViewLocator` de la
plantilla presente y sin usar.

**Alcance por capas**: solo `Spd.Presentacion`, salvo dos ampliaciones de lectura en `Spd.Aplicacion`
—destino de un aviso (H2.2) y búsqueda global (H2.4)— que no añaden reglas.

---

# Fase 1 — Marco único y sistema visual

**Objetivo**: que la aplicación deje de ser una colección de ventanas y empiece a verse de la farmacia.

**Alcance**: las catorce secciones que hoy se abren desde la pantalla principal. Las ventanas de
paciente se siguen abriendo como hasta ahora (las absorbe la fase 3).

### Hitos

| Hito | Entregable | Ficheros principales |
|---|---|---|
| **H1.1** | **Tema**: paleta clara y oscura como recursos, tipografías IBM Plex embebidas, escala tipográfica y estilos de `Button` (primario/secundario/enlace), `TextBox`, `ComboBox`, `CheckBox`, `ListBox` y `TabControl` | `Estilos/Paleta.axaml`, `Estilos/Tipografia.axaml`, `Estilos/Controles.axaml`, `Assets/Fuentes/`, `App.axaml` |
| **H1.2** | **Componentes de estado**: `Pastilla` (variantes neutra, acento, apto, aviso, bloqueo), `FranjaSeveridad`, `BarraMensaje` (texto + "¿Por qué?") | `Controles/Pastilla.axaml(.cs)`, `Controles/FranjaSeveridad.axaml(.cs)`, `Controles/BarraMensaje.axaml(.cs)` |
| **H1.3** | **Marco**: ventana única con navegación lateral (secciones, grupos, contadores, usuario y cierre de sesión) y región de contenido resuelta por el `ViewLocator` existente | `Views/AppShell.axaml(.cs)`, `ViewModels/AppShellViewModel.cs`, `Navegacion/Navegador.cs`, `Navegacion/Seccion.cs` |
| **H1.4** | **Fábrica de ViewModels**: recibe los servicios una sola vez y construye cada ViewModel a demanda; `App.axaml.cs` deja de encadenar constructores | `FabricaViewModels.cs`, `App.axaml.cs` |
| **H1.5** | **Migración de 14 secciones** a la región de contenido y borrado de sus ventanas: Pacientes, Preparaciones, Retirada, Exportar, Catálogo, Revisión de nomenclátor, Calidad, Control documental, Farmacia, Usuarios, Actualizaciones, Nomenclátor, Seguridad, Perfiles | los 14 `*Window.axaml(.cs)` correspondientes se eliminan |
| **H1.6** | **Ayuda contextual por vista** (research.md Decisión 6) y **anfitrión de vistas para los tests** (Decisión 7); los 29 tests de Presentación migrados | `AyudaContextual.cs`, `tests/Spd.Presentacion.Tests/AnfitrionDeVista.cs` + los 24 ficheros de test |

### Pruebas de aceptación automáticas

| Test | Comprueba | CA |
|---|---|---|
| `AppShellTests.Navega_a_cada_seccion_registrada_sin_lanzar` | Recorre las 14 secciones cambiando el contenido y verifica que la vista resuelta es la esperada y que no se abre ninguna ventana adicional | CA-1500 |
| `AppShellTests.Las_secciones_de_administracion_solo_para_administrador` | Con un Elaborador, las secciones de administración no están en la navegación ni son alcanzables | CA-1500 |
| `AppShellTests.Los_contadores_reflejan_los_avisos` | Con tres pacientes con faltantes, la sección de retirada expone contador 3 | CA-1501 |
| `TemaTests.Toda_clave_de_color_existe_en_las_dos_variantes` | Recorre los recursos declarados y falla si alguno solo está definido en una variante (el error clásico de tema) | CA-1502 |
| `TemaTests.Las_fuentes_embebidas_se_resuelven` | `IBM Plex Sans` e `IBM Plex Mono` cargan desde `avares://`, sin caer en la fuente por defecto | — |
| `AyudaContextualTests` (actualizado) | Cada **vista** registrada tiene apartado de Procedimiento y de Uso, y las claves de ambas tablas coinciden | CA-1503 |
| Los 29 tests de Presentación existentes, migrados a `AnfitrionDeVista` | Cada vista se realiza sin lanzar; se conserva la regresión F5 | FR-1542 |

### Pruebas de aceptación manuales (propietario)

1. Iniciar sesión: se abre **una** ventana; el menú lateral muestra las secciones y los contadores.
2. Recorrer las catorce secciones: ninguna abre ventana nueva; "atrás" vuelve a la anterior.
3. Pulsar **F1** en cuatro secciones distintas: abre el apartado correcto.
4. Cambiar el tema del sistema a oscuro y volver a claro: la aplicación sigue legible en ambos, sin
   texto de un tema sobre fondo del otro.
5. Entrar como Elaborador: no aparecen las secciones de administración.
6. Mirar `logs/log-*.txt`: el arranque hasta login sigue por debajo de 2 s.
7. Impresión general: ¿se ve como las maquetas aprobadas?

### Definición de terminado

`dotnet build` sin errores ni avisos nuevos · 322+ tests en verde · las catorce ventanas migradas
eliminadas del repositorio · `PROGRESO.md` actualizado · fusionada a `main`.

### Riesgos

- **Los estilos propios rompen controles que hoy se ven por defecto.** Mitigación: el tema se aplica
  *sobre* `FluentTheme`, solo sobrescribiendo lo necesario; el recorrido manual de las 14 secciones (paso
  2) es precisamente la comprobación.
- **La migración de 24 ficheros de test es mecánica pero amplia.** Mitigación: `AnfitrionDeVista` reduce
  cada test a dos líneas; se migran de una vez y se ejecuta la batería completa.

---

# Fase 2 — Inicio accionable, tablas y búsqueda

**Objetivo**: que la primera pantalla diga qué hay que hacer hoy y que los listados se lean como tablas.

### Hitos

| Hito | Entregable | Ficheros principales |
|---|---|---|
| **H2.1** | **Panel de inicio**: cuatro indicadores (faltantes, verificados sin entregar, sesiones a medias, última lectura ambiental) y lista de avisos, sobre el `IServicioAvisosInicio` que ya existe | `ViewModels/InicioViewModel.cs`, `Views/InicioView.axaml(.cs)` |
| **H2.2** | **Avisos accionables**: `AvisoInicio` gana destino (sección + paciente); pulsarlo navega al depósito, la preparación o la sección correspondiente | `Spd.Aplicacion/IServicioAvisosInicio.cs`, `ServicioAvisosInicio.cs`, `Navegacion/` |
| **H2.3** | **Tablas** en Preparaciones, Retirada, Pacientes y Catálogo: columnas con cabecera, ordenación, selección múltiple donde ya existe, numeración tabular | las cuatro vistas + `Estilos/Tabla.axaml` |
| **H2.4** | **Búsqueda global** en la cabecera: paciente, medicamento y blíster; Enter navega al resultado | `ViewModels/BusquedaGlobalViewModel.cs`, `Spd.Aplicacion/IServicioBusquedaGlobal.cs` |
| **H2.5** | **Acciones rápidas** del inicio: nueva sesión de preparación, nuevo paciente, generar en lote | `InicioView.axaml` |

### Pruebas de aceptación automáticas

| Test | Comprueba | CA |
|---|---|---|
| `InicioViewModelTests.Muestra_los_cuatro_indicadores_y_sus_avisos` | Con datos que disparan los cuatro tipos de aviso, los indicadores y la lista salen con los valores esperados | CA-1510 |
| `InicioViewModelTests.Cada_aviso_lleva_su_destino` | El aviso de faltantes apunta al depósito de ese paciente; el de sin entregar, a su preparación; el ambiental, a calidad | CA-1511 |
| `ServicioBusquedaGlobalTests` | "lópez" → paciente; "F-000041" → blíster; "654321" → medicamento; sin tildes ni mayúsculas; sin resultados devuelve vacío, no error | CA-1513 |
| `PreparacionesViewModelTests.Ordenar_conserva_la_seleccion` | Con dos filas seleccionadas, reordenar por otra columna mantiene la selección para el lote | CA-1512 |
| `InicioViewTests` (headless) | La vista se realiza con avisos reales sin lanzar | FR-1542 |

### Pruebas de aceptación manuales

1. Con la base de un día real, comprobar que el inicio refleja lo que efectivamente está pendiente.
2. Pulsar cada tipo de aviso y verificar que se llega al sitio donde se resuelve.
3. Ordenar Preparaciones por paciente y por validez; comprobar alineación de columnas y cifras.
4. Seleccionar tres pacientes al día y lanzar el lote desde la propia tabla.
5. Buscar en la cabecera un paciente por apellido, un blíster por número y un medicamento por CN.

### Definición de terminado

322+ tests en verde · el inicio ya no es una columna de botones · las cuatro tablas ordenables ·
`PROGRESO.md` · fusionada a `main`.

### Riesgos

- **Q4 sin respuesta** (DataGrid). Mitigación: si no hay respuesta al llegar aquí, se implementa la
  alternativa sin dependencia (research.md Decisión 4) y se deja anotado; migrar después a DataGrid
  sería un cambio local a las cuatro vistas.

---

# Fase 3 — Espacio del paciente

**Objetivo**: el paciente se trabaja entero en una pantalla; ninguna acción suya abre ventana.

### Hitos

| Hito | Entregable | Ficheros principales |
|---|---|---|
| **H3.1** | **Cabecera fija del paciente**: nombre, ficha, estado, alergias, idoneidad y consentimiento, retirada, blísteres, médico | `Views/Pacientes/CabeceraPacienteView.axaml(.cs)` |
| **H3.2** | **`PacienteContexto`** (research.md Decisión 5): una sola carga del paciente, notificación a cabecera y pestañas; elimina el desfase actual de la ficha | `Pacientes/PacienteContexto.cs` |
| **H3.3** | **Espacio con siete pestañas** que alojan las vistas existentes: Datos, Idoneidad, Tratamiento, Depósito, Preparación, Comunicaciones, Documentos | `ViewModels/PacienteWorkspaceViewModel.cs`, `Views/Pacientes/PacienteWorkspaceView.axaml(.cs)` |
| **H3.4** | **Indicador de pendiente por pestaña** (sin idoneidad/consentimiento, faltantes, blísteres sin entregar, comunicaciones sin respuesta) | `PacienteWorkspaceViewModel` |
| **H3.5** | **Paneles laterales** en lugar de ventanas: registrar envase, alta de representante, respuesta del médico, importar tratamiento, revocación, reelaboración | las vistas afectadas + `Controles/PanelLateral.axaml(.cs)` |
| **H3.6** | **Borrado de las 7 ventanas de paciente** y del parche de refresco al cerrar (`FichaPacienteViewModel.RecargarPaciente`) | `FichaPacienteWindow`, `TratamientoWindow`, `DepositoWindow`, `ComunicacionesMedicoWindow`, `IdoneidadConsentimientoWindow`, `PreparacionWindow`, `ImportarTratamientoWindow` |
| **H3.7** | **Ayuda actualizada** (Spec 014 FR-1525): apartados de Uso reescritos a pestañas y paneles; tabla contextual por vista | `Ayuda/uso__030..080*.md` |

### Pruebas de aceptación automáticas

| Test | Comprueba | CA |
|---|---|---|
| `PacienteWorkspaceViewTests.Recorre_las_siete_pestanas_sin_lanzar` | Con un paciente real con tratamiento, envases y un SPD, cada pestaña se realiza (regresión F5 en las siete) | CA-1520 |
| `PacienteContextoTests.Activar_al_paciente_actualiza_la_cabecera` | Registrada la evaluación APTO y la firma, el contexto notifica y la cabecera pasa a ACTIVO sin recrear la pantalla | CA-1521 |
| `PacienteWorkspaceViewModelTests.Marca_las_pestanas_con_pendiente` | Paciente con faltantes ⇒ Depósito pendiente; sin consentimiento ⇒ Idoneidad pendiente; blíster verificado sin entregar ⇒ Preparación pendiente | CA-1522 |
| `PanelLateralTests.Guardar_cierra_el_panel_y_refresca_la_lista` | Registrar un envase desde una línea deja el panel cerrado y la línea con su envase | CA-1523 |
| `IndiceAyudaTests` (existente) | Sigue en verde con las tablas por vista y los textos actualizados | FR-1525 |

### Pruebas de aceptación manuales

1. Ciclo completo de un paciente nuevo —alta, idoneidad, consentimiento, tratamiento, depósito,
   preparación— **sin que se abra una sola ventana**.
2. Registrar la firma del consentimiento y comprobar que la cabecera pasa a ACTIVO **en el momento**.
3. Comprobar los indicadores de pendiente en las pestañas de un paciente con faltantes.
4. Abrir y cerrar tres paneles laterales distintos; verificar que la lista de detrás queda actualizada.
5. F1 en cuatro pestañas: abre el apartado correspondiente.

### Definición de terminado

322+ tests en verde · siete ventanas eliminadas · ayuda actualizada · `PROGRESO.md` · fusionada a `main`.

### Riesgos

- **Es la fase con más superficie**: siete vistas cambian de anfitrión a la vez. Mitigación: se ejecuta
  pestaña a pestaña (una vista migrada y probada antes de la siguiente), no las siete de golpe.
- **Pérdida del caso "dos pacientes a la vez"** (spec.md §7). Mitigación: se decide con la prueba manual;
  si hace falta, se añade "abrir en ventana aparte" como excepción explícita.

---

# Fase 4 — Preparación como carril y blíster dibujado

**Objetivo**: que la pantalla más usada muestre el proceso que sigue y el blíster que hay en la mesa.

**Requiere**: respuesta a Q2 (modelo real de dispositivo) antes de H4.2.

### Hitos

| Hito | Entregable | Ficheros principales |
|---|---|---|
| **H4.1** | **`CarrilPasos`**: cinco pasos con estado (completado, actual, bloqueado + motivo), derivado del estado del SPD y de sus datos; sin lógica nueva | `Controles/CarrilPasos.axaml(.cs)`, `ViewModels/PasoPreparacion.cs` |
| **H4.2** | **`RejillaAlveolos`**: días × tomas a partir de las líneas del SPD, con fracción e identificación del medicamento; número de tomas configurable (Q2) | `Controles/RejillaAlveolos.axaml(.cs)` |
| **H4.3** | **Verificación como panel del paso 4**: las ocho preguntas del Anexo I.G con su texto, **verificador por nombre**, motivo de excepción | `PreparacionView.axaml`, `PreparacionViewModel` |
| **H4.4** | **Entrega como panel del paso 5**: adherencia, "refiere cambios" con aviso de sus consecuencias (tratamiento a revisión + carta al médico) | idem |
| **H4.5** | **Selectores por nombre** en toda la aplicación: médico en tratamiento y comunicaciones (hoy campo numérico de id) | `TratamientoView`, `ComunicacionesMedicoView` |
| **H4.6** | **Impresión desde el carril**: ficha, etiquetas e instrucciones ofrecidas en el paso que corresponde | `PreparacionView.axaml` |

### Pruebas de aceptación automáticas

| Test | Comprueba | CA |
|---|---|---|
| `CarrilPasosTests.El_paso_actual_corresponde_al_estado_del_spd` | Para BORRADOR / PREPARADO / VERIFICADO / ENTREGADO / ANULADO, el paso actual, los completados y los bloqueados son los esperados | CA-1530 |
| `CarrilPasosTests.Los_pasos_bloqueados_dan_su_motivo` | Entrega bloqueada indica "requiere verificación"; llenado bloqueado sin material lo indica | CA-1530 |
| `RejillaAlveolosTests.Refleja_la_pauta_y_los_dias` | Pauta ½-0-1-0 todos los días ⇒ 7 alvéolos de desayuno con ½, 7 de cena con 1, ninguno en almuerzo ni noche; pauta con días parciales deja los días sin toma vacíos | CA-1531 |
| `RejillaAlveolosTests.Dos_lineas_comparten_alveolo` | Dos medicamentos en la misma toma aparecen ambos identificados en el mismo alvéolo | CA-1531 |
| `PreparacionViewTests` (ampliado) | La vista con carril y rejilla se realiza sin lanzar, con un SPD real de dos líneas | FR-1542 |
| `SinIdentificadoresEnLaInterfazTests` | Ningún `.axaml` contiene un control numérico enlazado a un `*Id` de usuario o médico | CA-1532 |

### Pruebas de aceptación manuales

1. Recorrer el ciclo completo de un paciente por el carril: el paso actual es siempre el correcto y los
   bloqueados explican por qué.
2. Comparar la rejilla en pantalla con el blíster real que se está llenando: ¿coinciden alvéolos?
3. Verificar eligiendo verificador **por nombre**; provocar la excepción verificador = elaborador y
   comprobar que el motivo es obligatorio y el aviso sale junto al botón.
4. Registrar una entrega con "refiere cambios" y comprobar que avisa de lo que va a pasar antes de
   hacerlo.
5. Imprimir ficha, etiquetas e instrucciones desde el carril.

### Definición de terminado

322+ tests en verde · sin campos de identificador numérico en la interfaz · `PROGRESO.md` · fusionada a
`main` · **prueba manual del ciclo completo por parte del propietario** (la que sigue pendiente desde el
principio del proyecto).

### Riesgos

- **Q2 sin respuesta**: la rejilla se construye parametrizada con el estándar 7 × 4 por defecto; adaptar
  a otro dispositivo es cambiar un valor de configuración, no el control.
- **La rejilla es la pieza con más diseño propio.** Mitigación: se construye como control aislado con
  sus tests de datos, y su aspecto se valida contra la maqueta aprobada antes de integrarla.

---

## Secuencia recomendada y puntos de decisión

```
Fase 1 ──► prueba manual ──► Fase 2 ──► prueba manual ──► Fase 3 ──► prueba manual ──► Fase 4 ──► prueba manual final
   ▲                                        ▲                                              ▲
   │                                        │                                              │
  Q3 (densidad)                            Q4 (DataGrid)                                  Q2 (dispositivo)
```

Cada flecha de "prueba manual" es un punto de parada real: el propietario prueba lo entregado antes de
que empiece la fase siguiente. Q1 (orden de las fases) se da por resuelto con esta secuencia salvo que
el propietario indique otra.
