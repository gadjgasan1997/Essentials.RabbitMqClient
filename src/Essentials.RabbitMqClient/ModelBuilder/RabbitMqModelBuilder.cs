using RabbitMQ.Client;
using Essentials.Utils.Extensions;
using Essentials.Utils.Reflection.Helpers;
using Essentials.RabbitMqClient.Models;
using Essentials.RabbitMqClient.OptionsProvider;
using Essentials.RabbitMqClient.RabbitMqConnections;
using Essentials.RabbitMqClient.Subscriber;
using Essentials.RabbitMqClient.Subscriber.Extensions;
using Essentials.RabbitMqClient.Subscriber.Models;
using static Essentials.RabbitMqClient.Dictionaries.QueueLoggers;

namespace Essentials.RabbitMqClient.ModelBuilder;

/// <summary>
/// Билдер для создания моделей RabbitMq
/// </summary>
internal class RabbitMqModelBuilder
{
    private static uint _isConfigured;
    
    private readonly IOptionsProvider _provider;
    private readonly IRabbitMqConnectionFactory _factory;
    private readonly IEventsSubscriber _eventsSubscriber;

    public RabbitMqModelBuilder(
        IOptionsProvider provider,
        IRabbitMqConnectionFactory factory,
        IEventsSubscriber eventsSubscriber)
    {
        _provider = provider.CheckNotNull();
        _factory = factory.CheckNotNull();
        _eventsSubscriber = eventsSubscriber.CheckNotNull();
    }
    
    /// <summary>
    /// Регистрирует необходимые модели RabbitMq
    /// </summary>
    public void RegisterRabbitMqModels()
    {
        if (Interlocked.Exchange(ref _isConfigured, 1) == 1)
            return;
        
        foreach (var pair in _factory.GetAllConnections())
            RegisterConnectionRabbitMqModels(pair.Key, pair.Value);
        
        SubscribeToEvents();
    }

    /// <summary>
    /// Регистрирует необходимые модели соединения RabbitMq
    /// </summary>
    /// <param name="connectionKey">Ключ соединения</param>
    /// <param name="connection">Соединение</param>
    /// <returns></returns>
    private void RegisterConnectionRabbitMqModels(ConnectionKey connectionKey, IRabbitMqConnection connection)
    {
        try
        {
            RegisterQueues(connectionKey, connection);
            RegisterExchanges(connectionKey, connection);
        }
        catch (Exception exception)
        {
            MainLogger.Error(
                exception,
                $"Во время регистрации моделей RabbitMq для соединения с ключом '{connectionKey}' произошло исключение.");
        }
    }

    /// <summary>
    /// Регистрирует очереди
    /// </summary>
    /// <param name="connectionKey">Ключ соединения</param>
    /// <param name="connection">Соединение</param>
    /// <returns></returns>
    private void RegisterQueues(ConnectionKey connectionKey, IRabbitMqConnection connection)
    {
        foreach (var queue in _provider.GetQueuesForDeclare(connectionKey))
        {
            MainLogger.Info($"Происходит объявление очереди: '{queue}'");
            
            using var channel = connection.CreateModel();
            channel.QueueDeclare(
                queue.QueueKey.QueueName,
                queue.Durable,
                queue.Exclusive,
                queue.AutoDelete);
            
            AddQueueBindings(channel, queue);
        }
    }

    /// <summary>
    /// Добавляет привязки для очереди
    /// </summary>
    /// <param name="channel">Канал</param>
    /// <param name="queue">Опции очереди</param>
    private static void AddQueueBindings(IModel channel, Queue queue)
    {
        foreach (var binding in queue.Bindings)
        {
            MainLogger.Info($"Происходит привязка обменника к очереди с конфигурацией: '{binding}'");

            channel.QueueBind(queue.QueueKey.QueueName, binding.Exchange, binding.RoutingKey.Key);
        }
    }

    /// <summary>
    /// Регистрирует обменники
    /// </summary>
    /// <param name="connectionKey">Ключ соединения</param>
    /// <param name="connection">Соединение</param>
    /// <returns></returns>
    private void RegisterExchanges(ConnectionKey connectionKey, IRabbitMqConnection connection)
    {
        foreach (var exchange in _provider.GetExchangesForDeclare(connectionKey))
        {
            MainLogger.Info($"Происходит объявление обменника: '{exchange}'");
            
            using var channel = connection.CreateModel();
            channel.ExchangeDeclare(exchange.Name, exchange.Type, exchange.Durable, exchange.AutoDelete);
        }
    }

    /// <summary>
    /// Подписывается на события
    /// </summary>
    /// <returns></returns>
    private void SubscribeToEvents()
    {
        var assemblies = AssemblyHelpers.GetCurrentDomainAssemblies().ToArray();

        foreach (var (connectionKey, subscriptionsInfo) in _provider.GetSubscriptionsInfo())
        {
            foreach (var subscriptionInfo in subscriptionsInfo)
            {
                var @params = new SubscriptionParams(
                    connectionKey,
                    subscriptionInfo.QueueName,
                    subscriptionInfo.RoutingKey);

                _eventsSubscriber.SubscribeToEvent(@params, assemblies, subscriptionInfo.EventTypeName);
            }
        }
    }
}