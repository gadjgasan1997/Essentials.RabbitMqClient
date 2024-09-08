// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global

using System.ComponentModel.DataAnnotations;

namespace Essentials.RabbitMqClient.Options;

/// <summary>
/// Опции Rpc запроса
/// </summary>
internal class RpcRequestOptions
{
    /// <summary>
    /// Ключ публикации
    /// </summary>
    public KeyElement Key { get; set; } = new();

    /// <summary>
    /// Свойства публикации
    /// </summary>
    public ValueElement Options { get; set; } = new();
    
    public class KeyElement
    {
        /// <summary>
        /// Название обменника
        /// </summary>
        [Required]
        public string Exchange { get; set; } = null!;
        
        /// <summary>
        /// Полное название типа события
        /// </summary>
        [Required]
        public string TypeName { get; set; } = null!;
    
        /// <summary>
        /// Ключ маршрутизации
        /// </summary>
        [Required]
        public string RoutingKey { get; set; } = null!;
    }

    public class ValueElement
    {
        /// <summary>
        /// Тип содержимого
        /// </summary>
        public string? ContentType { get; set; }
    
        /// <summary>
        /// Количество попыток на отправку сообщений
        /// </summary>
        public int? RetryCount { get; set; }
    
        /// <summary>
        /// Режим доставки сообщения
        /// </summary>
        public byte? DeliveryMode { get; set; }

        /// <summary>
        /// Ключ маршрутизации для отправки ответов
        /// </summary>
        [Required]
        public string ReplyTo { get; set; } = null!;

        /// <summary>
        /// Время ожидания ответов, в секундах
        /// </summary>
        public int? Timeout { get; set; }
        
        /// <summary>
        /// Список перехватчиков сообщения
        /// </summary>
        public List<BehaviorInfo> Behaviors { get; set; } = new();
    }
}