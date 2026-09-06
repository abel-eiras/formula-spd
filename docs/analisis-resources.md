# Análisis de la carpeta `resources/` — cribado para ayuda, plantillas y PNT

**Fecha:** 2026-09-06
**Alcance:** 42 ficheros (16 MB) aportados por el usuario en `resources/` (carpeta no versionada). Se leyeron íntegramente todos los PDF (27) vía extracción de texto; los `.doc/.docx` son las fuentes editables de los mismos PNT/registros (se verificó por MD5 que los duplicados sueltos son idénticos a los de sus carpetas); el vídeo `AGPD_SPD.mp4` (8:26 min, 2016) no se ha podido visionar.
**Cruce realizado contra:** constitución (Art. I), `docs/data-model.md`, specs 001/002/006/007/009/010/014 y el código actual de `ServicioGeneracionDocumentos`.

---

## 0. Resumen ejecutivo

1. **La joya es `resources/PNTs/` + `resources/Registros/`**: son los 7 PNT modelo del **COF de A Coruña / CIM (Decreto 87/2022, Galicia)** con sus anexos. Es exactamente la familia normativa sobre la que está construida la app. Todo lo demás en `resources/` es contexto (Portalfarma 2013/2016, Aragón, legislación estatal antigua) y casi todo está **superado** por este juego de PNT.
2. **Discrepancia de numeración de anexos**: la constitución y las specs citan el *PNT del COF de Pontevedra (junio 2022)* con anexos "Anexo 1a/1b, 2, 3, 9…"; los PNT de `resources/` numeran "Anexo I.A…I.J, II.A…C, IV.A…C, V.A…B, VI.A". Son con casi total seguridad la misma familia de documentos (mismo Decreto, misma fecha, misma estructura), pero **el PNT de Pontevedra no está en `resources/`** y hay que decidir cuál es la fuente canónica.
3. **El Decreto 87/2022 no está en `resources/`** (todos los documentos lo citan, ninguno lo contiene). Es el texto legal número uno a añadir a `docs/legislacion/`. Tampoco están los *Criterios consensuados AEMPS (23-abr-2021)* que la Guía cita como referencia.
4. **Los 5 documentos que hoy genera la app (FICHA, ETQ-A, ETQ-R, INSTR, FICHA-PAC) son esqueletos que NO cumplen todavía el Art. I.2 de la constitución** ("contienen como mínimo todos los elementos que el PNT exige para cada anexo"). Ahora, por primera vez, tenemos el listado literal de elementos obligatorios de cada anexo — ver §2.3 para el cruce campo a campo. Es la corrección de mayor prioridad que sale de este análisis.
5. **Hay 8 documentos/registros que el PNT exige y que no figuran en el catálogo FR-700 de Spec 007** ni en ninguna spec: hoja de información inicial (I.A), información de protección de datos (I.D), acuse de recibo/recogida (I.I), organigrama (II.A), reconocimiento de firmas (II.C), calibración termómetro/higrómetro (IV.C), incidencia ambiental (V.B), registro de recepción de DDP (VI.A). Ver §2.4.

---

## 1. Inventario y veredicto por fichero

Leyenda de destino: **PNT** = fuente normativa del procedimiento · **AYUDA** = contenido para Spec 014 · **PLANTILLA** = modelo de documento imprimible (Spec 007) · **LEG** = `docs/legislacion/` · **NO** = no útil / obsoleto.

### 1.1 `resources/PNTs/` — PNT modelo COF A Coruña (Decreto 87/2022)

