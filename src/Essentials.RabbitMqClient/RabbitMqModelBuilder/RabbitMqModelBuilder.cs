using RabbitMQ.Client;
using Essentials.Utils.Extensions;
using Essentials.RabbitMqClient.Models;
using Essentials.RabbitMqClient.RabbitMqConnections;
using static Essentials.RabbitMqClient.Dictionaries.QueueLoggers;

namespace Essentials.RabbitMqClient.RabbitMqModelBuilder;

/// <summary>
/// Билдер для создания моделей RabbitMq
/// </summary>
internal class RabbitMqModelBuilder
{
    private readonly IOptionsProvider _provider;
    private readonly IRabbitMqConnectionFactory _factory;

    public RabbitMqModelBuilder(
        IOptionsProvider provider,
        IRabbitMqConnectionFactory factory)
    {
        _provider = provider.CheckNotNull();
        _factory = factory.CheckNotNull();
    }
    
    /// <summary>
    /// Регистрирует необходимые модели RabbitMq
    /// </summary>
    public void RegisterRabbitMqModels()
    {
        foreach (var pair in _factory.GetAllConnections())
            RegisterConnectionRabbitMqModels(pair.Key, pair.Value);
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

            throw;
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
}