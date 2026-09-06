-- Corrección 2026-09-06 tras el cribado de resources/ (docs/analisis-resources.md):
-- los documentos generados deben contener los elementos mínimos de cada anexo del PNT
-- (constitución Art. I.2). Dos huecos de datos lo impedían.

-- Anexo I.D (información de protección de datos): el documento nombra al Delegado de
-- Protección de Datos (el del Colegio, normalmente) con su contacto. Farmacia ya tenía
-- responsable_datos / direccion_derechos / email_derechos; faltaba el DPO.
ALTER TABLE Farmacia ADD COLUMN dpo_nombre TEXT;
ALTER TABLE Farmacia ADD COLUMN dpo_contacto TEXT;

-- Anexo I.G (ficha de preparación, control y entrega): la verificación final son ocho
-- preguntas SÍ/NO, no cinco (Spec 006 FR-651 quedó corta; el PNT manda, Art. I.1). Las tres
-- que faltaban se añaden con DEFAULT 0: las verificaciones anteriores conservan su registro
-- tal cual, sin reinterpretación (Art. III).
ALTER TABLE SPD_Verificacion ADD COLUMN verif_fabricante_pnt INTEGER NOT NULL DEFAULT 0;
ALTER TABLE SPD_Verificacion ADD COLUMN verif_etiqueta_ficha_paciente INTEGER NOT NULL DEFAULT 0;
ALTER TABLE SPD_Verificacion ADD COLUMN verif_trazabilidad INTEGER NOT NULL DEFAULT 0;
