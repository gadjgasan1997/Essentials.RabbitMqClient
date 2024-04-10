using Essentials.RabbitMqClient.Models;
using Essentials.RabbitMqClient.Options;

namespace Essentials.RabbitMqClient.RabbitMqConnections;

/// <summary>
/// Интерфейс для создания соединения с RabbitMq
/// </summary>
internal interface IRabbitMqConnectionFactory
{
    /// <summary>
    /// Добавляет соединение
    /// </summary>
    /// <param name="options">Опции соединения</param>
    void AddConnection(ConnectionOptions options);
    
    /// <summary>
    /// Возвращает соединение
    /// </summary>
    /// <param name="connectionKey">Ключ соединения</param>
    /// <returns>Соединение</returns>
    IRabbitMqConnection GetConnection(ConnectionKey connectionKey);

    /// <summary>
    /// Возвращает список существующих соединений
    /// </summary>
    /// <returns>Соединения</returns>
    IReadOnlyDictionary<ConnectionKey, IRabbitMqConnection> GetAllConnections();
}