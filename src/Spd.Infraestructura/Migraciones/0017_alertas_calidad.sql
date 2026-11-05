-- Spec 018: alertas de calidad de medicamentos (retirada de lotes comunicada por la AEMPS, el
-- laboratorio o el Colegio). Una fila por CN y lote. Nunca se borran: se cierran con fecha y
-- motivo (Art. III.1).
CREATE TABLE AlertaCalidad (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    referencia TEXT NOT NULL,
    fecha_alerta TEXT NOT NULL,
    cn TEXT NOT NULL,
    lote TEXT NOT NULL,
    lote_normalizado TEXT NOT NULL,
    descripcion TEXT,
    activa INTEGER NOT NULL DEFAULT 1,
    fecha_cierre TEXT,
    motivo_cierre TEXT,
    creado_en TEXT NOT NULL DEFAULT (datetime('now')),
    creado_por INTEGER,
    modificado_en TEXT,
    modificado_por INTEGER
);

CREATE INDEX idx_alerta_calidad_cn_lote ON AlertaCalidad(cn, lote_normalizado);
