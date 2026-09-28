# Prototipo — lector de DataMatrix

Modulo aislado, fuera de `src/` y sin engancharse a `SPD.sln`. Su unico objetivo
es confirmar que se puede interpretar la cadena que entrega el lector de
Abel antes de comprometer el diseno del parser en spec-012 (pregunta abierta Q1
de esa spec). No depende de Spd.Dominio ni de ningun otro proyecto de la app.

## Que hace

`LectorGs1DataMatrix.Leer(cadena)` recibe el texto que el lector USB (emulacion
de teclado) entrega al escanear un envase y devuelve GTIN, lote, caducidad,
numero de serie y, si el fabricante lo incluye, el Codigo Nacional — sin lanzar
excepcion nunca; si no reconoce la cadena devuelve `null`.

## Lo que se ha confirmado con codigos reales (2026-09-28)

- Los 3 codigos que trajo Abel se interpretan correctamente.
- Los 4 codigos de prueba oficiales del documento SEVeM-0108.03 "Pruebas de
  validacion de escaneres" (adjuntado por Abel) tambien, incluido el caso con
  dia de caducidad sin especificar (`00`).
- **Hallazgo importante, no contemplado en spec-012 v0.1:** el separador GS que
  exige GS1 entre campos de longitud variable **no llega como tal** al pegar
  la lectura — el propio documento de SEVeM lo advierte (nota 1): segun la
  configuracion del escaner puede aparecer como una comilla, una barra, otro
  caracter, o no aparecer nada. En las pruebas de Abel no aparecio nada. Esto
  hace que el limite entre lote y numero de serie sea ambiguo en el caso
  general — el parser lo resuelve probando todas las descomposiciones
  posibles dentro de los limites de longitud de GS1 y solo acepta el
  resultado si hay una unica forma de consumir toda la cadena (ver comentario
  en `LeerSinSeparadorGs`). Con los 3 codigos reales esto ha dado una solucion
  unica en los tres casos, pero **no hay garantia matematica de que siempre la
  haya** — un lote o serie que por azar contengan la subcadena de otro AI en
  el punto justo podrian producir cero o varias descomposiciones validas, y
  entonces el codigo se trataria como no reconocido (igual que uno realmente
  ilegible, FR-1204: no bloquea, se rellena a mano).
- **Segundo hallazgo, tambien fuera de spec-012 v0.1:** uno de los tres
  codigos de Abel incluye el Codigo Nacional codificado directamente en el AI
  `712` (identificador especifico de Espana para el numero de reembolso
  sanitario). Los otros dos no lo incluyen. spec-012 v0.1 asume que el CN
  **siempre** se resuelve por catalogo a partir del GTIN (FR-1202/1203); en la
  practica, cuando el fabricante incluye el 712, se podria usar directamente
  y evitar la asociacion manual del GTIN la primera vez.

## Como ejecutar los tests

```
cd prototipos/lector-datamatrix/tests
dotnet test
```

10/10 tests en verde: 3 codigos reales de Abel + 4 patrones oficiales de
SEVeM + 3 casos de cadena no reconocida.

## Fuera de alcance de este prototipo

- Integracion con Spd.Dominio / Spd.Presentacion / Spd.Aplicacion (Spec 005,
  Spec 003) — siguiente paso, pendiente de decidir con Abel.
- Lectura por camara: sigue fuera de alcance segun spec-012 §8; este
  prototipo solo interpreta texto, coherente con FR-1200 (lector USB tipo
  teclado).
- Decidir que hacer con `DiaSinEspecificar` (dia de caducidad "00"): el
  prototipo lo expone tal cual, no decide una politica (Articulo X).

## Preguntas para decidir antes de integrar

1. ¿Confirmas que tu lector nunca transmite el separador GS (ni siquiera como
   otro caracter)? Si a veces si lo transmite, conviene revisar la
   configuracion del lector primero (mas fiable que la heuristica de
   desambiguacion) — el propio documento SEVeM trae la prueba para
   comprobarlo.
2. ¿Quieres que spec-012 se actualice para contemplar el AI 712 (Codigo
   Nacional en el propio codigo) como alternativa a la asociacion manual
   GTIN→CN? Es un cambio de spec, no de esta spec en borrador — btw sigue en
   "Borrador para revisión", asi que se puede ajustar sin ceremonia de
   enmienda.
