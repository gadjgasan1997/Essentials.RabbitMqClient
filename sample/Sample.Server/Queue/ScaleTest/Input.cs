using Essentials.RabbitMqClient;

namespace Sample.Server.Queue.ScaleTest;

public class Input : IEvent
{
    public string Name { get; set; } = null!;
}