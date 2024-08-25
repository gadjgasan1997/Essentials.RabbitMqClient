using MediatR;
using Essentials.RabbitMqClient;

namespace Sample.Server.Queue.ScaleTest;

public class Output : IEvent, INotification
{
    public int Age { get; set; }
}