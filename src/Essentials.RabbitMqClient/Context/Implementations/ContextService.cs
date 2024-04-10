using NLog;
using System.Collections.Concurrent;

namespace Essentials.RabbitMqClient.Context.Implementations;

/// <inheritdoc cref="IContextService" />
internal class ContextService : IContextService
{
    private static readonly ConcurrentDictionary<string, List<KeyValuePair<string, object?>>> _propertiesMap = new();
    
    /// <inheritdoc cref="IContextService.SaveScopeProperties" />
    public void SaveScopeProperties(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return;

        _propertiesMap.AddOrUpdate(
            key,
            _ => ScopeContext.GetAllProperties().ToList(),
            (_, existingValue) => existingValue);
    }

    /// <inheritdoc cref="IContextService.RestoreScopeProperties" />
    public IDisposable? RestoreScopeProperties(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return null;
        
        return !_propertiesMap.TryGetValue(key, out var scopeProperties)
            ? null
            : ScopeContext.PushProperties(scopeProperties);
    }
}