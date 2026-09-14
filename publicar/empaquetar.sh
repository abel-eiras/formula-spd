#!/usr/bin/env bash
# Genera el paquete portable de Fórmula SPD para una plataforma, en artefactos/.
#
# Constitución Art. VI.4/VI.5: sin instalador. El paquete es una carpeta con un único ejecutable
# autocontenido (no hace falta instalar .NET), las licencias y un LEEME. La base de datos, los logs y
# las copias de seguridad se crean dentro de esa misma carpeta al usarla.
#
# Uso: publicar/empaquetar.sh <win-x64|linux-x64|osx-arm64|osx-x64>
# Los paquetes de macOS se generan en un Mac (lo hace el workflow de GitHub): allí se firman ad hoc,
# sin lo cual macOS en Apple Silicon no deja ni arrancar el ejecutable.
set -euo pipefail

rid="${1:?Uso: publicar/empaquetar.sh <win-x64|linux-x64|osx-arm64|osx-x64>}"
raiz="$(cd "$(dirname "$0")/.." && pwd)"
version="$(sed -n 's:.*<Version>\(.*\)</Version>.*:\1:p' "$raiz/src/Spd.Presentacion/Spd.Presentacion.csproj")"

case "$rid" in
  win-x64)   plataforma="windows-x64";         ejecutable="FormulaSPD.exe" ;;
  linux-x64) plataforma="linux-x64";           ejecutable="FormulaSPD" ;;
  osx-arm64) plataforma="macos-apple-silicon"; ejecutable="FormulaSPD" ;;
  osx-x64)   plataforma="macos-intel";         ejecutable="FormulaSPD" ;;
  *) echo "Plataforma no soportada: $rid" >&2; exit 1 ;;
esac

nombre="FormulaSPD-$version-$plataforma"
salida="$raiz/artefactos"
carpeta="$salida/$nombre"
rm -rf "$carpeta" "$salida/$nombre.zip" "$salida/$nombre.tar.gz"
mkdir -p "$salida"

echo "== Publicando $nombre"
dotnet publish "$raiz/src/Spd.Presentacion" -c Release -r "$rid" --self-contained true \
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=none \
  -o "$carpeta"

# Un nombre que se reconoce a simple vista. El ejecutable de un solo fichero se localiza a sí mismo por
# su ruta, así que renombrarlo es seguro.
origen="Spd.Presentacion"; [ "$rid" = "win-x64" ] && origen="Spd.Presentacion.exe"
mv "$carpeta/$origen" "$carpeta/$ejecutable"

if [[ "$rid" == osx-* ]]; then
  if [ "$(uname)" != "Darwin" ]; then
    echo "AVISO: $nombre no se ha firmado (hay que generarlo en un Mac); en Apple Silicon no arrancará." >&2
  else
    codesign --force --sign - "$carpeta/$ejecutable"
  fi
fi

cp "$raiz/publicar/LEEME-comun.txt" "$carpeta/LEEME.txt"
case "$rid" in
  win-x64)   cat "$raiz/publicar/LEEME-windows.txt" >> "$carpeta/LEEME.txt" ;;
  linux-x64) cat "$raiz/publicar/LEEME-linux.txt" >> "$carpeta/LEEME.txt"
             cp "$raiz/publicar/crear-acceso-directo.sh" "$raiz/publicar/formula-spd.png" "$carpeta/"
             chmod +x "$carpeta/crear-acceso-directo.sh" "$carpeta/$ejecutable" ;;
  osx-*)     cat "$raiz/publicar/LEEME-macos.txt" >> "$carpeta/LEEME.txt"
             cp "$raiz/publicar/Abrir FormulaSPD.command" "$carpeta/"
             chmod +x "$carpeta/Abrir FormulaSPD.command" "$carpeta/$ejecutable" ;;
esac
sed -i.bak "s/{VERSION}/$version/g" "$carpeta/LEEME.txt" && rm -f "$carpeta/LEEME.txt.bak"

echo "== Comprimiendo"
cd "$salida"
case "$rid" in
  linux-x64) tar -czf "$nombre.tar.gz" "$nombre" ;;
  osx-*)     ditto -c -k --keepParent "$nombre" "$nombre.zip" 2>/dev/null || zip -qry "$nombre.zip" "$nombre" ;;
  *)         zip -qr "$nombre.zip" "$nombre" ;;
esac
ls -lh "$salida" | grep "$nombre"
