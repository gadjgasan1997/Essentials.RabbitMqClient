using System.Diagnostics.CodeAnalysis;

namespace Essentials.RabbitMqClient.Publisher.Models;

/// <summary>
/// Параметры публикации сообщения
/// </summary>
/// <param name="Exchange">Обменник</param>
/// <param name="RoutingKey">Ключ маршрутизации</param>
/// <param name="ConnectionName">Название соединения, в которое публикуется сообщение</param>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public record PublishParams(
    string? Exchange = null,
    string? RoutingKey = null,
    string? ConnectionName = null);