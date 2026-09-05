# data-model.md — Modelo de datos consolidado

**Versión 0.5 — 5 de septiembre de 2026**
Sustituye al §3.3 del documento de Fase 1 en los puntos donde las specs 001/005/006 lo han corregido. Es la referencia que citan las specs; cuando una spec y este documento difieran, gana este documento y se corrige la spec.

Convenciones: `id` INTEGER PRIMARY KEY; fechas ISO-8601 en TEXT; booleanos INTEGER 0/1; toda tabla de negocio tiene además `creado_en`, `creado_por`, `modificado_en`, `modificado_por` (omitidos abajo).

---

## Cambios de esta versión sobre la Fase 1

1. **Paciente**: añadidos `sexo`, `dia_retirada`, `n_blisteres`.
2. **Farmacia**: añadidos `dia_retirada_defecto`, `n_blisteres_defecto`, `dias_antelacion_listado`; eliminado `validez_maxima_dias` (la validez de un blíster es siempre 7 días fijos, ya no configurable).
3. **Medicamento**: añadida regla de origen para `unidades_envase` (manual o regex sobre columna del nomenclátor importado).
4. **SPD**: eliminado el concepto de validez variable ≤14 días; pasa a ser siempre 7 días. Añadido `sesion_id` (GUID) para agrupar los blísteres generados juntos, sin que sea una entidad de negocio.
5. **SPD_Linea_Envase**: dejó de ser la tabla-excepción para repartos ocasionales; es ahora la forma normal de relacionar una línea con 1..n envases.
6. **Envase**: aclarado el ciclo de estados con `RESIDUO_SIGRE` y `ENTREGADO_PACIENTE` en vez de un genérico `RETIRADO`; regla de consumo "se agota primero el envase con menos unidades restantes".
7. **Rol**: simplificado a `ADMINISTRADOR` y `ELABORADOR`. Eliminados Titular/Farmacéutico/Técnico y el concepto de supervisor: la app no modela categoría profesional; la firma en papel es la decisión profesional, fuera de la aplicación.
8. **Farmacia**: sustituido el código de colegio provincial por un campo genérico `codigo_sanitario` (siglas/código de la farmacia, válido en cualquier provincia); añadidos `logo`, `cif`, `whatsapp`, `titular_o_comunidad_bienes`.
9. **Contacto**: añadido el flag `retira_medicacion` para identificar a la persona responsable de retirar el SPD, cuyo DNI alimenta el listado de retirada (Spec 005).
10. Nueva tabla **PerfilImportacionTratamiento** para la importación de tratamientos por copiar/pegar o fichero (CN, serie, lote, caducidad).
11. **Envase.unidades_iniciales**: aclarado que representa las unidades presentes en el momento del alta, no necesariamente un envase completo — puede darse de alta ya empezado.
12. **Tratamiento**: añadido `ajuste_unidades_manual` (override editable del cálculo de unidades a descontar del envase por semana).
13. **SPD**: añadido `version` (entero, empieza en 1, se incrementa en cada reelaboración antes de entregar).
14. Nueva tabla **SPD_Modificacion**: historial de reelaboraciones de un SPD no entregado (Constitución Artículo III.4).
15. Regla de consumo de fraccionables corregida (Spec 005 FR-522 v2): "entero más uno", no ceil() de la suma semanal.
16. **Farmacia**: añadidos `ruta_documentos_generados` (Spec 000 FR-031), `url_nomenclator` (Spec 000 FR-051) y `umbral_reutilizacion_lectura_ambiental_horas` (Spec 000 FR-022, por defecto 2).
17. **Usuario**: añadidos `intentos_fallidos_consecutivos` y `bloqueado` (Spec 000 FR-045).

---

## Farmacia (configuración, una sola fila)

