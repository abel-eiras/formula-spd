using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Última lectura ambiental y si todavía se reutiliza para una preparación (Spec 006 FR-630).</summary>
public sealed record LecturaAmbientalReciente(RegistroAmbiental Lectura, bool Reutilizable);
