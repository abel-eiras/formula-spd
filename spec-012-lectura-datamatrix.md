# Spec 012 — Lectura de código DataMatrix

**Estado:** Implementada
**Versión:** 0.5 — 2026-11-05
**Constitución aplicable:** 2.1.0 (Artículos V, X)
**Depende de:** Spec 005 (alta de envase), Spec 004 (selector de medicamento)
**Requerida por:** Ninguna; mejora opcional sobre Spec 005 FR-516 y Spec 004 FR-400

---

## 1. Propósito

Permitir capturar lote, caducidad y número de serie de un envase escaneando su código DataMatrix (formato GS1), como método principal de entrada en el alta de envase (Spec 005), reduciendo la entrada manual. Función marcada desde la Fase 1 como "útil pero a probar" por la dificultad real de la codificación española; probada primero como prototipo aislado con códigos reales y con los patrones oficiales de SEVeM-0108.03 "Pruebas de validación de escáneres" antes de integrarla (ver `prototipos/lector-datamatrix/README.md` para el proceso y los hallazgos).

## 2. Actores

| Actor | Puede |
|---|---|
| Elaborador / Administrador | Usar el lector en cualquier pantalla de alta de envase |

## 3. Escenarios de usuario

### E1 — Escanear un envase
Como elaborador, al dar de alta un envase, quiero escanear el código y que se rellenen lote, caducidad y número de serie automáticamente, sin teclearlos.

### E2 — Escanear un envase cuyo código no trae el CN
Como elaborador, si el código escaneado no incluye el Código Nacional (no todos los fabricantes lo codifican), quiero que se rellenen igualmente lote, caducidad y serie, y completar el CN a mano como hasta ahora.

### E3 — Código ilegible o escáner no disponible
Como elaborador, si el código no se lee bien o no tengo lector a mano, quiero poder introducir los mismos datos a mano sin que la pantalla me obligue a escanear.

## 4. Requisitos funcionales

- **FR-1200** Entrada por lector de código de barras USB que emula teclado (estándar en el sector, sin driver adicional) — la aplicación no requiere hardware de cámara ni driver propietario, solo captura el flujo de caracteres que el lector envía como si se tecleara.
- **FR-1201** Parseo del contenido GS1 DataMatrix según sus identificadores de aplicación: `(01)` GTIN-14, `(10)` número de lote, `(17)` fecha de caducidad (AAMMDD), `(21)` número de serie y, si el fabricante lo incluye, `(712)` Código Nacional (identificador específico de España). El parser reconoce estos identificadores en cualquier orden. Dos rutas de lectura:
  - Con separador GS (estándar GS1): cada campo de longitud variable queda delimitado sin ambigüedad.
  - Sin separador GS (caso habitual en la práctica — ver Nota 1 de SEVeM-0108.03: el escáner puede no transmitirlo, o transmitirlo como otro carácter): se prueban todas las formas posibles de descomponer la cadena dentro de los límites de longitud de GS1 y solo se acepta si hay una única descomposición completa (los cuatro campos obligatorios presentes). Si hay cero o varias, se trata como código no reconocido (FR-1204) — no hay garantía matemática de que siempre exista una única descomposición, aunque en la práctica (5 códigos reales + 4 patrones oficiales de SEVeM probados) siempre la hubo.
