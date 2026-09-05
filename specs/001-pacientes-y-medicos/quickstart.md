# Quickstart de validación — Spec 001

## Prerrequisitos

```bash
dotnet build
dotnet test
```

## Escenarios

### CA-001 — Número de ficha
Con prefijo "F-" y un paciente ya con "F-000041", crear otro paciente nuevo → recibe "F-000042", no editable.

### CA-002 — Mínimos para guardar
Formulario con solo nombre y apellidos → al guardar, error indicando que falta DNI, CIP o fecha de nacimiento.

### CA-003 — Duplicado por DNI
Paciente activo con DNI 12345678Z; crear otro con el mismo DNI → aviso con nombre y ficha del existente, permite continuar o cancelar.

### CA-004/CA-005 — Autocompletado y alta de médico en contexto
Escribir "fern" en el selector de médico de cabecera → aparece el médico existente; elegir "Nuevo médico…", rellenar apellidos/nombre, guardar → queda seleccionado sin salir de la ficha.

### CA-006/CA-007 — Edición y baja de médico
Cambiar el teléfono de un médico referenciado por varios pacientes → todas sus fichas muestran el teléfono nuevo. Intentar dar de baja un médico que es cabecera de un paciente activo → bloqueado, con el nombre del paciente.

### CA-008 — Representante sin DNI
Contacto tipo REPRESENTANTE_LEGAL sin DNI → no guarda, indica DNI obligatorio.

### CA-009/CA-010 — Baja y reactivación de paciente
Dar de baja un paciente con motivo → estado BAJA, todo lo demás sigue existiendo, auditoría registrada. Reactivarlo → pasa a EVALUACION, cabecera indica "sin consentimiento vigente".

### CA-011 — Búsqueda sin tildes
Paciente "José Núñez"; buscar "nunez" → aparece en resultados.

### CA-012/CA-013 — Permisos y auditoría
Un usuario Elaborador edita y guarda todos los campos sin restricción. Cambiar un teléfono y guardar → entrada de auditoría con el detalle antes/después.

### CA-014/CA-015 — CIP
Fecha 1991-04-10, apellidos "Eiras Espiño", sexo hombre, CIP vacío, pulsar "autocompletar CIP" → propone `910410EEIS1` con 12–14 en blanco. Con CIP `910410EEIS1014` ya guardado, cambiar el sexo a mujer y guardar → aviso de correspondencia, no bloquea.

## Notas

Todos los escenarios de esta spec son verificables end-to-end dentro de la propia spec (a
diferencia de varios de Spec 000 que dependían de Spec 001 para completarse) — Paciente, Contacto
y Medico son entidades propias de esta feature.
