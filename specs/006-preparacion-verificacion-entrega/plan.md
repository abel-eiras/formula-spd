# Implementation Plan: Preparación, verificación y entrega del SPD

**Branch**: `006-preparacion-verificacion-entrega` | **Date**: 2026-09-06 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `specs/006-preparacion-verificacion-entrega/spec.md`

## Summary

Ciclo de vida completo del blíster (SPD): sesión de preparación (1 o 2 blísteres), líneas con
reparto multi-envase reutilizando el servicio de asignación de Spec 005, verificación por
blíster, entrega conjunta o parcial, continuidad ("preparar siguiente" sin repetir datos) y
reelaboración versionada antes de entregar. FR-602 (precondición de idoneidad/consentimiento, Art.
I.3) se implementa contra un punto de extensión hacia Spec 002, que no existe en esta rama.
`RegistroAmbiental`/`MaterialAcondicionamiento` nacen aquí, tal como Spec 009 ya había anticipado.
La impresión real (FR-680/681) queda como punto de extensión hacia Spec 007.

## Constitution Check

- **Art. I.3**: las invariantes de dominio con test unitario exigido se cubren así: validez 7 días
  (`FR-601`, dentro del límite ≤14 del Art. I.3) probado en la creación de sesión; "no se prepara
  sin consentimiento/idoneidad" probado contra `IComprobadorIdoneidadYConsentimiento`; "solo
  medicamentos aptos para SPD" ya lo filtra `ListarVigentesDePaciente(...).Where(t => t.EnSpd)`
  (Spec 004) — **corrección del 2026-09-14**: ese filtro solo mira si el tratamiento va en el blíster,
  nunca la aptitud del medicamento, así que la regla no estaba aplicada. La enmienda 3.0.0 del Art. I.3
  la convierte en confirmación del farmacéutico sin bloqueo, implementada como FR-692; "verificador ≠ elaborador, excepción con motivo" probado en `Verificar`; "toda
  preparación registra temperatura y humedad" probado en `PasarAPreparado` (exige
  `registroAmbientalId`).
- **Art. III.4**: reelaboración conserva número de registro, versiona, exige nueva verificación —
  cubierto por FR-6120..6127 con tests dedicados (CA-6120..6126).
- **Art. IV.3**: cada `SPD_Linea` guarda instantánea completa, nunca referencia el catálogo actual
  para reconstruir lo impreso.
- **Art. V**: `PrepararSiguiente` es la aplicación directa de "preparar semana siguiente" que el
  propio artículo cita como ejemplo.
- **Sin violaciones.**

## Technical Context

**Language/Version**: C# sobre .NET 8 LTS (sin cambios).

**Primary Dependencies**: Las mismas ya fijadas (Dapper, Microsoft.Data.Sqlite 10.0.10). Sin
dependencias nuevas — QuestPDF (Art. VIII.2) no se usa en esta spec: la generación real de
documentos es de Spec 007.

**Storage**: SQLite, nueva migración `0009_preparacion.sql` (main ya tiene 0001-0008 tras
fusionar Specs 001/003/009/010/004/005/008/011). Siete tablas nuevas: `MaterialAcondicionamiento`,
`RegistroAmbiental`, `SPD`, `SPD_Linea`, `SPD_Linea_Envase`, `SPD_Verificacion`, `SPD_Modificacion`.

**Testing**: xUnit (Dominio/Aplicación) + Avalonia.Headless.XUnit (Presentación), mismo patrón.

**Target Platform**: Windows 11 x64 portable, sin cambios.

**Project Type**: Aplicación de escritorio, 4 capas ya existentes — puebla `Spd.Dominio` (`Spd`,
`SpdLinea`, `SpdLineaEnvase`, `SpdVerificacion`, `SpdModificacion`, `RegistroAmbiental`,
`MaterialAcondicionamiento`, enums, `IComprobadorIdoneidadYConsentimiento`), `Spd.Aplicacion`
(`ServicioPreparacion`), `Spd.Infraestructura` (5 repositorios nuevos), `Spd.Presentacion`
(pantalla de sesión de preparación, verificación, entrega, listado de Preparaciones).

**Performance Goals**: Sin objetivo numérico propio; volumen bajo por farmacia (unos pocos
blísteres al día).

**Constraints**: Atomicidad de "pasar a PREPARADO" sin transacción cruzando repositorios
(research.md Decisión 3) — válido en el modelo de uso de un solo elaborador por preparación.

**Scale/Scope**: Es la spec de mayor superficie del proyecto hasta ahora (7 tablas, ciclo de vida
de 5 estados, reelaboración versionada). Se implementa por fases (user stories) con checkpoints
verificados por tests antes de avanzar a la siguiente.
