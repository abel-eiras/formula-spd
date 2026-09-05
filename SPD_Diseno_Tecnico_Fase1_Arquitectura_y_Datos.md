# Aplicación de gestión del servicio SPD — Diseño técnico

**Fase 1 de 5: Arquitectura y modelo de datos**
Versión 0.1 — 4 de septiembre de 2026
Base normativa: PNT de SPD del COF de Pontevedra (junio 2022), Decreto 87/2022 (Galicia), anexos 1a–11 y registros asociados.

---

## 0. Cómo leer este documento

Cada apartado termina en decisiones numeradas (**D-xx**). Al final hay una lista de puntos de revisión que necesito confirmados antes de la Fase 2. Lo que no está marcado como pendiente lo doy por cerrado con tus respuestas.

Plan de fases:

| Fase | Contenido | Entregable |
|---|---|---|
| 1 | Arquitectura, stack, modelo de datos, mecánica de reutilización | Este documento |
| 2 | Flujos de trabajo y pantallas (mapa de navegación, wireframes en texto, reglas de validación) | Documento + maqueta HTML navegable |
| 3 | Documentos imprimibles (ficha, preparación, etiquetas, instrucciones, cartas, registros) | Especificación de cada documento + plantillas |
| 4 | Seguridad, cifrado, copias, auditoría, import/export | Especificación + esquema de perfiles de importación |
| 5 | Plan de construcción: estructura del proyecto, orden de desarrollo, pruebas, empaquetado | Roadmap + esqueleto de proyecto compilable |

---

## 1. Requisitos consolidados

### 1.1 Funcionales (de los documentos y tus respuestas)

| Ref | Requisito | Origen |
|---|---|---|
| RF-01 | Gestión de pacientes con ficha completa (Anexo 2), representante/persona autorizada, médico de cabecera, enfermedades crónicas, alergias | PNT §5 |
| RF-02 | Registro de idoneidad con los 7 criterios (Anexo 9) | PNT §5 |
| RF-03 | Consentimiento informado: generación del documento 1a/1b, registro de fecha de firma en papel | Respuesta 11–12 |
| RF-04 | Tratamiento del paciente: medicamentos dentro y fuera del SPD, problema de salud, prescriptor, posología D/A/C/N, vía, fechas inicio/fin, incidencias, intervención | Anexo 2 |
| RF-05 | Catálogo de medicamentos por CN con descripción física (forma, color, ranura, serigrafía) reutilizable entre pacientes y modificable en cualquier momento | Respuesta 7, Anexo 2 hoja oculta |
| RF-06 | Catálogo de médicos reutilizable | Tu petición |
| RF-07 | Depósito de medicación: envases en custodia por paciente con lote, caducidad, identificador único, unidades restantes | PNT §3, §5 |
| RF-08 | Preparación del SPD (Anexo 5): nº registro, periodo de validez, líneas con posología/unidades/lote/caducidad/serie, material de acondicionamiento, Tª/HR, elaborador, verificador (≠ elaborador salvo excepción justificada), checklist de verificación, entrega, control de adherencia, devoluciones | Anexo 5, respuesta 14 |
| RF-09 | Etiquetas anverso y reverso (Anexo 6) en A4, formato A5/A6 apaisado, con pictogramas de posología | Respuestas 8, 10, 22 |
| RF-10 | Hoja de instrucciones al paciente (Anexo 7) | Anexo 7 |
| RF-11 | Cartas al médico: presentación (Anexo 3) e incidencias (Anexo 4) | Anexos 3–4 |
| RF-12 | Registros de calidad: condiciones ambientales, limpieza, formación, residuos no SIGRE, firmas reconocidas, control de cambios del PNT, control de copias | Anexos 8, 10, 11 y registros |
| RF-13 | Continuidad: preparación de la semana siguiente partiendo de la anterior; cambios de medicación disparan revisión | PNT §5 |
| RF-14 | Bajas: marcado, nunca borrado automático; utilidad manual de purga de bajas > 5 años | Respuesta 18 |
| RF-15 | Import/export de pacientes y dispensaciones desde programas de gestión con distintos formatos | Respuesta 19 |
| RF-16 | Configuración de la farmacia (código PO-xxx-F, titular, dirección, contacto, nº colegiado) | Respuesta 3 |

### 1.2 No funcionales