| Campo | Tipo | Notas |
|---|---|---|
| **codigo_sanitario** | TEXT | Siglas o código sanitario de la farmacia, formato libre — sustituye al antiguo código de colegio provincial para que la app valga en cualquier provincia gallega |
| **logo** | BLOB/ruta | Imagen para cabecera de documentos |
| nombre | TEXT | Nombre comercial de la farmacia |
| **titular_o_comunidad_bienes** | TEXT | Nombre del titular o de la comunidad de bienes |
| **cif** | TEXT | |
| titular_colegiado | TEXT | Nº colegiado del titular, opcional, para impresión en documentos que lo requieran |
| direccion, cp, poblacion, provincia | TEXT | |
| telefono, fax, email | TEXT | |
| **whatsapp** | TEXT | |
| responsable_datos, direccion_derechos, email_derechos | TEXT | Para el consentimiento |
| prefijo_num_ficha, prefijo_num_spd | TEXT | |
| ruta_backup | TEXT | |
| **ruta_documentos_generados** | TEXT | Carpeta de salida de todo documento generado (Spec 000 FR-031, Spec 007). Misma validación de escritura que `ruta_backup` |
| **url_nomenclator** | TEXT | URL editable desde donde se descarga el fichero Excel/CSV del nomenclátor (Spec 000 FR-051, Spec 003/011) |
| **umbral_reutilizacion_lectura_ambiental_horas** | INTEGER | Por defecto 2 (Spec 000 FR-022, Spec 006 FR-630) |
| temp_min, temp_max, hr_min, hr_max | REAL | Por defecto 15/25/40/60 |
| **dia_retirada_defecto** | TEXT | LU/MA/MI/JU/VI/SA/DO — valor por defecto para pacientes nuevos |
| **n_blisteres_defecto** | INTEGER | 1 o 2 |
| **dias_antelacion_listado** | INTEGER | Días antes del día de retirada en que un paciente entra en el listado de retirada. Por defecto 2 |

## Rol
`ADMINISTRADOR`, `ELABORADOR`. La app no modela categoría profesional (titular/farmacéutico/técnico); ver Constitución Artículo VII.

## Usuario

| Campo | Tipo | Notas |
|---|---|---|
| nombre, apellidos, login, hash_password (Argon2id) | | |
| rol | TEXT | ADMINISTRADOR / ELABORADOR |
| cargo_pnt, colegiado, firma_abreviada | | Texto libre para lo que se imprime en registros de calidad (Anexo III firmas reconocidas); no condiciona permisos |
| activo, fecha_baja, debe_cambiar_password | | |
| **intentos_fallidos_consecutivos** | INTEGER | Por defecto 0; se resetea a 0 en login correcto (Spec 000 FR-045) |
| **bloqueado** | INTEGER | 0/1; se pone a 1 al llegar al umbral de intentos fallidos (por defecto 5); solo un Administrador lo pone a 0 (Spec 000 FR-045, sin expiración automática) |

## Medico (catálogo)

| Campo | Tipo | Notas |
|---|---|---|
| nombre, apellidos, colegiado, especialidad, centro | TEXT | |
| telefono, email, direccion | TEXT | |
| activo | | |

Índice de búsqueda sobre `apellidos || ' ' || nombre` normalizado sin tildes.

## Medicamento (catálogo, clave natural = CN)

| Campo | Tipo | Notas |
|---|---|---|
| cn | TEXT UNIQUE | 6 dígitos |
| nombre, principio_activo, laboratorio | TEXT | |
| forma_farmaceutica | TEXT | Catálogo cerrado |
| apto_spd, motivo_no_apto | | |
| fraccionable | INTEGER | |
| unidades_envase | INTEGER | **Origen:** MANUAL o IMPORTADO_REGEX (extraído de una columna del nomenclátor mediante un patrón configurable); siempre editable a mano |
| unidades_envase_origen | TEXT | `MANUAL` / `IMPORTADO_REGEX` — trazabilidad de cómo se rellenó, no bloquea edición |
| desc_forma, desc_color, desc_ranura, desc_serigrafia, desc_tamano | TEXT | |
| desc_texto | TEXT | Autogenerado, editable |
| gtin | TEXT | |
| activo | | |

