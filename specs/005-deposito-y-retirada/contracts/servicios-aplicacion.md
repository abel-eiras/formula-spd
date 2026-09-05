# Contratos — servicios de Aplicación de la Spec 005

## IServicioEnvases

- `Envase RegistrarEnvase(DatosAltaEnvase datos, int? usuarioQueEjecutaId)` — FR-510–FR-514;
  exige tratamiento activo `en_spd=1` para ese medicamento/paciente (FR-515, lanza
  `ErrorValidacionException` si no existe, indicando que hay que crearlo primero); bloquea serie
  duplicada en cualquier paciente (FR-512, CA-504); avisa (sin bloquear) si `caducidad` es
  anterior a hoy (FR-514).
- `Envase RegistrarEntregaFueraBlister(DatosEntregaFueraBlister datos, int? usuarioQueEjecutaId)` —
  FR-543; solo medicamentos `en_spd=0` en el tratamiento del paciente; crea el envase directo en
  `ENTREGADO_PACIENTE`, serie/lote opcionales, sin pasar por `EN_CUSTODIA`.
- `IReadOnlyList<Envase> ListarEnCustodiaDePaciente(int pacienteId)` — FR-517.
- `IReadOnlyList<Envase> ListarHistoricoDePaciente(int pacienteId)` — FR-517 "mostrar histórico".
- `Envase DarSalidaSigre(int envaseId, MotivoSalidaEnvase motivo, string? motivoDetalle, int? usuarioQueEjecutaId)` —
  FR-540; exige confirmación explícita del llamador (la pantalla decide, el servicio no vuelve a
  preguntar); pasa a `RESIDUO_SIGRE` con `unidades_restantes` desechadas.
- `IReadOnlyList<Envase> ProponerSalidaSigrePorFinDeTratamiento(int pacienteId, int medicamentoId)` —
  FR-541; solo consulta, no ejecuta.
- `IReadOnlyList<Envase> ProponerSalidaSigreMasivaPorBaja(int pacienteId)` — FR-542; solo consulta.
- `IReadOnlyList<Envase> DarSalidaSigreMasiva(int pacienteId, MotivoSalidaEnvase motivo, int? usuarioQueEjecutaId)` —
  FR-542, tras confirmación del llamador.

## IServicioAsignacionEnvases

- `int Calcular(Tratamiento tratamiento)` en `CalculadoraUnidadesADescontar` (función pura, sin
  interfaz de servicio: FR-522, research.md Decisión 6).
- `ResultadoDescuento Descontar(int tratamientoId, int? unidadesOverride, DateOnly caducidadMinima, int? usuarioQueEjecutaId)` —
  FR-520/521; calcula unidades vía `CalculadoraUnidadesADescontar` salvo que `unidadesOverride` se
  indique (para pruebas o para cuando Spec 006 ya conoce `unidades_dosis` de la línea); asigna
  envases `EN_CUSTODIA` del medicamento/paciente del tratamiento ordenados por
  `unidades_restantes asc, caducidad asc`, filtra `caducidad >= caducidadMinima`; persiste en BD;
  audita. Lanza `ErrorValidacionException` si no hay unidades suficientes disponibles (Spec 006
  decidirá cómo mostrar `ENVASE_PENDIENTE`, aquí solo se informa vía el resultado).
  Usado por: Spec 006 al pasar un SPD a PREPARADO (research.md Decisión 5), sin cambios de firma.

## IServicioListadoRetirada

- `IReadOnlyList<FilaListadoRetirada> ObtenerListado(DateOnly fechaReferencia, FiltrosListadoRetirada filtros)` —
  FR-530–FR-537; agrupa por paciente, ordena por próxima retirada y apellidos (FR-532); cada fila
  incluye `EnvasesARetirar` nulo (mostrado como "?" en la UI) cuando `unidades_envase` es
  desconocido (FR-536, CA-512).
- `void RegistrarImpresion(int? usuarioQueEjecutaId)` — FR-535; solo auditoría (`IMPRIMIR`); la
  generación real del documento es de Spec 007 (fuera de esta iteración).
- Depende de `IComprobadorCoberturaSpd` (research.md Decisión 3) para FR-530/CA-502; la
  implementación registrada en esta iteración (`ComprobadorCoberturaSpdNulo`) nunca excluye a
  nadie por este motivo.

## IServicioImportacionTratamientoEnvase

- `ResultadoImportacion ImportarDesdePegado(int pacienteId, PerfilImportacionTratamiento perfil, string textoTabulado, int? usuarioQueEjecutaId)` —
  FR-570/FR-573–FR-577, origen PORTAPAPELES.
- `ResultadoImportacion ImportarDesdeFichero(int pacienteId, PerfilImportacionTratamiento perfil, string contenidoCsv, int? usuarioQueEjecutaId)` —
  mismas reglas, origen FICHERO; solo CSV en esta iteración (research.md Decisión 7).
- `PerfilImportacionTratamiento GuardarPerfil(string nombre, OrigenImportacionTratamiento origen, string? separador, bool? tieneCabecera, MapeoColumnasImportacion mapeo)` —
  FR-572.
- `IReadOnlyList<PerfilImportacionTratamiento> ListarPerfiles()`.

Usado por: la pantalla "Importar tratamiento" (nueva) accesible desde Depósito/Tratamiento del
paciente.
