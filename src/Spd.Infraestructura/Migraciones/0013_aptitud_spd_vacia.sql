-- Spec 003 FR-301, revisado el 2026-09-14 por decisión del propietario: la aptitud SPD de un
-- medicamento puede estar VACÍA, que significa «nadie la ha confirmado todavía» — ni apto ni no apto.
-- Los medicamentos que llegan del nomenclátor no traen ese dato, y es el farmacéutico quien la
-- confirma al elaborar (Spec 006).
--
-- SQLite no permite quitar un NOT NULL con ALTER TABLE, así que se reconstruye la tabla:
--   · se copian TODAS las filas y columnas tal cual, conservando los id, que referencian Tratamiento,
--     Envase, SPD_Linea y Medicamento_Hist;
--   · los valores 0 y 1 ya guardados NO cambian (Art. III: nada se pierde ni se reescribe);
--   · solo desaparece el DEFAULT 1, de modo que lo nuevo nace sin confirmar.
--
-- Las claves foráneas están ACTIVAS en la conexión, y PRAGMA foreign_keys no se puede cambiar dentro
-- de la transacción con la que el aplicador envuelve cada migración. Por eso:
--   1. se aplazan las comprobaciones al COMMIT (defer_foreign_keys sí vale dentro de la transacción y
--      se desactiva sola al terminarla);
--   2. se copia la tabla aparte, se borra la original y se vuelve a crear con el MISMO nombre: las
--      tablas hijas la referencian por nombre, y al reinsertar los mismos id cada referencia vuelve a
--      encontrar su fila antes del COMMIT. Renombrar la original no sirve: SQLite reescribiría las
--      REFERENCES de las tablas hijas para seguirla.
-- Si faltara una sola fila, el COMMIT fallaría y la migración entera se desharía.
-- El CHECK impide cualquier valor que no sea 0, 1 o vacío.

PRAGMA defer_foreign_keys = ON;

CREATE TABLE Medicamento_copia AS SELECT * FROM Medicamento;

DROP TABLE Medicamento;

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
    apto_spd INTEGER CHECK (apto_spd IN (0, 1)),
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

INSERT INTO Medicamento (
    id, cn, nombre, nombre_normalizado, principio_activo, laboratorio, forma_farmaceutica,
    apto_spd, motivo_no_apto, fraccionable, unidades_envase, unidades_envase_origen,
    desc_forma, desc_color, desc_ranura, desc_serigrafia, desc_tamano, desc_texto,
    desc_vigente_desde, gtin, activo, creado_en, creado_por, modificado_en, modificado_por
)
SELECT
    id, cn, nombre, nombre_normalizado, principio_activo, laboratorio, forma_farmaceutica,
    apto_spd, motivo_no_apto, fraccionable, unidades_envase, unidades_envase_origen,
    desc_forma, desc_color, desc_ranura, desc_serigrafia, desc_tamano, desc_texto,
    desc_vigente_desde, gtin, activo, creado_en, creado_por, modificado_en, modificado_por
FROM Medicamento_copia;

DROP TABLE Medicamento_copia;
