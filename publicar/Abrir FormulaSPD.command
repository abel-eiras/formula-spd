#!/bin/bash
# Quita la marca de "descargado de Internet" de la carpeta de Fórmula SPD y lo arranca.
# Hace falta porque el programa no está firmado por Apple (versión beta).
cd "$(dirname "$0")" || exit 1
xattr -dr com.apple.quarantine . 2>/dev/null
exec ./FormulaSPD
