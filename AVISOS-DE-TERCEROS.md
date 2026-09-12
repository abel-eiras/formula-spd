# Avisos de terceros

El código de este repositorio se publica bajo la licencia MIT (ver [LICENSE](LICENSE)). Lo que se
lista aquí **no** es código propio y conserva su propia licencia: la MIT no la sustituye.

## Tipografías incluidas en el ejecutable

**IBM Plex Sans** e **IBM Plex Mono** — SIL Open Font License 1.1. Van embebidas en el binario
(`src/Spd.Presentacion/Assets/Fuentes`), y la OFL **exige distribuir su licencia** junto a ellas:
está en [`src/Spd.Presentacion/Assets/IBM-Plex-LICENSE.txt`](src/Spd.Presentacion/Assets/IBM-Plex-LICENSE.txt)
y viaja en la carpeta de publicación.

## Bibliotecas

| Biblioteca | Licencia |
|---|---|
| Avalonia, Avalonia.Desktop, Avalonia.Themes.Fluent, Avalonia.Fonts.Inter, Avalonia.Controls.DataGrid | MIT |
| CommunityToolkit.Mvvm | MIT |
| Dapper | Apache 2.0 |
| Microsoft.Data.Sqlite | MIT |
| Serilog, Serilog.Sinks.File | Apache 2.0 |
| Konscious.Security.Cryptography.Argon2 | MIT |
| SQLitePCLRaw.bundle_e_sqlcipher | Apache 2.0 (el paquete) e incluye **SQLCipher Community Edition**, licencia BSD de tres cláusulas |
| QuestPDF | **Community License** — gratuita solo por debajo del umbral de ingresos que QuestPDF publica en su web |

### Sobre QuestPDF

`Program.cs` declara `LicenseType.Community` explícitamente. Es la opción correcta para una farmacia
individual, pero **es una condición sobre los ingresos de quien lo usa, no sobre el software**: quien
tome este código para una organización por encima de ese umbral necesita una licencia de pago de
QuestPDF. La licencia MIT de este repositorio no le exime de eso.

## Textos normativos

Los documentos que la aplicación imprime reproducen los elementos mínimos de los anexos del
Procedimiento Normalizado de Trabajo de SPD del Colegio Oficial de Farmacéuticos y del Decreto
87/2022 de la Xunta de Galicia. Son los enunciados que la normativa gallega exige, y deben ser esos
para que la documentación resultante cumpla: no se reformulan.
