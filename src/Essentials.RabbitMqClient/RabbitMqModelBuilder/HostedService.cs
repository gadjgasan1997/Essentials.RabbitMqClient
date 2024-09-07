using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Essentials.Utils.Extensions;

namespace Essentials.RabbitMqClient.RabbitMqModelBuilder;

/// <summary>
/// Сервис для билда моделей, используемых RabbitMq
/// </summary>
internal class HostedService : IHostedService
{
    private readonly RabbitMqModelBuilder _modelBuilder;
    private readonly ILogger<HostedService> _logger;
    
    public HostedService(
        RabbitMqModelBuilder modelBuilder,
        ILogger<HostedService> logger)
    {
        _modelBuilder = modelBuilder.CheckNotNull();
        _logger = logger;
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _modelBuilder.RegisterRabbitMqModels();
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Во время объявления моделей RabbitMq произошло исключение");
            
            throw;
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}