### Medicamento_Hist
Copia de las columnas `desc_*` + `vigente_desde`, una fila por cambio.

## Paciente

| Campo | Tipo | Notas |
|---|---|---|
| num_ficha | TEXT UNIQUE | Autogenerado, no reutilizable |
| fecha_alta_ficha | TEXT | |
| nombre, apellidos | TEXT | |
| **sexo** | TEXT | `M` (mujer) / `H` (hombre). Obligatorio si se informa CIP |
| dni | TEXT | |
| fecha_nacimiento | TEXT | |
| num_ss, cip | TEXT | |
| direccion, cp, poblacion, telefono1, telefono2, email | TEXT | |
| medico_id | INTEGER | FK Medico |
| enfermedades_cronicas, alergias, observaciones | TEXT | |
| pictograma_comidas | INTEGER | |
| identificador_visual | TEXT | |
| **dia_retirada** | TEXT | LU/MA/MI/JU/VI/SA/DO. Prerrellenado desde Farmacia.dia_retirada_defecto |
| **n_blisteres** | INTEGER | 1 o 2. Prerrellenado desde Farmacia.n_blisteres_defecto |
| estado | TEXT | EVALUACION, ACTIVO, SUSPENDIDO, BAJA |
| fecha_baja, motivo_baja | | |

## Contacto (0..n por paciente)
paciente_id, tipo (FAMILIAR/REPRESENTANTE_LEGAL/PERSONA_AUTORIZADA/CUIDADOR), nombre, apellidos, dni, telefono, email, es_principal, **retira_medicacion** (INTEGER — marca a la persona cuyo DNI se usa en el listado de retirada, Spec 005; una misma persona con DNI puede estar vinculada como contacto en varios pacientes), activo.

## EvaluacionIdoneidad
paciente_id, fecha, farmaceutico_id, criterio_1…7, observaciones, resultado.

## Consentimiento
paciente_id, tipo (PACIENTE/REPRESENTANTE), contacto_id, fecha_firma, fecha_revocacion, impreso_en.

## Tratamiento (inmutable — un cambio cierra la fila y abre otra)

| Campo | Tipo | Notas |
|---|---|---|
| paciente_id, medicamento_id | FK | |
| en_spd | INTEGER | |
| problema_salud | TEXT | |
| medico_id | FK | Prescriptor, prerrellenado con el médico de cabecera |
| pauta_d, pauta_a, pauta_c, pauta_n | REAL | Admite fracciones; el valor introducido pertenece a un vocabulario cerrado (0, 1/4, 1/3, 1/2, 2/3, 3/4, 1, 1 1/4…) definido en Spec 004, para que la impresión en fracción (Spec 007 FR-741) sea exacta, no una aproximación de decimal |
| dias_semana | TEXT | Máscara "LMXJVSD" |
| pauta_texto, via, momento | | |
| fecha_inicio, fecha_fin | | |
| fecha_prescripcion_inicial, fecha_ultima_modificacion | | |
| tipo | TEXT | CRONICO/ESPORADICO |
| conocimiento_cumplimiento, incidencias, intervencion | TEXT | |
| estado | TEXT | ACTIVO/SUSPENDIDO/FINALIZADO/PENDIENTE_REVISION |
| **ajuste_unidades_manual** | INTEGER | Nulo = usar el cálculo automático (Spec 005 FR-522: "entero más uno" para pautas fraccionadas). Si se informa, sustituye el resultado calculado para las unidades a descontar del envase por semana; editable desde la ficha de tratamiento del paciente, con motivo opcional |

## Envase (custodia identificada por paciente)

