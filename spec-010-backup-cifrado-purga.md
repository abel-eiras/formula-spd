# Spec 010 — Backup, cifrado y purga

**Estado:** Borrador para revisión
**Versión:** 0.1 — 2026-09-04
**Constitución aplicable:** 2.1.0 (Artículos III, VI, VII)
**Depende de:** Spec 000 (rutas y primer arranque)
**Requerida por:** Ninguna; es transversal

---

## 1. Propósito

Copias de seguridad, activación/desactivación de cifrado de la base de datos, cambio de contraseña maestra, y la única eliminación física de datos permitida: la purga manual de pacientes dados de baja hace más de cinco años.

## 2. Actores

| Actor | Puede |
|---|---|
| Administrador | Todo lo de esta spec, en exclusiva |

## 3. Escenarios de usuario

### E1 — Backup automático al cerrar
Como administrador, quiero que al cerrar la aplicación se guarde una copia consistente sin tener que acordarme de hacerlo yo.

### E2 — Restaurar en otro ordenador
Como administrador, si el PC de la farmacia se rompe, quiero poder copiar la carpeta de instalación a otro equipo y seguir trabajando con todos los datos.

### E3 — Activar el cifrado más adelante
Como administrador, si al principio no activé el cifrado y luego cambio de opinión, quiero poder activarlo sin perder nada, con la clave de recuperación impresa antes de que se active de verdad.

### E4 — Cambiar la contraseña maestra
Como administrador, quiero poder cambiar la contraseña maestra sin tener que descifrar y volver a cifrar toda la base de datos manualmente.

### E5 — Limpiar pacientes muy antiguos
Como administrador, de vez en cuando quiero revisar los pacientes dados de baja hace más de cinco años y decidir, uno a uno, si los elimino definitivamente.

## 4. Requisitos funcionales

### 4.1 Backup

- **FR-1000** Al cerrar la aplicación, se ejecuta `VACUUM INTO` sobre la base de datos hacia un fichero temporal, se comprime junto con `config.json` en un `.zip` fechado (`spd-aaaammdd-hhmm.zip`) y se copia a `Farmacia.ruta_backup` (Spec 000 FR-030).
- **FR-1001** Si la ruta de backup no está disponible al cerrar (unidad de red desconectada, disco lleno), se avisa al usuario de forma visible antes de cerrar del todo, no en un log que nadie revisa; el cierre no se bloquea, pero el aviso no puede ignorarse sin verlo.
- **FR-1002** Rotación: se conservan los últimos 30 backups diarios y los últimos 12 backups mensuales (el primero de cada mes se promueve a mensual); el resto se elimina automáticamente — esta es la única eliminación automática de todo el sistema, y afecta solo a ficheros de backup, nunca a datos de negocio.
- **FR-1003** Backup manual bajo demanda desde Configuración, con el mismo procedimiento, en cualquier momento, sin esperar al cierre.
- **FR-1004** Restauración: apuntar la aplicación a una carpeta de instalación completa copiada de otro sitio (incluyendo `spd.db`, `config.json`) es una restauración válida por sí misma (Constitución Artículo VI.4); adicionalmente, un asistente de "Restaurar desde backup" permite descomprimir un `.zip` de `ruta_backup` sobre una instalación nueva.

### 4.2 Cifrado

- **FR-1010** Activar cifrado: pide la nueva contraseña maestra dos veces, genera una clave de recuperación de 24 palabras, **exige que el administrador confirme explícitamente haberla impreso o guardado** antes de proceder (Constitución Artículo VII.3), y entonces reescribe la base de datos con `sqlcipher_export`.
- **FR-1011** Desactivar cifrado: pide la contraseña maestra actual, confirma con un aviso explícito de las implicaciones (el fichero `spd.db` quedará legible por cualquiera con acceso a la carpeta), y reescribe la base de datos sin cifrar.
- **FR-1012** Cambio de contraseña maestra: `PRAGMA rekey`, instantáneo, sin necesidad de exportar/reimportar toda la base de datos. Genera una nueva clave de recuperación y exige la misma confirmación de impresión que FR-1010.
- **FR-1013** La contraseña maestra se solicita una vez por sesión de la aplicación (al arrancar), no se almacena en ningún fichero ni variable persistente (Constitución Artículo VII.2).
- **FR-1014** Si el cifrado está activo, cada backup (FR-1000) contiene la base de datos ya cifrada; el backup no añade ni quita seguridad respecto al original.

