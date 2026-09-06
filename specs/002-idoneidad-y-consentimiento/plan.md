# Implementation Plan: Idoneidad y consentimiento informado

**Branch**: `002-idoneidad-y-consentimiento` | **Date**: 2026-09-06 | **Spec**: [spec.md](./spec.md)

## Summary

Evaluación de idoneidad (9 casillas literales del PNT I §4.1, resultado propuesto por Dominio y
decidido por el farmacéutico) y consentimiento informado (un modelo, Anexo I.B, para paciente o
representante), con activación automática EVALUACION→ACTIVO, revocación con sugerencia de suspensión,
documento `CONSENT`, evaluación impresa en `FICHA-PAC`, y sustitución del comprobador nulo de Spec 006
por el real.

## Constitution Check

- **Art. I.3**: el invariante "sin consentimiento vigente y APTO no se prepara" pasa a ser real
  (`ComprobadorIdoneidadYConsentimientoReal`).
- **Art. II**: la app no firma; registra la fecha del hecho (`fecha_firma`, `fecha_revocacion`).
- **Art. III**: evaluaciones y consentimientos solo se añaden; la revocación es un campo, no un DELETE.
- **Art. V.1**: la app propone el resultado y prerrellena el firmante desde los contactos.
- **Art. IX.1**: `ResultadoPropuesto()` y `Consentimiento.Vigente` con test unitario en Dominio.
- **Sin violaciones.**

## Technical Context

**Language/Version**: C# / .NET 8 (sin cambios). **Dependencies**: ninguna nueva.
**Storage**: migración `0011_idoneidad_consentimiento.sql` (dos tablas, ver data-model.md).
**Testing**: xUnit (Dominio/Aplicación) + Avalonia.Headless para la ventana nueva (patrón de
`ListBox`+`ItemTemplate` con comando de ancestro, regresión F5 de Spec 001).
**Project Type**: escritorio, 4 capas: `Spd.Dominio` (entidades, enums, repos, comprobador real),
`Spd.Aplicacion` (`IServicioIdoneidadConsentimiento`), `Spd.Infraestructura` (repos Dapper, migración,
generador `CONSENT`), `Spd.Presentacion` (`IdoneidadConsentimientoWindow` desde la ficha del paciente).
