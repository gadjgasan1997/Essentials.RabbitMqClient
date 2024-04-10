using System.Net.Sockets;
using System.Reflection;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using static Essentials.RabbitMqClient.Dictionaries.QueueLoggers;

namespace Essentials.RabbitMqClient.RabbitMqConnections.Implementations;

internal class RabbitMqConnection : IRabbitMqConnection
{
    private readonly IConnectionFactory _connectionFactory;
    private readonly int _retryCount;
    private readonly object _syncRoot = new();
    private IConnection? _connection;

    public RabbitMqConnection(IConnectionFactory connectionFactory, int retryCount)
    {
        _connectionFactory = connectionFactory;
        _retryCount = retryCount;
    }

    private bool IsConnected => _connection is {IsOpen: true} && !Disposed;

    private bool Disposed { get; set; }

    public void Dispose()
    {
        MainLogger.Debug("Disposing connection {rabbitMqConnection}", _connection);

        if (Disposed)
            return;

        Disposed = true;

        if (_connection is null)
            return;

        try
        {
            _connection.ConnectionShutdown -= OnConnectionShutdown;
            _connection.CallbackException -= OnCallbackException;
            _connection.ConnectionBlocked -= OnConnectionBlocked;
            _connection.Close(0, "while disposing on client");
            _connection.Dispose();
        }
        catch (IOException ex)
        {
            MainLogger.Fatal(ex, "Ошибка при Dispose подключения к RabbitMq");
        }
    }

    public IModel CreateModel() => CreateConnection().CreateModel();

    private IConnection CreateConnection()
    {
        if (_connection is not null && !Disposed)
            return _connection;

        TryConnect();

        return _connection!;
    }

    private bool TryConnect()
    {
        if (IsConnected)
            return true;

        MainLogger.Debug("RabbitMQ Client is trying to connect");

        lock (_syncRoot)
        {
            var policy = Policy.Handle<SocketException>()
                               .Or<BrokerUnreachableException>()
                               .WaitAndRetry(_retryCount,
                                             retryAttempt => TimeSpan.FromSeconds(Math.Pow(1.2, retryAttempt)),
                                             (exception, time) =>
                                             {
                                                 Dispose();

                                                 MainLogger.Warn(
                                                     exception,
                                                     "RabbitMQ Client could not connect after {TimeOut}s ({ExceptionMessage})",
                                                     $"{time.TotalSeconds:n1}",
                                                     exception.Message);
                                             });

            policy.Execute(() =>
            {
                _connection = _connectionFactory
                        .CreateConnection(Assembly.GetEntryAssembly()?.FullName);
                Disposed = false;
            });

            if (IsConnected)
            {
                _connection!.ConnectionShutdown += OnConnectionShutdown;
                _connection!.CallbackException += OnCallbackException;
                _connection!.ConnectionBlocked += OnConnectionBlocked;

                MainLogger.Info(
                    "RabbitMQ Client acquired a persistent connection to '{HostName}' and is subscribed to failure events",
                    _connection.Endpoint.HostName);

                return true;
            }

            MainLogger.Fatal("FATAL ERROR: RabbitMQ connections could not be created and opened");

            return false;
        }
    }

    private static void OnConnectionBlocked(object? sender, ConnectionBlockedEventArgs e) =>
        MainLogger.Warn("A RabbitMQ connection is shutdown. Trying to re-connect...");

    private static void OnCallbackException(object? sender, CallbackExceptionEventArgs e) =>
        MainLogger.Warn("A RabbitMQ connection throws an exception. Trying to re-connect...");

    private static void OnConnectionShutdown(object? sender, ShutdownEventArgs reason) =>
        MainLogger.Warn("A RabbitMQ connection is on shutdown. Trying to re-connect...");
}