| Fichero | Qué es | Veredicto | Destino |
|---|---|---|---|
| **PNT I — Procedimiento General del servicio SPD** (29 pp.) | El PNT maestro: idoneidad, CI, ficha, preparación, sellado, verificación, entrega, residuos. Anexos I.A–I.J con el **texto literal** de todos los documentos al paciente/médico | ★★★ Imprescindible. Es la fuente de verdad de Specs 001/002/006/007/008 | PNT · PLANTILLA · AYUDA |
| **PNT II — Funciones, formación y responsabilidades** (10 pp.) | Puestos, obligaciones titular/adjunto/técnico, criterios de sustitución, formación acreditada. Anexos II.A organigrama, II.B registro formación, II.C reconocimiento de firmas | ★★ Útil. Fundamenta Spec 009 (formación) y la regla "verificador ≠ elaborador" | PNT · AYUDA · PLANTILLA (II.B, II.C) |
| **PNT III — Higiene del personal** (3 pp.) | 6 normas mínimas (indumentaria, lavado de manos, prohibiciones) | ★ Solo para la ayuda (sección Procedimiento). No genera datos ni documentos | AYUDA |
| **PNT IV — Limpieza del área, instrumental y mantenimiento** (9 pp.) | Plan diario/mensual/anual, limpieza de utillaje, calibración. Anexos IV.A registro limpieza (rejilla mensual), IV.B mantenimiento automatizados, IV.C calibración | ★★ Útil. **Conflicto** con Spec 009: la pantalla de limpieza se retiró tras tu prueba manual, pero el PNT exige el registro (ver §2.6) | PNT · PLANTILLA (IV.A, IV.C) · AYUDA |
| **PNT V — Condiciones ambientales** (8 pp.) | Rangos 15–25 ºC ±2 / 40–60 % ±5, lectura diaria, actuación ante desviación. Anexos V.A registro mensual **por dos zonas (preparación y almacenamiento) con actual/máx/mín**, V.B incidencia | ★★ Útil. **Conflicto** con el `RegistroAmbiental` actual (una sola lectura, sin zona ni máx/mín) — ver §2.5 | PNT · PLANTILLA (V.A, V.B) · AYUDA |
| **PNT VI — Recepción y almacenamiento de DDP** (6 pp.) | Inspección en recepción, registro con nº interno/proveedor/lote/cantidad/caducidad/decisión firmada, normas de almacén. Anexo VI.A | ★★ Útil. Amplía el `MaterialAcondicionamiento` actual (solo descripción+lote+fecha) — ver §2.4 | PNT · PLANTILLA (VI.A) · AYUDA |
| **PNT VII — Registro y conservación de documentación** (5 pp.) | Qué se conserva, cuánto (≥1 año; fichas ≥1 año tras baja), cómo se organiza, firma electrónica si es digital | ★★ Útil. Fundamenta el índice "documentación por momento del servicio" de Spec 014 FR-1403 y la purga de Spec 010 | PNT · AYUDA |
| `PNT I ... SPD_.docx` y `PNT III ... .doc` sueltos en `PNTs/` | Duplicados byte a byte de los que hay dentro de sus carpetas | Redundantes: borrar los sueltos | NO |

### 1.2 `resources/Registros/` — anexos de registro en Word/PDF

| Fichero | Equivale a | Veredicto |
|---|---|---|
| Ficha de preparación, control y entrega de SPD | Anexo I.G del PNT I (idéntico) | PLANTILLA — modelo literal de `FICHA` |
| Registro condiciones ambientales | Anexo V.A (idéntico) | PLANTILLA — `REG-AMB` |
| Registro formación del personal | Anexo II.B (idéntico) | PLANTILLA — `REG-FORM` |
| Registro limpieza | Anexo IV.A (idéntico) | PLANTILLA — `REG-LIMP` |
| Registro recogida de residuos NO SIGRE | Anexo I.J (idéntico) | PLANTILLA — `REG-RES` |

Son los mismos anexos extraídos a fichero propio; el valor añadido es que están en `.docx` editable (lo que Spec 007 FR-701 pedía originalmente) y sirven como **referencia visual de maquetación** para los PDF de QuestPDF.

### 1.3 Raíz de `resources/`

| Fichero | Qué es | Veredicto | Destino |
|---|---|---|---|
| **GUÍA IMPLANTACIÓN SPD** (CIM, junio 2022, 13 pp.) | Guía del COF para implantar el servicio según el Decreto 87/2022: declaración responsable SA480A, cese, personal/formación acreditada, zonas, equipamiento, documentación exigida, conservación, obligaciones, diagrama de flujo del PNT I | ★★★ Imprescindible para la ayuda: es la mejor "vista de inspector" del servicio. Casi seguro es el documento "junio 2022" que cita la constitución | AYUDA · PNT |
| **Fabricantes y modelos adheridos SIGRE** (4-ago-2022, 2 pp.) | 7 fabricantes / ~25 modelos de DDP adheridos a SIGRE + enlace oficial | ★ Útil como semilla del catálogo `MaterialAcondicionamiento` y como texto de ayuda en "Eliminación de residuos". Fecha 2022: hay que refrescarlo del enlace | AYUDA · datos semilla |

### 1.4 `resources/Portalfarma 2016/` — material del Consejo General (2013–2016)