| Ref | Requisito |
|---|---|
| RNF-01 | Windows 11, un solo PC, sin conexión a internet en ningún momento |
| RNF-02 | Ejecutable portable (idealmente un único .exe), sin instalador ni dependencias |
| RNF-03 | Fluidez: arranque < 2 s, respuesta inmediata en pantallas de preparación |
| RNF-04 | Base de datos local con cifrado opcional por contraseña maestra, modificable por el administrador |
| RNF-05 | Copia de seguridad automática al cerrar a ruta configurable, restaurable en otro PC |
| RNF-06 | Control de acceso: usuarios, credenciales, roles |
| RNF-07 | Auditoría de accesos y cambios (opcional si compleja → se incluye porque en el diseño elegido es barata, ver §5) |
| RNF-08 | Todo documento legal se imprime; el papel es la base legal |
| RNF-09 | Interfaz en castellano |
| RNF-10 | Toda la información se introduce una sola vez y se reutiliza |

---

## 2. Arquitectura y stack

### 2.1 Evaluación de opciones

El requisito que decide es RNF-02 + RNF-03: exe portable **y** fluido. Contrastado con la simplicidad de desarrollo.

| Opción | Portable | Arranque | Tamaño | SQLite cifrado | PDF | Desarrollo | Veredicto |
|---|---|---|---|---|---|---|---|
| **A. .NET 8 + Avalonia UI** | Sí, single-file self-contained | ~0,5–1 s | 60–80 MB | SQLitePCLRaw bundle_e_sqlcipher, maduro | QuestPDF | C#; Avalonia permite desarrollar en Linux | **Recomendada** |
| B. .NET 8 + WPF | Sí | ~0,5 s | 60–80 MB | Igual | Igual | Solo se desarrolla en Windows | Alternativa si no te importa desarrollar en Windows |
| C. Tauri 2 (Rust + HTML/JS) | Sí, exige WebView2 (viene en Win11) | ~0,3 s | 5–10 MB | rusqlite + sqlcipher, compilación delicada | Vía JS o Rust | Rust en backend, frontend web | Buena, pero dos lenguajes y compilación de SQLCipher en Windows es dolorosa |
| D. Python + PyInstaller onefile + pywebview | "Sí" | 4–10 s (descomprime en %TEMP% cada arranque) | 80–150 MB | Ruedas sqlcipher3 inestables en Windows | reportlab/weasyprint | Tu stack natural | **Descartada**: incumple RNF-03, falsos positivos de antivirus habituales, cifrado frágil |
| E. Electron | Sí (carpeta) | 1–2 s | 200+ MB | better-sqlite3-multiple-ciphers | Chromium print | JS | Descartada: tamaño y consumo no encajan con "portable y ligero" |

Nombro la rationalización antes de que aparezca: la opción D es la más cómoda para ti a corto plazo y la peor para el usuario final. Elegir Python "porque ya lo conozco" es la decisión que un año después obliga a reescribir. Si te niegas a C#, la alternativa honesta es C (Tauri), no D.

### 2.2 Stack elegido

```
Runtime        .NET 8 LTS (self-contained, single-file, win-x64, trimmed)
UI             Avalonia UI 11 + patrón MVVM (CommunityToolkit.Mvvm)
Base de datos  SQLite 3 vía Microsoft.Data.Sqlite
               + SQLitePCLRaw.bundle_e_sqlcipher (cifrado AES-256 opcional)
ORM            Dapper (SQL explícito, sin magia; el esquema lo controlamos nosotros)
Migraciones    Scripts SQL numerados embebidos, tabla schema_version
PDF/impresión  QuestPDF (licencia Community gratuita para < 1 M USD ingresos)
Hash contraseñas  Argon2id (Konscious.Security.Cryptography) — bcrypt como alternativa
Pictogramas    SVG embebidos como recursos
Logs           Serilog a fichero rotativo junto al exe
Tests          xUnit + SQLite en memoria
```

Resultado: `SPD.exe` de ~70 MB que se copia a una carpeta y funciona. Sin instalador, sin .NET previo, sin registro de Windows.

### 2.3 Estructura en disco (portable)

```
C:\SPD\                          ← carpeta elegida por la farmacia (o pendrive)
├── SPD.exe
├── spd.db                       ← base de datos (cifrada o no, ver §5)
├── config.json                  ← ruta de backup, ruta de impresión, preferencias UI
├── logs\
│   └── spd-20260904.log
├── plantillas\                  ← opcional: sobrescribir logo/cabeceras
│   └── logo.png
└── (backup en ruta configurada, por defecto .\backup\)
    └── spd-20260904-2130.bak    ← copia íntegra de spd.db + config.json, comprimida
```

Portabilidad real: copiar la carpeta completa a otro PC es una restauración. Si la BD está cifrada, la copia es igual de segura que el original.

### 2.4 Capas

```
┌──────────────────────────────────────────────┐
│ Presentación  (Avalonia Views + ViewModels)  │
├──────────────────────────────────────────────┤
│ Aplicación    (Servicios de caso de uso:     │
│   PacienteService, PreparacionService,       │
│   DocumentoService, ImportExportService,     │
│   BackupService, AuthService)                │
├──────────────────────────────────────────────┤
│ Dominio       (Entidades, reglas: validez    │
│   ≤14 días, verificador≠elaborador, etc.)    │
├──────────────────────────────────────────────┤
│ Infraestructura (Repositorios Dapper,        │
│   SQLite/SQLCipher, QuestPDF, ficheros)      │
└──────────────────────────────────────────────┘
```

