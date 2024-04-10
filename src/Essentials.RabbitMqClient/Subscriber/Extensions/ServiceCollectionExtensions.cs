using Essentials.RabbitMqClient.Subscriber.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace Essentials.RabbitMqClient.Subscriber.Extensions;

/// <summary>
/// Методы расширения для <see cref="IServiceCollection" />
/// </summary>
internal static class ServiceCollectionExtensions
{
    /// <summary>
    /// Настраивает сервис для подписки на события
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection ConfigureRabbitMqEventsSubscriber(this IServiceCollection services)
    {
        return services
            .AddSingleton<IEventsSubscriber, EventsSubscriber>()
            .AddSingleton<IEventsHandlerService, EventsHandlerService>();
    }
}