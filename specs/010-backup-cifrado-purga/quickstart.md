# Quickstart de validación — Spec 010 (solo 4.1 Backup + 4.2 Cifrado)

## Prerrequisitos

```bash
dotnet build
dotnet test
```

## Escenarios

### CA-1000 — Backup consistente al cerrar
Con cambios pendientes de escritura, cerrar la aplicación → existe un `.zip` fechado en
`Farmacia.RutaBackup` cuya base de datos (`VACUUM INTO`) abre sin errores de integridad
(`PRAGMA integrity_check`).

### CA-1001 — Aviso visible si falla la ruta
Configurar `RutaBackup` a una ruta inaccesible (p. ej. una carpeta que no existe) → al cerrar la
aplicación, aparece un aviso explícito antes de que la ventana termine de cerrarse; el cierre no
se bloquea indefinidamente.

### CA-1002 — Rotación de backups
Generar 40 backups diarios de prueba (fechas distintas) → al generar el 41, solo quedan 30
diarios más los mensuales promovidos (el primero de cada mes); ningún dato de negocio (tablas de
`spd.db`) se ve afectado por la rotación.

### CA-1003 — Activar cifrado exige confirmar impresión
Activar el cifrado con una contraseña maestra → el sistema muestra la frase de recuperación de 24
palabras y no continúa (no llama a `ActivarCifrado`) hasta que el administrador confirma
explícitamente haberla impreso o guardado.

### CA-1004 — Rekey no exporta toda la base
Con el cifrado ya activo, cambiar la contraseña maestra → la operación re-envuelve la MEK
(milisegundos), sin re-cifrar página a página ni exportar/reimportar el contenido de la base.

## Notas

- CA-1005/CA-1006/CA-1007 (purga) están fuera de esta iteración — ver spec.md, "Fuera de alcance".
- Verificación de la clave de recuperación: perder (simular) la contraseña maestra y usar la frase
  de 24 palabras en su lugar debe desenvolver la misma MEK y abrir la base con normalidad —
  cualquier test de integración de `IServicioCifrado` debe cubrir ambos caminos (contraseña y
  frase) llegando al mismo resultado.
- Cualquier test que abra una segunda conexión al mismo fichero para comprobar contraseñas debe
  usar `Pooling=False` en la cadena de conexión (research.md, hallazgo de la prueba de Decisión 1)
  — si no, un test que debería fallar con una clave incorrecta puede pasar por error.
