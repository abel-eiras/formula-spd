# Fase 1 — Modelo de datos: Ayuda

Sin persistencia (spec.md §5).

## EntradaAyuda (en memoria, `Spd.Presentacion.Ayuda`)

| Campo | Notas |
|---|---|
| Seccion | `uso` / `procedimiento` |
| Id | Parte del nombre de fichero tras `NNN-`; contrato para F1, enlaces y "¿Por qué?" |
| Orden | `NNN` del nombre de fichero |
| Titulo | Primera línea `# …` |
| Contenido | Resto del Markdown |

Fuente: recursos embebidos `Spd.Presentacion/Ayuda/{seccion}__{NNN}-{id}.md`.
