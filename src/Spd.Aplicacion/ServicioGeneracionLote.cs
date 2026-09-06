using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Lote (Spec 007 FR-722..725). Reutiliza los servicios de preparación y de documentos
/// sin duplicar reglas: si un paciente no tiene "envases al día" queda excluido con su motivo;
/// una sesión ya PREPARADA o VERIFICADA se usa tal cual; una en BORRADOR se pasa a PREPARADO con el
/// primer material activo y la última lectura ambiental reutilizable. `impreso_*_en` se registra
/// igual que en la generación individual (FR-725).</summary>
public sealed class ServicioGeneracionLote(
    IServicioPreparacion preparacion,
    IServicioGeneracionDocumentos documentos,
    IRepositorioSpd repositorioSpd,
    IRepositorioPacientes repositorioPacientes)
    : IServicioGeneracionLote
{
    public ResultadoLote Generar(IReadOnlyList<int> pacienteIds, TiposDocumentoLote tipos, int usuarioId)
    {
        if (!tipos.Alguno) throw new ErrorValidacionException("Elija al menos un tipo de documento (FR-723).");

        var generados = new List<LoteGenerado>();
        var excluidos = new List<LoteExcluido>();
        var fallidos = new List<LoteExcluido>();

        foreach (var pacienteId in pacienteIds.Distinct())
        {
            var paciente = repositorioPacientes.ObtenerPorId(pacienteId);
            var nombre = paciente is null ? $"paciente {pacienteId}" : $"{paciente.Nombre} {paciente.Apellidos}";
            try
            {
                var spds = LocalizarSesion(pacienteId);
                if (spds.Count == 0)
                {
                    var comprobacion = preparacion.ComprobarEnvasesAlDia(pacienteId);
                    if (!comprobacion.AlDia)
                    {
                        excluidos.Add(new LoteExcluido(pacienteId, nombre, comprobacion.Motivo ?? "No está listo"));
                        continue;
                    }
                    spds = preparacion.CrearSesion(pacienteId, usuarioId);
                }

                var avisos = new List<string>();
                foreach (var spd in spds.Where(s => s.Estado == EstadoSpd.Borrador))
                {
                    var material = preparacion.ListarMaterialesActivos().FirstOrDefault()
                        ?? throw new ErrorValidacionException("No hay material de acondicionamiento activo; cree uno antes de generar en lote.");
                    preparacion.AsignarMaterial(spd.Id, material.Id);
                    var lectura = preparacion.ObtenerOCrearLecturaAmbiental(null, null, usuarioId);
                    preparacion.PasarAPreparado(spd.Id, lectura.Id, usuarioId);
                    avisos.Add($"{spd.NumRegistro}: preparado automáticamente con material «{material.Descripcion}» y la última lectura ambiental.");
                }

                var ficheros = new List<string>();
                foreach (var spd in spds)
                {
                    if (tipos.Ficha)
                    {
                        ficheros.Add(documentos.GenerarFichaSpd(spd.Id, usuarioId).NombreFichero);
                        preparacion.RegistrarImpresion(spd.Id, TipoDocumentoSpd.Ficha, usuarioId);
                    }
                    if (tipos.Etiquetas)
                    {
                        ficheros.Add(documentos.GenerarEtiquetaAnverso(spd.Id, usuarioId).NombreFichero);
                        ficheros.Add(documentos.GenerarEtiquetaReverso(spd.Id, usuarioId).NombreFichero);
                        preparacion.RegistrarImpresion(spd.Id, TipoDocumentoSpd.Etiquetas, usuarioId);
                    }
                    if (tipos.Instrucciones)
                    {
                        ficheros.Add(documentos.GenerarInstrucciones(spd.Id, usuarioId).NombreFichero);
                        preparacion.RegistrarImpresion(spd.Id, TipoDocumentoSpd.Instrucciones, usuarioId);
                    }
                }
                generados.Add(new LoteGenerado(pacienteId, nombre, ficheros, avisos));
            }
            catch (ErrorValidacionException ex)
            {
                fallidos.Add(new LoteExcluido(pacienteId, nombre, ex.Message));
            }
        }

        return new ResultadoLote(generados, excluidos, fallidos);
    }

    /// <summary>FR-722: la última sesión del paciente si todavía no se entregó; si no, nada (se crea).</summary>
    private IReadOnlyList<SPD> LocalizarSesion(int pacienteId)
    {
        var ultima = repositorioSpd.ListarUltimaSesionDePaciente(pacienteId);
        return ultima.Any(s => s.Estado is EstadoSpd.Borrador or EstadoSpd.Preparado or EstadoSpd.Verificado)
            ? ultima.Where(s => s.Estado is EstadoSpd.Borrador or EstadoSpd.Preparado or EstadoSpd.Verificado).ToList()
            : [];
    }
}
