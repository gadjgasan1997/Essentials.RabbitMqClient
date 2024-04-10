namespace Essentials.RabbitMqClient.Dictionaries;

/// <summary>
/// Известные десериалайзеры, использующиеся для RabbitMq
/// </summary>
public static class KnownRabbitMqDeserializers
{
    /// <summary>
    /// Json
    /// </summary>
    public const string JSON = "RabbitMqJsonDeserializer";
    
    /// <summary>
    /// Xml
    /// </summary>
    public const string XML = "RabbitMqXmlDeserializer";
}