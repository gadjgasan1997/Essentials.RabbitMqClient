namespace Essentials.RabbitMqClient.Context;

/// <summary>
/// Сервис для получения информации о контекста
/// </summary>
internal interface IContextService
{
    /// <summary>
    /// Сохраняет свойства текущего скоупа
    /// </summary>
    /// <param name="key">Ключ для сохранения свойтсв</param>
    void SaveScopeProperties(string key);
    
    /// <summary>
    /// Восстанавливает свойства скоупа
    /// </summary>
    /// <param name="key">Ключ для восстановления свойств</param>
    IDisposable? RestoreScopeProperties(string key);
}