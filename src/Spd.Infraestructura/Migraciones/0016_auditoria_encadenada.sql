-- Spec 017: cada entrada nueva de auditoría guarda el hash SHA-256 de la anterior y el suyo propio,
-- de modo que modificar, borrar o reordenar entradas fuera de la aplicación rompe la cadena y se
-- detecta. Las entradas anteriores a esta migración quedan sin sellar (NULL): la tabla es de solo
-- inserción (Art. III.3) y no se reescriben.
ALTER TABLE Auditoria ADD COLUMN hash_previo TEXT;
ALTER TABLE Auditoria ADD COLUMN hash TEXT;
