using Essentials.RabbitMqClient.Models;
using Essentials.RabbitMqClient.Publisher.Models;
using Essentials.RabbitMqClient.Subscriber.Models;
// ReSharper disable ConvertIfStatementToReturnStatement

namespace Essentials.RabbitMqClient.OptionsProvider.Implementations;

/// <inheritdoc cref="IOptionsProvider" />
internal class OptionsProvider : IOptionsProvider
{
    /// <summary>
    /// Мапа ключей соединений на их опции
    /// </summary>
    private static readonly Dictionary<ConnectionKey, ConnectionOptions> _connectionsOptions = new();
    
    /// <summary>
    /// Мапа ключей публикуемых событий на список соединений, в которых они присутствуют
    /// </summary>
    private static readonly Dictionary<EventKey, HashSet<ConnectionKey>> _publishEventsToConnectionsMap = new();
    
    /// <inheritdoc cref="IOptionsProvider.AddConnectionOptions" />
    public void AddConnectionOptions(
        ConnectionKey connectionKey,
        IEnumerable<Queue> queuesForDeclare,
        IEnumerable<Exchange> exchangesForDeclare,
        IReadOnlyDictionary<SubscriptionKey, SubscriptionOptions> subscriptionsOptionsMap,
        Dictionary<PublishKey, PublishOptions> publishOptionsMap,
        Dictionary<PublishKey, RpcRequestOptions> rpcRequestsOptionsMap)
    {
        if (_connectionsOptions.ContainsKey(connectionKey))
            throw new InvalidOperationException($"Опции для соединения с ключом '{connectionKey}' уже существуют");

        foreach (var (key, _) in rpcRequestsOptionsMap)
        {
            if (_publishEventsToConnectionsMap.TryGetValue(key.EventKey, out var options))
                options.Add(connectionKey);
            else _publishEventsToConnectionsMap.Add(key.EventKey, new HashSet<ConnectionKey> { connectionKey });
        }
        
        foreach (var (key, _) in rpcRequestsOptionsMap)
        {
            if (_publishEventsToConnectionsMap.TryGetValue(key.EventKey, out var options))
                options.Add(connectionKey);
            else
                _publishEventsToConnectionsMap.Add(key.EventKey, new HashSet<ConnectionKey> { connectionKey });
        }

        var connectionOptions = new ConnectionOptions(
            queuesForDeclare: new List<Queue>(queuesForDeclare),
            exchangesForDeclare: new List<Exchange>(exchangesForDeclare),
            subscriptionsOptionsMap: new Dictionary<SubscriptionKey, SubscriptionOptions>(subscriptionsOptionsMap),
            publishOptionsMap: new Dictionary<PublishKey, PublishOptions>(publishOptionsMap),
            rpcRequestsOptionsMap: new Dictionary<PublishKey, RpcRequestOptions>(
                rpcRequestsOptionsMap));
        
        _connectionsOptions.Add(connectionKey, connectionOptions);
    }

    /// <inheritdoc cref="IOptionsProvider.GetQueuesForDeclare" />
    public IEnumerable<Queue> GetQueuesForDeclare(ConnectionKey connectionKey)
    {
        var connectionOptions = GetConnectionOptions(connectionKey);
        return connectionOptions.QueuesForDeclare;
    }