Las reglas del PNT viven en Dominio, no en la UI ni en SQL, para que sean testeables y estén en un solo sitio.

**Decisiones:**
- **D-01** Stack: .NET 8 + Avalonia + SQLite/SQLCipher + Dapper + QuestPDF. Un exe portable.
- **D-02** Arquitectura en 4 capas; reglas del PNT en Dominio.
- **D-03** Toda la instalación es una carpeta; backup = copia íntegra de esa carpeta menos el exe.

---

## 3. Modelo de datos

### 3.1 Principios

1. **Un dato, un sitio.** Médico, medicamento, material de acondicionamiento, usuario: catálogos referenciados por clave, nunca texto repetido.
2. **Catálogo vivo + instantánea legal.** Lo que se imprime en un SPD debe ser reproducible aunque el catálogo cambie después. Por eso cada línea de SPD guarda copia de la posología y la descripción física en el momento de la preparación. El catálogo se edita libremente (tu respuesta 7); la historia queda intacta.
3. **Nada se borra.** Todas las entidades principales tienen `activo` y `fecha_baja`. La purga de bajas > 5 años es una utilidad explícita del administrador.
4. **Reutilización por sugerencia, no por obligación.** Al escribir un nombre de médico o CN, la app propone coincidencias del catálogo; el usuario acepta o crea uno nuevo. El catálogo crece con el uso, no exige carga previa.

### 3.2 Diagrama de entidades

```
Farmacia (1 fila) ──┐
                    │
Usuario ◄──── Rol   │
   │                │
   │ (elaborador, verificador, entregador, firma registros)
   ▼
Paciente ──── Contacto (representante / persona autorizada / familiar)
   │  └──── Medico (médico de cabecera)          ┐
   │                                             │ catálogo
   ├── EvaluacionIdoneidad                        │
   ├── Consentimiento                             │
   ├── Tratamiento ──── Medicamento (por CN) ─────┤
   │        └──── Medico (prescriptor) ───────────┘
   ├── Envase (depósito)  ──── Medicamento
   ├── ComunicacionMedico ──── Medico
   └── SPD ──── SPD_Linea ──── Tratamiento (origen)
          │         └──── SPD_Linea_Envase ──── Envase
          ├── MaterialAcondicionamiento (catálogo)
          └── RegistroAmbiental (la lectura tomada al preparar)

Registros de calidad (independientes del paciente):
   RegistroAmbiental · RegistroLimpieza · FormacionPersonal ·
   RecogidaResiduos · FirmaReconocida (= Usuario) · ControlCambiosPNT · ControlCopias

Transversales:
   Auditoria · SchemaVersion · PerfilImportacion
```

### 3.3 Entidades — definición campo a campo

Convenciones: `id` INTEGER PRIMARY KEY; fechas ISO-8601 en TEXT; booleanos INTEGER 0/1; todos los campos `creado_en`, `creado_por`, `modificado_en`, `modificado_por` existen en todas las tablas de negocio y se omiten abajo por brevedad.

#### Farmacia (configuración, una sola fila)
| Campo | Tipo | Notas |
|---|---|---|
| codigo | TEXT | "PO-123-F" |
| nombre | TEXT | |
| titular | TEXT | Nombre del titular/regente |
| titular_colegiado | TEXT | Nº colegiado |
| direccion, cp, poblacion, provincia | TEXT | |
| telefono, fax, email | TEXT | Aparecen en etiquetas y cartas |
| responsable_datos | TEXT | Responsable del tratamiento (LOPD), puede coincidir con titular |
| direccion_derechos, email_derechos | TEXT | Para el texto de protección de datos del consentimiento |
| prefijo_num_ficha | TEXT | Ej. "F-" → numeración F-000123 |
| prefijo_num_spd | TEXT | Ej. "SPD-" |
| ruta_backup | TEXT | Ruta local |
| temp_min, temp_max, hr_min, hr_max | REAL | Por defecto 15/25/40/60; rangos del PNT |
| validez_maxima_dias | INTEGER | 14 |

#### Rol
Valores fijos: `TITULAR`, `FARMACEUTICO`, `TECNICO`, `ADMIN` (ADMIN es capacidad, no puesto: se asigna al titular por defecto).

