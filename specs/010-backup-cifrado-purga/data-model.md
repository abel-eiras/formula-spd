# Fase 1 — Modelo de datos: Backup, cifrado y purga (Spec 010, solo 4.1+4.2)

Sin tablas nuevas (spec.md, sección 5): esta spec opera sobre `Farmacia` (ya tiene `RutaBackup`
desde Spec 000) y `Auditoria`, ambas existentes. No hay migración SQL nueva.

## `config.json` (fuera de la base de datos, Art. VI.4)

Contenido de implementación (spec.md, "Fuera de alcance" ya lo excluye del negocio), pero se
documenta aquí porque es donde vive el estado del cifrado — nunca dentro de la propia base de
datos que ese estado protege.

```jsonc
{
  "cifrado": {
    // Ausente por completo si el cifrado nunca se ha activado.
    "sobreContrasena": {
      "salArgon2id": "base64...",
      "nonceAesGcm": "base64...",
      "textoCifradoMek": "base64...",
      "tagAesGcm": "base64..."
    },
    "sobreRecuperacion": {
      "salArgon2id": "base64...",
      "nonceAesGcm": "base64...",
      "textoCifradoMek": "base64...",
      "tagAesGcm": "base64..."
    }
  }
}
```

- `sobreContrasena`/`sobreRecuperacion`: la misma MEK (clave real de `PRAGMA key`) envuelta bajo
  dos claves derivadas distintas (research.md Decisión 3) — de la contraseña maestra elegida por
  el administrador, y de la frase de recuperación de 24 palabras. Ninguno de los dos secretos en
  claro (contraseña, frase, MEK) se guarda nunca en este fichero ni en ningún otro (Art. VII.2).
- Si `cifrado` está ausente, la base de datos no está cifrada — pero esto es solo una pista de
  arranque más rápida; el estado real siempre se confirma intentando abrir `spd.db` sin clave
  (research.md Decisión 2), nunca se confía ciegamente en este fichero (podría venir de un backup
  restaurado de otro momento, Art. VI.4).

## Entidades de dominio (`Spd.Dominio`, sin persistencia SQL)

- **BackupInfo** (no persistida, se construye listando ficheros): `NombreFichero`, `FechaHora`
  (parseada del nombre `spd-aaaammdd-hhmm.zip`), `EsMensual` (promovido por FR-1002), `TamanoBytes`.
- **SobreClave** (persistida solo dentro de `config.json`, nunca en SQL): `SalArgon2id`,
  `NonceAesGcm`, `TextoCifradoMek`, `TagAesGcm` — dos instancias, una por secreto (Decisión 3).
- **ClaveRecuperacionGenerada** (vive solo en memoria durante el flujo de activación/rekey, nunca
  persistida tal cual): las 24 palabras en texto plano, mostradas una vez para imprimir/guardar
  (FR-1010/CA-1003) y descartadas de memoria en cuanto el administrador confirma haberlas
  guardado.

## Relaciones con `Auditoria` (Art. VII.6)

Toda operación de esta spec deja traza: `BACKUP_MANUAL`, `BACKUP_AUTOMATICO`, `ACTIVAR_CIFRADO`,
`DESACTIVAR_CIFRADO`, `CAMBIAR_CONTRASENA_MAESTRA`, `RESTAURAR_BACKUP` — mismo patrón que
`RegistradorAuditoria` ya usado por todas las specs anteriores, sin cambios en su contrato.