| Fichero | Qué es | Veredicto |
|---|---|---|
| **PNT-SPD.pdf** (CGCOF, mayo 2013, 24 pp.) | PNT "de mínimos" estatal con anexos A–H. **Predecesor** del PNT I gallego | Útil solo como comparativa: aporta 3 cosas que el PNT gallego no tiene — *lista de comprobación de primera entrega* (Anexo H), *dispositivo de muestra con placebos*, *sugerencia de copia reducida de la etiqueta para que el paciente la lleve encima*. Validez "4 semanas" está **superada** por los 14 días del Decreto |
| Modelo_ficha_paciente, Modelo_etiquetas, Modelo_autorizacion_y_Consentimiento_informado, Modelo_carta_medico_responsable, Hoja_control_proceso_preparacion, Lista_comprobacion | Anexos B–H del PNT-SPD 2013 sueltos | **Superados** por los Anexos I.B–I.H del PNT I (2022), que ya están adaptados al Decreto y al RGPD. Solo Lista_comprobacion (Anexo H) no tiene equivalente gallego → candidata a checklist de primera entrega (Spec 006 FR-661 ya la menciona) |
| **CURSO_SPD_ok.pdf** (CGCOF, 38 pp.) | Curso de formación: adherencia, tests (Morisky-Green, Haynes-Sackett, Batalla, Hermes), ventajas, equipamiento, procedimiento paso a paso | ★★ Útil para la ayuda: es la única fuente con **texto explicativo** (el "porqué") y con herramientas concretas para el "control de adherencia" que el PNT exige registrar pero no dice cómo medir |
| Documento-Estrategico-SPD.pdf (CGCOF, 2013) | Estrategia político-profesional: acreditación, remuneración, seguro RC | Contexto. Un párrafo de introducción de la ayuda como mucho. No aporta requisitos |
| BOA-D93_2015 Requisitos SPD Aragón | Decreto de **otra comunidad** | NO (comparativa; Galicia tiene su propio Decreto) |
| Ley_29_2006_texto_consolidado (93 pp.) y Real_Decreto-ley_9/2011 (26 pp.) | Legislación estatal. Lo único relevante es el art. 84.1 (SPD post-dispensación) | **Superados**: la Ley 29/2006 fue refundida en el **RDL 1/2015** (hoy art. 86, que es el que cita la Guía). Guardar solo la referencia al art. 86 del RDL 1/2015 |
| LOPD.pdf (LO 15/1999) | Ley de protección de datos **derogada** | NO: hoy rige el RGPD + LOPDGDD 3/2018. El Anexo I.D del PNT I ya está redactado sobre el RGPD |
| AGPD_SPD.mp4 (2016, 8:26) | Vídeo, presumiblemente sobre protección de datos bajo la LOPD de 1999 | No visionado; por fecha y tema, probablemente obsoleto. Spec 014 excluye vídeo de la ayuda |

---

## 2. Hallazgos que afectan a lo ya construido

### 2.1 Fuente canónica: Pontevedra vs A Coruña

La constitución (Art. I.1) fija como fuente el *PNT del COF de Pontevedra (junio 2022)*. Las specs y el `data-model.md` usan esa numeración ("Anexo 2" = ficha paciente, "Anexo 1a/1b" = consentimiento, "Anexo 9" = idoneidad, "Anexo III" = firmas reconocidas). Los PNT de `resources/` son del COF de A Coruña (el Anexo I.D nombra a su DPO) y numeran distinto. Ambos derivan del mismo Decreto y de la misma Guía del CIM, y el contenido cruzado coincide en todo lo que he podido comprobar.

**Correspondencia probable** (a confirmar con el PNT de Pontevedra cuando esté):

| Specs / constitución | PNT A Coruña (`resources/`) |
|---|---|
| Anexo 1a / 1b (consentimiento paciente / representante) | Anexo I.B (un único modelo con "en nombre propio, o como representante legal…") |
| Anexo 2 (ficha del paciente) | Anexo I.E |
| Anexo 9 (evaluación de idoneidad, 7 criterios) | Bloque "EVALUACIÓN IDONEIDAD" del Anexo I.E + los 7 criterios orientativos del §4.1 (ver §2.7) |
| Anexo III (firmas reconocidas) | Anexo II.C |
| Hoja de elaboración / ficha de preparación | Anexo I.G |
| Etiquetas | Anexo I.F |
| Hoja de instrucciones | Anexo I.H |
| Carta al médico | Anexo I.C |
| Registro residuos no SIGRE | Anexo I.J |

**Decisión pendiente del usuario:** o bien se consigue el PNT de Pontevedra y se cruza, o bien se cambia el Art. I.1 de la constitución para citar el juego de A Coruña/CIM que sí tenemos. Mientras tanto, este documento usa la numeración de A Coruña.

### 2.2 Legislación que falta y que sobra

- **Falta (añadir a `docs/legislacion/`)**: Decreto 87/2022 (DOG 108, 7-jun-2022) — el texto legal real; Criterios consensuados AEMPS-CCAA para SPD (23-abr-2021); RDL 1/2015 art. 86 (basta la cita); Ley 3/2019 de ordenación farmacéutica de Galicia art. 13 (basta la cita); formulario SA480A de la sede electrónica (útil para la ayuda "inicio de actividad").
- **Sobra / obsoleto**: LO 15/1999, Ley 29/2006 completa, RDL 9/2011, Decreto de Aragón.

