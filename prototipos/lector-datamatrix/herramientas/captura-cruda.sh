#!/usr/bin/env bash
# Captura la cadena cruda que entrega el lector DataMatrix (emulacion de teclado)
# byte a byte, sin que nada la transforme, y la vuelca a un fichero de log.
#
# Uso:
#   bash captura-cruda.sh
# Escanea un envase (o pega una cadena) y pulsa Enter. Ctrl+D para salir.

LOG="$(dirname "$0")/captura-cruda.log"
echo "=== Nueva sesion de captura: $(date) ===" >> "$LOG"

echo "Escanea un envase (o pega la cadena) y pulsa Enter. Ctrl+D para salir."
echo "Log: $LOG"
echo

contador=0
while IFS= read -r linea; do
    contador=$((contador + 1))
    {
        echo "--- Lectura #$contador ---"
        echo "Texto : $linea"
        echo -n "Longitud: "
        printf '%s' "$linea" | wc -c
        echo -n "Hex   : "
        printf '%s' "$linea" | od -An -tx1 | tr -d '\n'
        echo
        echo
    } >> "$LOG"
    echo "Lectura #$contador capturada ($(printf '%s' "$linea" | wc -c) bytes)."
done

echo "Fin de la captura."
