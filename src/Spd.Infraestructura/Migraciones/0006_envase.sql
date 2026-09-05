-- Depósito de envases (Spec 005). Un envase pertenece a un paciente y a un medicamento y nunca
-- se reasigna (Art. III); solo transiciones de estado, nunca eliminación.

CREATE TABLE Envase (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    paciente_id INTEGER NOT NULL REFERENCES Paciente(id),
    medicamento_id INTEGER NOT NULL REFERENCES Medicamento(id),
    serie TEXT,
    lote TEXT,
    caducidad TEXT,
    unidades_iniciales INTEGER,
    unidades_restantes INTEGER,
    fecha_entrada TEXT NOT NULL DEFAULT (datetime('now')),
    origen TEXT NOT NULL DEFAULT 'Manual',
    estado TEXT NOT NULL DEFAULT 'EnCustodia',
    fecha_salida TEXT,
    motivo_salida TEXT,
    motivo_salida_detalle TEXT,
    entregado_a TEXT,
    creado_en TEXT NOT NULL DEFAULT (datetime('now')),
    creado_por INTEGER,
    modificado_en TEXT,
    modificado_por INTEGER
);

CREATE UNIQUE INDEX idx_envase_serie ON Envase(serie) WHERE serie IS NOT NULL;
CREATE INDEX idx_envase_paciente ON Envase(paciente_id);
CREATE INDEX idx_envase_paciente_medicamento_estado ON Envase(paciente_id, medicamento_id, estado);

-- Perfil de importación tratamiento+envase (Spec 005, FR-570-577). Motor genérico compartido con
-- Spec 011 diferido; este perfil solo cubre el mapeo mínimo de FR-571.
CREATE TABLE PerfilImportacionTratamiento (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    nombre TEXT NOT NULL UNIQUE,
    origen TEXT NOT NULL,
    separador TEXT,
    tiene_cabecera INTEGER,
    mapeo TEXT NOT NULL,
    creado_en TEXT NOT NULL DEFAULT (datetime('now')),
    creado_por INTEGER
);