### 2.3 Los documentos generados hoy no contienen los elementos mínimos (Art. I.2)

Cruce del código actual de `ServicioGeneracionDocumentos` contra el anexo correspondiente. ✔ = ya se imprime · ✘ = falta · ~ = parcial.

**`FICHA` — Anexo I.G "Ficha de preparación, control y entrega"**

| Elemento exigido | Estado | Dato disponible en el modelo |
|---|---|---|
| Paciente | ✔ | |
| Fecha de preparación | ✘ | `SPD.fecha_preparacion` |
| Fecha de entrega | ✘ | `SPD.fecha_entrega` |
| Nº registro DDP | ✔ | |
| Periodo de validez | ✔ | |
| Por medicamento: **CN** | ✘ | `SPD_Linea.snap_cn` |
| Posología D/A/C/N | ✔ | |
| **Lote, caducidad y nº serie** por envase | ~ (solo serie) | `SPD_Linea_Envase.snap_*` |
| Material de acondicionamiento: tipo y **nº de lote** | ✘ | `SPD.material_id` → `MaterialAcondicionamiento` |
| Temperatura y humedad en el momento de preparar | ✘ | `SPD.registro_ambiental_id` |
| Control de adherencia: entrega anterior SÍ/NO, otra información, cumple SÍ/NO | ✘ | `spd_anterior_recogido`, `unidades_no_administradas`, `observaciones_adherencia` |
| Verificación final: 8 preguntas SÍ/NO (ver §2.8) | ✘ | `SPD_Verificacion` (5 ítems) |
| Elaborado / Verificado / **Entregado** por, con firma y fecha | ~ (solo dos huecos sin nombre ni fecha) | `elaborador_id`, `verificador_id`, `entregador_id` + fechas |
| Leyenda "*D: Desayuno; A: Almuerzo; C: Cena; N: Noche" | ✘ | texto fijo |

**`ETQ-A` — Anexo I.F etiqueta anverso**

| Elemento | Estado |
|---|---|
| Datos farmacia: nombre, **dirección, teléfono** | ~ (solo nombre) |
| Datos paciente: nombre y apellidos, **teléfono de contacto** | ~ |
| Nº registro, **fecha de preparación**, periodo de validez | ~ (falta fecha de preparación) |
| "Recuerde que, además, hay que administrar: …" — **medicamentos del tratamiento NO incluidos en el DDP** | ✘ (dato: `Tratamiento.en_spd = 0`) |
| Advertencias: fuera del alcance de los niños; no usar después de la validez; conservar en lugar fresco, seco y protegido de la luz; comunicar cambios de medicación | ✘ (texto fijo) |

**`ETQ-R` — Anexo I.F etiqueta reverso**

| Elemento | Estado |
|---|---|
| Por medicamento: **CN**, nombre, **posología**, nº serie, lote, caducidad, **aspectos físicos de identificación** | ~ (falta CN, posología y descripción física — `Medicamento.descripcion_fisica`, Spec 003) |
| Advertencias (niños; conservación) | ✘ |

**`INSTR` — Anexo I.H hoja de instrucciones**

| Elemento | Estado |
|---|---|
| Datos farmacia: nombre, **dirección, teléfono** | ✘ |
| Fecha preparación, nº registro, periodo de validez | ~ (solo validez) |
| Medicamentos incluidos: **CN, médico prescriptor, posología, fecha prescripción / última modificación** | ~ (solo nombre y posología) |
| **Medicamentos NO incluidos** que forman parte del tratamiento (misma tabla) | ✘ |
| Advertencias de uso (4 líneas literales del anexo) | ✘ |

**`FICHA-PAC` — Anexo I.E ficha del paciente**

| Elemento | Estado |
|---|---|
| Ficha nº, **fecha** | ~ |
| Nombre, fecha nacimiento, DNI, dirección, población, CP, **teléfonos (2), e-mail** | ~ |
| **Familiar o cuidador: nombre, teléfono, e-mail** (contacto principal, Spec 001 FR-021) | ✘ |
| **Médico de familia: nombre, teléfono, e-mail** | ✘ |
| Enfermedades crónicas, alergias, **observaciones** | ~ |
| **Bloque EVALUACIÓN IDONEIDAD**: criterios de inclusión, observaciones, APTO/NO APTO, firma | ✘ (Spec 002 pendiente) |
| **Tabla medicamentos incluidos en DDP**: CN, medicamento, problema de salud, médico prescriptor, posología y vía, fecha inicio, fecha fin, PRM/RNM, intervención; **fecha de revisión** | ✘ (todos los campos existen en `Tratamiento`) |
| **Tabla medicamentos NO incluidos** (mismas columnas) | ✘ |
| **Control adherencia**: fecha, cumple SÍ/NO, observaciones (histórico) | ✘ (dato: entregas de `SPD`) |

