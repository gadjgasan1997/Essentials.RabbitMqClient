using Essentials.Utils.Extensions;
using Essentials.RabbitMqClient.Options;

namespace Essentials.RabbitMqClient.Configuration.Builders;

/// <summary>
/// Билдер обменника
/// </summary>
public class ExchangeBuilder
{
    private readonly string _exchangeName;
    private readonly string _exchangeType;
    private bool _isDurable;
    private bool _isAutoDelete;

    internal ExchangeBuilder(string exchangeName, string exchangeType)
    {
        _exchangeName = exchangeName.CheckNotNullOrEmpty("Название обменника для конфигурации не может быть пустым");
        _exchangeType = exchangeType.CheckNotNullOrEmpty("Тип обменника для конфигурации не может быть пустым");
    }
    
    /// <summary>
    /// Указывает, что обменник должен сохранять свое состояние
    /// </summary>
    /// <returns>Билдер обменника</returns>
    public ExchangeBuilder Durable()
    {
        _isDurable = true;
        return this;
    }
    
    /// <summary>
    /// Указывает, что обменник будет удален автоматически после остановки клиента
    /// </summary>
    /// <returns>Билдер обменника</returns>
    public ExchangeBuilder AutoDelete()
    {
        _isAutoDelete = true;
        return this;
    }
    
    /// <summary>
    /// Билдит опции обменника
    /// </summary>
    /// <returns>Опции обменника</returns>
    internal ExchangeOptions Build()
    {
        return new ExchangeOptions
        {
            Name = _exchangeName,
            Type = _exchangeType,
            Durable = _isDurable,
            AutoDelete = _isAutoDelete
        };
    }
}