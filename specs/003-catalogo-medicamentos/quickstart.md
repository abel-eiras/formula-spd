# Quickstart de validación — Spec 003

## Prerrequisitos

```bash
dotnet build
dotnet test
```

## Escenarios

### CA-300 — Alta mínima
Crear un medicamento con solo CN `654321` y nombre "Paracetamol 1g" → se guarda y aparece marcado
"descripción física pendiente" en cualquier pantalla que lo liste.

### CA-301 — Edición no reescribe instantáneas
Cambiar la descripción física de un medicamento de "comprimido blanco" a "comprimido amarillo" →
la ficha del medicamento muestra la nueva descripción y el historial conserva "comprimido blanco"
con su periodo de vigencia; ningún dato fuera de esta spec (líneas de SPD) se ve afectado, porque
esta spec no crea SPD.

### CA-302 — Aptitud editable con motivo
Medicamento con forma "Comprimido" (apto por defecto) → marcarlo manualmente como no apto sin
rellenar motivo → el sistema no guarda y pide el motivo. Con motivo relleno, guarda.

### CA-303 — CN duplicado bloqueado
Crear un medicamento con CN `654321` ya existente y activo → el sistema lo impide y muestra el
existente (nombre, estado).

### CA-304 — Importación no sobrescribe descripción física
Medicamento con descripción física completa; importar un fichero de nomenclátor (CSV con columnas
`CN`,`Nombre`) que incluye su CN → la pantalla de revisión no propone ningún cambio de descripción
física ni de aptitud SPD, solo compara nombre.

### CA-305 — Reactivación de CN dado de baja
Medicamento con CN `111111` dado de baja → crear uno nuevo con el mismo CN → se reactiva el
registro existente (misma fila, `activo = 1`), no aparece un segundo registro con ese CN.

## Notas

Todos los escenarios son verificables end-to-end dentro de esta spec salvo la referencia a "líneas
de SPD" de CA-301, que se limita a comprobar que esta spec no toca nada fuera de
Medicamento/Medicamento_Hist — la instantánea real de SPD_Linea la valida Spec 006 cuando exista.
