using System.Reflection;
using Essentials.Utils.Extensions;
using Essentials.Utils.Reflection.Extensions;
using Essentials.RabbitMqClient.Subscriber.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using static Essentials.RabbitMqClient.Dictionaries.QueueLoggers;

namespace Essentials.RabbitMqClient.Subscriber.Helpers;

/// <summary>
/// Хелперы для работы с рефлексией
/// </summary>
internal static class ReflectionHelpers
{
    /// <summary>
    /// Регистрирует обработчик события
    /// </summary>
    /// <param name="services"></param>
    /// <param name="options"></param>
    /// <param name="assemblies"></param>
    /// <returns></returns>
    public static void RegisterEventHandler(
        IServiceCollection services,
        RegisterEventsHandlersOptions options,
        Assembly[] assemblies)
    {
        try
        {
            var eventType = assemblies.GetTypeByName(options.EventTypeName, StringComparison.InvariantCultureIgnoreCase);
            var handlerDescriptionType = typeof(IEventHandler<>).MakeGenericType(eventType);
            
            RegisterHandler(services, handlerDescriptionType, options.HandlerType);
        }
        catch (Exception ex)
        {
            MainLogger.Error(ex,
                $"Во время регистрации обработчика для события с типом '{options.EventTypeName}' произошло исключение.");

            throw;
        }
    }

    private static void RegisterHandler(
        IServiceCollection services,
        Type handlerDescriptionType,
        Type handlerImplementationType)
    {
        var method = GetMethodWithNameAndParams(
            typeof(ServiceCollectionDescriptorExtensions),
            nameof(ServiceCollectionDescriptorExtensions.TryAddScoped),
            paramTypes: [typeof(IServiceCollection), typeof(Type), typeof(Type)]);

        method
            .CheckNotNull()
            .Invoke(
                obj: typeof(ServiceCollectionDescriptorExtensions),
                parameters: [services, handlerDescriptionType, handlerImplementationType]);
    }

    private static MethodInfo? GetMethodWithNameAndParams(
        Type staticType,
        string methodName,
        params Type[] paramTypes)
    {
        return staticType
            .GetMethods()
            .Where(methodInfo => methodInfo.Name == methodName)
            .Where(Predicate)
            .SingleOrDefault();

        bool Predicate(MethodInfo method) =>
            method
                .GetParameters()
                .Select<ParameterInfo, Type>(parameter => parameter.ParameterType)
                .Select(type => !type.IsGenericType ? type : type.GetGenericTypeDefinition())
                .SequenceEqual(paramTypes);
    }
}