# Diagnostico de codigos DataMatrix no reconocidos

Guia para cuando aparezca un envase cuyo codigo el prototipo no reconoce (o
reconoce mal). Nace de un caso real: ver "Historia de un caso real" mas abajo.

## Antes de nada: no transcribas cadenas a mano desde una foto o una captura de pantalla

La primera vez que se investigo un fallo aqui, la cadena cruda se leyo a ojo
desde una foto de la terminal. Parecia un bug grave (el lote y el numero de
serie llevaban digitos de mas). Al capturar la cadena real con el metodo de
abajo, resulto que la transcripcion a mano tenia errores de lectura y el
parser funcionaba bien en ese caso. Moraleja: para diagnosticar un problema
de parseo, la cadena cruda tiene que salir del sistema tal cual, nunca de una
lectura visual.

## Como capturar la cadena cruda sin que nada la transforme

Pegar la cadena directamente en un chat o editor puede normalizar saltos de
linea, colapsar espacios o perder caracteres de control invisibles (como el
separador GS, `0x1D`, que segun la nota 1 de SEVeM-0108.03 puede llegar como
otro caracter distinto segun la configuracion del escaner). Para descartar
eso, usa el script de captura, que vuelca cada lectura a un fichero junto con
su longitud exacta en bytes y su volcado hexadecimal completo:

```
bash herramientas/captura-cruda.sh
```

Escanea el envase (o pega la cadena) y pulsa Enter; repite con tantos codigos
como haga falta; Ctrl+D para terminar. El resultado queda en
`herramientas/captura-cruda.log` (ignorado por git, `*.log`). El volcado
hexadecimal es la fuente de verdad: si aparece o no aparece el separador GS,
si hay espacios de mas, etc., se ve ahi sin ambiguedad.

## Que reconoce el parser ahora mismo

`LeerGs1DataMatrix` (en `src/LectorGs1DataMatrix.cs`) solo sabe interpretar
estos AI (Application Identifier) de GS1:

| AI    | Campo                          | Longitud       |
|-------|---------------------------------|----------------|
| `01`  | GTIN                             | fija, 14       |
| `17`  | Fecha de caducidad (AAMMDD)      | fija, 6        |
| `10`  | Lote                             | variable, ≤20  |
| `21`  | Numero de serie                  | variable, ≤20  |
| `712` | Codigo Nacional (NHRN Espana)    | variable, ≤10  |

Cualquier otro AI (por ejemplo `710`/`711`/`713`/`714`, variantes del NHRN
para otros paises, o cualquier AI que un fabricante decida incluir) **no se
reconoce como marcador**. Esto importa sobre todo en la ruta sin separador
GS (`LeerSinSeparadorGs`): como los campos variables (10, 21, 712) aceptan
cualquier caracter hasta que el parser reconoce el siguiente AI, un AI que no
esta en la lista de arriba no corta nada — sus digitos se cuelan dentro del
cuerpo del campo variable anterior.

## Sintoma de "hay un AI que no reconocemos"

Si un codigo se lee sin excepcion pero el lote o el numero de serie salen con
digitos de mas al final (comparado con lo impreso en el envase), y sobre todo
si ese sufijo de mas **se repite igual en codigos de fabricantes distintos**,
lo mas probable es que ese sufijo sea el principio de un AI real que el
parser no tiene en su lista. No es ruido aleatorio: GS1 reutiliza los mismos
AI entre fabricantes, asi que un AI no soportado deja siempre la misma huella
digital al principio de su contenido.

### Como localizarlo

1. Captura la cadena cruda con el script de arriba (nunca a mano).
2. Consigue el lote y el numero de serie reales impresos en el envase (texto
   junto al DataMatrix, no el propio codigo).
3. Localiza esos dos valores como subcadenas dentro de la cadena cruda. Lo
   que sobra entre el final de uno de ellos y el inicio del literal `10`,
   `21`, `712`, etc. es un AI (y su contenido) que no esta soportado.
4. Antes de anadir el AI nuevo al parser, decide con Abel si hace falta
   (Articulo V: no dupliques lo que ya existe, y aqui tambien aplica no
   anadir soporte a un AI que solo aparece una vez y nunca se va a usar).

## Historia de un caso real (2026-09-28)

Con los 5 envases fotografiados por Abel:

| GTIN             | Lote (real) | Numero de serie (real) | Caducidad  |
|------------------|-------------|--------------------------|------------|
| `05413787999316` | `3012801`   | `4000LQG554DTM1`         | 2030-10-31 |
| `08470007642664` | `B010B`     | `78339902223173`         | 2027-12-30 |
| `08470009716639` | `412036X`   | `4000JFRH2XAN6S`         | 2028-10-31 |
| `08470006950647` | `RA0462`    | `17234261214629`         | 2028-07-31 |
| `08470006543870` | `12505293`  | `9436795376788`          | 2026-02-28 |

Las 5 cadenas crudas, capturadas con el metodo de arriba, estan en los tests
(`CodigoReal1` a `CodigoReal5` en `tests/LectorGs1DataMatrixTests.cs`) y se
interpretan correctamente. No se encontro ningun AI no soportado en estos 5
codigos — el fallo que parecia existir al principio (digitos de mas en lote
y serie) era un error de transcripcion manual desde una foto, no un bug real
(ver seccion de arriba).

Si hubo un bug real, pero distinto: el quinto codigo (GTIN `08470006543870`)
no se reconocia (`NO RECONOCIDO`) aunque su cadena era perfectamente valida.
La ruta sin separador GS encontraba **dos** formas de consumir la cadena
entera: una separaba correctamente lote/caducidad/serie, y la otra se comia
la fecha de caducidad dentro del lote (dejando ese campo obligatorio sin
valor). Como el codigo solo contaba cuantas descomposiciones llegaban al
final de la cadena, sin comprobar si tenian todos los campos obligatorios,
trataba las dos como igual de "validas" y, ante la ambiguedad, descartaba el
codigo entero — aunque solo una de las dos podria haber producido un
resultado real.

Corregido anadiendo `EsDecomposicionCompleta`: antes de contar cuantas
descomposiciones hay, se descartan las que no tienen los 4 campos
obligatorios (`01`, `10`, `17`, `21`). Si con eso queda una sola, se acepta.
Ver el commit correspondiente y el test `CodigoReal5_...`.

## Que hacer si un codigo nuevo sigue sin reconocerse tras aplicar esto

1. Descarta primero que sea el escenario de ambiguedad real documentado en
   el README (lote/serie que por azar producen mas de una descomposicion
   *completa* — este si es un limite matematico del formato sin separador
   GS, no un bug).
2. Si no es eso, sigue los pasos de "Como localizarlo" de arriba para ver si
   hay un AI no soportado.
3. Si el lector a veces transmite el separador GS y a veces no (pregunta 1
   abierta en el README), revisa antes la configuracion del escaner — es mas
   fiable que depender de la heuristica de desambiguacion.
