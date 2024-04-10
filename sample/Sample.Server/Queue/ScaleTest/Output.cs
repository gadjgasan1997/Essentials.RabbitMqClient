using Essentials.RabbitMqClient;

namespace Sample.Server.Queue.ScaleTest;

public class Output : IEvent
{
    public int Age { get; set; }
}