# Configuración (Administrador)

## Farmacia

Datos que aparecen en todos los documentos: código sanitario, nombre, titular o comunidad de bienes, CIF, dirección, teléfono, fax, email, WhatsApp, **logo**. **Protección de datos**: responsable del tratamiento y vías para ejercer derechos (si son distintos de los generales) y **Delegado de Protección de Datos** con su contacto (normalmente el del Colegio) — van al documento RGPD. **Prefijos** de número de ficha y de SPD. **Rutas** de copias de seguridad y de documentos generados (validar que existen y se puede escribir; conviene que no estén dentro de la carpeta de instalación). **Valores por defecto**: día de retirada, número de blísteres, días de antelación del listado de retirada, rangos de temperatura y humedad, horas de reutilización de la lectura ambiental. "Generar backup ahora" crea una copia manual.

## Usuarios

Alta con nombre, apellidos, usuario, contraseña inicial (debe cambiarla al entrar), rol **Administrador** o **Elaborador**, cargo según el PNT, nº de colegiado y firma abreviada (para los documentos). Baja lógica; desbloqueo tras intentos fallidos; restablecer contraseña.

## Seguridad

**Cifrado** de la base de datos con contraseña maestra: al activarlo se muestra una **frase de recuperación de 24 palabras** que hay que guardar fuera del ordenador; sin contraseña ni frase no hay forma de abrir los datos. Se puede desactivar con la contraseña.

**Purga de pacientes antiguos**: lista los pacientes en BAJA cuya baja es anterior a los años de retención configurados (mínimo 5, constitución Art. III.2). Se purgan **uno a uno**, escribiendo la contraseña del administrador en cada purga. Antes de borrar queda una fila de auditoría con el número de ficha y el nombre; un paciente con envases todavía en custodia no se purga. Es la única eliminación física de datos de toda la aplicación y nunca se ejecuta sola.

## Actualizaciones

Comprueba si hay una versión nueva de la aplicación (necesita conexión) y la descarga; la instalación es manual.

## Nomenclátor

URL del fichero de la AEMPS. **Descargar ahora** trae el fichero y da de alta en el catálogo todos los medicamentos que no estén: los de baja quedan inactivos, los efectos y accesorios no entran y lo que ya existía no se toca. Todo entra con la aptitud para SPD sin confirmar. **Importar al catálogo el último fichero descargado** repite la carga sin volver a descargar. Los cambios de nombre de lo que ya existía se revisan en [[uso:catalogo-medicamentos]].

## Perfiles de importación y exportar pacientes

Perfiles que indican qué columna del CSV del programa de gestión corresponde a cada dato (tratamientos y envases; pacientes). "Exportar pacientes" genera un CSV con el listado según un perfil.

Procedimientos relacionados: [[procedimiento:documentacion-y-conservacion]], [[procedimiento:personal-higiene-limpieza]].