| Campo | Tipo | Notas |
|---|---|---|
| paciente_id, medicamento_id | FK | Nunca se reasigna a otro paciente |
| serie | TEXT UNIQUE | Un envase físico = una serie, en todo el sistema |
| lote, caducidad | | |
| unidades_iniciales, unidades_restantes | INTEGER | `unidades_iniciales` = unidades presentes **en el momento del alta**, no necesariamente el envase completo — un envase puede darse de alta ya empezado |
| fecha_entrada, origen (ESCANEADO/MANUAL/IMPORTADO) | | |
| **estado** | TEXT | `EN_CUSTODIA` → `AGOTADO` \| `RESIDUO_SIGRE` \| `ENTREGADO_PACIENTE`. Sin estado "devuelto al stock": no existe esa operación |
| fecha_salida, motivo_salida | TEXT | Motivo: CESE_TRATAMIENTO, CAMBIO_TRATAMIENTO, CADUCADO, DETERIORADO, FALLECIMIENTO, BAJA_PACIENTE, OTRO |

**Regla de consumo:** al descontar de una línea, se ordena por `unidades_restantes ASC, caducidad ASC` — se agota primero el envase ya empezado. El sobrante nunca se descarta mientras el tratamiento siga activo.

## MaterialAcondicionamiento (catálogo)
descripcion, lote, fecha_entrada, activo.

## SPD (cabecera — siempre un blíster, nunca una hoja para dos)

| Campo | Tipo | Notas |
|---|---|---|
| num_registro | TEXT UNIQUE | Correlativo, propio de cada blíster |
| paciente_id | FK | |
| **version** | INTEGER | Empieza en 1; se incrementa en cada reelaboración antes de entregar (Constitución Artículo III.4, Spec 006 §4.12) |
| **sesion_id** | TEXT (GUID) | Agrupa los SPD creados juntos en una sesión de preparación. No es una FK a una tabla de negocio |
| validez_desde, validez_hasta | TEXT | Siempre 7 días exactos, consecutivos entre blísteres de la misma sesión |
| fecha_preparacion | | |
| material_id | FK | |
| registro_ambiental_id | FK | Puede compartirse entre los SPD de una misma sesión |
| elaborador_id | FK Usuario | Cualquier usuario ELABORADOR |
| verificador_id, fecha_verificacion, excepcion_verificador | | Por blíster, nunca compartido; verificador_id ≠ elaborador_id salvo excepción con motivo |
| resultado_verificacion | | |
| entregador_id, fecha_entrega, entregado_a | | |
| primera_entrega, spd_anterior_recogido, unidades_no_administradas, observaciones_adherencia | | |
| cambios_medicacion_preguntado, observaciones_etiqueta | | |
| estado | TEXT | BORRADOR → PREPARADO → VERIFICADO → ENTREGADO / ANULADO |
| impreso_ficha_en, impreso_etiquetas_en, impreso_instrucciones_en | | |

## SPD_Linea (una por medicamento incluido en ese blíster)

| Campo | Tipo | Notas |
|---|---|---|
| spd_id, tratamiento_id, medicamento_id | FK | |
| snap_nombre, snap_cn, snap_pauta_d/a/c/n, snap_dias_semana, snap_desc_texto, snap_momento | | Instantánea |
| **unidades_dosis** | REAL | Suma semanal real de dosis para el paciente (puede ser fraccionaria: 3,5); es la que se imprime en la etiqueta/ficha como consumo del paciente |
| **unidades_envase** | INTEGER | Unidades enteras a descontar del envase (Spec 005 FR-522: igual a `unidades_dosis` si no hay fracción; `floor(unidades_dosis)+1` o el override manual si la hay) |
| incidencias | | |
| estado_linea | TEXT | `NORMAL` / `EXCLUIDA` (con motivo) / `ENVASE_PENDIENTE` (continuidad sin saldo) |
| motivo_exclusion | TEXT | |

## SPD_Linea_Envase (normal tener varias filas por línea)

