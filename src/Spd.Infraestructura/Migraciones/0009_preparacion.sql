-- Preparación, verificación y entrega del SPD (Spec 006). Un blíster = un SPD (nunca una hoja
-- para dos). RegistroAmbiental y MaterialAcondicionamiento nacen aquí: Spec 009 las retiró de su
-- propio alcance a favor de esta spec (ver 0004_registros_calidad.sql, nota de alcance).

CREATE TABLE MaterialAcondicionamiento (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    descripcion TEXT NOT NULL,
    lote TEXT NOT NULL,
    fecha_entrada TEXT NOT NULL,
    activo INTEGER NOT NULL DEFAULT 1
);

CREATE TABLE RegistroAmbiental (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    fecha TEXT NOT NULL,
    temperatura REAL NOT NULL,
    humedad REAL NOT NULL,
    fuera_rango INTEGER NOT NULL,
    usuario_id INTEGER REFERENCES Usuario(id)
);

CREATE TABLE SPD (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    num_registro TEXT NOT NULL UNIQUE,
    correlativo_num_registro INTEGER NOT NULL,
    paciente_id INTEGER NOT NULL REFERENCES Paciente(id),
    version INTEGER NOT NULL DEFAULT 1,
    sesion_id TEXT NOT NULL,
    validez_desde TEXT NOT NULL,
    validez_hasta TEXT NOT NULL,
    fecha_preparacion TEXT,
    material_id INTEGER REFERENCES MaterialAcondicionamiento(id),
    registro_ambiental_id INTEGER REFERENCES RegistroAmbiental(id),
    elaborador_id INTEGER NOT NULL REFERENCES Usuario(id),
    verificador_id INTEGER REFERENCES Usuario(id),
    fecha_verificacion TEXT,
    excepcion_verificador_motivo TEXT,
    resultado_verificacion TEXT,
    entregador_id INTEGER REFERENCES Usuario(id),
    fecha_entrega TEXT,
    entregado_a TEXT,
    primera_entrega INTEGER NOT NULL DEFAULT 0,
    spd_anterior_recogido INTEGER,
    unidades_no_administradas TEXT,
    observaciones_adherencia TEXT,
    cambios_medicacion_preguntado INTEGER NOT NULL DEFAULT 0,
    observaciones_etiqueta TEXT,
    estado TEXT NOT NULL DEFAULT 'Borrador',
    motivo_anulacion TEXT,
    impreso_ficha_en TEXT,
    impreso_etiquetas_en TEXT,
    impreso_instrucciones_en TEXT,
    creado_en TEXT NOT NULL DEFAULT (datetime('now')),
    creado_por INTEGER,
    modificado_en TEXT,
    modificado_por INTEGER
);

CREATE INDEX idx_spd_paciente ON SPD(paciente_id);
CREATE INDEX idx_spd_sesion ON SPD(sesion_id);

CREATE TABLE SPD_Linea (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    spd_id INTEGER NOT NULL REFERENCES SPD(id),
    tratamiento_id INTEGER NOT NULL REFERENCES Tratamiento(id),
    medicamento_id INTEGER NOT NULL REFERENCES Medicamento(id),
    snap_nombre TEXT NOT NULL,
    snap_cn TEXT NOT NULL,
    snap_pauta_d TEXT,
    snap_pauta_a TEXT,
    snap_pauta_c TEXT,
    snap_pauta_n TEXT,
    snap_dias_semana TEXT NOT NULL,
    snap_desc_texto TEXT,
    snap_momento TEXT,
    unidades_dosis REAL NOT NULL,
    unidades_envase INTEGER NOT NULL,
    incidencias TEXT,
    estado_linea TEXT NOT NULL DEFAULT 'Normal',
    motivo_exclusion TEXT
);

CREATE INDEX idx_spd_linea_spd ON SPD_Linea(spd_id);

CREATE TABLE SPD_Linea_Envase (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    spd_linea_id INTEGER NOT NULL REFERENCES SPD_Linea(id),
    envase_id INTEGER NOT NULL REFERENCES Envase(id),
    unidades_tomadas REAL NOT NULL,
    snap_serie TEXT,
    snap_lote TEXT,
    snap_caducidad TEXT
);

CREATE INDEX idx_spd_linea_envase_linea ON SPD_Linea_Envase(spd_linea_id);

CREATE TABLE SPD_Verificacion (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    spd_id INTEGER NOT NULL REFERENCES SPD(id),
    verificador_id INTEGER NOT NULL REFERENCES Usuario(id),
    fecha TEXT NOT NULL,
    verif_aspecto INTEGER NOT NULL,
    verif_etiqueta_datos INTEGER NOT NULL,
    verif_etiqueta_validez INTEGER NOT NULL,
    verif_instrucciones INTEGER NOT NULL,
    verif_contenido INTEGER NOT NULL,
    resultado TEXT NOT NULL,
    excepcion_motivo TEXT
);

CREATE INDEX idx_spd_verificacion_spd ON SPD_Verificacion(spd_id);

CREATE TABLE SPD_Modificacion (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    spd_id INTEGER NOT NULL REFERENCES SPD(id),
    version_anterior INTEGER NOT NULL,
    version_nueva INTEGER NOT NULL,
    fecha TEXT NOT NULL,
    usuario_id INTEGER NOT NULL REFERENCES Usuario(id),
    origen_solicitud TEXT NOT NULL,
    motivo TEXT NOT NULL,
    resumen_cambios TEXT NOT NULL,
    lineas_snapshot_anterior TEXT NOT NULL
);

CREATE INDEX idx_spd_modificacion_spd ON SPD_Modificacion(spd_id);