| Permiso | TITULAR | FARMACEUTICO | TECNICO |
|---|---|---|---|
| Ver y editar pacientes/tratamientos | ✔ | ✔ | ✔ (solo lectura, propuesta) |
| Preparar SPD (elaborador) | ✔ | ✔ | ✔ bajo supervisión (registra farmacéutico supervisor) |
| Verificar SPD | ✔ | ✔ | ✖ |
| Entregar SPD | ✔ | ✔ | ✔ |
| Registros de calidad (ambiental, limpieza) | ✔ | ✔ | ✔ |
| Formación, residuos, firmas, PNT | ✔ | ✔ | ✖ |
| Usuarios, contraseña maestra, backup, purga, configuración | ADMIN | — | — |

#### Usuario (= registro de firmas reconocidas, Anexo III del Anexo 11)
| Campo | Tipo | Notas |
|---|---|---|
| nombre, apellidos | TEXT | |
| login | TEXT UNIQUE | |
| hash_password | TEXT | Argon2id |
| rol | TEXT | FK Rol |
| es_admin | INTEGER | |
| cargo_pnt | TEXT | Texto tal como aparece en el organigrama: "Farmacéutico elaborador SPD" |
| colegiado | TEXT | Nulo para técnicos |
| firma_abreviada | TEXT | Iniciales que se imprimen en registros |
| activo, fecha_baja | | |
| debe_cambiar_password | INTEGER | Alta con contraseña provisional |

#### Medico (catálogo)
| Campo | Tipo | Notas |
|---|---|---|
| nombre, apellidos | TEXT | |
| colegiado | TEXT | Opcional |
| especialidad | TEXT | "Medicina de familia" por defecto |
| centro | TEXT | Centro de salud / hospital |
| telefono, email, direccion | TEXT | |
| activo | | |

Índice de búsqueda sobre `apellidos || ' ' || nombre` normalizado sin tildes.

#### Medicamento (catálogo, clave natural = CN)
| Campo | Tipo | Notas |
|---|---|---|
| cn | TEXT UNIQUE | 6 dígitos (7 con dígito de control si se importa del nomenclátor: se guardan los 6) |
| nombre | TEXT | Nombre comercial + dosis + forma, ej. "ENALAPRIL CINFA 20 mg comprimidos" |
| principio_activo | TEXT | Opcional, útil para duplicidades |
| laboratorio | TEXT | Opcional |
| forma_farmaceutica | TEXT | Catálogo cerrado: COMPRIMIDO, COMPRIMIDO_LP, CAPSULA, CAPSULA_LP, GRAGEA, PASTILLA, PILDORA, OTRA_NO_SPD |
| apto_spd | INTEGER | Derivado de forma + flags; editable manualmente con motivo |
| motivo_no_apto | TEXT | Ej. "termolábil", "efervescente", "informe laboratorio" |
| fraccionable | INTEGER | Comprimido ranurado / info del titular |
| unidades_envase | INTEGER | Para calcular envases necesarios |
| **Descripción física (la parte reutilizada):** | | |
| desc_forma | TEXT | redondo, oblongo, ovalado, cápsula dura, cápsula blanda… |
| desc_color | TEXT | |
| desc_ranura | TEXT | sin ranura / una ranura / cruz |
| desc_serigrafia | TEXT | Texto grabado |
| desc_tamano | TEXT | pequeño / mediano / grande |
| desc_texto | TEXT | Frase final que va a la etiqueta: "comprimido blanco redondo pequeño". **Se autogenera** de los campos anteriores y se puede sobrescribir |
| gtin | TEXT | GTIN-14 leído por DataMatrix; permite resolver CN al escanear |
| activo | | |

Historial de la descripción física: tabla `Medicamento_Hist` con copia de las columnas `desc_*` + `vigente_desde`. Se inserta una fila cada vez que cambian. Sirve para saber qué se imprimió en un SPD antiguo sin depender de la instantánea de la línea (doble seguridad, coste cero).

#### Paciente
| Campo | Tipo | Notas |
|---|---|---|
| num_ficha | TEXT UNIQUE | Autogenerado con prefijo configurable, no reutilizable |
| fecha_alta_ficha | TEXT | "Fecha de creación" del Anexo 2 |
| nombre, apellidos | TEXT | |
| dni | TEXT | |
| fecha_nacimiento | TEXT | |
| num_ss | TEXT | |
| cip | TEXT | Código de identificación personal (tarjeta sanitaria) |
| direccion, cp, poblacion | TEXT | |
| telefono1, telefono2, email | TEXT | |
| medico_id | INTEGER | FK Medico (cabecera / responsable) |
| enfermedades_cronicas | TEXT | Texto libre, líneas |
| alergias | TEXT | |
| observaciones | TEXT | |
| pictograma_comidas | INTEGER | Mostrar pictogramas antes/después de comer en etiqueta |
| identificador_visual | TEXT | Para convivientes: "punto verde", "pegatina azul" |
| estado | TEXT | EVALUACION, ACTIVO, SUSPENDIDO, BAJA |
| fecha_baja, motivo_baja | | |

