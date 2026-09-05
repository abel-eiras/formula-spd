-- Registros de calidad (Spec 009): formación, residuos y control documental.
-- Numerada 0002 en esta rama porque parte de main (solo Spec 000); ver plan.md sobre la
-- coordinación de numeración pendiente con las ramas 001/003 al mergear.
-- Todas las tablas de esta spec son de solo alta (Art. III.1): ningún UPDATE/DELETE de negocio.
--
-- Nota de alcance (2026-09-05, tras prueba manual del usuario): el registro ambiental y de
-- limpieza como pantallas independientes de esta spec se retiraron. La temperatura/humedad se
-- rellenará más adelante, opcionalmente, al generar la hoja de elaboración del blíster
-- (Spec 006/007) — no tiene sentido un registro ambiental "suelto" cuando las hojas de un día se
-- generan todas a la vez pero los blísteres se preparan a lo largo de la jornada con
-- temperatura/humedad distintas. Ver PROGRESO.md para el detalle de la decisión.

CREATE TABLE FormacionPersonal (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    usuario_id INTEGER NOT NULL REFERENCES Usuario(id),
    nombre_curso TEXT NOT NULL,
    entidad_organizadora TEXT,
    fecha TEXT NOT NULL,
    acreditado INTEGER NOT NULL DEFAULT 0,
    creado_en TEXT NOT NULL DEFAULT (datetime('now'))
);

CREATE TABLE RecogidaResiduos (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    fecha TEXT NOT NULL,
    empresa_gestora TEXT NOT NULL,
    usuario_id INTEGER NOT NULL REFERENCES Usuario(id),
    observaciones TEXT,
    creado_en TEXT NOT NULL DEFAULT (datetime('now'))
);

CREATE TABLE ControlCambiosPNT (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    documento TEXT NOT NULL,
    version TEXT NOT NULL,
    descripcion_cambio TEXT NOT NULL,
    fecha TEXT NOT NULL,
    redactado_por INTEGER NOT NULL REFERENCES Usuario(id),
    revisado_por INTEGER NOT NULL REFERENCES Usuario(id),
    aprobado_por INTEGER NOT NULL REFERENCES Usuario(id),
    creado_en TEXT NOT NULL DEFAULT (datetime('now'))
);

CREATE TABLE ControlCopias (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    documento TEXT NOT NULL,
    num_copia INTEGER NOT NULL,
    usuario_id INTEGER NOT NULL REFERENCES Usuario(id),
    fecha TEXT NOT NULL,
    creado_en TEXT NOT NULL DEFAULT (datetime('now'))
);
