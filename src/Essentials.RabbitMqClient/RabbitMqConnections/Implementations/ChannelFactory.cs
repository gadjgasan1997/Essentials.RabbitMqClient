using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Essentials.RabbitMqClient.Models;
using Essentials.RabbitMqClient.Subscriber.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Essentials.Utils.Extensions;

namespace Essentials.RabbitMqClient.RabbitMqConnections.Implementations;

/// <inheritdoc cref="IChannelFactory" />
internal class ChannelFactory : IChannelFactory, IDisposable
{
    private readonly IRabbitMqConnectionFactory _connectionFactory;
    
    private static readonly ConcurrentDictionary<ConnectionKey, IModel> _channelsForPublish = new();
    private static readonly ConcurrentDictionary<(ConnectionKey, SubscriptionKey), IModel> _channelsForSubscribe = new();

    private static readonly ConcurrentDictionary<HashSet<string>, (ConnectionKey, SubscriptionKey)> _consumersToKeysMap =
        new();

    public ChannelFactory(IRabbitMqConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory.CheckNotNull();
    }
    
    /// <inheritdoc cref="IChannelFactory.GetOrCreateChannelForPublish" />
    public IModel GetOrCreateChannelForPublish(ConnectionKey connectionKey)
    {
        if (_channelsForPublish.TryGetValue(connectionKey, out var channel))
            return channel;

        var connection = _connectionFactory.GetConnection(connectionKey);

        channel = connection.CreateModel();
        _channelsForPublish[connectionKey] = channel;

        return channel;
    }

    /// <inheritdoc cref="IChannelFactory.GetOrCreateChannelForSubscribe" />
    public IModel GetOrCreateChannelForSubscribe(
        ConnectionKey connectionKey,
        SubscriptionKey subscriptionKey,
        SubscriptionOptions options)
    {
        if (_channelsForSubscribe.TryGetValue((connectionKey, subscriptionKey), out var channel))
            return channel;
        
        var connection = _connectionFactory.GetConnection(connectionKey);
        channel = connection.CreateModel();
        
        _channelsForSubscribe[(connectionKey, subscriptionKey)] = channel;
        _channelsForSubscribe[(connectionKey, subscriptionKey)].BasicQos(0, options.PrefetchCount, false);

        return channel;
    }

    /// <inheritdoc cref="IChannelFactory.CreateConsumer" />
    public AsyncEventingBasicConsumer CreateConsumer(ConnectionKey connectionKey, SubscriptionKey subscriptionKey)
    {
        if (!_channelsForSubscribe.TryGetValue((connectionKey, subscriptionKey), out var channel))
        {
            throw new KeyNotFoundException(
                $"Не найден канал с ключом соединения '{connectionKey}' и ключом подписки '{subscriptionKey}'");
        }

        var queueKey = subscriptionKey.QueueKey;
        
        var consumer = new AsyncEventingBasicConsumer(channel);
        
        channel.BasicConsume(
            queue: queueKey.QueueName,
            autoAck: false,
            consumer: consumer,
            consumerTag: queueKey.QueueName);

        _consumersToKeysMap.TryAdd(
            new HashSet<string>
            {
                subscriptionKey.RoutingKey.Key,
                queueKey.QueueName
            },
            (connectionKey, subscriptionKey));
        
        return consumer;
    }

    /// <inheritdoc cref="IChannelFactory.TryGetSubscriptionKeys" />
    public bool TryGetSubscriptionKeys(
        string[] consumerTags,
        string routingKey,
        [NotNullWhen(true)] out ConnectionKey? connectionKey,
        [NotNullWhen(true)] out SubscriptionKey? subscriptionKey)
    {
        var keys = consumerTags.Concat([routingKey]);
        
        var pair = _consumersToKeysMap.FirstOrDefault(pair => pair.Key.SetEquals(keys));
        connectionKey = pair.Value.Item1;
        subscriptionKey = pair.Value.Item2;
        
        return connectionKey is not null && subscriptionKey is not null;
    }

    public void Dispose()
    {
        foreach (var model in _channelsForPublish.Select(pair => pair.Value))
        {
            model.Close();
            model.Dispose();
        }
        
        foreach (var model in _channelsForSubscribe.Select(pair => pair.Value))
        {
            model.Close();
            model.Dispose();
        }
    }
}