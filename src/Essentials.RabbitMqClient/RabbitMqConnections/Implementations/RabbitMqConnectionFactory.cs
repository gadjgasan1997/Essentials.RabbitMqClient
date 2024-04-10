using Essentials.RabbitMqClient.Models;
using Essentials.RabbitMqClient.Options;
using Essentials.RabbitMqClient.RabbitMqConnections.Helpers;
using RabbitMQ.Client;
using static Essentials.RabbitMqClient.Dictionaries.QueueLoggers;

namespace Essentials.RabbitMqClient.RabbitMqConnections.Implementations;

/// <inheritdoc cref="IRabbitMqConnectionFactory" />
internal class RabbitMqConnectionFactory : IRabbitMqConnectionFactory
{
    private readonly Dictionary<ConnectionKey, IRabbitMqConnection> _connections = new();

    /// <inheritdoc cref="IRabbitMqConnectionFactory.AddConnection" />
    public void AddConnection(ConnectionOptions options)
    {
        var connectionKey = ConnectionKey.New(options.Name);
        
        if (_connections.ContainsKey(connectionKey))
            throw new InvalidOperationException(
                $"Происодит попытка добавить второе соединение RabbitMq с названием '{options.Name}'");
        
        var connectionFactory = new ConnectionFactory
        {
            HostName = options.Host,
            UserName = options.UserName,
            Password = options.Password,
            VirtualHost = options.VirtualHost,
            Port = options.Port,
            DispatchConsumersAsync = options.DispatchConsumersAsync ?? true,
            RequestedHeartbeat = options.RequestedHeartbeat ?? ConnectionFactory.DefaultHeartbeat
        };

        CryptoHelpers.ConfigureSsl(connectionFactory, options.Ssl);
        
        var connection = new RabbitMqConnection(connectionFactory, options.ConnectRetryCount ?? 5);

        _connections.Add(connectionKey, connection);
    }

    /// <inheritdoc cref="IRabbitMqConnectionFactory.GetConnection" />
    public IRabbitMqConnection GetConnection(ConnectionKey connectionKey)
    {
        if (!_connections.TryGetValue(connectionKey, out var connection))
            throw new KeyNotFoundException($"Не найдено соединение RabbitMq с ключом '{connectionKey}'");

        return connection;
    }

    /// <inheritdoc cref="IRabbitMqConnectionFactory.GetAllConnections" />
    public IReadOnlyDictionary<ConnectionKey, IRabbitMqConnection> GetAllConnections() => _connections;
    
    public void Dispose()
    {
        MainLogger.Info("Disposing rabbit connection factory");

        foreach (var rabbitMqConnection in _connections.Values)
            rabbitMqConnection.Dispose();

        _connections.Clear();
    }
}