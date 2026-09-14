#!/usr/bin/env bash
# Añade Fórmula SPD al menú de aplicaciones del usuario, apuntando a esta carpeta.
set -euo pipefail
carpeta="$(cd "$(dirname "$0")" && pwd)"
destino="${XDG_DATA_HOME:-$HOME/.local/share}/applications/formula-spd.desktop"
mkdir -p "$(dirname "$destino")"
cat > "$destino" <<DESKTOP
[Desktop Entry]
Type=Application
Name=Fórmula SPD
Comment=Sistemas Personalizados de Dosificación
Exec="$carpeta/FormulaSPD"
Icon=$carpeta/formula-spd.png
Path=$carpeta
Terminal=false
Categories=Office;MedicalSoftware;
DESKTOP
chmod +x "$destino"
echo "Acceso directo creado en $destino"
