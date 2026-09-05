# Contratos — servicios de Aplicación de la Spec 009

Interfaz entre `Spd.Presentacion` y `Spd.Aplicacion`. Firmas en pseudo-C#; la implementación exacta
se decide en `/speckit-tasks`/`/speckit-implement`. Dos servicios, separados por permisos: uno para
el día a día (Elaborador/Administrador) y otro exclusivo de Administrador (FR-942).

## IServicioRegistrosCalidad

- `RegistroAmbiental RegistrarAmbiental(DatosRegistroAmbiental datos, int? usuarioQueEjecutaId)` —
  FR-900/FR-901/FR-902; calcula y congela `fuera_rango` (research.md Decisión 2, CA-900).
- `IReadOnlyList<RegistroAmbiental> ListarAmbiental()` — FR-900.
- `RegistroLimpieza RegistrarLimpieza(TipoLimpieza tipo, string? observaciones, int? usuarioQueEjecutaId)` —
  FR-910/FR-911; fecha/usuario a "ahora"/actual sin más pasos (CA-901).
- `IReadOnlyList<RegistroLimpieza> ListarLimpieza()` — FR-910.
- `FormacionPersonal RegistrarFormacion(DatosFormacion datos, int? usuarioQueEjecutaId)` —
  FR-920; nunca sustituye una formación anterior (FR-921, CA-902).
- `IReadOnlyList<FormacionPersonal> ListarFormacion(int usuarioId)` — FR-920.
- `RecogidaResiduos RegistrarRecogidaResiduos(DatosRecogidaResiduos datos, int? usuarioQueEjecutaId)` — FR-930.
- `IReadOnlyList<RecogidaResiduos> ListarRecogidaResiduos()` — FR-930.
- `ResultadoAvisoRegistros ComprobarAvisos()` — FR-950; compara con
  `Farmacia.umbral_dias_aviso_calidad` (research.md Decisión 3, CA-904). No bloquea nada, es
  informativo.

Usado por: el panel de inicio (avisos) y el menú de registros de esta spec, y (más adelante) la
pantalla de preparación de Spec 006 para los botones de limpieza de un clic (FR-911) — sin
necesitar cambios en este contrato.

## IServicioControlDocumental

- `ControlCambiosPNT RegistrarCambioPnt(DatosCambioPnt datos, int administradorQueEjecutaId)` —
  FR-940/FR-942; lanza `ErrorValidacionException` si el usuario no es Administrador (research.md
  Decisión 4, CA-903).
- `IReadOnlyList<ControlCambiosPNT> ListarCambiosPnt(int usuarioQueEjecutaId)` — FR-942; misma
  comprobación de rol también para consultar, no solo para escribir (CA-903 habla de "acceder").
- `ControlCopias RegistrarCopia(DatosCopia datos, int administradorQueEjecutaId)` — FR-941/FR-942.
- `IReadOnlyList<ControlCopias> ListarCopias(int usuarioQueEjecutaId)` — FR-942.

Usado por: la pantalla de control documental de esta spec, exclusiva de Administrador.
