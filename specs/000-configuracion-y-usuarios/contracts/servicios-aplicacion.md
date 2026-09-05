# Contratos — servicios de Aplicación de la Spec 000

Esta app no expone API externa (Art. VI: sin red salvo dos excepciones). El "contrato" relevante es
la interfaz entre `Spd.Presentacion` y `Spd.Aplicacion`: lo que las vistas/ViewModels pueden invocar,
y lo que otras specs (001, 002, 003, 005, 006, 007, 009, 010, 011) pueden reutilizar de esta.

Firmas en pseudo-C# — la implementación exacta (async/await, DTOs concretos) se decide en
`/speckit-tasks` e `/speckit-implement`, no aquí.

## IServicioAsistentePrimerArranque

- `bool HayConfiguracionInicial()` — comprueba si existe base de datos/fila Farmacia (FR-000).
- `void EjecutarPaso(PasoAsistente paso, DatosPaso datos)` — valida y guarda cada paso; lanza
  `ErrorValidacionException` si faltan campos obligatorios del paso (FR-001).
- `void FinalizarAsistente()` — solo permitido tras completar los 5 pasos en orden (FR-000).

Usado por: `Spd.Presentacion` (pantalla de arranque). No lo consume ninguna otra spec.

## IServicioConfiguracionFarmacia

- `Farmacia ObtenerConfiguracion()`
- `void ActualizarDatosFarmacia(DatosFarmacia datos)` — FR-010/FR-011/FR-012.
- `ResultadoValidacionRuta ValidarRuta(string ruta)` — usado por FR-030/FR-031; devuelve si
  existe, es escribible, y si coincide con la carpeta de instalación (FR-032, solo aviso).
- `void ActualizarPrefijos(string prefijoFicha, string prefijoSpd)` — FR-013; no reescribe
  numeraciones ya asignadas (CA-001).
- `void ActualizarValoresDefecto(ValoresDefecto valores)` — FR-020/021/022; no reescribe entidades
  ya personalizadas (CA-006).

Usado por: Spec 001 (`prefijo_num_ficha`, `dia_retirada_defecto`, `n_blisteres_defecto`), Spec 002
(datos de responsable/ARCO), Spec 005 (`dias_antelacion_listado`), Spec 006/009 (rangos
ambientales, umbral de reutilización), Spec 007 (`ruta_documentos_generados`), Spec 010
(`ruta_backup`).

## IServicioUsuarios

- `Usuario CrearUsuario(DatosAltaUsuario datos)` — FR-040/FR-041; genera o acepta contraseña
  provisional, marca `debe_cambiar_password=1`.
- `void DarDeBaja(int usuarioId, int usuarioQueEjecuta)` — FR-043; lanza
  `UltimoAdministradorException` si viola FR-042/CA-002.
- `void CambiarPassword(int usuarioId, string passwordNueva)` — FR-044 (cambio propio).
- `void ResetearPassword(int usuarioId, int administradorId)` — FR-044 (reseteo por Administrador).
- `void RegistrarIntentoLogin(string login, bool exito)` — FR-045; incrementa/resetea
  `intentos_fallidos_consecutivos`, bloquea al llegar al umbral, registra `LOGIN_FALLIDO` en
  auditoría.
- `void DesbloquearUsuario(int usuarioId, int administradorId)` — FR-045; solo rol Administrador.

Usado por: toda spec que registre autoría de acciones (005, 006, 007, 008, 009) consulta
`Usuario.activo`/nombre para mostrar el histórico (CA-003), sin volver a dar de baja ni reactivar.

## IServicioActualizaciones

- `ResultadoComprobacion ComprobarActualizaciones()` — FR-050; llama a GitHub Releases
  (Decisión 3 de `research.md`), nunca automático (Art. VI.3).
- `void DescargarActualizacion(string urlAsset)` — FR-050.

Usado por: solo Spec 000 (pantalla Configuración/Actualizaciones).

## IServicioNomenclator

- `ResultadoDescarga DescargarNomenclator()` — FR-051/052; descarga desde `url_nomenclator`,
  devuelve éxito/fecha o motivo de fallo (CA-005). No parsea el contenido.

Usado por: Spec 003/011 (consumen el fichero descargado, no este servicio).
