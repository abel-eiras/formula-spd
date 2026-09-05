-- Registros de calidad (Spec 009): ambiental, limpieza, formación, residuos y control documental.
-- Numerada 0002 en esta rama porque parte de main (solo Spec 000); ver plan.md sobre la
-- coordinación de numeración pendiente con las ramas 001/003 al mergear.
-- Todas las tablas de esta spec son de solo alta (Art. III.1): ningún UPDATE/DELETE de negocio.

CREATE TABLE RegistroAmbiental (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    fecha_hora TEXT NOT NULL,
    temperatura REAL NOT NULL,
    humedad REAL NOT NULL,
    usuario_id INTEGER NOT NULL REFERENCES Usuario(id),
    observaciones TEXT,
    spd_id INTEGER, -- Nullable: no nulo solo cuando exista Spec 006 (FR-900)
    fuera_rango INTEGER NOT NULL, -- Congelado al registrar (Art. IV, CA-900), nunca recalculado
    creado_en TEXT NOT NULL DEFAULT (datetime('now'))
);

CREATE TABLE RegistroLimpieza (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    fecha TEXT NOT NULL,
    usuario_id INTEGER NOT NULL REFERENCES Usuario(id),
    tipo TEXT NOT NULL CHECK (tipo IN ('PRE_PREPARACION', 'POST_PREPARACION', 'RUTINARIA')),
    observaciones TEXT,
    creado_en TEXT NOT NULL DEFAULT (datetime('now'))
);

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

-- Adición aditiva (research.md Decisión 3, FR-950): umbral único configurable de aviso.
ALTER TABLE Farmacia ADD COLUMN umbral_dias_aviso_calidad INTEGER NOT NULL DEFAULT 7;