Además: Art. IX.3 exige que "cada plantilla tiene un test que genera el PDF con datos de ejemplo y comprueba la presencia de los elementos obligatorios"; los tests actuales solo comprueban que el fichero existe. Pendiente añadir aserciones sobre el contenido (p. ej. extrayendo el texto del PDF generado).

### 2.4 Documentos que el PNT exige y ninguna spec contempla

| Anexo | Documento | Encaje propuesto |
|---|---|---|
| I.A | Hoja de información al paciente (3 páginas de texto fijo con datos de la farmacia) | Documentación base (Spec 007 FR-730), código `INFO-PAC`; también íntegro en la ayuda |
| I.D | Información sobre protección de datos (RGPD) — responsable, categorías, plazos de conservación, derechos, DPO | Documentación base, código `RGPD`. `Farmacia` ya tiene `responsable_datos, direccion_derechos, email_derechos`; **falta DPO** (nombre + contacto, del COF) y el email/teléfono de la farmacia ya existen. Debe entregarse junto con el consentimiento (Spec 002) |
| I.I | Acuse de recibo / recogida de DDP (nº registro, fecha entrega, fecha recogida, quién recoge y relación con el paciente, quién entrega, observaciones) | Nuevo código `ACUSE` por SPD/entrega. Datos ya existen en `SPD` (`entregado_a`, `fecha_entrega`, `entregador_id`); falta "relación con el paciente" (se puede derivar del `Contacto` si `entregado_a` se vincula a uno) |
| II.A | Organigrama (titular/regente → adjuntos → técnicos → limpieza, con sustituto en ausencia) | Documentación base, generable desde `Usuario.cargo_pnt`. Prioridad baja |
| II.C | Reconocimiento de firmas (cargo, nombre, apellidos, firma, firma abreviada) | Documentación base, código `FIRMAS`. `Usuario.firma_abreviada` ya existe (data-model lo anticipa como "Anexo III") |
| IV.C | Registros de calibración de termómetro e higrómetro | Documentación base (rejilla vacía). Sin dato en la app |
| V.B | Registro de incidencia de temperatura/humedad (código correlativo/año, descripción, análisis de causas, evaluación, aceptar/rechazar, medida correctora) | Enganche natural con `RegistroAmbiental.fuera_rango = 1`. Fase posterior; mientras tanto, plantilla vacía en documentación base |
| VI.A | Registro de recepción de DDP (nº registro interno, producto, proveedor, fecha recepción, lote, cantidad, nº envases, caducidad, condiciones, decisión aceptación/rechazo fechada y firmada, observaciones) | Ampliar `MaterialAcondicionamiento` (hoy: descripción, lote, fecha_entrada). Hasta entonces, plantilla vacía |
| PNT-SPD 2013 Anexo H | Lista de comprobación de primera entrega | Opcional (no lo exige el PNT gallego, pero Spec 006 FR-661 ya prevé "checklist de primera entrega"). Buen contenido para la ayuda |

### 2.5 Registro ambiental: lo que el PNT V pide frente a lo que hay

El `RegistroAmbiental` actual (migración 0009): fecha, temperatura, humedad, fuera_rango, usuario. El Anexo V.A exige, **por cada una de dos zonas** (preparación de SPD y almacenamiento de medicamentos): día, hora, temperatura **actual / máxima / mínima**, humedad **actual / máxima / mínima**, firma; lectura al menos diaria; los días no laborables se registra máx/mín a primera hora del siguiente día hábil. Propuesta: añadir `zona` (PREPARACION/ALMACENAMIENTO) y `temp_max, temp_min, hr_max, hr_min` (opcionales) a la entidad; el registro hecho desde la preparación (Spec 006) es el de zona PREPARACION. El `REG-AMB` impreso es la rejilla mensual por zona.

### 2.6 Limpieza: se retiró de Spec 009 pero el PNT IV la exige

Tras tu prueba manual del 2026-09-05 se retiraron las pantallas de limpieza (y ambiental) de Spec 009. El ambiental volvió con Spec 006; la limpieza no. El PNT IV exige registro diario (suelos/mesa/cubos y utillaje), mensual (armarios) y anual (techos/paredes), con el Anexo IV.A como rejilla mensual. Opciones: (a) mínimo viable — `REG-LIMP` como plantilla vacía en documentación base para rellenar a mano; (b) recuperar la entidad `RegistroLimpieza` con los 4 tipos del anexo (diaria-superficies, diaria-utillaje, mensual, anual) y los botones de un clic pre/post preparación que Spec 009 FR-911 ya describía. Decisión tuya; (a) cumple el PNT en papel sin tocar la app.

