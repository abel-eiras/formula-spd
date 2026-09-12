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

## Antes del recorrido — médicos y contactos (Spec 001 US2/US3)

Se construyeron después de escribir este guion, y el médico de cabecera es ahora parte de la ficha.

1. **Catálogo de médicos** (menú lateral, trabajo diario). Dar de alta dos médicos. **Comprobar**: con
   nombre y apellidos basta; la columna "Pacientes" empieza en 0.
2. Crear un tercer médico con los mismos nombre y apellidos que el primero. **Comprobar**: avisa del
   posible duplicado **y lo crea igualmente** (FR-034); el aviso no bloquea.
3. En la ficha de un paciente, campo **Médico de cabecera**: escribir dos letras del apellido.
   **Comprobar**: encuentra sin tildes ni mayúsculas; "Cambiar" lo suelta.
4. Escribir un apellido que no existe y pulsar **"Nuevo médico…"**. **Comprobar**: se abre un panel a la
   derecha con el apellido ya puesto, **la ficha de detrás se sigue viendo**, y al guardar queda
   seleccionado sin haber perdido nada de lo que estabas rellenando.
5. Volver al catálogo e intentar dar de baja al médico de cabecera de ese paciente. **Comprobar**: lo
   rechaza **diciendo a qué paciente hay que reasignar** (FR-036), no con un "no se puede" a secas.
6. **Contactos** (en la pestaña Datos del paciente, debajo de los datos). Añadir un familiar sin DNI:
   entra. Marcarlo como "retira la medicación": **comprobar** que exige DNI (FR-021c).
7. Añadir un segundo contacto y marcarlo también como quien retira. **Comprobar**: el primero se
   desmarca solo; solo puede haber uno.
8. Sin nadie marcado como quien retira, **comprobar** que la pantalla avisa de que se usará el DNI del
   propio paciente, y que la columna "DNI retirada" del listado de retirada lo refleja.
9. Dar de baja un contacto y marcar **"Ver histórico"**. **Comprobar**: sigue ahí con su fecha de baja y
   ha perdido las marcas de principal y de retirada.

## Tras la fase 3 — Espacio del paciente

1. Alta de paciente → idoneidad → consentimiento → tratamiento → depósito → preparación.
   **Comprobar**: no se abre ninguna ventana en todo el recorrido.
2. Registrar la firma del consentimiento. **Comprobar**: la cabecera pasa a ACTIVO en el momento.
3. Paciente con faltantes. **Comprobar**: la pestaña de Depósito aparece marcada como pendiente.
4. Registrar un envase desde una línea del blíster. **Comprobar**: el panel se cierra y la línea ya lo
   muestra.
5. En la pestaña Tratamiento, cambiar la pauta de un medicamento y pulsar **"Ver historial"**.
   **Comprobar**: aparecen los dos tramos, el vigente marcado, cada uno con la pauta y los días que
   estuvieron en vigor y el motivo por el que terminó el anterior.
6. F1 en cuatro pestañas distintas.

## Tras la fase 4 — Preparación

1. Ciclo completo por el carril: sesión → ambiente y material → llenado → verificación → entrega.
   **Comprobar**: el paso actual es siempre el correcto y los bloqueados explican por qué.
2. Comparar la rejilla de alvéolos con el blíster real que se está llenando.
3. Verificar eligiendo verificador por nombre; provocar verificador = elaborador. **Comprobar**: motivo
   obligatorio y aviso junto al botón, con "¿Por qué?".
4. Entrega marcando "refiere cambios". **Comprobar**: avisa de las consecuencias antes de aplicarlas.
5. Imprimir ficha, etiquetas e instrucciones desde el carril y abrir los PDF.
