# Spec 016 — Disposición del blíster

**Estado:** Implementada
**Versión:** 1.0 — 2026-11-05
**Constitución aplicable:** 3.0.0 (Artículos V, X, XI)
**Depende de:** Spec 000 (configuración de la farmacia), Spec 015 FR-1541 (rejilla de alvéolos)
**Requerida por:** Ninguna

---

## 1. Propósito

Que la rejilla de alvéolos de la pantalla de preparación se vea **igual que el blíster físico** que usa la farmacia. Los fabricantes difieren: unos imprimen los días por orden desde el primero, otros de lunes a domingo fijos; unos ponen el primer día a la izquierda y otros a la derecha; unos la toma del desayuno arriba y otros la de la noche. La traducción mental de una disposición a otra es justo donde se equivoca quien llena el blíster (Spec 015 FR-1541). Idea tomada del análisis de programas de SPD existentes; implementación propia.

## 2. Actores

| Actor | Puede |
|---|---|
| Administrador | Configurar la disposición en Configuración / Farmacia |
| Elaborador | Ver la rejilla de preparación con esa disposición |

## 3. Requisitos funcionales

- **FR-1601** Primera columna: **cronológica** (el primer día de validez a la izquierda y cada columna el día siguiente; la de siempre) o **convencional** (lunes a domingo fijos). En la convencional, cada columna lleva la primera fecha con ese día de la semana a partir del inicio de validez, de modo que todas las fechas siguen dentro del periodo de validez del SPD (Art. I.3) y cada etiqueta de columna lleva su fecha real.
- **FR-1602** Sentido de los días: de izquierda a derecha (por defecto) o de derecha a izquierda.
- **FR-1603** Orden de las tomas: desayuno arriba (por defecto) o noche arriba. Un blíster girado 180° es «derecha a izquierda» y «noche arriba» a la vez; no hay una opción aparte (Art. X.2).
- **FR-1604** Se configura una vez para toda la farmacia (Art. V.2) y se aplica al abrir la preparación de un paciente. Solo cambia cómo se pinta la rejilla: no cambia qué va en cada alvéolo, ni las líneas, ni los documentos.

## 4. Entidades clave

`Farmacia.blister_inicio` (`CRONOLOGICO` / `CONVENCIONAL`), `Farmacia.blister_dias_derecha_izquierda`, `Farmacia.blister_noche_arriba` (migración 0015). Dominio: `DisposicionBlister`.

## 5. Criterios de aceptación

**CA-1601** Dado un blíster que empieza en jueves con disposición convencional, cuando abro la preparación, entonces la primera columna es «LU» con la fecha del lunes siguiente y un tratamiento de solo lunes aparece en esa columna.

**CA-1602** Dada la disposición «derecha a izquierda» y «noche arriba», cuando abro la preparación, entonces el primer día de validez está en la última columna y la fila de arriba es la noche.

**CA-1603** Dada cualquier disposición convencional, entonces las siete columnas son siete fechas distintas dentro del periodo de validez.

## 6. Fuera de alcance

- Orden de llenado sugerido o lado por el que se empieza a llenar: la aplicación no guía el llenado alvéolo a alvéolo; una opción sin efecto sería una opción «por si acaso» (Art. X.1).
- Blísteres con un número de tomas distinto de cuatro: exigiría cambiar el dominio (Spec 015 FR-1541, Q2).
