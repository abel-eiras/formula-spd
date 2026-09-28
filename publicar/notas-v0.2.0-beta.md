Segunda **beta** de **Fórmula SPD**. La novedad principal es el **lector de códigos DataMatrix**: escanear el envase rellena serie, lote y caducidad, y en el mostrador lleva directamente a la retirada del paciente que lo necesita. Además, varias mejoras para que el trabajo de cada semana necesite menos teclas.

> **Sigue siendo una beta.** La prueba un grupo reducido de farmacias. Puede tener errores: no la uses todavía como único registro del servicio y mantén las copias de seguridad.

## Novedades

### Lector de DataMatrix

Funciona con cualquier lector USB que escriba como un teclado (el habitual en farmacia), sin instalar nada.

- **Alta de envase escaneando**, en el Depósito del paciente, en la Retirada de envases y en la Preparación: se rellenan número de serie, lote y caducidad, y el código nacional si el fabricante lo incluye en el código (no todos lo hacen; si no viene, se escribe a mano como hasta ahora). Tras escanear, el cursor salta a las unidades y **Intro** guarda.
- **En el mostrador**: pulsa **Ctrl+F** y escanea el envase recién dispensado. Si ese medicamento le falta a un solo paciente, se abre su retirada con todo relleno; si le falta a varios, eliges para quién es.
- **Al dar de alta un tratamiento**, escanear el envase encuentra el medicamento sin buscarlo por nombre.
- Si un código no se reconoce, la aplicación lo dice y se sigue a mano: nunca bloquea.

Probado con envases reales y con las pruebas oficiales de validación de escáneres de SEVeM. **Antes de empezar, comprueba tu lector**: escanea un envase en el Bloc de notas y verifica que las mayúsculas, los símbolos y las letras Y/Z salen bien (Bloq. Mayús activado y desactivado); si no, ajusta el lector a la distribución de teclado española. Consulta la ayuda (F1) en cualquier pantalla de envases.

### Menos teclas en el día a día

- **Unidades del envase desde el catálogo**: ya no hay que escribirlas en cada envase. Si el catálogo no las sabe, puedes guardarlas la primera vez (solo si el envase está completo).
- **Depósito**: el medicamento se elige de un desplegable con los que el paciente tiene en tratamiento, en vez de teclear el código nacional.
- **Preparación**: se ve la última lectura de temperatura y humedad y si todavía vale, y el material de acondicionamiento viene elegido con el último lote usado.
- **Pacientes que ya estaban en el servicio**: el botón **Marcar como apto** (pestaña Idoneidad y consentimiento) registra la evaluación y el consentimiento que ya firmaron en papel, indicando la fecha de la firma, sin repetir la entrevista.
- **Nomenclátor**: la dirección de descarga del Ministerio de Sanidad viene ya puesta, y en una instalación nueva se ofrece descargarlo al terminar la configuración inicial.
- **Teclado**: Ctrl+F lleva al buscador desde cualquier pantalla; en el buscador, Intro abre el primer resultado y Esc lo cierra.

La ayuda integrada (F1) explica todo lo anterior.

## Descargas

| Sistema | Paquete |
|---|---|
| **Windows 10/11 (64 bits)** — la plataforma principal | `FormulaSPD-0.2.0-beta-windows-x64.zip` |
| Linux (64 bits) | `FormulaSPD-0.2.0-beta-linux-x64.tar.gz` |
| macOS con chip Apple (M1 o posterior) — *experimental* | `FormulaSPD-0.2.0-beta-macos-apple-silicon.zip` |
| macOS con procesador Intel — *experimental* | `FormulaSPD-0.2.0-beta-macos-intel.zip` |

`SHA256SUMS.txt` permite comprobar que la descarga está íntegra.

## Actualizar desde la 0.1.0-beta

Esta versión **no cambia la base de datos**: tus pacientes, tratamientos y envases se conservan tal cual.

1. Haz una copia de seguridad (se genera sola al cerrar la aplicación).
2. Descarga el paquete de tu sistema y sustituye solo el ejecutable (`FormulaSPD.exe` en Windows) en tu carpeta de Fórmula SPD.
3. Abre la aplicación: el título de la ventana debe decir 0.2.0-beta.

Configuración → Actualizaciones → «Comprobar actualizaciones» avisa de esta versión desde la anterior.

## Instalación nueva

No hay instalador, a propósito: **toda la instalación es una carpeta**. Descomprímela en una carpeta tuya donde puedas escribir (por ejemplo, Documentos) y abre `FormulaSPD`. Cada paquete trae un `LEEME.txt` con los pasos detallados.

- **Windows**: la primera vez puede aparecer «Windows protegió su PC», porque el programa no está firmado con un certificado comercial. Pulsa «Más información» → «Ejecutar de todas formas».
- **macOS**: no está firmado por Apple ni se ha probado todavía en un Mac. Abre la carpeta y haz clic derecho → «Abrir» sobre `Abrir FormulaSPD.command`.

## Si algo falla

Anota qué estabas haciendo, qué esperabas y qué pasó, y envía el fichero más reciente de la carpeta `logs/` a quien te facilitó la beta. Si falla la lectura de un código concreto, indica qué envase era.

Licencia MIT. El código está en este repositorio. El logotipo de Fórmula farma es una marca de su titular y no está incluido en la licencia MIT.
