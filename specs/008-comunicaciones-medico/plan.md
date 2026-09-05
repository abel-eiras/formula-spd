# Implementation Plan: Comunicaciones al médico

**Branch**: `008-comunicaciones-medico` | **Date**: 2026-09-06 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `specs/008-comunicaciones-medico/spec.md`

## Summary

Registro de comunicaciones al médico (presentación del servicio, incidencia detectada, llamada
telefónica), inmutable salvo el campo de respuesta (Art. III/FR-807). FR-805 (aviso real desde una
entrega, Spec 006) y FR-806 (documento real, Spec 007) se implementan como puntos de extensión
invocables, sin fabricar las pantallas de esas specs futuras.

## Constitution Check

- **Art. I/II (base normativa, el papel es la base legal)**: la aplicación registra el hecho de la
  comunicación; la carta impresa en sí (Spec 007) sigue siendo el documento legal, esta spec no
  sustituye eso.
- **Art. III (nada se borra)**: `ComunicacionMedico` no tiene operación de borrado ni de edición
  general; solo `RegistrarRespuesta` (un único UPDATE acotado a dos campos).
- **Art. V (un dato, una entrada)**: el médico y el paciente se referencian por clave (Spec 001),
  nunca se copian sus datos salvo lo estrictamente necesario para el documento (fuera de alcance
  de esta spec).
- **Sin violaciones.**

## Technical Context

**Language/Version**: C# sobre .NET 8 LTS (sin cambios).

**Primary Dependencies**: Las mismas ya fijadas (Dapper, Microsoft.Data.Sqlite 10.0.10). Sin
dependencias nuevas.

**Storage**: SQLite, nueva migración `0007_comunicaciones_medico.sql` (main ya tiene 0001-0006
tras fusionar Specs 001/003/009/010/004/005).

**Testing**: xUnit (Dominio/Aplicación) + Avalonia.Headless.XUnit (Presentación), mismo patrón.

**Target Platform**: Windows 11 x64 portable, sin cambios.

**Project Type**: Aplicación de escritorio, 4 capas ya existentes — puebla `Spd.Dominio`
(`ComunicacionMedico`, `TipoComunicacionMedico`), `Spd.Aplicacion` (`ServicioComunicacionesMedico`),
`Spd.Infraestructura` (`RepositorioComunicacionesMedico`), `Spd.Presentacion` (botón "Comunicar
incidencia" en `TratamientoView`, nueva pantalla de comunicaciones del paciente).

**Performance Goals**: Sin objetivo numérico propio; volumen bajo por farmacia.

**Constraints**: Inmutabilidad salvo respuesta (Art. III/FR-807); SQL explícito sin ORM (Art.
VIII.4).

**Scale/Scope**: Varias comunicaciones por paciente a lo largo del tiempo, volumen bajo por
farmacia.
