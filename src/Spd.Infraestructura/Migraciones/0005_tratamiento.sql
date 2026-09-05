-- Tratamiento del paciente (Spec 004). Versionado en la propia tabla: un cambio clínicamente
-- relevante cierra la fila vigente y crea una nueva (Art. IV.3/IV.4), en vez de una tabla de
-- historial separada como Medicamento_Hist (research.md Decisión 1).

CREATE TABLE Tratamiento (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    paciente_id INTEGER NOT NULL REFERENCES Paciente(id),
    medicamento_id INTEGER NOT NULL REFERENCES Medicamento(id),
    en_spd INTEGER NOT NULL DEFAULT 1,
    problema_salud TEXT,
    medico_id INTEGER REFERENCES Medico(id),
    pauta_d TEXT,
    pauta_a TEXT,
    pauta_c TEXT,
    pauta_n TEXT,
    pauta_texto TEXT,
    dias_semana TEXT NOT NULL DEFAULT '1111111',
    via TEXT,
    momento TEXT,
    fecha_inicio TEXT NOT NULL,
    fecha_fin TEXT,
    fecha_prescripcion_inicial TEXT NOT NULL,
    tipo TEXT NOT NULL DEFAULT 'Cronico',
    conocimiento_cumplimiento TEXT,
    incidencias TEXT,
    intervencion TEXT,
    estado TEXT NOT NULL DEFAULT 'Activo',
    ajuste_unidades_manual INTEGER,
    creado_en TEXT NOT NULL DEFAULT (datetime('now')),
    creado_por INTEGER,
    modificado_en TEXT,
    modificado_por INTEGER
);

CREATE INDEX idx_tratamiento_paciente ON Tratamiento(paciente_id);
CREATE INDEX idx_tratamiento_medicamento ON Tratamiento(paciente_id, medicamento_id);
