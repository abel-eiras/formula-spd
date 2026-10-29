-- Grupo explícito de catálogo y marca realmente usada en cada caja (Art. IV.3).
ALTER TABLE Medicamento ADD COLUMN grupo_homogeneo TEXT;
CREATE INDEX IF NOT EXISTS ix_medicamento_grupo_homogeneo ON Medicamento(grupo_homogeneo);
ALTER TABLE SPD_Linea_Envase ADD COLUMN snap_cn TEXT;
ALTER TABLE SPD_Linea_Envase ADD COLUMN snap_nombre TEXT;
ALTER TABLE SPD_Linea_Envase ADD COLUMN snap_desc_texto TEXT;