| Campo | Tipo | Notas |
|---|---|---|
| spd_linea_id | FK | |
| envase_id | FK | |
| unidades_tomadas | REAL | |
| snap_serie, snap_lote, snap_caducidad | TEXT | Copiados del envase en el momento del descuento, para que la instantánea no dependa de que el envase original siga existiendo con esos datos |

## SPD_Verificacion (una fila por intento — se conservan todas)
spd_id, verificador_id, fecha, verif_aspecto, verif_etiqueta_datos, verif_etiqueta_validez, verif_instrucciones, verif_contenido, resultado, excepcion_motivo.

## SPD_Modificacion (historial de reelaboraciones — Constitución Artículo III.4)

| Campo | Tipo | Notas |
|---|---|---|
| spd_id | FK | |
| version_anterior, version_nueva | INTEGER | |
| fecha, usuario_id | | |
| origen_solicitud | TEXT | PACIENTE / FAMILIAR / MEDICO / FARMACEUTICO / OTRO |
| motivo | TEXT | |
| resumen_cambios | TEXT JSON | Por línea: añadida / eliminada / modificada, con las unidades antes/después y el efecto en custodia (delta devuelto o consumido) |
| lineas_snapshot_anterior | TEXT JSON | Copia de las líneas y sus filas de envase antes de la reelaboración, para reconstrucción íntegra si hace falta en inspección |

## ComunicacionMedico
paciente_id, medico_id, tipo (PRESENTACION/INCIDENCIA/TELEFONO), fecha, incidencias_detectadas, propuesta, respuesta, fecha_respuesta, farmaceutico_id.

## Registros de calidad
RegistroAmbiental, RegistroLimpieza, FormacionPersonal, RecogidaResiduos, ControlCambiosPNT, ControlCopias — sin cambios respecto a la Fase 1.

## Auditoria (solo INSERT)
fecha_hora, usuario_id, accion, entidad, entidad_id, detalle (JSON antes/después).

## PerfilImportacion
nombre, tipo (PACIENTES/DISPENSACIONES/MEDICAMENTOS/NOMENCLATOR), separador, codificacion, tiene_cabecera, mapeo (JSON), **regex_unidades_envase** (patrón para extraer `unidades_envase` de una columna del nomenclátor en perfiles tipo NOMENCLATOR).

## PerfilImportacionTratamiento (Spec 005)
Perfil específico para dar de alta tratamiento + envase de un paciente en un solo paso desde el programa de gestión.
| Campo | Tipo | Notas |
|---|---|---|
| nombre | TEXT | |
| origen | TEXT | PORTAPAPELES / FICHERO |
| separador, tiene_cabecera | | Solo si origen = FICHERO |
| mapeo | TEXT JSON | Columnas de origen → {cn, num_serie, lote, caducidad}, mínimo exigido |

El resultado de una importación crea o reutiliza el tratamiento (por CN + paciente, según reglas de Spec 004) y da de alta el envase correspondiente (Spec 005 FR-510) en un solo lote, fila a fila.

---

## Diagrama de relaciones (resumen)

```
Farmacia (1 fila)

Usuario ── Rol
Medico ◄──────────────┐ catálogo
Medicamento ◄──────────┤ catálogo
                       │
Paciente ── Contacto   │
   │  └── Medico (cabecera)
   ├── EvaluacionIdoneidad
   ├── Consentimiento
   ├── Tratamiento ── Medicamento, Medico (prescriptor)
   ├── Envase ── Medicamento           [1 paciente : n envases, nunca compartidos]
   ├── ComunicacionMedico ── Medico
   └── SPD (sesion_id agrupa 1..2)
          ├── SPD_Linea ── Tratamiento
          │      └── SPD_Linea_Envase (1..n) ── Envase
          ├── SPD_Verificacion (1..n, histórico)
          └── MaterialAcondicionamiento, RegistroAmbiental
```
