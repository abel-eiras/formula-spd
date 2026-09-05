namespace Spd.Aplicacion;

/// <summary>Consulta puntual de un medicamento por CN al CIMA REST API público de la AEMPS
/// (https://cima.aemps.es/cima/rest/), en el momento del alta (escaneado o tecleado). Alternativa
/// más fiable que el nomenclátor de facturación para forma farmacéutica: CIMA la da en un
/// vocabulario cerrado (`formaFarmaceuticaSimplificada`) en vez de tener que extraerla por regex
/// de un texto libre. Nunca sustituye la decisión manual de aptitud SPD ni descripción física
/// (Art. I.2/I.3): solo propone datos, editables antes de guardar.</summary>
public interface IServicioConsultaCima
{
    Task<ResultadoConsultaCima> ConsultarPorCnAsync(string cn);
}