### 2.7 Spec 002 (idoneidad): el "Anexo 9 con 7 criterios" probablemente son estos

El PNT I §4.1 enumera **7 criterios de inclusión orientativos**: (1) polimedicados con pautas complejas; (2) mayores que viven solos o dependientes sin cuidador; (3) deficiencia cognitiva o demencia; (4) problemas de adherencia; (5) programas concertados con la Administración; (6) dificultades expresadas por la propia persona para gestionar su medicación; (7) criterio del médico o del farmacéutico. Y añade 2 condiciones "es importante que": motivación y capacidad de manejar el DDP (destreza manual, agudeza visual — paciente o cuidador). El bloque del Anexo I.E es texto libre ("Criterios de inclusión / Observaciones / APTO – NO APTO / Firma"), sin regla de combinación: **el resultado APTO/NO APTO es criterio profesional**, no una fórmula. Esto resuelve el `[NEEDS CLARIFICATION]` de FR-200/201 de forma más sencilla de lo previsto: 7 casillas orientativas + 2 condiciones + observaciones + resultado elegido por el farmacéutico (obligatorio justificar NO_APTO, como ya dice FR-204). El texto literal del consentimiento (Anexo I.B) y de la carta al médico (Anexo I.C) también está ya disponible para Specs 002 y 008.

### 2.8 Verificación: 5 ítems (Spec 006) frente a 8 preguntas (Anexo I.G)

| Anexo I.G (verificación final) | Spec 006 FR-651 |
|---|---|
| ¿El contenido del DDP es correcto? (comprobar con ficha de paciente) | ✔ "cada alveolo contiene lo que corresponde" |
| ¿Se siguieron instrucciones del fabricante y PNT durante todo el proceso? | ✘ |
| ¿Coinciden los datos de DDP de la etiqueta con la ficha de preparación? | ✔ "datos de la etiqueta coinciden con la ficha" |
| ¿Coinciden los datos de etiqueta y ficha de paciente a la fecha actual? | ~ (fusionado con el anterior) |
| ¿Se garantizó trazabilidad entre envase original y DDP? | ✘ (la app lo garantiza por construcción; puede imprimirse como "SÍ" automático) |
| ¿El DDP está identificado con su periodo de validez? | ✔ |
| ¿Se cumplimentó la hoja de instrucciones según normativa? | ✔ |
| ¿Existen alteraciones visibles en el producto acabado? | ✔ "integridad del blíster" |

Propuesta: ampliar el checklist a las 8 preguntas literales del anexo (dos de ellas pueden venir pre-marcadas por la app: trazabilidad y hoja de instrucciones generada), y que `FICHA` las imprima tal cual. Cambio pequeño en `ChecklistVerificacion` + `SPD_Verificacion`.

### 2.9 Otras reglas del PNT que la app debería conocer

- **Hoja de instrucciones "en cada entrega"** (PNT I §4.4.1.c) — no "por blíster". Resuelve la pregunta abierta de Spec 006 FR-682: una sola hoja por sesión de dos blísteres es conforme.
- **Preparación el mismo día del desemblistado**; **no conservar unidades divididas sobrantes** para la siguiente preparación (§4.3) — reglas de procedimiento para la ayuda; la segunda podría reflejarse en Spec 005 (no reingresar fracciones).
- **Validez > 14 días es excepcional y exige justificación + análisis de riesgos documentado** (§4.4) — hoy la app fija 7 días; si algún día se permite más, hace falta campo de justificación.
- **Retirar las etiquetas del DDP antes de depositarlo en SIGRE** (§4.8) — ayuda.
- **Conservación**: documentación ≥1 año; fichas ≥1 año tras la baja (Decreto). El Anexo I.D de A Coruña concreta la política RGPD: **bloqueo al año de inactividad, borrado al cuarto año**. Es una política de purga lista para usar en Spec 010 §4.3 (que sigue diferida).
- **Firma electrónica** obligatoria si la documentación que requiere firma se conserva solo en digital (Guía §5.3, PNT VII) — refuerza el Art. II de la constitución ("el papel es la base legal"): la app genera, el papel se firma.
- **Declaración responsable SA480A** antes de empezar y nueva declaración ante cambios de titular/instalaciones; cese con un mes de antelación — contenido de ayuda para el titular.

---

## 3. Qué va al menú de ayuda (Spec 014)

Spec 014 pide dos secciones: *Uso de la aplicación* (no sale de `resources/`, sale de las pantallas) y *Procedimiento del servicio SPD*. Para la segunda, `resources/` da prácticamente todo el contenido:

