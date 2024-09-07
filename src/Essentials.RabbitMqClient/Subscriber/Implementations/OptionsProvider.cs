using Essentials.RabbitMqClient.Models;
using Essentials.RabbitMqClient.Subscriber.Models;

namespace Essentials.RabbitMqClient.Subscriber.Implementations;

/// <inheritdoc cref="IOptionsProvider" />
internal class OptionsProvider : IOptionsProvider
{
    /// <summary>
    /// Мапа ключей соединений на их опции
    /// </summary>
    private static readonly Dictionary<ConnectionKey, ConnectionOptions> _connectionsOptions = new();

    /// <inheritdoc cref="IOptionsProvider.AddConnectionOptions" />
    public void AddConnectionOptions(
        ConnectionKey connectionKey,
        IReadOnlyDictionary<SubscriptionKey, SubscriptionOptions> subscriptionsOptionsMap)
    {
        var connectionOptions = new ConnectionOptions(subscriptionsOptionsMap);
        _connectionsOptions.Add(connectionKey, connectionOptions);
    }

    /// <inheritdoc cref="IOptionsProvider.GetSubscriptionOptions" />
    public SubscriptionOptions GetSubscriptionOptions(ConnectionKey connectionKey, SubscriptionKey subscriptionKey)
    {
        var connectionOptions = GetConnectionOptions(connectionKey);

        if (!connectionOptions.SubscriptionsOptionsMap.TryGetValue(subscriptionKey, out var options))
        {
            throw new KeyNotFoundException(
                $"Не найдены опции подписки на событие с ключом '{subscriptionKey}' в соединении '{connectionKey}'");
        }

        return options;
    }

    /// <inheritdoc cref="IOptionsProvider.GetOptionsForRegisterEventsHandlers" />
    public IEnumerable<RegisterEventsHandlersOptions> GetOptionsForRegisterEventsHandlers()
    {
        return _connectionsOptions
            .SelectMany(options => options.Value.SubscriptionsOptionsMap)
            .Select(options => options.Value)
            .Select(options =>
                new RegisterEventsHandlersOptions(
                    options.EventTypeName,
                    options.HandlerTypeName));
    }

    /// <inheritdoc cref="IOptionsProvider.GetSubscriptionsInfo" />
    public IReadOnlyDictionary<ConnectionKey, IEnumerable<SubscriptionInfo>> GetSubscriptionsInfo()
    {
        return _connectionsOptions.ToDictionary(
            pair => pair.Key,
            pair => pair.Value.SubscriptionsOptionsMap.Select(options =>
                new SubscriptionInfo(
                    options.Key.QueueKey,
                    options.Key.RoutingKey,
                    options.Value.EventTypeName)));
    }

    private static ConnectionOptions GetConnectionOptions(ConnectionKey connectionKey)
    {
        if (!_connectionsOptions.TryGetValue(connectionKey, out var options))
            throw new KeyNotFoundException($"Не найдены опции соединения с ключом '{connectionKey}'");

        return options;
    }

    /// <summary>
    /// Опции соединения
    /// </summary>
    private class ConnectionOptions
    {
        public ConnectionOptions(
            IReadOnlyDictionary<SubscriptionKey, SubscriptionOptions> subscriptionsOptionsMap)
        {
            SubscriptionsOptionsMap = subscriptionsOptionsMap;
        }
        
        /// <summary>
        /// Мапа с опциями подписки на события
        /// </summary>
        public IReadOnlyDictionary<SubscriptionKey, SubscriptionOptions> SubscriptionsOptionsMap { get; }
    }
}