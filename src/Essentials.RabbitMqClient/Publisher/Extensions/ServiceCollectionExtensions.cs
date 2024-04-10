using Essentials.RabbitMqClient.Publisher.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace Essentials.RabbitMqClient.Publisher.Extensions;

/// <summary>
/// Методы расширения для <see cref="IServiceCollection" />
/// </summary>
internal static class ServiceCollectionExtensions
{
    /// <summary>
    /// Настраивает сервис для публикации событий
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection ConfigureRabbitMqEventsPublisher(this IServiceCollection services) =>
        services
            .AddSingleton<IAskManager, AskManager>()
            .AddSingleton<IEventsPublisher, EventsPublisher>();
}