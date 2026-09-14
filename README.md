# Fórmula SPD — Sistemas Personalizados de Dosificación

Programa de escritorio para el servicio SPD de las farmacias gallegas, construido sobre el
*Procedimiento Normalizado de Trabajo* del Colegio de Farmacéuticos de Pontevedra y el
Decreto 87/2022 de la Xunta de Galicia.

> **Estado: beta.** La versión 0.1.0-beta está en pruebas en un grupo reducido de farmacias.

## Descargar

Los paquetes están en [Releases](https://github.com/abel-eiras/formula-spd/releases). No hay instalador:
cada paquete es una carpeta con un único ejecutable autocontenido; se descomprime y se abre. Cada
uno trae un `LEEME.txt` con los pasos para su sistema. Windows 11 x64 es la plataforma principal;
Linux se ofrece además, y macOS de forma experimental.

## Principios

- **Sin red**, salvo dos excepciones explícitas: comprobar actualizaciones y descargar el nomenclátor.
- **Portable**: la instalación completa —programa, base de datos, logs y copias— es una carpeta.
- **Nada se borra** y lo que se entregó queda congelado tal como se entregó.

Las reglas completas están en la [constitución del proyecto](.specify/memory/constitution.md) y el
detalle de cada funcionalidad en [`specs/`](specs/).

## Desarrollo

Requiere el SDK de .NET 8.

```bash
dotnet test
dotnet run --project src/Spd.Presentacion
```

## Generar los paquetes

```bash
publicar/empaquetar.sh linux-x64
publicar/empaquetar.sh win-x64
```

Deja el paquete comprimido en `artefactos/`. Los de macOS (`osx-arm64`, `osx-x64`) se generan en un
Mac, donde se firman. Al empujar una etiqueta `vX.Y.Z` o `vX.Y.Z-beta…` que coincida con `<Version>`
de `src/Spd.Presentacion/Spd.Presentacion.csproj`, el workflow
[`publicar.yml`](.github/workflows/publicar.yml) pasa los tests, genera los cuatro paquetes y crea
la release con las notas de `publicar/notas-<etiqueta>.md`.

## Licencia

[MIT](LICENSE). Los componentes de terceros y sus licencias están en
[AVISOS-DE-TERCEROS.md](AVISOS-DE-TERCEROS.md).

El logotipo de Fórmula farma (`src/Spd.Presentacion/Assets/formula-farma-logo*.png`,
`formula-spd.ico`, `publicar/formula-spd.png`) es una marca de su titular: **no** está incluido en
la licencia MIT y no puede usarse para distribuir versiones modificadas.
