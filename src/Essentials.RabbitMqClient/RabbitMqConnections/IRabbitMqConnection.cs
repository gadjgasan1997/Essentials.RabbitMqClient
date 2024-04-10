using RabbitMQ.Client;

namespace Essentials.RabbitMqClient.RabbitMqConnections;

/// <summary>
/// Соединение с RabbitMq
/// </summary>
internal interface IRabbitMqConnection : IDisposable
{
    /// <summary>
    /// Создает канал
    /// </summary>
    /// <returns></returns>
    IModel CreateModel();
}