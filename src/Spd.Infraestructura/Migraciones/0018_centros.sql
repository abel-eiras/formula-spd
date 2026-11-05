-- Spec 019: centros (residencias, centros de día) a los que la farmacia entrega blísteres de varios
-- pacientes. Catálogo (Art. IV.1): se define una vez y se referencia por clave. Baja lógica (Art. III.1).
CREATE TABLE Centro (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    nombre TEXT NOT NULL,
    direccion TEXT,
    telefono TEXT,
    persona_contacto TEXT,
    activo INTEGER NOT NULL DEFAULT 1,
    fecha_baja TEXT,
    motivo_baja TEXT,
    creado_en TEXT NOT NULL DEFAULT (datetime('now')),
    creado_por INTEGER,
    modificado_en TEXT,
    modificado_por INTEGER
);

ALTER TABLE Paciente ADD COLUMN centro_id INTEGER REFERENCES Centro(id);
CREATE INDEX idx_paciente_centro ON Paciente(centro_id);
