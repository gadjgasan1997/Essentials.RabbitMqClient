namespace Essentials.RabbitMqClient.Options;
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable CollectionNeverUpdated.Global

/// <summary>
/// Модель
/// </summary>
internal class ModelOptions
{
    /// <summary>
    /// Опции очередей
    /// </summary>
    public List<QueueOptions> Queues { get; set; } = [];

    /// <summary>
    /// Опции обменников
    /// </summary>
    public List<ExchangeOptions> Exchanges { get; set; } = [];

    /// <summary>
    /// Опции подписок на очереди
    /// </summary>
    public List<SubscriptionOptions> SubscriptionsOptions { get; set; } = [];

    /// <summary>
    /// Опции публикации сообщений
    /// </summary>
    public List<PublishOptions> PublishOptions { get; set; } = [];

    /// <summary>
    /// Опции Rpc запросов
    /// </summary>
    public List<RpcRequestOptions> RpcRequestsOptions { get; set; } = [];
}