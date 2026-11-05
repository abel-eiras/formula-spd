-- spec-012 FR-1211: carácter que el lector (modo teclado) manda en lugar del separador GS.
ALTER TABLE Farmacia ADD COLUMN separador_lector TEXT;
-- Spec 016 (disposición del blíster): cómo se ve el blíster físico de esta farmacia en la rejilla de
-- preparación. Por defecto, la disposición que la aplicación usaba hasta ahora.
ALTER TABLE Farmacia ADD COLUMN blister_inicio TEXT NOT NULL DEFAULT 'CRONOLOGICO';
ALTER TABLE Farmacia ADD COLUMN blister_dias_derecha_izquierda INTEGER NOT NULL DEFAULT 0;
ALTER TABLE Farmacia ADD COLUMN blister_noche_arriba INTEGER NOT NULL DEFAULT 0;
