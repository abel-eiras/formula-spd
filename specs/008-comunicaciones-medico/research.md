# Research: Comunicaciones al médico

## Decisión 1 — Sin migración de parámetros nuevos

`ComunicacionMedico` ya está diseñada en `docs/data-model.md` §ComunicacionMedico:
`paciente_id, medico_id, tipo (PRESENTACION/INCIDENCIA/TELEFONO), fecha, incidencias_detectadas,
propuesta, respuesta, fecha_respuesta, farmaceutico_id`. Esta spec crea la tabla y la entidad tal
cual; no toca ninguna otra tabla existente.

## Decisión 2 — Punto de extensión para el aviso de Spec 006 (FR-805)

Sin la pantalla de entrega (Spec 006), no existe un "aviso de tratamiento pendiente de revisión"
real que abrir. Se define `IServicioComunicacionesMedico.PrepararDesdeAvisoCambioReferido(int
pacienteId, int medicoId)`, que recibe directamente los datos ya resueltos (paciente y médico de
cabecera) y devuelve un `DatosAltaComunicacionMedico` listo para pasar a `Crear` — no persiste
nada por sí mismo (FR-807: no hay borradores). Spec 006 invocará este mismo método desde su propio
aviso sin cambiar la firma pública — mismo patrón que Spec 005 aplicó con
`IComprobadorCoberturaSpd` para su punto de extensión hacia Spec 006.

## Decisión 3 — FR-806 como dato expuesto, no como acción

Sin Spec 007 (impresión), no hay documento real que generar. Se añade `ComunicacionMedico.EsImprimible`
(propiedad calculada: `Tipo != Telefono`) para que la futura pantalla de impresión sepa qué
comunicaciones ofrecer, sin que esta spec genere ningún documento. Alternativa descartada: no
exponer nada y dejarlo íntegramente a Spec 007 — se prefiere exponer el dato ya resuelto porque
es una regla de negocio de esta spec (FR-806), no de la maquetación.

## Decisión 4 — Inmutabilidad salvo respuesta (FR-807)

Igual que Spec 004 (Tratamiento) separa "campos no clínicos editables en el sitio" de "cambios que
cierran fila", `ServicioComunicacionesMedico` distingue `Crear` (inmutable tras guardarse) de
`RegistrarRespuesta` (el único UPDATE permitido: `Respuesta`, `FechaRespuesta`). No hay ninguna
otra operación de escritura.
