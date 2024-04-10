using LanguageExt;
using Essentials.Utils.Extensions;
using Sample.Client.Models;
using Essentials.RabbitMqClient.Publisher;
using Essentials.Serialization.Helpers;

namespace Sample.Client.Services;

public class TestService : ITestService
{
    private readonly ILogger<TestService> _logger;
    private readonly IEventsPublisher _eventsPublisher;
    
    public TestService(ILogger<TestService> logger, IEventsPublisher eventsPublisher)
    {
        _logger = logger;
        _eventsPublisher = eventsPublisher.CheckNotNull();
    }
    
    public async Task SendRpcCallAsync()
    {
        var result = await _eventsPublisher
            .AskAsync<Output, Input>(
                new Output
                {
                    Name = "Ivan"
                },
                CancellationToken.None);

        _ = result
            .Match(
                Succ: input =>
                {
                    _logger.LogInformation("Ответ на Rpc вызов: " + JsonHelpers.Serialize(input));
                    
                    return Unit.Default;
                },
                Fail: exception =>
                {
                    _logger.LogError(exception, "Ошибка осуществления Rpc вызова");
                    
                    return Unit.Default;
                });
    }
}