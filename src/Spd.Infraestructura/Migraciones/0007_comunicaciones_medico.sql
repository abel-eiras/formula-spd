-- Comunicaciones al médico (Spec 008). Inmutable salvo el campo de respuesta (Art. III/FR-807):
-- ninguna otra columna se actualiza tras el alta.

CREATE TABLE ComunicacionMedico (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    paciente_id INTEGER NOT NULL REFERENCES Paciente(id),
    medico_id INTEGER NOT NULL REFERENCES Medico(id),
    tipo TEXT NOT NULL,
    fecha TEXT NOT NULL,
    incidencias_detectadas TEXT,
    propuesta TEXT,
    respuesta TEXT,
    fecha_respuesta TEXT,
    farmaceutico_id INTEGER REFERENCES Usuario(id),
    creado_en TEXT NOT NULL DEFAULT (datetime('now')),
    creado_por INTEGER,
    modificado_en TEXT,
    modificado_por INTEGER
);

CREATE INDEX idx_comunicacion_medico_paciente ON ComunicacionMedico(paciente_id);
