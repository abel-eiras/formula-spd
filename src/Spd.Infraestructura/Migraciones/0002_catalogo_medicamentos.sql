-- Catálogo de medicamentos (Spec 003).
-- Numerada 0002 en esta rama porque parte de main (solo Spec 000); ver plan.md §Project
-- Structure sobre la coordinación de numeración pendiente con la rama 001 al mergear.

CREATE TABLE Medicamento (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    cn TEXT NOT NULL UNIQUE,
    nombre TEXT NOT NULL,
    nombre_normalizado TEXT NOT NULL,
    principio_activo TEXT,
    laboratorio TEXT,
    forma_farmaceutica TEXT
        CHECK (forma_farmaceutica IN ('COMPRIMIDO', 'COMPRIMIDO_LIBERACION_PROLONGADA', 'CAPSULA',
                                       'CAPSULA_LIBERACION_PROLONGADA', 'GRAGEA', 'PASTILLA',
                                       'PILDORA', 'OTRA_NO_APTA')),
    apto_spd INTEGER NOT NULL DEFAULT 1,
    motivo_no_apto TEXT,
    fraccionable INTEGER NOT NULL DEFAULT 0,
    unidades_envase INTEGER,
    unidades_envase_origen TEXT NOT NULL DEFAULT 'MANUAL'
        CHECK (unidades_envase_origen IN ('MANUAL', 'IMPORTADO_REGEX')),
    desc_forma TEXT,
    desc_color TEXT,
    desc_ranura TEXT,
    desc_serigrafia TEXT,
    desc_tamano TEXT,
    desc_texto TEXT,
    desc_vigente_desde TEXT NOT NULL DEFAULT (datetime('now')),
    gtin TEXT,
    activo INTEGER NOT NULL DEFAULT 1,
    creado_en TEXT NOT NULL DEFAULT (datetime('now')),
    creado_por INTEGER,
    modificado_en TEXT,
    modificado_por INTEGER
);

-- Medicamento_Hist: solo inserción (Art. III.3); cada fila documenta un periodo de vigencia ya
-- cerrado de la descripción física (research.md Decisión 4).
CREATE TABLE Medicamento_Hist (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    medicamento_id INTEGER NOT NULL REFERENCES Medicamento(id),
    desc_forma TEXT,
    desc_color TEXT,
    desc_ranura TEXT,
    desc_serigrafia TEXT,
    desc_tamano TEXT,
    desc_texto TEXT,
    vigente_desde TEXT NOT NULL,
    vigente_hasta TEXT NOT NULL
);