- **FR-1202** Un escaneo reconocido rellena siempre lote, caducidad y número de serie en el formulario de alta de envase (Spec 005 FR-510), dejando todos los campos editables antes de guardar (día de caducidad sin especificar en el código, AI 17 con día `00`: se interpreta como el último día del mes, editable si no es correcto).
- **FR-1203** Si el código incluye el AI `712` (Código Nacional), se rellena también el campo CN. Si no lo incluye — caso habitual, no todos los fabricantes lo codifican —, el campo CN queda vacío y se introduce a mano exactamente igual que en el alta manual.
- **FR-1204** Si el código no se puede parsear (formato inesperado, lectura parcial, ambigüedad sin separador GS), el sistema no bloquea nada: muestra un aviso y deja los campos vacíos para introducción manual, exactamente igual que si no se hubiera escaneado nada.
- **FR-1205** El campo de captura de escáner convive con los campos manuales sin necesidad de activarlo o desactivarlo: si no hay lector conectado, se rellenan los campos a mano directamente y el campo de escaneo simplemente no se usa.
- **FR-1206** El selector de medicamento (Spec 004 FR-400, usado en el alta de tratamiento y en cualquier pantalla que elija un medicamento del catálogo) admite el mismo escaneo para encontrar el medicamento, no solo para registrar el envase: busca primero por CN si el código trae el AI 712; si no, por `Medicamento.gtin`. Sin coincidencia por ninguna de las dos vías, deja el CN tecleado en el buscador para que «Nuevo medicamento…» lo prerellene — no existe asociación automática GTIN→CN por catálogo, sigue siendo mejora futura si hiciera falta.

- **FR-1207** El buscador global de la cabecera (Spec 015 FR-1513) acepta un escaneo: al pulsar Intro sobre un texto que es un DataMatrix, se resuelve el medicamento (CN del AI 712, si no GTIN) y se listan los pacientes a los que les falta en el listado de retirada (Spec 005 FR-531). Con uno solo se va directamente a su retirada con el registro de ese medicamento abierto y la lectura aplicada; con varios, se elige; si no le falta a nadie se muestra el medicamento, y si no está en el catálogo se dice. Es el flujo del mostrador: tras dispensar en el programa de gestión, escanear la caja sin buscar antes al paciente.
- **FR-1208** Tras una lectura correcta en cualquier alta de envase, el foco pasa al campo de unidades (propuestas desde el catálogo, Spec 005 FR-513b) e Intro en ese campo guarda: escanear + Intro es un alta completa cuando el envase está entero.
- **FR-1209** (v0.5) El CN se deduce del GTIN español cuando el código no trae el AI 712: el GTIN-14 de un medicamento español es `0` + `847000` + CN de seis dígitos + dígito de control GS1 (el EAN-13 lineal de los medicamentos sin receta, el mismo sin el cero inicial). Solo se acepta si el dígito de control GS1 es correcto; el dígito de control propio del CN no se calcula ni se valida. Un AI 712 de siete dígitos (CN con su dígito de control) se busca también por sus seis primeros dígitos. Orden de identificación en todas las pantallas: AI 712 tal cual, AI 712 sin dígito de control, CN del GTIN, `Medicamento.gtin`. Sustituye a la limitación de FR-1206 («no existe asociación automática GTIN→CN»).
- **FR-1210** (v0.5) Lectura ambigua: si sin separador GS quedan varias descomposiciones válidas (con los cuatro campos obligatorios, GTIN con dígito de control correcto y fecha posible), no se adivina ni se descarta el código: la pantalla enseña las candidatas (lote · serie · caducidad) y la persona elige la que coincide con el lote impreso en la caja, o las descarta y teclea. Para identificar el medicamento basta con la identidad común (GTIN y CN coinciden en todas las candidatas), así que el selector de medicamento y el buscador global funcionan también con una lectura ambigua.
- **FR-1211** (v0.5) Separador sustituto: en Administración → «Lector de códigos» se configura el carácter que el lector (modo teclado) manda en lugar del GS; se trata exactamente como un GS en todas las pantallas a partir del siguiente escaneo. Uno a tres símbolos, sin letras, dígitos ni espacios. También se ignoran el identificador de simbología (`]d2`, `]C1`, `]Q3`) y un GS inicial (FNC1).
- **FR-1212** (v0.5) Prueba del lector: en la misma pantalla se escanea una caja y se enseñan los caracteres recibidos (los de control, visibles: ⟨GS⟩, ⟨CR⟩…), si llegó el GS o el separador sustituto, y lo interpretado (GTIN, CN y si es deducido, lote, caducidad, serie, o las candidatas si es ambiguo). Probar no guarda nada.

## 5. Entidades clave

Ninguna nueva; usa los campos existentes de `Envase` (Spec 005), incluido `Envase.Origen = ESCANEADO` cuando el alta se rellenó por esta vía. v0.5: `Farmacia.separador_lector` (migración 0015).

