using System.Text.Json;
using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Ciclo de vida completo del blíster (FR-600..695, FR-6120..6127). Toda escritura
/// registra en auditoría (Art. VII.6).</summary>
public sealed class ServicioPreparacion(
    IRepositorioSpd repositorioSpd,
    IRepositorioSpdLineas repositorioLineas,
    IRepositorioSpdLineaEnvases repositorioLineaEnvases,
    IRepositorioSpdVerificaciones repositorioVerificaciones,
    IRepositorioSpdModificaciones repositorioModificaciones,
    IRepositorioRegistrosAmbientales repositorioAmbiental,
    IRepositorioMaterialAcondicionamiento repositorioMaterial,
    IRepositorioPacientes repositorioPacientes,
    IRepositorioTratamientos repositorioTratamientos,
    IRepositorioMedicamentos repositorioMedicamentos,
    IRepositorioEnvases repositorioEnvases,
    IRepositorioFarmacia repositorioFarmacia,
    IServicioAsignacionEnvases servicioAsignacion,
    IServicioEnvases servicioEnvases,
    IServicioListadoRetirada servicioListadoRetirada,
    IComprobadorIdoneidadYConsentimiento comprobadorIdoneidad,
    IRegistradorAuditoria auditoria)
    : IServicioPreparacion
{
    public IReadOnlyList<SPD> CrearSesion(int pacienteId, int elaboradorId)
    {
        var paciente = repositorioPacientes.ObtenerPorId(pacienteId)
            ?? throw new ErrorValidacionException($"No existe el paciente {pacienteId}.");
        if (paciente.Estado != EstadoPaciente.Activo)
            throw new ErrorValidacionException("El paciente debe estar ACTIVO para abrir una sesión de preparación (FR-602).");
        if (!comprobadorIdoneidad.Aprobado(pacienteId))
            throw new ErrorValidacionException("El paciente no tiene idoneidad/consentimiento vigentes (Art. I.3, FR-602).");

        var tratamientosActivos = repositorioTratamientos.ListarVigentesDePaciente(pacienteId).Where(t => t.EnSpd).ToList();
        if (tratamientosActivos.Count == 0)
            throw new ErrorValidacionException("El paciente no tiene ningún tratamiento activo en SPD (FR-602).");

        var faltantes = servicioListadoRetirada.ObtenerListado(
            DateOnly.FromDateTime(DateTime.Today), new FiltrosListadoRetirada(SoloConFaltantes: true, PacienteId: pacienteId));
        if (faltantes.Count > 0)
            throw new ErrorValidacionException(
                "El paciente tiene faltantes pendientes en el listado de retirada; no se puede abrir sesión (FR-602, CA-601).");

        var farmacia = repositorioFarmacia.Obtener() ?? throw new ErrorValidacionException("No hay configuración de farmacia.");
        var correlativo = repositorioSpd.ObtenerSiguienteCorrelativo();
        var sesionId = Guid.NewGuid();
        var validezDesde = DateOnly.FromDateTime(DateTime.Today);

        var creados = new List<SPD>();
        for (var b = 0; b < paciente.NBlisteres; b++)
        {
            var spd = new SPD
            {
                NumRegistro = $"{farmacia.PrefijoNumSpd}{correlativo:D6}",
                CorrelativoNumRegistro = correlativo,
                PacienteId = pacienteId,
                SesionId = sesionId,
                ValidezDesde = validezDesde,
                ValidezHasta = validezDesde.AddDays(6),
                ElaboradorId = elaboradorId
            };
            spd.Id = repositorioSpd.Crear(spd);

            foreach (var tratamiento in tratamientosActivos)
                CrearLineaDesdeTratamiento(spd, tratamiento);

            auditoria.Registrar(elaboradorId, "CREAR_SESION_PREPARACION", "SPD", spd.Id, $"sesion={sesionId}");
            creados.Add(spd);

            correlativo++;
            validezDesde = spd.ValidezHasta.AddDays(1);
        }

        return creados;
    }

    private void CrearLineaDesdeTratamiento(SPD spd, Tratamiento tratamiento)
    {
        var medicamento = repositorioMedicamentos.ObtenerPorId(tratamiento.MedicamentoId);
        if (medicamento is null) return;

        var linea = new SpdLinea
        {
            SpdId = spd.Id,
            TratamientoId = tratamiento.Id,
            MedicamentoId = medicamento.Id,
            SnapNombre = medicamento.Nombre,
            SnapCn = medicamento.Cn,
            SnapPautaD = tratamiento.PautaD,
            SnapPautaA = tratamiento.PautaA,
            SnapPautaC = tratamiento.PautaC,
            SnapPautaN = tratamiento.PautaN,
            SnapDiasSemana = tratamiento.DiasSemana,
            SnapDescTexto = medicamento.DescTexto,
            SnapMomento = tratamiento.Momento,
            UnidadesDosis = CalculadoraUnidadesADescontar.SumaSemanalReal(tratamiento),
            UnidadesEnvase = CalculadoraUnidadesADescontar.Calcular(tratamiento)
        };
        linea.Id = repositorioLineas.Crear(linea);
    }

    public RegistroAmbiental ObtenerOCrearLecturaAmbiental(double? temperatura, double? humedad, int? usuarioId)
    {
        if (temperatura is null || humedad is null)
        {
            var ultima = repositorioAmbiental.ObtenerUltimo();
            var farmaciaActual = repositorioFarmacia.Obtener();
            if (ultima is not null && farmaciaActual is not null
                && (DateTime.UtcNow - ultima.Fecha).TotalHours < farmaciaActual.UmbralReutilizacionLecturaAmbientalHoras)
                return ultima;
            throw new ErrorValidacionException("No hay lectura ambiental reciente que reutilizar; indique temperatura y humedad (FR-630).");
        }

        var farmacia = repositorioFarmacia.Obtener() ?? throw new ErrorValidacionException("No hay configuración de farmacia.");
        var fueraRango = temperatura < farmacia.TempMin || temperatura > farmacia.TempMax
            || humedad < farmacia.HrMin || humedad > farmacia.HrMax;

        var registro = new RegistroAmbiental
        {
            Fecha = DateTime.UtcNow, Temperatura = temperatura.Value, Humedad = humedad.Value,
            FueraRango = fueraRango, UsuarioId = usuarioId
        };
        registro.Id = repositorioAmbiental.Crear(registro);
        return registro;
    }

    public MaterialAcondicionamiento CrearMaterial(string descripcion, string lote, DateOnly fechaEntrada)
    {
        var material = new MaterialAcondicionamiento { Descripcion = descripcion, Lote = lote, FechaEntrada = fechaEntrada };
        material.Id = repositorioMaterial.Crear(material);
        return material;
    }

    public IReadOnlyList<MaterialAcondicionamiento> ListarMaterialesActivos() => repositorioMaterial.ListarActivos();

    public void AsignarMaterial(int spdId, int materialId)
    {
        var spd = repositorioSpd.ObtenerPorId(spdId) ?? throw new ErrorValidacionException($"No existe el SPD {spdId}.");
        spd.MaterialId = materialId;
        repositorioSpd.Actualizar(spd);
    }

    public void ExcluirLinea(int lineaId, string motivo, int? usuarioId)
    {
        var linea = repositorioLineas.ObtenerPorId(lineaId) ?? throw new ErrorValidacionException($"No existe la línea {lineaId}.");
        linea.EstadoLinea = EstadoLinea.Excluida;
        linea.MotivoExclusion = motivo;
        repositorioLineas.Actualizar(linea);

        auditoria.Registrar(usuarioId, "EXCLUIR_LINEA_SPD", "SpdLinea", lineaId, motivo);
    }

    public Envase RegistrarEnvaseDesdeLinea(int lineaId, DatosAltaEnvase datos, int? usuarioId)
    {
        _ = repositorioLineas.ObtenerPorId(lineaId) ?? throw new ErrorValidacionException($"No existe la línea {lineaId}.");
        return servicioEnvases.RegistrarEnvase(datos, usuarioId);
    }

    public void PasarAPreparado(int spdId, int registroAmbientalId, int? usuarioId)
    {
        var spd = repositorioSpd.ObtenerPorId(spdId) ?? throw new ErrorValidacionException($"No existe el SPD {spdId}.");
        if (spd.Estado != EstadoSpd.Borrador)
            throw new ErrorValidacionException("Solo se puede pasar a PREPARADO un SPD en BORRADOR.");
        if (spd.MaterialId is null)
            throw new ErrorValidacionException("Falta asignar el material de acondicionamiento (FR-632).");

        var lineas = repositorioLineas.ListarPorSpd(spdId).Where(l => l.EstadoLinea != EstadoLinea.Excluida).ToList();

        var faltantes = new List<string>();
        foreach (var linea in lineas)
        {
            var disponibles = CalcularDisponibles(spd.PacienteId, linea.MedicamentoId, spd.ValidezHasta);
            if (disponibles < linea.UnidadesEnvase)
                faltantes.Add($"{linea.SnapNombre} (faltan {linea.UnidadesEnvase - disponibles} unidades)");
        }
        if (faltantes.Count > 0)
            throw new ErrorValidacionException($"No hay envases suficientes para: {string.Join("; ", faltantes)} (FR-612, CA-603).");

        foreach (var linea in lineas)
            DescontarYRegistrarEnvases(linea, spd.ValidezHasta, usuarioId);

        spd.Estado = EstadoSpd.Preparado;
        spd.FechaPreparacion = DateTime.UtcNow;
        spd.RegistroAmbientalId = registroAmbientalId;
        repositorioSpd.Actualizar(spd);

        auditoria.Registrar(usuarioId, "PASAR_A_PREPARADO", "SPD", spdId, null);
    }

    private int CalcularDisponibles(int pacienteId, int medicamentoId, DateOnly caducidadMinima)
        => repositorioEnvases.ListarEnCustodiaDePacienteYMedicamento(pacienteId, medicamentoId)
            .Where(e => e.Caducidad is { } c && c >= caducidadMinima)
            .Sum(e => e.UnidadesRestantes ?? 0);

    private void DescontarYRegistrarEnvases(SpdLinea linea, DateOnly caducidadMinima, int? usuarioId)
        => DescontarYRegistrarEnvases(linea.Id, linea.TratamientoId, linea.UnidadesEnvase, caducidadMinima, usuarioId);

    private void DescontarYRegistrarEnvases(int spdLineaId, int tratamientoId, int unidadesEnvase, DateOnly caducidadMinima, int? usuarioId)
    {
        var resultado = servicioAsignacion.Descontar(tratamientoId, unidadesEnvase, caducidadMinima, usuarioId);
        foreach (var asignacion in resultado.Asignaciones)
            repositorioLineaEnvases.Crear(new SpdLineaEnvase
            {
                SpdLineaId = spdLineaId,
                EnvaseId = asignacion.Envase.Id,
                UnidadesTomadas = asignacion.UnidadesTomadas,
                SnapSerie = asignacion.Envase.Serie,
                SnapLote = asignacion.Envase.Lote,
                SnapCaducidad = asignacion.Envase.Caducidad
            });
    }

    public SPD Verificar(int spdId, int verificadorId, ChecklistVerificacion checklist, string? excepcionMotivo, int? usuarioId)
    {
        var spd = repositorioSpd.ObtenerPorId(spdId) ?? throw new ErrorValidacionException($"No existe el SPD {spdId}.");
        if (spd.Estado != EstadoSpd.Preparado)
            throw new ErrorValidacionException("Solo se puede verificar un SPD en PREPARADO (FR-650).");

        var esExcepcion = verificadorId == spd.ElaboradorId;
        if (esExcepcion && (string.IsNullOrWhiteSpace(excepcionMotivo) || excepcionMotivo.Trim().Length < 10))
            throw new ErrorValidacionException(
                "Verificador = elaborador exige un motivo de excepción de al menos 10 caracteres (FR-652, Art. I.3/VII.5).");

        var resultado = checklist.TodosAptos ? ResultadoVerificacion.Apto : ResultadoVerificacion.NoApto;
        var motivoRegistrado = esExcepcion ? excepcionMotivo : null;

        repositorioVerificaciones.Crear(new SpdVerificacion
        {
            SpdId = spdId, VerificadorId = verificadorId, Fecha = DateTime.UtcNow,
            VerifAspecto = checklist.Aspecto, VerifEtiquetaDatos = checklist.EtiquetaDatos,
            VerifEtiquetaValidez = checklist.EtiquetaValidez, VerifInstrucciones = checklist.Instrucciones,
            VerifContenido = checklist.Contenido, VerifFabricantePnt = checklist.FabricanteYPnt,
            VerifEtiquetaFichaPaciente = checklist.EtiquetaFichaPaciente, VerifTrazabilidad = checklist.Trazabilidad,
            Resultado = resultado, ExcepcionMotivo = motivoRegistrado
        });

        spd.VerificadorId = verificadorId;
        spd.FechaVerificacion = DateTime.UtcNow;
        spd.ExcepcionVerificadorMotivo = motivoRegistrado;
        spd.ResultadoVerificacion = resultado;
        if (resultado == ResultadoVerificacion.Apto) spd.Estado = EstadoSpd.Verificado;
        repositorioSpd.Actualizar(spd);

        auditoria.Registrar(usuarioId, "VERIFICAR_SPD", "SPD", spdId, resultado.ToString());
        return spd;
    }

    public IReadOnlyList<SPD> RegistrarEntrega(DatosEntregaSpd datos, IReadOnlyList<int> spdIds, int? usuarioId)
    {
        var entregados = new List<SPD>();
        foreach (var spdId in spdIds)
        {
            var spd = repositorioSpd.ObtenerPorId(spdId) ?? throw new ErrorValidacionException($"No existe el SPD {spdId}.");
            if (spd.Estado != EstadoSpd.Verificado)
                throw new ErrorValidacionException($"El SPD {spd.NumRegistro} debe estar VERIFICADO para entregarse (FR-660).");

            spd.Estado = EstadoSpd.Entregado;
            spd.EntregadorId = usuarioId;
            spd.FechaEntrega = datos.Fecha.ToDateTime(TimeOnly.MinValue);
            spd.EntregadoA = datos.EntregadoA;
            spd.PrimeraEntrega = datos.PrimeraEntrega;
            spd.SpdAnteriorRecogido = datos.SpdAnteriorRecogido;
            spd.UnidadesNoAdministradas = datos.UnidadesNoAdministradas;
            spd.ObservacionesAdherencia = datos.ObservacionesAdherencia;
            spd.CambiosMedicacionPreguntado = datos.CambiosMedicacionPreguntado;
            spd.ObservacionesEtiqueta = datos.ObservacionesEtiqueta;
            repositorioSpd.Actualizar(spd);

            auditoria.Registrar(usuarioId, "ENTREGAR_SPD", "SPD", spdId, null);
            entregados.Add(spd);
        }

        // FR-663: un cambio de medicación referido por el paciente sin prescripción deja los
        // tratamientos en SPD pendientes de revisión, lo que bloquea la sesión siguiente (FR-674)
        // hasta que un farmacéutico lo confirme con el médico (Spec 008 FR-805).
        if (datos.CambiosMedicacionReferidos && entregados.Count > 0)
        {
            var pacienteId = entregados[0].PacienteId;
            foreach (var tratamiento in repositorioTratamientos.ListarVigentesDePaciente(pacienteId)
                         .Where(t => t.EnSpd && t.Estado == EstadoTratamiento.Activo))
            {
                tratamiento.Estado = EstadoTratamiento.PendienteRevision;
                repositorioTratamientos.Actualizar(tratamiento);
                auditoria.Registrar(usuarioId, "TRATAMIENTO_PENDIENTE_REVISION", "Tratamiento", tratamiento.Id, "cambios referidos en la entrega (FR-663)");
            }
        }
        return entregados;
    }

    public IReadOnlyList<SPD> PrepararSiguiente(int pacienteId, int elaboradorId, out ResultadoContinuidad resultado)
    {
        var paciente = repositorioPacientes.ObtenerPorId(pacienteId)
            ?? throw new ErrorValidacionException($"No existe el paciente {pacienteId}.");

        var ultimaSesion = repositorioSpd.ListarUltimaSesionDePaciente(pacienteId);
        if (ultimaSesion.Count == 0 || ultimaSesion.Any(s => s.Estado != EstadoSpd.Entregado))
            throw new ErrorValidacionException(
                "Solo se puede preparar la sesión siguiente cuando todos los SPD de la última sesión están ENTREGADO (FR-670).");

        var tratamientosActivos = repositorioTratamientos.ListarVigentesDePaciente(pacienteId).Where(t => t.EnSpd).ToList();
        if (tratamientosActivos.Any(t => t.Estado == EstadoTratamiento.PendienteRevision))
            throw new ErrorValidacionException(
                "Hay un tratamiento pendiente de revisión; no se puede abrir la sesión siguiente (FR-674).");

        var farmacia = repositorioFarmacia.Obtener() ?? throw new ErrorValidacionException("No hay configuración de farmacia.");
        var correlativo = repositorioSpd.ObtenerSiguienteCorrelativo();
        var sesionId = Guid.NewGuid();
        var validezDesde = ultimaSesion[0].ValidezHasta.AddDays(1);

        var modificadas = new List<SpdLinea>();
        var nuevas = new List<Tratamiento>();
        var eliminadas = new List<SpdLinea>();
        var envasePendientes = new List<SpdLinea>();
        var nuevosSpd = new List<SPD>();

        for (var i = 0; i < paciente.NBlisteres; i++)
        {
            var spdAnterior = i < ultimaSesion.Count ? ultimaSesion[i] : null;
            var lineasAnteriores = spdAnterior is not null ? repositorioLineas.ListarPorSpd(spdAnterior.Id) : [];

            var nuevo = new SPD
            {
                NumRegistro = $"{farmacia.PrefijoNumSpd}{correlativo:D6}",
                CorrelativoNumRegistro = correlativo,
                PacienteId = pacienteId,
                SesionId = sesionId,
                ValidezDesde = validezDesde,
                ValidezHasta = validezDesde.AddDays(6),
                ElaboradorId = elaboradorId,
                MaterialId = spdAnterior?.MaterialId
            };
            nuevo.Id = repositorioSpd.Crear(nuevo);

            foreach (var tratamiento in tratamientosActivos)
            {
                var medicamento = repositorioMedicamentos.ObtenerPorId(tratamiento.MedicamentoId);
                if (medicamento is null) continue;

                var anterior = lineasAnteriores.FirstOrDefault(l => l.MedicamentoId == tratamiento.MedicamentoId);
                var linea = new SpdLinea
                {
                    SpdId = nuevo.Id, TratamientoId = tratamiento.Id, MedicamentoId = medicamento.Id,
                    SnapNombre = medicamento.Nombre, SnapCn = medicamento.Cn,
                    SnapPautaD = tratamiento.PautaD, SnapPautaA = tratamiento.PautaA,
                    SnapPautaC = tratamiento.PautaC, SnapPautaN = tratamiento.PautaN,
                    SnapDiasSemana = tratamiento.DiasSemana, SnapDescTexto = medicamento.DescTexto, SnapMomento = tratamiento.Momento,
                    UnidadesDosis = CalculadoraUnidadesADescontar.SumaSemanalReal(tratamiento),
                    UnidadesEnvase = CalculadoraUnidadesADescontar.Calcular(tratamiento)
                };

                if (anterior is null)
                {
                    nuevas.Add(tratamiento);
                }
                else if (anterior.SnapPautaD != tratamiento.PautaD || anterior.SnapPautaA != tratamiento.PautaA
                         || anterior.SnapPautaC != tratamiento.PautaC || anterior.SnapPautaN != tratamiento.PautaN
                         || anterior.SnapDiasSemana != tratamiento.DiasSemana || anterior.SnapMomento != tratamiento.Momento)
                {
                    modificadas.Add(linea);
                }

                var disponibles = CalcularDisponibles(pacienteId, medicamento.Id, nuevo.ValidezHasta);
                if (disponibles < linea.UnidadesEnvase)
                {
                    linea.EstadoLinea = EstadoLinea.EnvasePendiente;
                    envasePendientes.Add(linea);
                }

                linea.Id = repositorioLineas.Crear(linea);
            }

            foreach (var lineaAnterior in lineasAnteriores)
                if (!tratamientosActivos.Any(t => t.MedicamentoId == lineaAnterior.MedicamentoId))
                    eliminadas.Add(lineaAnterior);

            auditoria.Registrar(elaboradorId, "PREPARAR_SIGUIENTE", "SPD", nuevo.Id, $"sesion={sesionId}");
            nuevosSpd.Add(nuevo);

            correlativo++;
            validezDesde = nuevo.ValidezHasta.AddDays(1);
        }

        resultado = new ResultadoContinuidad(modificadas, nuevas, eliminadas, envasePendientes);
        return nuevosSpd;
    }

    public SPD Reelaborar(int spdId, OrigenSolicitudReelaboracion origen, string motivo, int usuarioId)
    {
        var spd = repositorioSpd.ObtenerPorId(spdId) ?? throw new ErrorValidacionException($"No existe el SPD {spdId}.");
        if (spd.Estado != EstadoSpd.Preparado && spd.Estado != EstadoSpd.Verificado)
            throw new ErrorValidacionException("Solo se puede reelaborar un SPD en PREPARADO o VERIFICADO (FR-6120, CA-6125).");

        var lineasAnteriores = repositorioLineas.ListarPorSpd(spdId);
        var envasesAnteriores = lineasAnteriores.SelectMany(l => repositorioLineaEnvases.ListarPorLinea(l.Id)).ToList();
        var snapshotAnterior = JsonSerializer.Serialize(new { Lineas = lineasAnteriores, Envases = envasesAnteriores });

        var tratamientosActivos = repositorioTratamientos.ListarVigentesDePaciente(spd.PacienteId).Where(t => t.EnSpd).ToList();
        var lineasVigentes = lineasAnteriores.Where(l => l.EstadoLinea != EstadoLinea.Excluida).ToList();
        var resumenCambios = new List<Dictionary<string, object?>>();

        foreach (var tratamiento in tratamientosActivos)
        {
            var medicamento = repositorioMedicamentos.ObtenerPorId(tratamiento.MedicamentoId);
            if (medicamento is null) continue;

            var anterior = lineasVigentes.FirstOrDefault(l => l.MedicamentoId == tratamiento.MedicamentoId);
            var unidadesNuevas = CalculadoraUnidadesADescontar.Calcular(tratamiento);

            if (anterior is null)
            {
                var lineaNueva = new SpdLinea
                {
                    SpdId = spdId, TratamientoId = tratamiento.Id, MedicamentoId = medicamento.Id,
                    SnapNombre = medicamento.Nombre, SnapCn = medicamento.Cn,
                    SnapPautaD = tratamiento.PautaD, SnapPautaA = tratamiento.PautaA,
                    SnapPautaC = tratamiento.PautaC, SnapPautaN = tratamiento.PautaN,
                    SnapDiasSemana = tratamiento.DiasSemana, SnapDescTexto = medicamento.DescTexto, SnapMomento = tratamiento.Momento,
                    UnidadesDosis = CalculadoraUnidadesADescontar.SumaSemanalReal(tratamiento), UnidadesEnvase = unidadesNuevas
                };
                lineaNueva.Id = repositorioLineas.Crear(lineaNueva);
                DescontarYRegistrarEnvases(lineaNueva, spd.ValidezHasta, usuarioId);
                resumenCambios.Add(new() { ["medicamento"] = medicamento.Nombre, ["cambio"] = "NUEVA", ["antes"] = 0, ["despues"] = unidadesNuevas });
            }
            else if (unidadesNuevas > anterior.UnidadesEnvase)
            {
                var diferencia = unidadesNuevas - anterior.UnidadesEnvase;
                var antes = anterior.UnidadesEnvase;
                DescontarYRegistrarEnvases(anterior.Id, tratamiento.Id, diferencia, spd.ValidezHasta, usuarioId);
                anterior.UnidadesEnvase = unidadesNuevas;
                anterior.UnidadesDosis = CalculadoraUnidadesADescontar.SumaSemanalReal(tratamiento);
                repositorioLineas.Actualizar(anterior);
                resumenCambios.Add(new() { ["medicamento"] = medicamento.Nombre, ["cambio"] = "AUMENTADA", ["antes"] = antes, ["despues"] = unidadesNuevas });
            }
            else if (unidadesNuevas < anterior.UnidadesEnvase)
            {
                var diferencia = anterior.UnidadesEnvase - unidadesNuevas;
                DevolverDiferencia(anterior.Id, diferencia);
                var antes = anterior.UnidadesEnvase;
                anterior.UnidadesEnvase = unidadesNuevas;
                anterior.UnidadesDosis = CalculadoraUnidadesADescontar.SumaSemanalReal(tratamiento);
                repositorioLineas.Actualizar(anterior);
                resumenCambios.Add(new() { ["medicamento"] = medicamento.Nombre, ["cambio"] = "REDUCIDA", ["antes"] = antes, ["despues"] = unidadesNuevas });
            }
        }

        foreach (var lineaVigente in lineasVigentes)
        {
            if (tratamientosActivos.Any(t => t.MedicamentoId == lineaVigente.MedicamentoId)) continue;
            DevolverDiferencia(lineaVigente.Id, lineaVigente.UnidadesEnvase);
            var antes = lineaVigente.UnidadesEnvase;
            lineaVigente.EstadoLinea = EstadoLinea.Excluida;
            lineaVigente.MotivoExclusion = "Eliminada en reelaboración";
            repositorioLineas.Actualizar(lineaVigente);
            resumenCambios.Add(new() { ["medicamento"] = lineaVigente.SnapNombre, ["cambio"] = "ELIMINADA", ["antes"] = antes, ["despues"] = 0 });
        }

        var versionAnterior = spd.Version;
        spd.Version++;
        spd.Estado = EstadoSpd.Preparado;
        spd.VerificadorId = null;
        spd.FechaVerificacion = null;
        spd.ExcepcionVerificadorMotivo = null;
        spd.ResultadoVerificacion = null;
        repositorioSpd.Actualizar(spd);

        repositorioModificaciones.Crear(new SpdModificacion
        {
            SpdId = spdId, VersionAnterior = versionAnterior, VersionNueva = spd.Version, Fecha = DateTime.UtcNow,
            UsuarioId = usuarioId, OrigenSolicitud = origen, Motivo = motivo,
            ResumenCambios = JsonSerializer.Serialize(resumenCambios), LineasSnapshotAnterior = snapshotAnterior
        });

        auditoria.Registrar(usuarioId, "REELABORAR_SPD", "SPD", spdId, $"version {versionAnterior}->{spd.Version}");
        return spd;
    }

    private void DevolverDiferencia(int spdLineaId, int unidadesADevolver)
    {
        var filas = repositorioLineaEnvases.ListarPorLinea(spdLineaId).OrderByDescending(f => f.Id).ToList();
        var pendiente = unidadesADevolver;
        foreach (var fila in filas)
        {
            if (pendiente <= 0) break;
            var devolver = Math.Min((decimal)pendiente, fila.UnidadesTomadas);
            if (devolver <= 0) continue;

            var envase = repositorioEnvases.ObtenerPorId(fila.EnvaseId);
            if (envase is not null)
            {
                envase.UnidadesRestantes = (envase.UnidadesRestantes ?? 0) + (int)devolver;
                if (envase.Estado == EstadoEnvase.Agotado) envase.Estado = EstadoEnvase.EnCustodia;
                repositorioEnvases.Actualizar(envase);
            }

            fila.UnidadesTomadas -= devolver;
            repositorioLineaEnvases.Actualizar(fila);
            pendiente -= (int)devolver;
        }
    }

    public void RegistrarImpresion(int spdId, TipoDocumentoSpd tipo, int? usuarioId)
    {
        var spd = repositorioSpd.ObtenerPorId(spdId) ?? throw new ErrorValidacionException($"No existe el SPD {spdId}.");
        var ahora = DateTime.UtcNow;
        switch (tipo)
        {
            case TipoDocumentoSpd.Ficha: spd.ImpresoFichaEn = ahora; break;
            case TipoDocumentoSpd.Etiquetas: spd.ImpresoEtiquetasEn = ahora; break;
            case TipoDocumentoSpd.Instrucciones: spd.ImpresoInstruccionesEn = ahora; break;
        }
        repositorioSpd.Actualizar(spd);

        auditoria.Registrar(usuarioId, "IMPRIMIR", "SPD", spdId, tipo.ToString());
    }

    public void Anular(int spdId, string motivo, int? usuarioId)
    {
        var spd = repositorioSpd.ObtenerPorId(spdId) ?? throw new ErrorValidacionException($"No existe el SPD {spdId}.");
        spd.Estado = EstadoSpd.Anulado;
        spd.MotivoAnulacion = motivo;
        repositorioSpd.Actualizar(spd);

        auditoria.Registrar(usuarioId, "ANULAR_SPD", "SPD", spdId, motivo);
    }

    public IReadOnlyList<SPD> ListarPorFiltro(FiltrosPreparaciones filtros)
        => repositorioSpd.Listar(filtros.Estado, filtros.PacienteId, filtros.ElaboradorId);

    public IReadOnlyList<SpdLinea> ListarLineas(int spdId) => repositorioLineas.ListarPorSpd(spdId);

    public ResultadoEnvasesAlDia ComprobarEnvasesAlDia(int pacienteId)
    {
        var paciente = repositorioPacientes.ObtenerPorId(pacienteId);
        if (paciente is null) return new ResultadoEnvasesAlDia(false, "Paciente inexistente");
        if (paciente.Estado != EstadoPaciente.Activo) return new ResultadoEnvasesAlDia(false, $"Paciente en {paciente.Estado}");
        if (!comprobadorIdoneidad.Aprobado(pacienteId)) return new ResultadoEnvasesAlDia(false, "Sin idoneidad o consentimiento vigentes");

        var tratamientos = repositorioTratamientos.ListarVigentesDePaciente(pacienteId).Where(t => t.EnSpd).ToList();
        if (tratamientos.Count == 0) return new ResultadoEnvasesAlDia(false, "Sin tratamiento activo en SPD");
        if (tratamientos.Any(t => t.Estado == EstadoTratamiento.PendienteRevision))
            return new ResultadoEnvasesAlDia(false, "Tratamiento pendiente de revisión");

        // Saldo real en custodia para la próxima sesión completa (n blísteres de 7 días), con la
        // misma regla de caducidad que PasarAPreparado; no depende de la ventana del listado de retirada.
        var caducidadMinima = DateOnly.FromDateTime(DateTime.Today).AddDays(7 * paciente.NBlisteres - 1);
        var faltantes = new List<string>();
        foreach (var t in tratamientos)
        {
            var necesarias = CalculadoraUnidadesADescontar.Calcular(t) * paciente.NBlisteres;
            var disponibles = CalcularDisponibles(pacienteId, t.MedicamentoId, caducidadMinima);
            if (disponibles < necesarias)
                faltantes.Add($"{repositorioMedicamentos.ObtenerPorId(t.MedicamentoId)?.Nombre ?? $"medicamento {t.MedicamentoId}"} ({necesarias - disponibles})");
        }
        return faltantes.Count == 0
            ? new ResultadoEnvasesAlDia(true, null)
            : new ResultadoEnvasesAlDia(false, "Faltan: " + string.Join(", ", faltantes));
    }
}
