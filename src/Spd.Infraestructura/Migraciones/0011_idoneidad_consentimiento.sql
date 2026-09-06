-- Spec 002: evaluación de idoneidad (bloque del Anexo I.E del PNT I) y consentimiento informado
-- (Anexo I.B). Ninguna de las dos tablas admite DELETE (Art. III): la evaluación es solo INSERT
-- (la más reciente es la vigente, FR-202) y el consentimiento solo se actualiza para anotar la
-- firma, la revocación o la impresión (FR-212/214).

CREATE TABLE EvaluacionIdoneidad (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    paciente_id INTEGER NOT NULL REFERENCES Paciente(id),
    fecha TEXT NOT NULL,
    farmaceutico_id INTEGER REFERENCES Usuario(id),
    criterio_1 INTEGER NOT NULL DEFAULT 0,
    criterio_2 INTEGER NOT NULL DEFAULT 0,
    criterio_3 INTEGER NOT NULL DEFAULT 0,
    criterio_4 INTEGER NOT NULL DEFAULT 0,
    criterio_5 INTEGER NOT NULL DEFAULT 0,
    criterio_6 INTEGER NOT NULL DEFAULT 0,
    criterio_7 INTEGER NOT NULL DEFAULT 0,
    condicion_motivacion INTEGER NOT NULL DEFAULT 0,
    condicion_destreza INTEGER NOT NULL DEFAULT 0,
    observaciones TEXT,
    resultado TEXT NOT NULL CHECK (resultado IN ('APTO', 'NO_APTO'))
);

CREATE INDEX idx_evaluacion_idoneidad_paciente ON EvaluacionIdoneidad(paciente_id);

CREATE TABLE Consentimiento (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    paciente_id INTEGER NOT NULL REFERENCES Paciente(id),
    tipo TEXT NOT NULL CHECK (tipo IN ('PACIENTE', 'REPRESENTANTE')),
    contacto_id INTEGER REFERENCES Contacto(id),
    fecha_creacion TEXT NOT NULL,
    fecha_firma TEXT,
    fecha_revocacion TEXT,
    motivo_revocacion TEXT,
    impreso_en TEXT
);

CREATE INDEX idx_consentimiento_paciente ON Consentimiento(paciente_id);
