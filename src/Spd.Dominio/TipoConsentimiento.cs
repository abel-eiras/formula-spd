namespace Spd.Dominio;

/// <summary>Quién firma el consentimiento informado (Spec 002 FR-210): el propio paciente, o un
/// representante legal / persona autorizada registrado como contacto.</summary>
public enum TipoConsentimiento
{
    Paciente,
    Representante
}
