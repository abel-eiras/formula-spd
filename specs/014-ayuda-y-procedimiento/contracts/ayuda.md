# Contratos — Spec 014

## Identificadores de apartado (contrato para F1, enlaces `[[seccion:id]]` y "¿Por qué?")

**Procedimiento**: `servicio-spd`, `idoneidad`, `consentimiento-rgpd`, `ficha-y-tratamiento`,
`deposito-y-retirada`, `preparacion`, `verificacion`, `entrega`, `comunicacion-medico`, `residuos`,
`personal-higiene-limpieza`, `documentacion-y-conservacion`, `porque-de-los-bloqueos`.

**Uso**: `inicio`, `pacientes`, `ficha-paciente`, `idoneidad-consentimiento`, `tratamientos`,
`deposito`, `preparacion`, `comunicaciones-medico`, `retirada-envases`, `catalogo-medicamentos`,
`registros-calidad`, `configuracion`, `documentos-generados`.

## `AyudaContextual.Registrar(Window)`

Tabla `nombre de tipo de ventana → id de Procedimiento`; F1 abre `AyudaWindow` en ese apartado.
Ventanas sin entrada abren el índice.

## `AyudaWindow.Abrir(string? seccion, string? id)`

Abre la ventana y selecciona el apartado; con `id` nulo muestra el índice.

## `IndiceAyuda`

- `Todas`, `Uso`, `Procedimiento` (ordenadas por `Orden`).
- `Obtener(seccion, id)`.
- `Buscar(texto)` → coincidencias por título (primero) y contenido, sin tildes ni mayúsculas.
