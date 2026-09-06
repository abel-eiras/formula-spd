# Plan y progreso — Spec 014: Ayuda de la aplicación y guía de procedimiento

Este documento se actualiza al completar cada fase, cada test y cada prueba. No se reordena
retroactivamente lo ya marcado como hecho; solo se añade.

**Nota de contexto**: el propietario nombró el menú de ayuda como uno de los destinos del cribado de
`resources/` (`docs/analisis-resources.md` §3, mapa de contenido). Se bifurca de `main` tras Spec 002
y las cartas al médico; sin migración ni dependencias nuevas.

- **2026-09-06** — Ciclo `/speckit-specify` → `plan` → `tasks` → `implement` de forma autónoma.
  Contenido en Markdown embebido (FR-1405, Art. XI.5) con renderizador propio (títulos, listas,
  negrita, enlaces internos `[[seccion:id]]` como botones) — sin librería nueva (research.md
  Decisión 1).

- **F1 contextual** (FR-1401): `AyudaContextual.Registrar(this)` en las 21 ventanas posteriores al
  inicio de sesión, con tabla ventana → apartado de Procedimiento; cada apartado enlaza a su "Uso".
  Botón "Ayuda (F1)" en la pantalla principal (FR-1400).

- **"¿Por qué? (F1)"** (FR-1402) junto a la barra de mensajes de Preparación e Idoneidad/consentimiento,
  que abre "El porqué de los bloqueos": un apartado por regla con su origen (constitución Art. I.3,
  PNT I–V) y qué hacer.

- **Contenido**: 13 apartados de Procedimiento (servicio y requisitos; idoneidad; consentimiento y
  RGPD; ficha y tratamiento; depósito y retirada; preparación; verificación; entrega y continuidad;
  comunicación con el médico; residuos y SIGRE; personal/higiene/limpieza/ambiente/recepción de DDP;
  documentación por momento del servicio y conservación (FR-1403); porqué de los bloqueos) con los
  bloques *Qué exige el PNT / Qué hago en la aplicación / Qué pasa si se omite* (FR-1411), y 13 de
  Uso, uno por pantalla o grupo (FR-1412; un test cruza las tablas de ventanas con los apartados).

- **Búsqueda** (FR-1404) sin tildes ni mayúsculas con `Normalizador` de Spec 001; el título puntúa
  antes que el contenido y el resultado indica su sección.

## Pendiente (documentado, no fabricado)

- Q2: impresión del checklist de documentación como documento (`CHECKLIST-PROC`).
- "¿Por qué?" solo en Preparación e Idoneidad; el resto de pantallas tienen F1.
- Revisión de estilo del contenido por el propietario (el texto se redactó a partir de los PNT de A
  Coruña; si la fuente canónica pasa a ser el PNT de Pontevedra, cambian referencias de anexos).
- Prueba manual real (`dotnet run`): F1 y navegación por enlaces solo se han probado en headless.
