using Essentials.RabbitMqClient.Models;
using Essentials.RabbitMqClient.Publisher.Models;

namespace Essentials.RabbitMqClient.Publisher;

/// <summary>
/// Провайдер для получения опций
/// </summary>
internal interface IOptionsProvider
{
    /// <summary>
    /// Добавляет опции соединения
    /// </summary>
    /// <param name="connectionKey">Ключ соединения</param>
    /// <param name="publishOptionsMap">Мапа с опциями публикации событий</param>
    /// <param name="rpcRequestsOptionsMap">Мапа с опциями Rpc запросов</param>
    void AddConnectionOptions(
        ConnectionKey connectionKey,
        Dictionary<PublishKey, PublishOptions> publishOptionsMap,
        Dictionary<PublishKey, RpcRequestOptions> rpcRequestsOptionsMap);
    
    /// <summary>
    /// Возвращает ключ соединения для публикации события
    /// </summary>
    /// <param name="eventKey">Ключ события</param>
    /// <returns>Ключ соединения</returns>
    ConnectionKey GetConnectionKeyForPublishEvent(EventKey eventKey);

    /// <summary>
    /// Возвращает ключ соединения для публикации события
    /// </summary>
    /// <param name="eventKey">Ключ события</param>
    /// <param name="params">Параметры публикации</param>
    /// <returns>Ключ соединения</returns>
    ConnectionKey GetConnectionKeyForPublishEvent(EventKey eventKey, PublishParams @params);
    
    /// <summary>
    /// Возвращает обменник для публикации события
    /// </summary>
    /// <param name="connectionKey">Ключ соединения</param>
    /// <param name="eventKey">Ключ события</param>
    /// <returns>Обменник</returns>
    string GetExchangeForPublishEvent(ConnectionKey connectionKey, EventKey eventKey);

    /// <summary>
    /// Возвращает ключ маршрутизации для публикации события
    /// </summary>
    /// <param name="connectionKey">Ключ соединения</param>
    /// <param name="eventKey">Ключ события</param>
    /// <returns>Ключ маршрутизации</returns>
    string GetRoutingKeyForPublishEvent(ConnectionKey connectionKey, EventKey eventKey);

    /// <summary>
    /// Возвращает опции публикации события
    /// </summary>
    /// <param name="connectionKey">Ключ соединения</param>
    /// <param name="publishKey">Ключ публикации</param>
    /// <returns>Опции публикации</returns>
    PublishOptions GetPublishOptions(ConnectionKey connectionKey, PublishKey publishKey);

    /// <summary>
    /// Возвращает опции Rpc запроса
    /// </summary>
    /// <param name="connectionKey">Ключ соединения</param>
    /// <param name="publishKey">Ключ публикации события</param>
    /// <returns>Опции Rpc ответа</returns>
    RpcRequestOptions GetRpcRequestOptions(ConnectionKey connectionKey, PublishKey publishKey);
}