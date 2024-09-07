using Essentials.RabbitMqClient.Models;

namespace Essentials.RabbitMqClient.RabbitMqModelBuilder;

/// <summary>
/// Провайдер для получения опций
/// </summary>
internal interface IOptionsProvider
{
    /// <summary>
    /// Добавляет опции соединения
    /// </summary>
    /// <param name="connectionKey">Ключ соединения</param>
    /// <param name="queuesForDeclare">Список очередей для объявления</param>
    /// <param name="exchangesForDeclare">Список обменников для объявления</param>
    void AddConnectionOptions(
        ConnectionKey connectionKey,
        IEnumerable<Queue> queuesForDeclare,
        IEnumerable<Exchange> exchangesForDeclare);
    
    /// <summary>
    /// Возвращает список очередей для объявления
    /// </summary>
    /// <param name="connectionKey">Ключ соединения</param>
    /// <returns>Список очередей</returns>
    IEnumerable<Queue> GetQueuesForDeclare(ConnectionKey connectionKey);

    /// <summary>
    /// Возвращает список обменников для объявления
    /// </summary>
    /// <param name="connectionKey">Ключ соединения</param>
    /// <returns>Список обменников</returns>
    IEnumerable<Exchange> GetExchangesForDeclare(ConnectionKey connectionKey);
}