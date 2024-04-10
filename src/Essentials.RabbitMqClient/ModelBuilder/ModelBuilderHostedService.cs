using Microsoft.Extensions.Hosting;
using Essentials.Utils.Extensions;

namespace Essentials.RabbitMqClient.ModelBuilder;

/// <summary>
/// Сервис для билда моделей, используемых RabbitMq
/// </summary>
internal class ModelBuilderHostedService : IHostedService
{
    private readonly RabbitMqModelBuilder _modelBuilder;
    
    public ModelBuilderHostedService(RabbitMqModelBuilder modelBuilder)
    {
        _modelBuilder = modelBuilder.CheckNotNull();
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _modelBuilder.RegisterRabbitMqModels();

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}