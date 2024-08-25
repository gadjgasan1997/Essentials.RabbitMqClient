using System.Reflection;
using System.Diagnostics.CodeAnalysis;
using Essentials.Utils.Extensions;
using Essentials.Utils.Reflection.Extensions;
using Essentials.RabbitMqClient.OptionsProvider;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using static Essentials.RabbitMqClient.Dictionaries.QueueLoggers;

namespace Essentials.RabbitMqClient.Helpers;

/// <summary>
/// Хелперы для работы с рефлексией
/// </summary>
internal static class ReflectionHelpers
{
    /// <summary>
    /// Регистрирует обработчик события
    /// </summary>
    /// <param name="services"></param>
    /// <param name="subscriptionInfo"></param>
    /// <param name="assemblies"></param>
    /// <returns></returns>
    public static void RegisterEventHandler(
        IServiceCollection services,
        SubscriptionInfo subscriptionInfo,
        Assembly[] assemblies)
    {
        if (string.IsNullOrWhiteSpace(subscriptionInfo.HandlerTypeName))
        {
            MainLogger.Warn(
                $"Для события {subscriptionInfo.EventTypeName} не указан собственный обработчик - будет использован базовый");
            
            return;
        }

        if (subscriptionInfo.HandlerTypeName == typeof(RabbitMqEventHandler<>).FullName)
        {
            RegisterDefaultHandler(
                services,
                assemblies,
                subscriptionInfo.EventTypeName);
            
            return;
        }

        if (subscriptionInfo.HandlerTypeName == typeof(RabbitMqRpcRequestHandler<,>).FullName)
        {
            if (string.IsNullOrWhiteSpace(subscriptionInfo.ResponseTypeName))
            {
                MainLogger.Error(
                    $"Для события с типом '{subscriptionInfo.EventTypeName}' не указан тип ответа " +
                    $"в поле '{nameof(subscriptionInfo.ResponseTypeName)}', который является обязательным параметром при использовании " +
                    $"стандартного обработчика '{typeof(RabbitMqRpcRequestHandler<,>)}'");
            
                return;
            }
            
            RegisterRpcRequestHandler(
                services,
                assemblies,
                subscriptionInfo.EventTypeName,
                subscriptionInfo.ResponseTypeName);
            
            return;
        }
        
        RegisterHandler(
            services,
            assemblies,
            subscriptionInfo.EventTypeName,
            subscriptionInfo.HandlerTypeName);
    }
    
    /// <summary>
    /// Регистрирует обработчик
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assemblies">Список сборок</param>
    /// <param name="eventTypeName">Название типа события</param>
    /// <param name="handlerTypeName">Название типа обработчика события</param>
    /// <exception cref="ArgumentNullException"></exception>
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
    private static void RegisterHandler(
        IServiceCollection services,
        Assembly[] assemblies,
        string eventTypeName,
        string handlerTypeName)
    {
        try
        {
            var eventType = assemblies.GetTypeByName(eventTypeName, StringComparison.InvariantCultureIgnoreCase);
            var handlerDescriptionType = typeof(IEventHandler<>).MakeGenericType(eventType);
            var handlerImplementationType = assemblies.GetTypeByName(handlerTypeName, StringComparison.InvariantCultureIgnoreCase);
            
            RegisterHandler(services, handlerDescriptionType, handlerImplementationType);
        }
        catch (Exception ex)
        {
            MainLogger.Error(ex,
                $"Во время регистрации обработчика для события с типом '{eventTypeName}' произошло исключение.");

            throw;
        }
    }
    
    /// <summary>
    /// Регистрирует обработчик по-умолчанию
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assemblies">Список сборок</param>
    /// <param name="eventTypeName">Название типа события</param>
    /// <exception cref="ArgumentNullException"></exception>
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
    private static void RegisterDefaultHandler(
        IServiceCollection services,
        Assembly[] assemblies,
        string eventTypeName)
    {
        try
        {
            var eventType = assemblies.GetTypeByName(eventTypeName, StringComparison.InvariantCultureIgnoreCase);
            var handlerDescriptionType = typeof(IEventHandler<>).MakeGenericType(eventType);
            var handlerImplementationType = typeof(RabbitMqEventHandler<>).MakeGenericType(eventType);
            
            RegisterHandler(services, handlerDescriptionType, handlerImplementationType);
        }
        catch (Exception ex)
        {
            MainLogger.Error(ex,
                $"Во время регистрации обработчика для события с типом '{eventTypeName}' произошло исключение.");

            throw;
        }
    }

    /// <summary>
    /// Регистрирует обработчик по-умолчанию
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assemblies">Список сборок</param>
    /// <param name="eventTypeName">Название типа события</param>
    /// <param name="responseTypeName">Название типа ответа</param>
    /// <exception cref="ArgumentNullException"></exception>
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
    private static void RegisterRpcRequestHandler(
        IServiceCollection services,
        Assembly[] assemblies,
        string eventTypeName,
        string responseTypeName)
    {
        try
        {
            var eventType = assemblies.GetTypeByName(eventTypeName, StringComparison.InvariantCultureIgnoreCase);
            var responseType = assemblies.GetTypeByName(responseTypeName, StringComparison.InvariantCultureIgnoreCase);
            
            var handlerDescriptionType = typeof(IEventHandler<>).MakeGenericType(eventType);
            var handlerImplementationType = typeof(RabbitMqRpcRequestHandler<,>).MakeGenericType(eventType, responseType);
            
            RegisterHandler(services, handlerDescriptionType, handlerImplementationType);
        }
        catch (Exception ex)
        {
            MainLogger.Error(ex,
                $"Во время регистрации обработчика для события с типом '{eventTypeName}' " +
                $"и ответа с типом '{responseTypeName}' произошло исключение.");

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