using MediatR;
using Essentials.Utils.Extensions;
using Essentials.RabbitMqClient.Publisher;

namespace Essentials.RabbitMqClient;

/// <summary>
/// Обработчик Rpc запроса от RabbitMqClient
/// </summary>
/// <typeparam name="TEvent">Тип события</typeparam>
/// <typeparam name="TResponse">Тип ответа</typeparam>
public class RabbitMqRpcRequestHandler<TEvent, TResponse> : IEventHandler<TEvent>
    where TEvent : IEvent, IRequest<TResponse>
    where TResponse : IEvent, INotification
{
    private readonly IMediator _mediator;
    private readonly IEventsPublisher _publisher;

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="mediator">Медиатр</param>
    /// <param name="publisher">Паблишер</param>
    public RabbitMqRpcRequestHandler(IMediator mediator, IEventsPublisher publisher)
    {
        _mediator = mediator.CheckNotNull();
        _publisher = publisher.CheckNotNull();
    }

    /// <inheritdoc cref="IEventHandler{TEvent}.HandleAsync" />
    public async Task HandleAsync(TEvent request)
    {
        var response = await _mediator.Send(request);
        response.CheckNotNull("Ответ для отдачи на rpc запрос не может быть null");

        _ = _publisher
            .PublishRpcResponse(response)
            .IfFail(exception => throw exception);
    }
}