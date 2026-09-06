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

1. Alta de paciente → idoneidad → consentimiento → tratamiento → depósito → preparación.
   **Comprobar**: no se abre ninguna ventana en todo el recorrido.
2. Registrar la firma del consentimiento. **Comprobar**: la cabecera pasa a ACTIVO en el momento.
3. Paciente con faltantes. **Comprobar**: la pestaña de Depósito aparece marcada como pendiente.
4. Registrar un envase desde una línea del blíster. **Comprobar**: el panel se cierra y la línea ya lo
   muestra.
5. F1 en cuatro pestañas distintas.

## Tras la fase 4 — Preparación

1. Ciclo completo por el carril: sesión → ambiente y material → llenado → verificación → entrega.
   **Comprobar**: el paso actual es siempre el correcto y los bloqueados explican por qué.
2. Comparar la rejilla de alvéolos con el blíster real que se está llenando.
3. Verificar eligiendo verificador por nombre; provocar verificador = elaborador. **Comprobar**: motivo
   obligatorio y aviso junto al botón, con "¿Por qué?".
4. Entrega marcando "refiere cambios". **Comprobar**: avisa de las consecuencias antes de aplicarlas.
5. Imprimir ficha, etiquetas e instrucciones desde el carril y abrir los PDF.
