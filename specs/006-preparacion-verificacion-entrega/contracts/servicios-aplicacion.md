# Contratos — servicios de Aplicación de la Spec 006

## IServicioPreparacion

- `IReadOnlyList<Spd> CrearSesion(int pacienteId, int elaboradorId)` — FR-600..604; valida
  paciente ACTIVO, `IComprobadorIdoneidadYConsentimiento` (research.md Decisión 1), tratamiento
  activo y sin faltantes (Spec 005); crea 1 o 2 SPD en `Borrador` con sus `SPD_Linea` (sin
  `SPD_Linea_Envase` todavía, research.md Decisión 2).
- `RegistroAmbiental ObtenerOCrearLecturaAmbiental(double? temperatura, double? humedad, int usuarioId)` —
  FR-630/631; si `temperatura`/`humedad` son null y hay una lectura de menos de
  `Farmacia.UmbralReutilizacionLecturaAmbientalHoras`, la reutiliza; si no, crea una nueva.
- `void AsignarMaterial(int spdId, int materialId)` — FR-632.
- `void ExcluirLinea(int lineaId, string motivo, int usuarioId)` — FR-614.
- `Envase RegistrarEnvaseDesdeLinea(int lineaId, DatosAltaEnvase datos, int? usuarioId)` — FR-620;
  delega en `IServicioEnvases.RegistrarEnvase` (Spec 005) sin cambiar su contrato.
- `void PasarAPreparado(int spdId, int registroAmbientalId, int usuarioId)` — FR-612/CA-603/604;
  valida saldo de todas las líneas no excluidas (research.md Decisión 3) antes de descontar
  ninguna; descuenta vía `IServicioAsignacionEnvases.Descontar` (Spec 005) línea a línea y crea
  `SPD_Linea_Envase`.
- `Spd Verificar(int spdId, int verificadorId, ChecklistVerificacion checklist, string? excepcionMotivo, int usuarioId)` —
  FR-650..653; exige `excepcionMotivo` (≥10 caracteres) si `verificadorId == spd.ElaboradorId`.
- `IReadOnlyList<Spd> RegistrarEntrega(DatosEntrega datos, IReadOnlyList<int> spdIds, int usuarioId)` —
  FR-660..663.
- `IReadOnlyList<Spd> PrepararSiguiente(int pacienteId, int elaboradorId, out ResultadoContinuidad resultado)` —
  FR-670..674 (research.md Decisión 5).
- `Spd Reelaborar(int spdId, OrigenSolicitudReelaboracion origen, string motivo, IReadOnlyList<DatosLineaReelaborada> lineasNuevas, int usuarioId)` —
  FR-6120..6127.
- `void RegistrarImpresion(int spdId, TipoDocumentoSpd tipo, int? usuarioId)` — FR-680/681
  (research.md Decisión 8); solo timestamp + auditoría, sin generar fichero.
- `void Anular(int spdId, string motivo, int usuarioId)`.
- `IReadOnlyList<Spd> ListarPorFiltro(FiltrosPreparaciones filtros)` — FR-690.

Usado por: la pantalla de sesión de preparación (nueva) y, más adelante, Spec 007 (impresión real
sobre `RegistrarImpresion` sin cambiar la firma) y Spec 008 (ya construida, consumida desde aquí
para FR-663 si se decide enganchar la UI).
