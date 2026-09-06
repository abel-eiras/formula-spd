using System;
using Spd.Aplicacion;
using Spd.Dominio;

namespace Spd.Presentacion.Pacientes;

/// <summary>Las pestañas del espacio del paciente (Spec 015 FR-1530).</summary>
public enum PestanaPaciente
{
    Datos,
    Idoneidad,
    Tratamiento,
    Deposito,
    Preparacion,
    Comunicaciones,
    Documentos
}

/// <summary>El paciente que se está mirando, compartido por la cabecera y las siete pestañas
/// (research.md Decisión 5).
///
/// Resuelve un defecto real del diseño anterior: cada ventana releía el paciente por su cuenta y
/// ninguna se enteraba de lo que hacían las demás, así que la ficha mostraba EVALUACION después de
/// que la ventana de idoneidad hubiera activado al paciente. El parche era volver a leer al cerrar
/// la ventana (`FichaPacienteViewModel.RecargarPaciente` + `ventana.Closed +=`); sin ventanas que
/// cerrar, el parche no vale, y tampoco hacía falta: quien cambia al paciente avisa aquí y todo lo
/// que lo muestra se entera.</summary>
public sealed class PacienteContexto(IServicioPacientes servicio)
{
    public Paciente? Paciente { get; private set; }

    /// <summary>Null mientras el alta no se ha guardado: sin `paciente_id` no hay tratamientos,
    /// depósito, preparación ni comunicaciones que valgan (FR-400, FR-510, FR-600, FR-801).</summary>
    public int? PacienteId => Paciente?.Id;

    /// <summary>Lo dispara quien cambia al paciente; lo escuchan la cabecera y las pestañas.</summary>
    public event Action<Paciente?>? Cambiado;

    /// <summary>Petición de cambio de pestaña desde otra pestaña, con un argumento opcional (el
    /// prerrelleno de una comunicación al médico, por ejemplo). Antes esto abría una ventana.</summary>
    public event Action<PestanaPaciente, object?>? PestanaSolicitada;

    public void Establecer(Paciente? paciente)
    {
        Paciente = paciente;
        Cambiado?.Invoke(paciente);
    }

    /// <summary>Relee el paciente de la base de datos. Lo llama quien sabe que pudo cambiar algo
    /// que él mismo no escribió: la idoneidad, que activa al paciente (Spec 002 FR-213).</summary>
    public void Recargar()
    {
        if (Paciente is null) return;
        var actual = servicio.ObtenerPorId(Paciente.Id);
        if (actual is not null) Establecer(actual);
    }

    public void IrA(PestanaPaciente pestana, object? argumento = null)
        => PestanaSolicitada?.Invoke(pestana, argumento);
}
