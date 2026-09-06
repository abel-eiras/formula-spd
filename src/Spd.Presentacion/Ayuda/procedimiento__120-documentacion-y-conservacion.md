# Documentación: qué debe existir en cada momento y cuánto se conserva

Lista de comprobación pensada para la autoevaluación antes de una inspección (PNT VII, Guía del CIM §5.3). Cada punto indica dónde se ve en la aplicación.

## De la farmacia, una vez

- Declaración responsable presentada (SA480A) — fuera de la aplicación.
- Siete PNT aprobados, firmados y fechados por el titular; versiones obsoletas separadas — control de cambios y copias en [[uso:registros-calidad]] (control documental).
- Registros de formación acreditada del personal — [[uso:registros-calidad]].
- Organigrama y reconocimiento de firmas — modelo oficial; cargos y firmas abreviadas en [[uso:configuracion]] (usuarios).
- Datos de la farmacia, logo, colegiado del titular, protección de datos (responsable, DPO) — [[uso:configuracion]].
- Documentación de soporte para evaluar medicamentos: Real Farmacopea, Bot PLUS / CIMA — la aptitud y descripción física se guardan en [[uso:catalogo-medicamentos]].

## En el alta de un paciente

- Ficha del paciente cumplimentada (datos, familiar/cuidador, médico, salud) — [[uso:ficha-paciente]], documento `FICHA-PAC`.
- Hoja de información inicial entregada (Anexo I.A) — modelo oficial.
- Evaluación de idoneidad con resultado y firma — [[uso:idoneidad-consentimiento]]; se imprime en `FICHA-PAC`.
- Consentimiento informado firmado, copia para el paciente y copia archivada — `CONSENT`, con fecha de firma registrada.
- Información de protección de datos entregada — `RGPD`.
- Hoja de medicación activa / recetas que justifiquen el tratamiento — en papel, junto a la ficha.

## Antes de preparar

- Paciente **ACTIVO**: idoneidad APTO vigente + consentimiento vigente — cabecera de [[uso:idoneidad-consentimiento]].
- Tratamiento activo y revisado, sin líneas pendientes de revisión — [[uso:tratamientos]].
- Envases suficientes en custodia para cada línea — [[uso:deposito]] y [[uso:retirada-envases]].
- Lectura de temperatura y humedad del día y material de acondicionamiento identificado — [[uso:preparacion]].
- Limpieza diaria hecha — registro oficial en papel.

## Para entregar

- Blíster **VERIFICADO** por un farmacéutico distinto (o excepción con motivo) — [[uso:preparacion]].
- Ficha de preparación, control y entrega impresa con elaborador y verificador — `FICHA`.
- Etiquetas anverso y reverso en el DDP, hoja de instrucciones — `ETQ-A`, `ETQ-R`, `INSTR`.
- Primera entrega: prospectos; continuación: control de adherencia anotado — datos de entrega en [[uso:preparacion]].
- Acuse de recibo firmado — modelo oficial.

## Conservación y plazos

- Toda la documentación generada en cualquier fase de la preparación: **mínimo un año**.
- Fichas de paciente: **al menos un año después de la baja** del servicio (constitución Art. I.3).
- Política de protección de datos entregada al paciente: bloqueo al año de inactividad y borrado al cuarto año. La aplicación **no borra nunca** por sí sola: solo el administrador puede purgar, paciente a paciente, con confirmación (Art. III).
- Documentación en digital que requiera firma: firma electrónica, o se conserva el papel firmado. La aplicación genera los documentos; **el papel firmado es la base legal** (Art. II).
- Copias de seguridad y cifrado de la base de datos — [[uso:configuracion]] (seguridad). Auditoría de toda acción: solo inserción, sin edición ni borrado.

## Organización del archivo

Carpeta por paciente (consentimiento, ficha, prescripciones, fichas de preparación); carpeta de formación; carpeta de PNT vigentes y otra de obsoletos; carpeta de registros (limpieza, calibración, ambiental, residuos). Archivadores con llave o zona de acceso restringido; equipos informáticos con identificación de usuario — la aplicación exige inicio de sesión y registra quién hace cada cosa.