#### Contacto (0..n por paciente)
| Campo | Tipo | Notas |
|---|---|---|
| paciente_id | FK | |
| tipo | TEXT | FAMILIAR, REPRESENTANTE_LEGAL, PERSONA_AUTORIZADA, CUIDADOR |
| nombre, apellidos, dni | TEXT | DNI necesario si firma el Anexo 1b |
| telefono, email | TEXT | |
| es_principal | INTEGER | El que aparece en la ficha |

#### EvaluacionIdoneidad (1..n por paciente, la última es la vigente)
| Campo | Tipo | Notas |
|---|---|---|
| paciente_id | FK | |
| fecha | TEXT | |
| farmaceutico_id | FK Usuario | |
| criterio_1 … criterio_7 | INTEGER | Los 7 del Anexo 9 |
| observaciones | TEXT | Justificación beneficio-riesgo |
| resultado | TEXT | APTO / NO_APTO |

#### Consentimiento (1..n por paciente; uno vigente)
| Campo | Tipo | Notas |
|---|---|---|
| paciente_id | FK | |
| tipo | TEXT | PACIENTE (1a) / REPRESENTANTE (1b) |
| contacto_id | FK | Quién firma si es 1b |
| fecha_firma | TEXT | Fecha del papel firmado |
| fecha_revocacion | TEXT | |
| impreso_en | TEXT | Trazabilidad de generación del documento |

Regla de dominio: no se puede preparar un SPD sin consentimiento vigente y evaluación APTO.

#### Tratamiento (una línea por medicamento activo del paciente = filas del Anexo 2 reverso)
| Campo | Tipo | Notas |
|---|---|---|
| paciente_id | FK | |
| medicamento_id | FK | |
| en_spd | INTEGER | Dentro o fuera del blíster (las dos tablas del Anexo 2 son la misma entidad con este flag) |
| problema_salud | TEXT | Indicación |
| medico_id | FK | Prescriptor; por defecto el de cabecera |
| **Posología:** | | |
| pauta_d, pauta_a, pauta_c, pauta_n | REAL | Unidades en desayuno/almuerzo/cena/noche. Admite 0.5, 0.25 |
| dias_semana | TEXT | Máscara "LMXJVSD" o subconjunto; por defecto todos. Cubre "puntualmente varía por día" |
| pauta_texto | TEXT | Para medicamentos fuera del SPD sin estructura D/A/C/N: "1 aplicación cada 12 h" |
| via | TEXT | ORAL por defecto |
| momento | TEXT | ANTES_COMER / DESPUES_COMER / INDIFERENTE → pictograma |
| fecha_inicio, fecha_fin | TEXT | fecha_fin nula = crónico |
| fecha_prescripcion_inicial, fecha_ultima_modificacion | TEXT | Anexo 7 |
| tipo | TEXT | CRONICO / ESPORADICO |
| conocimiento_cumplimiento | TEXT | Anotación de la entrevista |
| incidencias | TEXT | |
| intervencion | TEXT | |
| estado | TEXT | ACTIVO / SUSPENDIDO / FINALIZADO |

Un cambio de posología no edita la fila: crea una nueva y marca la anterior FINALIZADO con fecha. Así "F. inicio / F. fin / Últ. modificación" salen solas y el histórico del tratamiento es consultable.

Unidades semanales (Anexo 5) = `(pauta_d+pauta_a+pauta_c+pauta_n) × nº días marcados`. Calculado, no almacenado.

#### Envase (depósito de medicación en custodia)
| Campo | Tipo | Notas |
|---|---|---|
| paciente_id | FK | Propiedad del paciente |
| medicamento_id | FK | |
| lote | TEXT | |
| caducidad | TEXT | |
| serie | TEXT | Nº identificador único (21) |
| unidades_iniciales | INTEGER | Del catálogo, editable |
| unidades_restantes | INTEGER | Se descuenta en cada SPD |
| fecha_entrada | TEXT | Dispensación / depósito |
| origen | TEXT | ESCANEADO / MANUAL / IMPORTADO |
| estado | TEXT | EN_CUSTODIA / AGOTADO / DEVUELTO / RETIRADO (caducado, cambio de tratamiento → SIGRE) |
| fecha_salida, motivo_salida | | |

Esto es lo que hace posible la trazabilidad "envase dispensado → SPD preparado" y avisa cuando quedan unidades para menos de una semana.

#### MaterialAcondicionamiento (catálogo)
| Campo | Tipo |
|---|---|
| descripcion | TEXT ("Blíster 7×4 sellado adhesivo, marca X") |
| lote | TEXT |
| fecha_entrada | TEXT |
| activo | INTEGER |

Al preparar se propone el último lote usado.

