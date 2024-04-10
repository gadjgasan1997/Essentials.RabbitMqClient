using Essentials.Utils.Extensions;

namespace Essentials.RabbitMqClient.Models;

/// <summary>
/// Привязка
/// </summary>
/// <param name="Exchange">Название обменника</param>
/// <param name="RoutingKey">Ключ маршрутизации</param>
internal record Binding(string Exchange, RoutingKey RoutingKey)
{
    /// <summary>
    /// Название обменника
    /// </summary>
    public string Exchange { get; } = Exchange.CheckNotNullOrEmpty();
    
    /// <summary>
    /// Ключ маршрутизации
    /// </summary>
    public RoutingKey RoutingKey { get; } = RoutingKey.CheckNotNull();
}