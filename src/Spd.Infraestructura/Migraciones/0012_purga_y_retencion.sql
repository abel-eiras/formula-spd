-- Spec 010 §4.3 (purga manual de pacientes con baja antigua). La constitución (Art. III.2) fija
-- "baja superior a cinco años"; el documento de protección de datos (Anexo I.D) que se entrega al
-- paciente enuncia el plazo, así que se guarda como valor configurable con ese defecto para que
-- lo impreso y lo aplicado coincidan siempre.
ALTER TABLE Farmacia ADD COLUMN anios_retencion_purga INTEGER NOT NULL DEFAULT 5;