#### SPD (cabecera de la ficha de preparación, control y entrega — Anexo 5)
| Campo | Tipo | Notas |
|---|---|---|
| num_registro | TEXT UNIQUE | Autogenerado |
| paciente_id | FK | |
| validez_desde, validez_hasta | TEXT | Regla: ≤ 14 días; `validez_hasta` ≤ mínima caducidad de los envases usados |
| justificacion_validez | TEXT | Obligatoria si > 7 días |
| fecha_preparacion | TEXT | |
| material_id | FK | |
| registro_ambiental_id | FK | La lectura Tª/HR tomada antes de preparar (se crea desde la misma pantalla) |
| elaborador_id | FK Usuario | |
| supervisor_id | FK Usuario | Obligatorio si el elaborador es TECNICO |
| verificador_id | FK Usuario | |
| fecha_verificacion | TEXT | |
| excepcion_verificador | TEXT | Motivo si verificador = elaborador |
| verif_aspecto, verif_etiqueta_datos, verif_etiqueta_validez, verif_instrucciones, verif_contenido | INTEGER | Checklist APTO/NO APTO |
| resultado_verificacion | TEXT | APTO / NO_APTO |
| entregador_id | FK Usuario | |
| fecha_entrega | TEXT | |
| entregado_a | TEXT | Paciente / nombre del cuidador |
| primera_entrega | INTEGER | Dispara checklist de primera entrega (sabe usarlo, entiende etiquetas, prospectos entregados) |
| spd_anterior_recogido | INTEGER | Control de adherencia |
| unidades_no_administradas | TEXT | Texto o JSON por medicamento |
| observaciones_adherencia | TEXT | |
| cambios_medicacion_preguntado | INTEGER | "Se le debe preguntar sobre posibles cambios" |
| observaciones_etiqueta | TEXT | Campo "Observaciones" del anverso |
| estado | TEXT | BORRADOR → PREPARADO → VERIFICADO → ENTREGADO / ANULADO |
| impreso_ficha_en, impreso_etiquetas_en, impreso_instrucciones_en | TEXT | Trazabilidad de impresión |

#### SPD_Linea (una por medicamento incluido en ese SPD)
| Campo | Tipo | Notas |
|---|---|---|
| spd_id | FK | |
| tratamiento_id | FK | Origen |
| medicamento_id | FK | |
| **Instantánea legal:** | | |
| snap_nombre, snap_cn | TEXT | |
| snap_pauta_d/a/c/n, snap_dias_semana | | |
| snap_desc_texto | TEXT | Descripción física tal cual se imprimió |
| snap_momento | TEXT | |
| unidades | REAL | Calculado al crear, congelado |
| incidencias | TEXT | Columna "Incidencias" del Anexo 5 |

#### SPD_Linea_Envase (qué envases alimentaron cada línea; normalmente 1, a veces 2 cuando se agota uno)
| Campo | Tipo |
|---|---|
| spd_linea_id | FK |
| envase_id | FK |
| unidades_tomadas | REAL |

De aquí salen lote, caducidad y nº de serie de la etiqueta reverso y del Anexo 5. Si hay dos envases, se imprimen ambos lotes.

#### ComunicacionMedico
| Campo | Tipo | Notas |
|---|---|---|
| paciente_id, medico_id | FK | |
| tipo | TEXT | PRESENTACION (Anexo 3) / INCIDENCIA (Anexo 4) / TELEFONO |
| fecha | TEXT | |
| incidencias_detectadas | TEXT | |
| propuesta | TEXT | |
| respuesta | TEXT | Lo que contestó el médico |
| fecha_respuesta | TEXT | |
| farmaceutico_id | FK | |

#### Registros de calidad

| Tabla | Campos |
|---|---|
| RegistroAmbiental | fecha, hora, temperatura, humedad, usuario_id, observaciones, fuera_rango (calculado y guardado), spd_id (nulo si es lectura rutinaria) |
| RegistroLimpieza | fecha, usuario_id, tipo (PRE_PREPARACION / POST_PREPARACION / RUTINARIA), observaciones |
| FormacionPersonal | usuario_id, tipo (FARMACEUTICO: curso+entidad / TECNICO: formador_id), nombre_curso, entidad, formador_id, fecha, acreditado |
| RecogidaResiduos | fecha, empresa_gestora, usuario_id, observaciones |
| ControlCambiosPNT | documento (código PNT), version, cambios, fecha, redactado_por, revisado_por, aprobado_por |
| ControlCopias | documento, num_copia, usuario_id, fecha |

