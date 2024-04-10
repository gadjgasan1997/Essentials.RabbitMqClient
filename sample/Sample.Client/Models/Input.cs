using Essentials.RabbitMqClient;

namespace Sample.Client.Models;

public class Input : IEvent
{
    public int Age { get; set; }
}