# Fase 1 — Modelo de datos: Tratamiento del paciente

## Migración nueva: `Tratamiento`

Columnas (docs/data-model.md §Tratamiento, sin cambios de contenido, solo de implementación):

| Campo | Tipo SQL | Notas |
|---|---|---|
| id | INTEGER PK | |
| paciente_id | INTEGER FK Paciente | |
| medicamento_id | INTEGER FK Medicamento | |
| en_spd | INTEGER (bool) | |
| problema_salud | TEXT NULL | |
| medico_id | INTEGER FK Medico NULL | Prescriptor |
| pauta_d, pauta_a, pauta_c, pauta_n | TEXT NULL | Nombre del enum `FraccionDosis`, NULL si `en_spd=0` con `pauta_texto` |
| pauta_texto | TEXT NULL | Solo si `en_spd=0` (FR-401) |
| dias_semana | TEXT | Máscara "LMXJVSD", `1`/`0` por posición |
| via, momento | TEXT NULL | |
| fecha_inicio | TEXT | |
| fecha_fin | TEXT NULL | |
| fecha_prescripcion_inicial | TEXT | Copiada de la fila anterior al cerrar/abrir (FR-410) |
| tipo | TEXT | CRONICO/ESPORADICO |
| conocimiento_cumplimiento, incidencias, intervencion | TEXT NULL | Editables en el sitio (FR-411) |
| estado | TEXT | ACTIVO/SUSPENDIDO/FINALIZADO/PENDIENTE_REVISION |
| ajuste_unidades_manual | INTEGER NULL | Editable en el sitio (FR-411/FR-430) |
| creado_en, creado_por, modificado_en, modificado_por | | Convención ya usada en todas las tablas de negocio |

Sin `Tratamiento_Hist`: cada fila ya es una versión completa (research.md Decisión 1). El
"tratamiento vigente" de un medicamento para un paciente es la fila sin `fecha_fin` con `estado`
distinto de `FINALIZADO`.

## Enums nuevos (`Spd.Dominio`)

- **FraccionDosis**: `Cero, UnCuarto, UnTercio, Media, DosTercios, TresCuartos, Uno, UnoYCuarto,
  UnoYMedio` (research.md Decisión 2), con `Valor` (decimal) y `Texto` (p. ej. "1/2").
- **TipoTratamiento**: `Cronico, Esporadico`.
- **EstadoTratamiento**: `Activo, Suspendido, Finalizado, PendienteRevision`.

## Relaciones

`Tratamiento` referencia `Paciente` (Spec 001), `Medicamento` (Spec 003) y `Medico` (Spec 001) por
clave — nunca duplica su nombre en texto libre (Art. IV.1).
