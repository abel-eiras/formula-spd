-- Pacientes, contactos y catálogo de médicos (Spec 001).
-- Mismo patrón que 0001: SQL explícito, CHECK para dominios cerrados, baja lógica (Art. III.1).

CREATE TABLE Medico (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    nombre TEXT NOT NULL,
    apellidos TEXT NOT NULL,
    colegiado TEXT,
    especialidad TEXT NOT NULL DEFAULT 'Medicina de familia',
    centro TEXT,
    telefono TEXT,
    email TEXT,
    direccion TEXT,
    activo INTEGER NOT NULL DEFAULT 1,
    busqueda_normalizada TEXT NOT NULL,
    creado_en TEXT NOT NULL DEFAULT (datetime('now')),
    creado_por INTEGER,
    modificado_en TEXT,
    modificado_por INTEGER
);

CREATE TABLE Paciente (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    num_ficha TEXT NOT NULL UNIQUE,
    correlativo_num_ficha INTEGER NOT NULL UNIQUE,
    fecha_alta_ficha TEXT NOT NULL,
    nombre TEXT NOT NULL,
    apellidos TEXT NOT NULL,
    sexo TEXT CHECK (sexo IN ('M', 'H')),
    dni TEXT,
    fecha_nacimiento TEXT,
    num_ss TEXT,
    cip TEXT,
    direccion TEXT,
    cp TEXT,
    poblacion TEXT,
    telefono1 TEXT,
    telefono2 TEXT,
    email TEXT,
    medico_id INTEGER REFERENCES Medico(id),
    enfermedades_cronicas TEXT,
    alergias TEXT,
    observaciones TEXT,
    pictograma_comidas INTEGER NOT NULL DEFAULT 0,
    identificador_visual TEXT,
    dia_retirada TEXT NOT NULL CHECK (dia_retirada IN ('LU', 'MA', 'MI', 'JU', 'VI', 'SA', 'DO')),
    n_blisteres INTEGER NOT NULL DEFAULT 1 CHECK (n_blisteres IN (1, 2)),
    estado TEXT NOT NULL DEFAULT 'EVALUACION'
        CHECK (estado IN ('EVALUACION', 'ACTIVO', 'SUSPENDIDO', 'BAJA')),
    fecha_baja TEXT,
    motivo_baja TEXT
        CHECK (motivo_baja IN ('FALLECIMIENTO', 'RENUNCIA', 'TRASLADO',
                                'HOSPITALIZACION_PROLONGADA', 'CRITERIO_FARMACEUTICO', 'OTRO')),
    motivo_baja_detalle TEXT,
    busqueda_normalizada TEXT NOT NULL,
    creado_en TEXT NOT NULL DEFAULT (datetime('now')),
    creado_por INTEGER,
    modificado_en TEXT,
    modificado_por INTEGER
);

-- correlativo_num_ficha es la única fuente de verdad del "siguiente número" (research.md
-- Decisión 3); num_ficha se calcula una sola vez a partir de él y nunca se recalcula (FR-001).

CREATE TABLE Contacto (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    paciente_id INTEGER NOT NULL REFERENCES Paciente(id),
    tipo TEXT NOT NULL CHECK (tipo IN ('FAMILIAR', 'REPRESENTANTE_LEGAL', 'PERSONA_AUTORIZADA', 'CUIDADOR')),
    nombre TEXT NOT NULL,
    apellidos TEXT NOT NULL,
    dni TEXT,
    telefono TEXT,
    email TEXT,
    es_principal INTEGER NOT NULL DEFAULT 0,
    retira_medicacion INTEGER NOT NULL DEFAULT 0,
    activo INTEGER NOT NULL DEFAULT 1,
    fecha_baja TEXT,
    creado_en TEXT NOT NULL DEFAULT (datetime('now')),
    creado_por INTEGER,
    modificado_en TEXT,
    modificado_por INTEGER
);
