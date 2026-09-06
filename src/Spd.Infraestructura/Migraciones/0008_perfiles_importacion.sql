-- Perfiles genéricos de importación/exportación (Spec 011, FR-1100/1130). La exportación reutiliza
-- el mismo mapeo en sentido inverso (research.md Decisión 3): no hay tabla separada de exportación.

CREATE TABLE PerfilImportacion (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    nombre TEXT NOT NULL UNIQUE,
    tipo TEXT NOT NULL,
    separador TEXT NOT NULL DEFAULT ',',
    codificacion TEXT NOT NULL DEFAULT 'UTF-8',
    tiene_cabecera INTEGER NOT NULL DEFAULT 1,
    mapeo TEXT NOT NULL,
    regex_unidades_envase TEXT,
    creado_en TEXT NOT NULL DEFAULT (datetime('now')),
    creado_por INTEGER,
    modificado_en TEXT,
    modificado_por INTEGER
);
