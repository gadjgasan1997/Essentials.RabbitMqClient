namespace Essentials.RabbitMqClient.Publisher.Models;

/// <summary>
/// Базовый ответ на Rpc запрос
/// </summary>
public class BaseRpcResponse
{
    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="success">Признак успеха</param>
    /// <param name="responseCode">Код ответа</param>
    /// <param name="responseMessage">Сообщение с ответом</param>
    public BaseRpcResponse(
        bool success,
        int responseCode,
        string responseMessage)
    {
        Success = success;
        ResponseCode = responseCode;
        ResponseMessage = responseMessage;
    }

    /// <summary>
    /// Признак успеха
    /// </summary>
    public bool Success { get; }
    
    /// <summary>
    /// Код ответа
    /// </summary>
    public int ResponseCode { get; }
    
    /// <summary>
    /// Сообщение с ответом
    /// </summary>
    public string ResponseMessage { get; }
}