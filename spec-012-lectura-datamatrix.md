# Spec 012 — Lectura de código DataMatrix

**Estado:** Implementada
**Versión:** 0.2 — 2026-09-28
**Constitución aplicable:** 2.1.0 (Artículos V, X)
**Depende de:** Spec 005 (alta de envase)
**Requerida por:** Ninguna; mejora opcional sobre Spec 005 FR-516

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

## 5. Entidades clave

Ninguna nueva; usa los campos existentes de `Envase` (Spec 005), incluido `Envase.Origen = ESCANEADO` cuando el alta se rellenó por esta vía.

## 6. Criterios de aceptación

**CA-1200 Código con CN incluido rellena todo**
Dado un código DataMatrix que incluye el AI 712, cuando lo escaneo, entonces el formulario de alta de envase se rellena con CN, lote, caducidad y serie sin teclear nada.

**CA-1201 Código sin CN rellena el resto y pide el CN a mano**
Dado un código DataMatrix sin AI 712, cuando lo escaneo, entonces se rellenan lote, caducidad y serie, y el campo CN queda vacío para completarlo a mano.

**CA-1202 Código ilegible no bloquea**
Dado un código que el parser no reconoce, cuando lo escaneo, entonces veo un aviso y puedo seguir rellenando el formulario a mano sin ningún bloqueo.

**CA-1203 Alta manual sin escáner**
Dado que no tengo lector conectado, cuando abro el alta de envase, entonces puedo completar todos los campos a mano sin que la pantalla exija un escaneo previo.

## 7. Casos límite

- Lector configurado con un sufijo de tecla Intro al final de cada lectura: se asume comportamiento estándar de los lectores del sector; si un modelo concreto da problemas, es una cuestión de configuración del propio lector, no de esta spec.
- Separador GS ausente o sustituido por otro carácter según la configuración del escáner (SEVeM-0108.03, nota 1): ver ruta de respaldo de FR-1201. Si un código real produce una ambigüedad genuina (varias descomposiciones completas), se trata como código no reconocido (FR-1204); revisar antes la configuración del lector suele ser más fiable que depender de la heurística.

## 8. Fuera de alcance de esta spec

- Lectura por cámara/webcam: fuera de alcance; el lector USB tipo teclado es el escenario probado y suficiente.
- Asociación automática de GTIN a CN a través del catálogo: el campo `Medicamento.gtin` existe en el catálogo (Spec 003) pero esta spec no lo usa para resolver el CN de una lectura — el CN solo se rellena cuando el propio código lo trae (AI 712); en cualquier otro caso se introduce a mano. Posible mejora futura si aparece necesidad real.
- Cualquier verificación adicional de autenticidad del medicamento (sistema de verificación de medicamentos SVM/FMD europeo): fuera de alcance total del proyecto.

## 9. Preguntas abiertas

Ninguna. Q1 (¿merece la pena implementarlo?) quedó resuelta al probarlo con códigos reales y los patrones oficiales de SEVeM-0108.03 (2026-09-28): funciona, y se integró en Depósito, Retirada de envases y Preparación (los tres puntos de alta de envase de Spec 005 FR-516).
