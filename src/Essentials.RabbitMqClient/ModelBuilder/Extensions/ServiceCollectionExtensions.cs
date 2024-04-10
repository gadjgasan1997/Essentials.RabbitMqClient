using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Essentials.RabbitMqClient.ModelBuilder.Extensions;

/// <summary>
/// Методы расширения для <see cref="IServiceCollection" />
/// </summary>
internal static class ServiceCollectionExtensions
{
    /// <summary>
    /// Настраивает билдер для создания моделей RabbitMq
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection ConfigureRabbitMqModelBuilder(this IServiceCollection services) =>
        services.AddSingleton<RabbitMqModelBuilder>().RegisterHostedService();
    
    /// <summary>
    /// Регистрирует хостед сервис
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    private static IServiceCollection RegisterHostedService(this IServiceCollection services)
    {
        if (services.Any(descriptor => descriptor.ImplementationType == typeof(ModelBuilderHostedService)))
            return services;

        services.AddSingleton<IHostedService, ModelBuilderHostedService>();
        return services;
    }
}