## 6. Criterios de aceptación

**CA-1200 Código con CN incluido rellena todo**
Dado un código DataMatrix que incluye el AI 712, cuando lo escaneo, entonces el formulario de alta de envase se rellena con CN, lote, caducidad y serie sin teclear nada.

**CA-1201 Código sin CN rellena el resto y pide el CN a mano**
Dado un código DataMatrix sin AI 712, cuando lo escaneo, entonces se rellenan lote, caducidad y serie, y el campo CN queda vacío para completarlo a mano.

**CA-1202 Código ilegible no bloquea**
Dado un código que el parser no reconoce, cuando lo escaneo, entonces veo un aviso y puedo seguir rellenando el formulario a mano sin ningún bloqueo.

**CA-1203 Alta manual sin escáner**
Dado que no tengo lector conectado, cuando abro el alta de envase, entonces puedo completar todos los campos a mano sin que la pantalla exija un escaneo previo.

**CA-1205 Escanear en el buscador lleva a la retirada**
Dado un paciente al que le faltan envases de un medicamento, cuando escaneo un envase de ese medicamento en el buscador de la cabecera, entonces se abre su retirada con serie, lote, caducidad y unidades ya puestos, y un Intro registra el envase.

**CA-1204 Escanear encuentra el medicamento al dar de alta un tratamiento**
Dado un medicamento ya en el catálogo con su GTIN guardado, cuando escaneo su envase desde el selector de medicamento de un tratamiento nuevo, entonces el medicamento queda seleccionado sin buscar por nombre ni CN.

**CA-1206 CN deducido del GTIN** (v0.5)
Dado un medicamento con CN 695064 en el catálogo sin GTIN guardado, cuando escaneo una caja con GTIN 08470006950647 sin AI 712, entonces el medicamento se identifica.

**CA-1207 Lectura ambigua** (v0.5)
Dado un código sin separador GS que admite dos lecturas, cuando lo escaneo al registrar un envase, entonces veo las dos candidatas, no se rellena nada hasta que elijo una, y al elegirla se rellenan lote, serie y caducidad de esa candidata.

**CA-1208 Separador sustituto** (v0.5)
Dado «~» configurado como separador sustituto, cuando escaneo una caja cuyo lector manda «~» en lugar del GS, entonces la lectura es inequívoca.

## 7. Casos límite

- Lector configurado con un sufijo de tecla Intro al final de cada lectura: se asume comportamiento estándar de los lectores del sector; si un modelo concreto da problemas, es una cuestión de configuración del propio lector, no de esta spec.
- Separador GS ausente o sustituido por otro carácter según la configuración del escáner (SEVeM-0108.03, nota 1): ver ruta de respaldo de FR-1201. Si un código real produce una ambigüedad genuina (varias descomposiciones completas), desde la v0.5 se ofrecen las candidatas (FR-1210); configurar el separador sustituto (FR-1211) la evita de raíz.

## 8. Fuera de alcance de esta spec

- Lectura por cámara/webcam: fuera de alcance; el lector USB tipo teclado es el escenario probado y suficiente.
- Aprender asociaciones GTIN→medicamento tras el primer escaneo: no implementado. Desde la v0.5 el CN se deduce del GTIN español (FR-1209), que cubre las cajas españolas; las extranjeras sin AI 712 siguen resolviéndose por `Medicamento.gtin` ya guardado.
- Cualquier verificación adicional de autenticidad del medicamento (sistema de verificación de medicamentos SVM/FMD europeo): fuera de alcance total del proyecto.

## 9. Preguntas abiertas

Ninguna. Q1 (¿merece la pena implementarlo?) quedó resuelta al probarlo con códigos reales y los patrones oficiales de SEVeM-0108.03 (2026-09-28): funciona, y se integró en Depósito, Retirada de envases y Preparación (los tres puntos de alta de envase de Spec 005 FR-516) y, desde la v0.3, también en el selector de medicamento (FR-1206) para encontrar el medicamento al escanear, no solo para registrar el envase.
