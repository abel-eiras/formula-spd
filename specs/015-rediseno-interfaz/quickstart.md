# Quickstart — validación de Spec 015

Guion de la prueba manual al final de cada fase. Antes de empezar, si se cambió de rama:

```bash
rm -f src/Spd.Presentacion/bin/Debug/net8.0/spd.db
dotnet run --project src/Spd.Presentacion
```

## Tras la fase 1 — Marco único

1. Iniciar sesión. **Comprobar**: una sola ventana; menú lateral con secciones y contadores.
2. Recorrer las catorce secciones. **Comprobar**: ninguna abre ventana; "atrás" funciona.
3. F1 en Preparaciones, Retirada, Calidad y Farmacia. **Comprobar**: abre el apartado correcto.
4. Cambiar el tema del sistema a oscuro y volver a claro. **Comprobar**: legible en ambos.
5. Entrar como Elaborador. **Comprobar**: sin secciones de administración.
6. `logs/log-*.txt`. **Comprobar**: arranque hasta login < 2 s.

## Tras la fase 2 — Inicio y tablas

1. **Comprobar** que el inicio muestra lo que realmente está pendiente hoy.
2. Pulsar un aviso de cada tipo. **Comprobar**: lleva al sitio donde se resuelve.
3. Ordenar Preparaciones por paciente y por validez. **Comprobar**: columnas alineadas, cifras a la
   derecha, selección conservada.
4. Seleccionar tres pacientes al día y generar el lote desde la tabla.
5. Buscar en la cabecera: apellido de un paciente, nº de un blíster, CN de un medicamento.

## Tras la fase 3 — Espacio del paciente

El recorrido empieza **por el paciente**, que es el orden en que ocurre en el mostrador. El médico y
los contactos se dan de alta cuando hacen falta, sin salir de su ficha: no son un paso previo.

1. **Nuevo paciente**. Rellenar nombre, apellidos y DNI. Todavía no guardar.
2. Campo **Médico de cabecera**: escribir dos letras del apellido de un médico que no existe.
   **Comprobar**: no lo encuentra, pero ofrece **"Nuevo médico…"**.
3. Pulsar **"Nuevo médico…"**. **Comprobar**: se abre un panel a la derecha con el apellido ya escrito,
   **la ficha de detrás se sigue viendo**, y al crearlo queda seleccionado en el campo **sin que se
   haya perdido nada de lo que llevabas tecleado** ni haya hecho falta guardar antes.
4. **Guardar** el paciente. **Comprobar**: la cabecera muestra su nº de ficha y el médico quedó
   asignado en el mismo gesto.
5. **Contactos** (en esta misma pestaña Datos, debajo de los datos). Antes de guardar el botón estaba
   apagado —un contacto cuelga de una ficha que aún no existía—; ahora ya se pueden añadir.
   **Comprobar**: no hay que cambiar de pantalla.
6. Añadir un familiar sin DNI: entra. Marcarlo como **"retira la medicación"**: **comprobar** que
   entonces sí exige DNI (FR-021c).
7. Añadir un segundo contacto y marcarlo también como quien retira. **Comprobar**: el primero se
   desmarca solo; solo puede haber uno (FR-021b).
8. Quitar la marca a los dos. **Comprobar**: la pantalla avisa de que se usará el DNI del propio
   paciente, y la columna "DNI retirada" del listado de retirada lo refleja.
9. Dar de baja un contacto y marcar **"Ver histórico"**. **Comprobar**: sigue ahí con su fecha de baja y
   ha perdido las marcas de principal y de retirada.
10. Seguir el recorrido: idoneidad → consentimiento → tratamiento → depósito → preparación.
    **Comprobar**: no se abre ninguna ventana en todo el trayecto.
11. Registrar la firma del consentimiento. **Comprobar**: la cabecera pasa a ACTIVO en el momento.
12. Paciente con faltantes. **Comprobar**: la pestaña de Depósito aparece marcada como pendiente.
13. Registrar un envase desde una línea del blíster. **Comprobar**: el panel se cierra y la línea ya lo
    muestra.
14. En la pestaña Tratamiento, cambiar la pauta de un medicamento y pulsar **"Ver historial"**.
    **Comprobar**: aparecen los dos tramos, el vigente marcado, cada uno con la pauta y los días que
    estuvieron en vigor y el motivo por el que terminó el anterior.
15. F1 en cuatro pestañas distintas.

## Mantenimiento del catálogo de médicos

Esto **no** hace falta para dar de alta a nadie: es la pantalla a la que se va de vez en cuando a
corregir datos o a limpiar. Se prueba después del recorrido, con médicos ya creados desde las fichas.

1. **Catálogo de médicos** (menú lateral). **Comprobar**: están los que diste de alta desde las fichas,
   y la columna "Pacientes" cuenta los que los tienen de cabecera.
2. Crear un médico con los mismos nombre y apellidos que uno existente. **Comprobar**: avisa del
   posible duplicado **y lo crea igualmente** (FR-034) — pueden ser dos personas distintas, o la misma
   en dos centros.
3. Corregir el centro de un médico ya asignado a un paciente. **Comprobar**: el cambio se ve en la
   ficha del paciente sin tocarla (FR-035: referencia, no copia).
4. Intentar dar de baja al médico de cabecera de un paciente activo. **Comprobar**: lo rechaza
   **diciendo a qué paciente hay que reasignar** (FR-036), no con un "no se puede" a secas.

## Tras la fase 4 — Preparación

1. Ciclo completo por el carril: sesión → ambiente y material → llenado → verificación → entrega.
   **Comprobar**: el paso actual es siempre el correcto y los bloqueados explican por qué.
2. Comparar la rejilla de alvéolos con el blíster real que se está llenando.
3. Verificar eligiendo verificador por nombre; provocar verificador = elaborador. **Comprobar**: motivo
   obligatorio y aviso junto al botón, con "¿Por qué?".
4. Entrega marcando "refiere cambios". **Comprobar**: avisa de las consecuencias antes de aplicarlas.
5. Imprimir ficha, etiquetas e instrucciones desde el carril y abrir los PDF.