| Apartado de Procedimiento (FR-1410) | Fuente principal | Complemento |
|---|---|---|
| Qué es el SPD, marco legal, requisitos para prestarlo (declaración responsable, formación acreditada, zonas, equipamiento) | Guía Implantación §1–5, §7–8 | PNT II §4.3 |
| Información inicial e idoneidad del paciente | PNT I §4.1 + Anexo I.A íntegro | Curso §2.6, §4.1 |
| Consentimiento informado y protección de datos | PNT I §4.2 + Anexos I.B, I.D | Curso §2.3 (porqué del CI) |
| Entrevista y ficha del paciente; revisión del tratamiento, PRM/RNM, intervenciones | PNT I §4.3 (listado de PRM, categorías RNM, tipos de intervención) | Curso §4.3, §4.5 |
| Medicamentos aptos / no aptos para SPD; fracciones; cadena de frío | PNT I §4.3 "Medicamentos susceptibles" | Curso Tabla 3 |
| Cambios de prescripción y continuidad | PNT I §4.3 último bloque | Curso §4.6 (informe de alta hospitalaria) |
| Preparación: condiciones previas, higiene, ambiente, material, documentación, llenado | PNT I §4.4, PNT III, PNT V | Curso §4.7 |
| Sellado y verificación (con el porqué de cada ítem) | PNT I §4.5–4.6 + Anexo I.G | — |
| Entrega, primera entrega, control de adherencia | PNT I §4.7 + Anexo I.I | Curso Tabla 1 (tests Morisky-Green, Haynes-Sackett…); PNT-SPD 2013 Anexo H (checklist primera entrega, placebos de muestra) |
| Residuos y SIGRE | PNT I §4.8 + lista de fabricantes SIGRE | — |
| Personal: funciones, sustituciones, formación, firmas | PNT II | — |
| Limpieza y mantenimiento; calibración | PNT IV | — |
| Recepción y almacenamiento de DDP | PNT VI | — |
| Documentación: qué existe en cada momento, cuánto se conserva (índice de inspección FR-1403) | PNT VII + Guía §5.3 | Anexo I.D (plazos RGPD) |
| Enlaces "¿Por qué?" de los bloqueos (FR-1402): verificador ≠ elaborador, validez ≤14 días, consentimiento obligatorio, no unidades divididas sobrantes, temperatura/humedad fuera de rango | PNT I §3 y §4.4–4.6; PNT V §4.2 | — |

Formato: Spec 014 FR-1405 exige Markdown embebido. Todo lo anterior es texto ya escrito en castellano en los PNT; el trabajo es reescribirlo en clave "qué hago en la aplicación en este paso y por qué", no redactarlo desde cero.

---

## 4. Qué va a plantillas imprimibles (Spec 007) — catálogo FR-700 revisado

| Código | Documento | Anexo | Estado hoy | Acción |
|---|---|---|---|---|
| `FICHA` | Ficha de preparación, control y entrega | I.G | Esqueleto | Completar según §2.3 |
| `ETQ-A` / `ETQ-R` | Etiquetas anverso / reverso | I.F | Esqueleto | Completar según §2.3 |
| `INSTR` | Hoja de instrucciones | I.H | Esqueleto | Completar según §2.3; una por entrega |
| `FICHA-PAC` | Ficha del paciente | I.E | Esqueleto | Completar según §2.3 |
| `CONSENT` | Consentimiento informado | I.B | Pendiente (Spec 002) | Texto literal ya disponible |
| `IDONEIDAD` | Evaluación de idoneidad | I.E (bloque) | Pendiente (Spec 002) | Forma parte de `FICHA-PAC`, no necesita documento aparte |
| `CARTA-PRES` | Carta de presentación al médico | I.C | Pendiente (Spec 008) | Texto literal ya disponible |
| `CARTA-INC` | Carta de incidencias | — | Pendiente | El PNT no tiene modelo; se deriva de I.C |
| `RETIRADA` | Listado de retirada de envases | — | Pendiente | Documento interno de la app, sin anexo |
| `REG-AMB` | Registro condiciones ambientales | V.A | Pendiente | Rejilla mensual **por zona** con máx/mín |
| `REG-LIMP` | Registro de limpieza | IV.A | Pendiente | Rejilla mensual (4 bloques) |
| `REG-FORM` | Registro de formación | II.B | Pendiente | Tabla simple |
| `REG-RES` | Registro residuos no SIGRE | I.J | Pendiente | Tabla simple |
| **`INFO-PAC`** (nuevo) | Hoja de información al paciente | I.A | — | Documentación base |
| **`RGPD`** (nuevo) | Información protección de datos | I.D | — | Documentación base; entregar con el CI |
| **`ACUSE`** (nuevo) | Acuse de recibo / recogida | I.I | — | Por entrega |
| **`FIRMAS`** (nuevo) | Reconocimiento de firmas | II.C | — | Documentación base |
| **`ORGANIGRAMA`** (nuevo) | Organigrama | II.A | — | Documentación base, prioridad baja |
| **`REG-CALIB`** (nuevo) | Calibración termómetro/higrómetro | IV.C | — | Documentación base (vacío) |
| **`REG-INC-AMB`** (nuevo) | Incidencia ambiental | V.B | — | Documentación base (vacío); enganche futuro con `fuera_rango` |
| **`REG-DDP`** (nuevo) | Recepción de DDP | VI.A | — | Documentación base (vacío) hasta ampliar `MaterialAcondicionamiento` |
| `CHECKLIST-1ENT` (opcional) | Lista de comprobación primera entrega | PNT-SPD 2013 H | — | Opcional; Spec 006 FR-661 |

