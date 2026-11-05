# Spec 017 — Integridad de la auditoría

**Estado:** Implementada
**Versión:** 1.0 — 2026-11-05
**Constitución aplicable:** 3.0.0 (Artículos III.3, VII.6, X)
**Depende de:** Spec 000 (auditoría), Spec 010 (seguridad)
**Requerida por:** Ninguna

---

## 1. Propósito

El Art. III.3 dice que la auditoría es de solo inserción para la aplicación. Esta spec permite **demostrarlo**: cada entrada lleva un sello (hash SHA-256) que incluye el sello de la anterior, de forma que modificar, borrar, reordenar o intercalar entradas directamente en el fichero de la base de datos rompe la cadena y una comprobación lo detecta. Útil ante una inspección.

## 2. Requisitos funcionales

- **FR-1701** Cada entrada nueva guarda `hash_previo` (el sello de la última entrada sellada, o 64 ceros si es la primera) y `hash` = SHA-256 de `hash_previo`, fecha-hora, usuario, acción, entidad, id de entidad y detalle, cada campo precedido de su longitud (un nulo se distingue de un texto vacío).
- **FR-1702** Configuración / Seguridad → «Comprobar la auditoría» (Administrador) recorre toda la tabla en orden de inserción y dice si está íntegra o cuál es la primera entrada con problema y por qué: contenido modificado, no enlaza con la anterior (falta, sobra o se reordenó una entrada) o entrada sin sellar posterior al inicio del sellado (insertada por fuera).
- **FR-1703** Las entradas anteriores a la migración 0016 quedan sin sellar y se cuentan aparte; no son un problema. No se reescriben (Art. III.3: no hay UPDATE sobre la auditoría).
- **FR-1704** La comprobación deja su propia traza con el resultado (Art. VII.6), escrita después de comprobar.

## 3. Entidades clave

`Auditoria.hash_previo`, `Auditoria.hash` (migración 0016). Dominio: `CadenaAuditoria`, `EntradaAuditoria`, `ResultadoCadenaAuditoria`.

## 4. Criterios de aceptación

**CA-1701** Dadas entradas registradas por la aplicación, cuando compruebo la auditoría, entonces está íntegra.
**CA-1702** Dada una entrada modificada con SQL directo, cuando compruebo, entonces se señala esa entrada como modificada.
**CA-1703** Dada una entrada intermedia borrada con SQL directo, cuando compruebo, entonces se detecta que la cadena no enlaza.

## 5. Límites conocidos

- Borrar las **últimas** entradas no deja rastro dentro de la propia tabla; lo cubren las copias de seguridad automáticas al cerrar (Art. VI.6), que conservan la cadena de cada momento.
- Quien tenga el fichero y conozca el algoritmo podría recalcular toda la cadena: el sellado detecta manipulaciones, no las impide. El cifrado (Spec 010) es lo que impide leer o escribir el fichero sin contraseña.
