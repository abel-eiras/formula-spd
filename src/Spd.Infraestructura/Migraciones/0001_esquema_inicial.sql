-- Esquema inicial: Farmacia (fila única), Usuario, Auditoria (Spec 000).
-- Toda tabla de negocio lleva creado_en/creado_por/modificado_en/modificado_por
-- (docs/data-model.md, convención). Auditoria es solo-INSERT (Art. III.3).

CREATE TABLE Farmacia (
    id INTEGER PRIMARY KEY CHECK (id = 1),
    codigo_sanitario TEXT NOT NULL,
    logo TEXT,
    nombre TEXT NOT NULL,
    titular_o_comunidad_bienes TEXT NOT NULL,
    cif TEXT NOT NULL,
    titular_colegiado TEXT,
    direccion TEXT NOT NULL,
    cp TEXT NOT NULL,
    poblacion TEXT NOT NULL,
    provincia TEXT,
    telefono TEXT NOT NULL,
    fax TEXT,
    email TEXT,
    whatsapp TEXT,
    responsable_datos TEXT,
    direccion_derechos TEXT,
    email_derechos TEXT,
    prefijo_num_ficha TEXT NOT NULL DEFAULT '',
    prefijo_num_spd TEXT NOT NULL DEFAULT '',
    ruta_backup TEXT,
    ruta_documentos_generados TEXT,
    url_nomenclator TEXT,
    umbral_reutilizacion_lectura_ambiental_horas INTEGER NOT NULL DEFAULT 2,
    temp_min REAL NOT NULL DEFAULT 15,
    temp_max REAL NOT NULL DEFAULT 25,
    hr_min REAL NOT NULL DEFAULT 40,
    hr_max REAL NOT NULL DEFAULT 60,
    dia_retirada_defecto TEXT NOT NULL DEFAULT 'LU',
    n_blisteres_defecto INTEGER NOT NULL DEFAULT 1,
    dias_antelacion_listado INTEGER NOT NULL DEFAULT 2,
    creado_en TEXT NOT NULL DEFAULT (datetime('now')),
    creado_por INTEGER,
    modificado_en TEXT,
    modificado_por INTEGER
);

-- CHECK (id = 1): Farmacia es una fila única en toda la instalación (Art. IV.1).

CREATE TABLE Usuario (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    nombre TEXT NOT NULL,
    apellidos TEXT NOT NULL,
    login TEXT NOT NULL UNIQUE,
    hash_password TEXT NOT NULL,
    rol TEXT NOT NULL CHECK (rol IN ('ADMINISTRADOR', 'ELABORADOR')),
    cargo_pnt TEXT,
    colegiado TEXT,
    firma_abreviada TEXT,
    activo INTEGER NOT NULL DEFAULT 1,
    fecha_baja TEXT,
    debe_cambiar_password INTEGER NOT NULL DEFAULT 0,
    intentos_fallidos_consecutivos INTEGER NOT NULL DEFAULT 0,
    bloqueado INTEGER NOT NULL DEFAULT 0,
    creado_en TEXT NOT NULL DEFAULT (datetime('now')),
    creado_por INTEGER,
    modificado_en TEXT,
    modificado_por INTEGER
);

-- Auditoria: solo INSERT en toda la aplicación (Art. III.3, Art. VII.6).
CREATE TABLE Auditoria (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    fecha_hora TEXT NOT NULL,
    usuario_id INTEGER,
    accion TEXT NOT NULL,
    entidad TEXT NOT NULL,
    entidad_id INTEGER,
    detalle TEXT
);
