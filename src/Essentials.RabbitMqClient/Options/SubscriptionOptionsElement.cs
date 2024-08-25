using System.Diagnostics.CodeAnalysis;
using System.ComponentModel.DataAnnotations;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace Essentials.RabbitMqClient.Options;

/// <summary>
/// Опции подписки на очередь
/// </summary>
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "CollectionNeverUpdated.Global")]
internal class SubscriptionOptionsElement
{
    /// <summary>
    /// Ключ подписки
    /// </summary>
    public KeyElement Key { get; set; } = new();

    /// <summary>
    /// Свойства подписки
    /// </summary>
    public ValueElement Options { get; set; } = new();

    public class KeyElement
    {
        /// <summary>
        /// Название очереди
        /// </summary>
        [Required]
        public string QueueName { get; set; } = null!;
    
        /// <summary>
        /// Ключ маршрутизации
        /// </summary>
        [Required]
        public string RoutingKey { get; set; } = null!;
    }

    public class ValueElement
    {
        /// <summary>
        /// Полное название типа события
        /// </summary>
        [Required]
        public string TypeName { get; set; } = null!;
        
        /// <summary>
        /// Название типа ответа
        /// </summary>
        public string? ResponseTypeName { get; set; }
        
        /// <summary>
        /// Название типа обработчика
        /// </summary>
        public string? HandlerTypeName { get; set; }
    
        /// <summary>
        /// Признак необходимости вычитывать сообщения
        /// </summary>
        public bool? NeedConsume { get; set; }
    
        /// <summary>
        /// Количество сообщений, которое может забрать подписчик
        /// </summary>
        public ushort? PrefetchCount { get; set; }
    
        /// <summary>
        /// Тип содержимого
        /// </summary>
        public string? ContentType { get; set; }

        /// <summary>
        /// Признак необходимости установить событие в задаче (RPC)
        /// </summary>
        public bool? Correlation { get; set; } = false;
        
        /// <summary>
        /// Список перехватчиков сообщения
        /// </summary>
        public List<BehaviorInfo> Behaviors { get; set; } = new();
    }
}