### 4.3 Purga

- **FR-1020** Utilidad "Purgar pacientes antiguos", solo Administrador. Lista pacientes con `estado = BAJA` y `fecha_baja` anterior a hoy menos 5 años.
- **FR-1021** La purga es **paciente a paciente**, nunca masiva de un clic: el administrador marca cada paciente que quiere eliminar de la lista, y confirma con su propia contraseña antes de ejecutar.
- **FR-1022** La purga elimina físicamente al paciente y sus datos relacionados en cascada: contactos, evaluaciones, consentimientos, tratamientos, envases, SPD y sus líneas, comunicaciones. Antes de borrar, deja una fila de auditoría con `num_ficha`, nombre y fecha de purga — el hecho de que existió y fue purgado queda registrado aunque el contenido desaparezca.
- **FR-1023** No existe purga automática ni programada bajo ningún concepto (Constitución Artículo III.2). Esta utilidad nunca se ejecuta sola.
- **FR-1024** Un paciente con envases todavía `EN_CUSTODIA` (no debería ocurrir tras 5 años de baja, pero se comprueba) no se purga sin antes resolver esos envases (salida a SIGRE o similar).

## 5. Entidades clave

Ninguna nueva; opera sobre `Farmacia` (rutas), `Auditoria`, y en cascada sobre todas las entidades de un paciente (Spec 001, 002, 004, 005, 006, 008).

## 6. Criterios de aceptación

**CA-1000 Backup consistente al cerrar**
Dado que cierro la aplicación con cambios pendientes de escritura, cuando reviso la carpeta de backup, entonces existe un `.zip` fechado cuya base de datos abre sin errores de integridad.

**CA-1001 Aviso visible si falla la ruta**
Dado que la ruta de backup no es accesible, cuando cierro la aplicación, entonces veo un aviso explícito antes de que la aplicación termine de cerrarse.

**CA-1002 Rotación de backups**
Dado 40 backups diarios acumulados, cuando se genera el número 41, entonces solo quedan 30 diarios más los mensuales promovidos, sin haber tocado ningún dato de negocio.

**CA-1003 Activar cifrado exige confirmar impresión**
Dado que activo el cifrado, cuando el sistema genera la clave de recuperación, entonces no continúa hasta que confirmo explícitamente haberla impreso o guardado.

**CA-1004 Rekey no exporta toda la base**
Dado el cifrado ya activo, cuando cambio la contraseña maestra, entonces la operación es prácticamente instantánea (rekey), no una exportación completa.

**CA-1005 Purga uno a uno con contraseña**
Dado tres pacientes elegibles para purga, cuando marco solo uno y confirmo con mi contraseña, entonces únicamente ese paciente se elimina; los otros dos siguen existiendo.

**CA-1006 Traza de purga sobrevive al borrado**
Dado un paciente purgado, cuando reviso auditoría, entonces encuentro la entrada de purga con su número de ficha y nombre, aunque el paciente ya no exista en ninguna otra tabla.

**CA-1007 Purga bloqueada por envases en custodia**
Dado un paciente elegible para purga con un envase todavía EN_CUSTODIA, cuando intento purgarlo, entonces el sistema lo impide hasta resolver ese envase.

## 7. Casos límite

- Cifrado activo y se pierde la contraseña maestra sin la clave de recuperación a mano: los datos son irrecuperables por diseño (Constitución Artículo VII.3); esta spec no añade ninguna puerta trasera.
- Backup en curso justo cuando el sistema se apaga (corte de luz): `VACUUM INTO` escribe a un fichero temporal antes de mover/comprimir, de forma que un corte a mitad deja como mucho un backup incompleto descartable, nunca corrompe `spd.db`.
- Purga de un paciente que aparece referenciado como médico prescriptor... no aplica (los médicos son una entidad distinta); un paciente purgado no deja referencias colgantes porque el borrado es en cascada completo sobre sus propias tablas.

## 8. Fuera de alcance de esta spec

- Contenido y formato exacto del fichero de configuración `config.json` (detalle de implementación, no de negocio).
- Import/export con otros programas (Spec 011): no es backup, es intercambio de datos con terceros.

## 9. Preguntas abiertas

| # | Pregunta | Propuesta si no hay respuesta |
|---|---|---|
| Q1 | FR-1002: ¿30 diarios + 12 mensuales sigue siendo el reparto deseado, o prefieres ajustarlo? | Mantener; es la decisión ya tomada en la Fase 1 |