#### Auditoria
| Campo | Tipo |
|---|---|
| fecha_hora | TEXT |
| usuario_id | FK |
| accion | TEXT (LOGIN, LOGOUT, LOGIN_FALLIDO, CREAR, EDITAR, BAJA, IMPRIMIR, EXPORTAR, IMPORTAR, BACKUP, RESTAURAR, CAMBIO_CLAVE_MAESTRA, PURGA) |
| entidad, entidad_id | TEXT, INTEGER |
| detalle | TEXT (JSON con campos cambiados: antes/después) |

Se escribe desde los repositorios; no requiere nada del usuario. Es la parte "barata" que mencioné: con Dapper es un `INSERT` en cada método de escritura.

#### PerfilImportacion
| Campo | Tipo |
|---|---|
| nombre | TEXT ("Farmatic pacientes", "Nixfarma dispensaciones") |
| tipo | TEXT (PACIENTES / DISPENSACIONES / MEDICAMENTOS) |
| separador, codificacion, tiene_cabecera | |
| mapeo | TEXT JSON: columna origen → campo destino + transformación (formato fecha, mayúsculas…) |

Los perfiles de los programas habituales vienen precargados y son editables; uno nuevo se crea desde el asistente de importación mapeando columnas. Detalle en Fase 4.

**Decisiones:**
- **D-04** Catálogos: Medico, Medicamento (por CN), MaterialAcondicionamiento, Usuario. Todo lo demás referencia por FK.
- **D-05** Instantánea legal en SPD_Linea + historial de descripción física en Medicamento_Hist.
- **D-06** Tratamiento inmutable: un cambio de pauta cierra la fila y abre otra.
- **D-07** Posología estructurada D/A/C/N con fracciones y máscara de días; texto libre solo para medicamentos fuera del SPD.
- **D-08** Envase como entidad de custodia con unidades restantes; trazabilidad por SPD_Linea_Envase.
- **D-09** Auditoría incluida de serie.

---

## 4. Mecánica de reutilización (tu requisito principal)

### 4.1 Médicos
- Campo de médico en ficha de paciente, tratamiento y comunicaciones = **buscador con autocompletado** sobre el catálogo. Escribir "Fern" muestra "Fernández Souto, Ana — CS A Estrada".
- Si no existe: botón "Nuevo médico" abre un formulario mínimo (apellidos, nombre, centro) sin salir de la pantalla. Queda en el catálogo.
- Al añadir un tratamiento, el prescriptor se **prerrellena con el médico de cabecera del paciente**. Se cambia solo si es un especialista.
- Al generar una carta (Anexos 3/4), los datos del médico y de la farmacia se toman de los catálogos: no se teclea nada.

### 4.2 Medicamentos
- Al añadir un tratamiento se busca por **CN o por nombre** (búsqueda por fragmento, sin tildes). Si el CN existe, se cargan forma farmacéutica, aptitud SPD, fraccionable, unidades por envase y descripción física.
- Si no existe: alta rápida con CN + nombre + forma. La descripción física puede rellenarse ahora o **la primera vez que se prepare un SPD con él** (la pantalla de preparación avisa "sin descripción física" y permite completarla ahí mismo; se guarda en el catálogo, no solo en el SPD).
- Cambiar la descripción física desde cualquier sitio actualiza el catálogo para todos los pacientes a partir de ese momento; los SPD ya preparados conservan su instantánea.
- `desc_texto` se autogenera ("cápsula dura roja y blanca, mediana, serigrafía 'A20'") y se puede corregir a mano.
- Lectura DataMatrix (si las pruebas salen bien): escanear un envase en el alta de depósito resuelve el CN desde el GTIN (para medicamentos españoles, GTIN-14 `0847000` + CN 6 dígitos + control), y rellena lote, caducidad y serie. Si el GTIN no está en el catálogo, se pide el CN una vez y se guarda el GTIN en el medicamento para la siguiente. Especificación del parser en Fase 4.

### 4.3 Tratamiento → preparación → semana siguiente
- **Nueva preparación** parte del tratamiento activo del paciente: todas las líneas `en_spd=1` y `estado=ACTIVO` se cargan con su posología. El elaborador no teclea posologías; solo confirma y asigna envases (propuesta automática: el envase en custodia con caducidad más próxima que tenga unidades).
- **Preparación siguiente**: botón "Preparar semana siguiente" sobre el último SPD entregado: nueva cabecera con validez = siguiente periodo, mismas líneas si el tratamiento no ha cambiado. Si ha cambiado, se marcan en color las líneas nuevas/modificadas/eliminadas respecto al SPD anterior (esto es la "revisión tras cambio de medicación" del PNT hecha visible).
- Material de acondicionamiento: se propone el último lote usado.
- Condiciones ambientales: si hay una lectura registrada en las últimas N horas (configurable, por defecto 2 h) se reutiliza; si no, se pide en la misma pantalla y se guarda en el registro ambiental general.

