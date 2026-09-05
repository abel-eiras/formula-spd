# Contratos — servicios de Aplicación de la Spec 010 (solo 4.1 Backup + 4.2 Cifrado)

Interfaz entre `Spd.Presentacion` y `Spd.Aplicacion`. Firmas en pseudo-C#; la implementación exacta
se decide en `/speckit-tasks`/`/speckit-implement`.

## IServicioBackup

- `ResultadoBackup GenerarBackup(int? usuarioQueEjecutaId, bool esAutomatico)` — FR-1000/FR-1003;
  `VACUUM INTO` + zip + copia a `Farmacia.RutaBackup`, aplica rotación (FR-1002) tras copiar. Usado
  tanto al cerrar la aplicación (`esAutomatico = true`, `usuarioQueEjecutaId` el de la sesión en
  curso si hay una, puede ser `null` si se cierra desde la pantalla de login) como bajo demanda
  desde Configuración (`esAutomatico = false`, siempre con un Administrador identificado) — mismo
  método, el llamador decide cuándo y con qué atribución; `esAutomatico` solo cambia la acción
  registrada en auditoría (`BACKUP_AUTOMATICO`/`BACKUP_MANUAL`).
- `IReadOnlyList<BackupInfo> ListarBackups()` — para mostrar el histórico y validar CA-1002.
- `ResultadoRestauracion RestaurarDesdeZip(string rutaZip, string carpetaDestino, int administradorQueEjecutaId)` —
  FR-1004, asistente de restauración; descomprime sobre una instalación nueva. Recibe el id del
  administrador para dejar traza en auditoría (Art. VII.6 — hallazgo de `/speckit-analyze`: sin
  este parámetro la restauración quedaría sin atribuir a nadie). No se necesita para la
  restauración "copiar la carpeta a mano" (Art. VI.4), que no pasa por ningún servicio.

## IServicioCifrado

- `bool EstaActivo()` — Decisión 2 de research.md: intenta abrir sin clave, no lee ningún flag
  guardado.
- `ResultadoActivarCifrado ActivarCifrado(string contrasenaMaestra, int administradorQueEjecutaId)` —
  FR-1010; genera MEK + frase de recuperación de 24 palabras, devuelve la frase en el resultado
  para mostrarla una vez (CA-1003 exige confirmación de impresión ANTES de que este método se
  invoque de verdad — la Presentación se encarga de esa confirmación previa, este método ya asume
  que se confirmó).
- `void DesactivarCifrado(string contrasenaMaestraActual, int administradorQueEjecutaId)` — FR-1011.
- `ResultadoCambioContrasena CambiarContrasenaMaestra(string contrasenaActual, string contrasenaNueva, int administradorQueEjecutaId)` —
  FR-1012; re-envuelve la MEK, no toca `PRAGMA rekey` (Decisión 3 de research.md aclara que esta
  operación es distinta de un rekey real de la base de datos).
- `string ClaveActualParaConexion(string contrasenaMaestra)` — desenvuelve la MEK a partir de la
  contraseña (o, con un parámetro alternativo, de la frase de recuperación) para pasarla a
  `PRAGMA key` al arrancar (FR-1013); nunca se guarda el resultado más allá de la sesión en curso.

Usado por: el asistente de arranque (pedir contraseña maestra si `EstaActivo()`), y una nueva
pantalla de Configuración / Seguridad para activar, desactivar y cambiar la contraseña.
