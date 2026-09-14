Primera versión **beta** de **Fórmula SPD**, el programa para el servicio de Sistemas Personalizados de Dosificación de las farmacias gallegas: pacientes, idoneidad y consentimiento, tratamientos, depósito y retirada de envases, preparación, verificación y entrega, documentos del PNT, comunicaciones con el médico y registros de calidad.

> **Es una beta.** La prueba un grupo reducido de farmacias. Puede tener errores: no la uses todavía como único registro del servicio y configura desde el primer día las copias de seguridad.

## Descargas

| Sistema | Paquete |
|---|---|
| **Windows 10/11 (64 bits)** — la plataforma principal | `FormulaSPD-0.1.0-beta-windows-x64.zip` |
| Linux (64 bits) | `FormulaSPD-0.1.0-beta-linux-x64.tar.gz` |
| macOS con chip Apple (M1 o posterior) — *experimental* | `FormulaSPD-0.1.0-beta-macos-apple-silicon.zip` |
| macOS con procesador Intel — *experimental* | `FormulaSPD-0.1.0-beta-macos-intel.zip` |

`SHA256SUMS.txt` permite comprobar que la descarga está íntegra.

## Instalación

No hay instalador, a propósito: **toda la instalación es una carpeta**. Descomprímela en una carpeta tuya donde puedas escribir (por ejemplo, Documentos) y abre `FormulaSPD`. No hace falta instalar nada más. La base de datos, los logs y los documentos se crean dentro de esa carpeta, así que copiarla a otro ordenador es una restauración completa.

- **Windows**: la primera vez puede aparecer «Windows protegió su PC», porque el programa no está firmado con un certificado comercial. Pulsa «Más información» → «Ejecutar de todas formas».
- **macOS**: no está firmado por Apple ni se ha probado todavía en un Mac. Abre la carpeta y haz clic derecho → «Abrir» sobre `Abrir FormulaSPD.command`.
- Cada paquete trae un `LEEME.txt` con los pasos detallados.

## Si algo falla

Anota qué estabas haciendo, qué esperabas y qué pasó, y envía el fichero más reciente de la carpeta `logs/` a quien te facilitó la beta.

## Actualizar

Configuración → Actualizaciones → «Comprobar actualizaciones» avisa de las betas siguientes. Para instalar una: copia de seguridad, descarga y sustituye solo el ejecutable; los datos se conservan y la base de datos se actualiza sola.

Licencia MIT. El código está en este repositorio. El logotipo de Fórmula farma es una marca de su titular y no está incluido en la licencia MIT.
