using System;
using System.Collections.Generic;
using System.Linq;

namespace Spd.Presentacion.Navegacion;

/// <summary>Navegación del marco único (Spec 015 FR-1502): sustituye el contenido de la región
/// central, con historial para volver atrás. No abre ventanas.
///
/// Es un objeto compartido: el marco lo escucha para pintar la sección, y cualquier ViewModel que
/// necesite llevar al usuario a otro sitio (un aviso del inicio, la revisión del nomenclátor) lo
/// recibe y llama a <see cref="Navegar"/>.</summary>
public sealed class Navegador(bool esAdministrador)
{
    private readonly Stack<Destino> _historial = new();

    public bool EsAdministrador { get; } = esAdministrador;

    public Destino? Actual { get; private set; }

    public bool PuedeVolver => _historial.Count > 0;

    public event Action<Destino>? Navegado;

    /// <summary>Las secciones de administración no son alcanzables para un Elaborador, ni siquiera
    /// por una ruta indirecta (FR-1501, misma regla de visibilidad que antes del rediseño).</summary>
    public bool PuedeNavegar(Seccion seccion)
        => EsAdministrador || !EsDeAdministracion(seccion);

    public void Navegar(Destino destino)
    {
        if (!PuedeNavegar(destino.Seccion)) return;
        if (Actual is not null) _historial.Push(Actual);
        Establecer(destino);
    }

    public void Atras()
    {
        if (_historial.Count == 0) return;
        Establecer(_historial.Pop());
    }

    private void Establecer(Destino destino)
    {
        Actual = destino;
        Navegado?.Invoke(destino);
    }

    private static bool EsDeAdministracion(Seccion seccion)
        => EntradaNavegacion.Todas.FirstOrDefault(e => e.Seccion == seccion)?.SoloAdministrador ?? false;
}