    /// <inheritdoc cref="IOptionsProvider.GetExchangesForDeclare" />
    public IEnumerable<Exchange> GetExchangesForDeclare(ConnectionKey connectionKey)
    {
        var connectionOptions = GetConnectionOptions(connectionKey);
        return connectionOptions.ExchangesForDeclare;
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

    /// <inheritdoc cref="IOptionsProvider.GetConnectionKeyForPublishEvent(EventKey)" />
    public ConnectionKey GetConnectionKeyForPublishEvent(EventKey eventKey)
    {
        if (!_publishEventsToConnectionsMap.TryGetValue(eventKey, out var connections) || connections.Count is 0)
        {
            throw new InvalidOperationException(
                $"Не удалось найти соединение для публикации события с ключом '{eventKey}'");
        }

        // Если найдено более одного соединения, выкидываем исключение
        if (connections.Count > 1)
        {
            throw new InvalidOperationException(
                $"Не удалось разрешить соединение для публикации события с ключом '{eventKey}'. " +
                $"По ключу найдено более одного соединения. Название соединения требуется явно указать в параметрах публикации.");
        }

        return connections.Single();
    }

    /// <inheritdoc cref="IOptionsProvider.GetConnectionKeyForPublishEvent(EventKey, PublishParams)" />
    public ConnectionKey GetConnectionKeyForPublishEvent(EventKey eventKey, PublishParams @params)
    {
        // Если в параметрах передано название соединения, то используем его
        if (!string.IsNullOrWhiteSpace(@params.ConnectionName))
            return ConnectionKey.New(@params.ConnectionName);

        return GetConnectionKeyForPublishEvent(eventKey);
    }

    /// <inheritdoc cref="IOptionsProvider.GetExchangeForPublishEvent(ConnectionKey, EventKey)" />
    public string GetExchangeForPublishEvent(ConnectionKey connectionKey, EventKey eventKey)
    {
        var connectionOptions = GetConnectionOptions(connectionKey);
        if (!connectionOptions.EventsToExchangesMap.TryGetValue(eventKey, out var exchanges) || exchanges.Count is 0)
        {
            throw new InvalidOperationException(
                $"Не удалось найти обменник для публикации события '{eventKey}' в соединении '{connectionKey}'.");
        }
        
        // Если найдено более одного обменника, выкидываем исключение
        if (exchanges.Count > 1)
        {
            throw new InvalidOperationException(
                $"Не удалось разрешить обменник для публикации события '{eventKey}' в соединении '{connectionKey}'. " +
                $"По данному событию найдено более одного обменника: '{string.Join(", ", exchanges)}'. " +
                "Название обменника требуется явно указать в параметрах публикации.");
        }

        return exchanges.Single();
    }

    /// <inheritdoc cref="IOptionsProvider.GetRoutingKeyForPublishEvent(ConnectionKey, EventKey)" />
    public string GetRoutingKeyForPublishEvent(ConnectionKey connectionKey, EventKey eventKey)
    {
        var connectionOptions = GetConnectionOptions(connectionKey);
        if (!connectionOptions.EventsToRoutingKeysMap.TryGetValue(eventKey, out var keys) || keys.Count is 0)
        {
            throw new InvalidOperationException(
                $"Не удалось найти ключ маршрутизации для публикации события '{eventKey}' в соединении '{connectionKey}'.");
        }
            
        // Если найдено более одного ключа маршрутизации, выкидываем исключение
        if (keys.Count > 1)
        {
            throw new InvalidOperationException(
                $"Не удалось разрешить ключ маршрутизации для публикации события '{eventKey}' в соединении '{connectionKey}'. " +
                $"По данному событию найдено более одного ключа маршрутизации: '{string.Join(", ", keys)}'. " +
                "Название ключа маршрутизации требуется явно указать в параметрах публикации.");
        }

        return keys.Single().Key;
    }

    /// <inheritdoc cref="IOptionsProvider.GetPublishOptions" />
    public PublishOptions GetPublishOptions(ConnectionKey connectionKey, PublishKey publishKey)
    {
        var connectionOptions = GetConnectionOptions(connectionKey);
        if (!connectionOptions.PublishOptionsMap.TryGetValue(publishKey, out var @event))
        {
            throw new KeyNotFoundException(
                $"Не найдены опции публикации сообщения с ключом '{publishKey}' " +
                $"в соединении '{connectionKey}'");
        }

        return @event;
    }

    /// <inheritdoc cref="IOptionsProvider.GetRpcRequestOptions" />
    public RpcRequestOptions GetRpcRequestOptions(
        ConnectionKey connectionKey,
        PublishKey publishKey)
    {
        var connectionOptions = GetConnectionOptions(connectionKey);
        if (!connectionOptions.RpcRequestsOptionsMap.TryGetValue(publishKey, out var @event))
        {
            throw new KeyNotFoundException(
                $"Не найдены опции Rpc запроса с ключом '{publishKey}' " +
                $"в соединении '{connectionKey}'");
        }

        return @event;
    }

    /// <inheritdoc cref="IOptionsProvider.GetSubscriptionsInfo" />
    public IReadOnlyDictionary<ConnectionKey, IEnumerable<SubscriptionInfo>> GetSubscriptionsInfo() =>
        _connectionsOptions.ToDictionary(
            pair => pair.Key,
            pair => pair.Value.SubscriptionsOptionsMap.Select(options =>
                new SubscriptionInfo(
                    options.Key.QueueKey,
                    options.Key.RoutingKey,
                    options.Value.EventTypeName,
                    options.Value.HandlerTypeName)));

    #region Private

    /// <summary>
    /// Возвращает опции соединения по ключу
    /// </summary>
    /// <param name="connectionKey">Ключ соединения</param>
    /// <returns>Опции соединения</returns>
    /// <exception cref="KeyNotFoundException"></exception>
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
            IEnumerable<Queue> queuesForDeclare,
            IEnumerable<Exchange> exchangesForDeclare,
            IReadOnlyDictionary<SubscriptionKey, SubscriptionOptions> subscriptionsOptionsMap,
            IReadOnlyDictionary<PublishKey, PublishOptions> publishOptionsMap,
            IReadOnlyDictionary<PublishKey, RpcRequestOptions> rpcRequestsOptionsMap)
        {
            QueuesForDeclare = queuesForDeclare;
            ExchangesForDeclare = exchangesForDeclare;
            
            SubscriptionsOptionsMap = subscriptionsOptionsMap;
            PublishOptionsMap = publishOptionsMap;

            RpcRequestsOptionsMap = rpcRequestsOptionsMap;

            foreach (var (key, _) in publishOptionsMap)
            {
                AddOrUpdateExchange(key.EventKey, key.Exchange);
                AddOrUpdateRoutingKey(key.EventKey, key.RoutingKey);
            }
            
            foreach (var (key, _) in rpcRequestsOptionsMap)
            {
                AddOrUpdateExchange(key.EventKey, key.Exchange);
                AddOrUpdateRoutingKey(key.EventKey, key.RoutingKey);
            }
        }