Los `.docx` de `resources/Registros/` y los anexos del PNT I son la referencia de maquetación de todos ellos.

---

## 5. Propuesta de reubicación dentro del repo

`docs/legislacion/` y `docs/plantillas-word-originales/` existen vacías desde el 5-sep, claramente para esto. Propuesta (no ejecutada, pendiente de tu OK):

```
docs/pnt/                                  ← nuevo
  PNT-I-procedimiento-general.pdf (+ .docx)
  PNT-II-funciones-formacion.pdf (+ .doc)
  PNT-III-higiene.pdf (+ .doc)
  PNT-IV-limpieza-mantenimiento.pdf (+ .doc)
  PNT-V-condiciones-ambientales.pdf (+ .doc)
  PNT-VI-recepcion-ddp.pdf (+ .doc)
  PNT-VII-conservacion-documentacion.pdf (+ .doc)
  GUIA-implantacion-SPD-CIM-2022.pdf
docs/plantillas-word-originales/
  Anexo-I.G-ficha-preparacion-control-entrega.docx (+ .pdf)
  Anexo-V.A-registro-condiciones-ambientales.docx (+ .pdf)
  Anexo-II.B-registro-formacion.docx (+ .pdf)
  Anexo-IV.A-registro-limpieza.docx (+ .pdf)
  Anexo-I.J-registro-residuos-no-sigre.docx (+ .pdf)
docs/legislacion/
  SIGRE-fabricantes-modelos-adheridos-2022-08.pdf
  (a añadir) Decreto-87-2022-Galicia.pdf
  (a añadir) AEMPS-criterios-consensuados-SPD-2021-04-23.pdf
  referencias.md  ← citas a RDL 1/2015 art. 86 y Ley 3/2019 art. 13, sin el texto completo
docs/referencia-historica/                 ← opcional; o directamente fuera del repo
  PNT-SPD-CGCOF-2013.pdf, CURSO-SPD-CGCOF.pdf, Documento-Estrategico-2013.pdf
```

**Fuera del repo** (no aportan nada a la app): Ley 29/2006, RDL 9/2011, LOPD 1999, BOA Aragón, los 6 modelos sueltos de Portalfarma (están dentro de PNT-SPD-2013), el vídeo, los dos duplicados sueltos de `PNTs/`.

Tamaño: los PNT + Guía + registros + SIGRE suman ~3 MB; entra en git sin problema. El resto (~13 MB) es lo que conviene dejar fuera.

---

## 6. Siguientes pasos sugeridos, por prioridad

1. **Decidir la fuente canónica** (§2.1): conseguir el PNT de Pontevedra o actualizar el Art. I.1 de la constitución al juego COF A Coruña/CIM. Todo lo demás depende de esto para la numeración de anexos.
2. **Completar los 5 documentos existentes** hasta los elementos mínimos de sus anexos (§2.3) y añadir tests de presencia de elementos (Art. IX.3). Es la deuda más clara respecto a la constitución y toca solo `ServicioGeneracionDocumentos` + tests; los datos ya están en el modelo.
3. **Spec 002** ya no está bloqueada por contenido: criterios (§2.7), consentimiento (I.B) y protección de datos (I.D) están literales. Añadir `RGPD` al alcance de la spec y el campo DPO a `Farmacia`.
4. **Ampliar el checklist de verificación a las 8 preguntas** del Anexo I.G (§2.8) — cambio pequeño en Spec 006.
5. **Documentación base (Spec 007 §4.4)** con los nuevos códigos de §4: es lo que permite cumplir en papel PNT IV, V.B, VI.A e II sin construir pantallas nuevas.
6. **Registro ambiental por zona con máx/mín** (§2.5) y decisión sobre limpieza (§2.6).
7. **Spec 014 (ayuda)**: redactar la sección Procedimiento a partir del mapa de §3.
8. Reubicar ficheros según §5 y añadir el Decreto 87/2022 y los criterios AEMPS a `docs/legislacion/`.