### 4.4 Documentos
Ningún documento tiene campos de texto libre que ya existan en otra parte. Ficha, etiquetas, instrucciones y cartas se generan íntegramente de la base de datos; los únicos campos editables en el momento de imprimir son "Observaciones" de la etiqueta y el cuerpo de la propuesta en la carta de incidencias.

### 4.5 Farmacia
Todo el bloque "Datos de identificación de la farmacia" que aparece en cada anexo sale de la fila única de configuración. Se rellena una vez en el primer arranque (asistente inicial).

---

## 5. Seguridad — decisiones de diseño (detalle en Fase 4)

- **Contraseña maestra** = clave de SQLCipher. Si el titular elige "sin cifrado" en la instalación, la BD es SQLite plano; puede activarse el cifrado después desde Administración (la app reescribe la BD con `sqlcipher_export`). Cambiar la contraseña maestra = `PRAGMA rekey`, instantáneo.
- La contraseña maestra **no se almacena**: se pide al arrancar la aplicación (una vez por sesión de PC). Los usuarios individuales tienen su propia contraseña (Argon2id) y entran después. Dos capas: "abrir la caja" (maestra) y "quién soy" (usuario).
- Consecuencia que tienes que aceptar: si se pierde la contraseña maestra y no hay backup sin cifrar, los datos son irrecuperables. Mitigación: en la activación del cifrado se genera una **clave de recuperación** de 24 palabras que se imprime y se guarda en el archivo físico de la farmacia. Sin esto no activo el cifrado en el diseño.
- Bloqueo automático de sesión de usuario tras N minutos de inactividad (configurable, por defecto 10).
- Backup al cerrar: copia de `spd.db` (con `VACUUM INTO`, consistente aunque haya escrituras) + `config.json` en un zip con nombre fechado en la ruta configurada. Rotación: conservar últimos 30 diarios + 12 mensuales. Restauración desde Administración o simplemente copiando la carpeta.
- Purga: utilidad de administrador que lista pacientes con `fecha_baja` < hoy − 5 años, obliga a marcar cada uno y confirmar con contraseña de administrador; borra en cascada (tratamientos, envases, SPD, comunicaciones) dejando una fila en Auditoría con num_ficha y fecha de purga.

---

## 6. Alcance por versiones

| Versión | Incluye |
|---|---|
| **1.0 (MVP)** | Configuración inicial · usuarios y roles · pacientes, contactos, médicos · idoneidad y consentimiento (impresión) · tratamiento · catálogo de medicamentos con descripción física · depósito de envases manual · preparación / verificación / entrega con reglas del PNT · impresión de Anexos 2, 5, 6 (anverso y reverso), 7 · registro ambiental y de limpieza · backup al cerrar · cifrado opcional · auditoría |
| 1.1 | Cartas al médico (Anexos 3, 4) y registro de comunicaciones · formación, residuos, control de cambios, control de copias · "preparar semana siguiente" con diff · avisos: envases insuficientes, caducidades, consentimientos, lecturas ambientales fuera de rango |
| 1.2 | Import/export con perfiles por programa de gestión · lector DataMatrix · importación de nomenclátor |
| 1.3 | Panel de indicadores (pacientes activos, SPD/semana, incidencias, adherencia) · purga de bajas > 5 años |

---

## 7. Puntos de revisión antes de la Fase 2

1. **Stack** (D-01). ¿Aceptas C# / .NET + Avalonia? Si no, dime si vamos a Tauri; no volveré a proponer Python empaquetado.
2. **Contraseña maestra al arrancar + contraseña de usuario después** (dos pasos). Alternativa: una sola contraseña (la del primer usuario que entra desbloquea la BD) — más cómoda, pero entonces cualquier usuario conoce implícitamente la clave de cifrado. ¿Dos pasos?
3. **Clave de recuperación impresa** al activar cifrado. ¿De acuerdo?
4. **Tratamiento inmutable** (D-06): un cambio de pauta genera una fila nueva. Implica que la ficha impresa muestra la historia si se quiere. ¿Te parece bien o prefieres editar en sitio y guardar solo la fecha de última modificación?
5. **Técnico como elaborador** con farmacéutico supervisor registrado. ¿Lo permitimos o solo farmacéuticos elaboran?
6. **Justificación obligatoria para validez > 7 días** (el PNT solo exige justificar > 14). Lo he puesto en 7 porque "normalmente 1 semana"; puede subirse a 14.
7. **Rotación de backups** 30 diarios + 12 mensuales en la ruta configurada. ¿Ajusto?
8. Alcance del MVP (§6). ¿Mueves algo entre versiones?

Con esto cerrado, la Fase 2 entrega el mapa de pantallas, el flujo de cada caso de uso y las reglas de validación campo a campo, más una maqueta HTML navegable para que lo pruebes antes de escribir una línea de C#.