        /// <summary>
        /// Мапа ключей событий на обменники для них
        /// </summary>
        public Dictionary<EventKey, HashSet<string>> EventsToExchangesMap { get; } = new();

        /// <summary>
        /// Мапа ключей событий на ключи маршрутизации для них
        /// </summary>
        public Dictionary<EventKey, HashSet<RoutingKey>> EventsToRoutingKeysMap { get; } = new();
        
        /// <summary>
        /// Список очередей для объявления
        /// </summary>
        public IEnumerable<Queue> QueuesForDeclare { get; }
        
        /// <summary>
        /// Список обменников для объявления
        /// </summary>
        public IEnumerable<Exchange> ExchangesForDeclare { get; }

        /// <summary>
        /// Мапа с опциями подписки на события
        /// </summary>
        public IReadOnlyDictionary<SubscriptionKey, SubscriptionOptions> SubscriptionsOptionsMap { get; }
        
        /// <summary>
        /// Мапа с опциями публикации событий
        /// </summary>
        public IReadOnlyDictionary<PublishKey, PublishOptions> PublishOptionsMap { get; }
        
        /// <summary>
        /// Мапа с опциями Rpc запросов
        /// </summary>
        public IReadOnlyDictionary<PublishKey, RpcRequestOptions> RpcRequestsOptionsMap { get; }
        
        private void AddOrUpdateExchange(EventKey eventKey, string exchange)
        {
            if (EventsToExchangesMap.TryGetValue(eventKey, out var exchangesSet))
                exchangesSet.Add(exchange);
            else
                EventsToExchangesMap.Add(eventKey, new HashSet<string> { exchange });
        }
        
        private void AddOrUpdateRoutingKey(EventKey eventKey, RoutingKey? routingKey)
        {
            if (routingKey is null)
                return;
            
            if (EventsToRoutingKeysMap.TryGetValue(eventKey, out var keysSet))
                keysSet.Add(routingKey);
            else
                EventsToRoutingKeysMap.Add(eventKey, new HashSet<RoutingKey> { routingKey });
        }
    }

    #endregion
}