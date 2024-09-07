using Essentials.RabbitMqClient.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Essentials.RabbitMqClient.RabbitMqModelBuilder.Extensions;

/// <summary>
/// Методы расширения для <see cref="IServiceCollection" />
/// </summary>
internal static class ServiceCollectionExtensions
{
    /// <summary>
    /// Настраивает билдер для создания моделей RabbitMq
    /// </summary>
    /// <param name="services"></param>
    /// <param name="connectionsOptions"></param>
    /// <returns></returns>
    public static IServiceCollection ConfigureRabbitMqModelBuilder(
        this IServiceCollection services,
        IReadOnlyDictionary<string, ModelOptions> connectionsOptions)
    {
        var provider = OptionsProviderBuilder.Build(connectionsOptions);
        
        return services
            .AddSingleton<IOptionsProvider, Implementations.OptionsProvider>(_ => provider)
            .AddSingleton<RabbitMqModelBuilder>()
            .AddSingleton<IHostedService, HostedService>();
    }
}