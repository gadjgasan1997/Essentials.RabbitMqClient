namespace Essentials.RabbitMqClient.Dictionaries;

/// <summary>
/// Известные сериалайзеры/десериалайзеры, использующиеся для RabbitMq
/// </summary>
public static class KnownRabbitMqSerializers
{
    /// <summary>
    /// Json
    /// </summary>
    public const string JSON = "RabbitMqJsonSerializer";
    
    /// <summary>
    /// Xml
    /// </summary>
    public const string XML = "RabbitMqXmlSerializer";
}