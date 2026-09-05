# Spec 012 — Lectura de código DataMatrix

**Estado:** Borrador para revisión
**Versión:** 0.1 — 2026-09-04
**Constitución aplicable:** 2.1.0 (Artículos V, X)
**Depende de:** Spec 003 (medicamentos, campo GTIN), Spec 005 (alta de envase)
**Requerida por:** Ninguna; mejora opcional sobre Spec 005 FR-516

---

## 1. Propósito

Permitir capturar GTIN, lote, caducidad y número de serie de un envase escaneando su código DataMatrix (formato GS1), reduciendo la entrada manual en el alta de envase (Spec 005). Función marcada desde la Fase 1 como "útil pero a probar" por la dificultad real de la codificación española.

## 2. Actores

| Actor | Puede |
|---|---|
| Elaborador / Administrador | Usar el lector en cualquier pantalla de alta de envase |

## 3. Escenarios de usuario

### E1 — Escanear un envase conocido
Como elaborador, al dar de alta un envase de un medicamento cuyo GTIN ya está asociado a un CN, quiero escanear el código y que se rellenen CN, lote, caducidad y serie automáticamente.

### E2 — Escanear un envase de un medicamento nuevo para el sistema
Como elaborador, si el GTIN escaneado no está asociado a ningún CN todavía, quiero que se me pida el CN una sola vez y que el sistema recuerde esa asociación para la próxima vez.

### E3 — Código ilegible o escáner no disponible
Como elaborador, si el código no se lee bien o no tengo lector a mano, quiero poder introducir los mismos datos a mano sin que la pantalla me obligue a escanear.

## 4. Requisitos funcionales

- **FR-1200** Entrada por lector de código de barras USB que emula teclado (estándar en el sector, sin driver adicional) — la aplicación no requiere hardware de cámara ni driver propietario, solo captura el flujo de caracteres que el lector envía como si se tecleara.
- **FR-1201** Parseo del contenido GS1 DataMatrix según sus identificadores de aplicación estándar: `(01)` GTIN-14, `(10)` número de lote, `(17)` fecha de caducidad (AAMMDD), `(21)` número de serie. El parser reconoce estos identificadores en cualquier orden dentro de la cadena escaneada, separados por el carácter de control GS habitual en este estándar.
- **FR-1202** Si el GTIN-14 leído coincide con un `Medicamento.gtin` ya registrado, se rellenan automáticamente CN (heredado del medicamento), lote, caducidad y serie en el formulario de alta de envase (Spec 005 FR-510), dejando todos los campos editables antes de guardar.
- **FR-1203** Si el GTIN no está asociado a ningún medicamento, se pide el CN una vez (búsqueda igual que en el resto de la aplicación, Spec 003 FR-305); al guardar, el sistema ofrece asociar ese GTIN al medicamento elegido para futuras lecturas.
- **FR-1204** Si el código no se puede parsear (formato inesperado, lectura parcial), el sistema no bloquea nada: muestra un aviso y deja los campos vacíos para introducción manual, exactamente igual que si no se hubiera escaneado nada.
- **FR-1205** El campo de captura de escáner puede activarse o desactivarse por el usuario en cualquier pantalla de alta de envase, para no interferir con la introducción manual si no hay lector conectado.

## 5. Entidades clave

Ninguna nueva; usa `Medicamento.gtin` (Spec 003) y los campos existentes de `Envase` (Spec 005).

## 6. Criterios de aceptación

**CA-1200 GTIN conocido rellena todo**
Dado un GTIN ya asociado al CN 654321, cuando escaneo un envase con ese GTIN, entonces el formulario de alta de envase se rellena con CN, lote, caducidad y serie sin teclear nada.

**CA-1201 GTIN nuevo pide CN una vez**
Dado un GTIN no asociado a ningún medicamento, cuando lo escaneo y elijo el CN correspondiente, entonces al guardar el envase, ese GTIN queda asociado a ese CN para la próxima lectura.

**CA-1202 Código ilegible no bloquea**
Dado un código que el parser no reconoce, cuando lo escaneo, entonces veo un aviso y puedo seguir rellenando el formulario a mano sin ningún bloqueo.

**CA-1203 Alta manual sin escáner**
Dado que no tengo lector conectado, cuando abro el alta de envase, entonces puedo completar todos los campos a mano sin que la pantalla exija un escaneo previo.

## 7. Casos límite

- Dos envases físicos distintos con el mismo GTIN pero distinto lote/serie (normal, el GTIN identifica el producto, no la unidad): el sistema usa el GTIN solo para resolver el CN; lote, caducidad y serie siempre vienen del propio código escaneado, nunca copiados de una lectura anterior.
- Lector configurado con un sufijo de tecla Intro al final de cada lectura: se asume comportamiento estándar de los lectores del sector; si un modelo concreto da problemas, es una cuestión de configuración del propio lector, no de esta spec.

## 8. Fuera de alcance de esta spec

- Lectura por cámara/webcam: fuera de alcance de 1.2; el lector USB tipo teclado es el escenario probado y suficiente.
- Cualquier verificación adicional de autenticidad del medicamento (sistema de verificación de medicamentos SVM/FMD europeo): fuera de alcance total del proyecto.

## 9. Preguntas abiertas

| # | Pregunta | Propuesta si no hay respuesta |
|---|---|---|
| Q1 | ¿Merece la pena esta spec en la v1.2 tal como se estimó en la Fase 1, o el propietario prefiere probarlo con un lector real antes de comprometer el diseño del parser? | Mantener como opcional en 1.2; el alta manual (Spec 005) funciona igual de bien sin esto, así que no bloquea nada retrasarla |
