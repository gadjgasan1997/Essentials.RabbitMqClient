﻿using System.Reflection;
using Essentials.Utils.Extensions;
using Essentials.Utils.Reflection.Extensions;
using System.Diagnostics.CodeAnalysis;
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
    /// Регистрирует обработчик
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assemblies">Список сборок</param>
    /// <param name="eventTypeName">Название типа события</param>
    /// <param name="handlerTypeName">Название типа обработчика события</param>
    /// <exception cref="ArgumentNullException"></exception>
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
    public static void RegisterHandler(
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
    public static void RegisterDefaultHandler(
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

    private static void RegisterHandler(
        IServiceCollection services,
        Type handlerDescriptionType,
        Type handlerImplementationType)
    {
        /*// Получение и вызов метода
        var method = typeof(ServiceCollectionDescriptorExtensions)
            .GetMethodWithNameAndParams(
                nameof(ServiceCollectionDescriptorExtensions.TryAddScoped),
                paramTypes: new[] { typeof(IServiceCollection), typeof(Type), typeof(Type) });

        method
            .CheckNotNull()
            .Invoke(
                obj: typeof(ServiceCollectionDescriptorExtensions),
                parameters: new object?[] { services, handlerDescriptionType, handlerImplementationType });*/
    }
}