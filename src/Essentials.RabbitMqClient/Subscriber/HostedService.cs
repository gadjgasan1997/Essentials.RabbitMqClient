using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Essentials.Utils.Extensions;
using Essentials.Utils.Reflection.Helpers;
using Essentials.RabbitMqClient.Subscriber.Models;
using Essentials.RabbitMqClient.Subscriber.Extensions;

namespace Essentials.RabbitMqClient.Subscriber;

/// <summary>
/// Сервис для автоматической подписки на события
/// </summary>
internal class HostedService : IHostedService
{
    private readonly IOptionsProvider _provider;
    private readonly IEventsSubscriber _eventsSubscriber;
    private readonly ILogger<HostedService> _logger;
    
    public HostedService(
        IOptionsProvider provider,
        IEventsSubscriber eventsSubscriber,
        ILogger<HostedService> logger)
    {
        _provider = provider.CheckNotNull();
        _eventsSubscriber = eventsSubscriber.CheckNotNull();
        _logger = logger;
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            SubscribeToEvents();
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Во время подписки на события произошло исключение");
            
            throw;
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    /// <summary>
    /// Подписывается на события
    /// </summary>
    /// <returns></returns>
    private void SubscribeToEvents()
    {
        var assemblies = AssemblyHelpers.GetCurrentDomainAssemblies().ToArray();

        foreach (var (connectionKey, subscriptionsInfo) in _provider.GetSubscriptionsInfo())
        {
            foreach (var info in subscriptionsInfo)
            {
                var @params = new SubscriptionParams(
                    connectionKey,
                    info.QueueName,
                    info.RoutingKey);

                _eventsSubscriber.SubscribeToEvent(@params, assemblies, info.EventTypeName);
            }
        }
    }
}