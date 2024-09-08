using System.Reflection;
using Essentials.Serialization;
using Essentials.Serialization.Serializers;
using Essentials.Serialization.Deserializers;
using Essentials.RabbitMqClient.Dictionaries;
using Essentials.RabbitMqClient.Extensions;
using Essentials.RabbitMqClient.Configuration.Builders;

namespace Sample.Server;

public class Startup
{
    private IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        // Переопределение сериалайзера в Json.
        // При необходимости в конструктор можно передать свои опции,
        // а также прокинуть один из прочих существующих сериалайзеров
        // или вообще создать собственный, реализующий соответствующий интерфейс.
        // Ключи обязательно должны быть такими, как я прописал, так как моя либа ищет именно по ним
        EssentialsSerializersFactory.AddOrUpdateByKey(KnownRabbitMqSerializers.JSON, () => new NativeJsonSerializer());
        EssentialsSerializersFactory.AddOrUpdateByKey(KnownRabbitMqSerializers.XML, () => new XmlSerializer());
        EssentialsDeserializersFactory.AddOrUpdateByKey(KnownRabbitMqDeserializers.JSON, () => new NativeJsonDeserializer());
        EssentialsDeserializersFactory.AddOrUpdateByKey(KnownRabbitMqDeserializers.XML, () => new XmlDeserializer());
        
        // Настройка существующих соединений с RabbitMq
        services.AddMediatR(configuration => configuration.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));
        
        services.ConfigureRabbitMqConnections(
            Configuration,
            builder => builder.ConfigureConnection("esb", ConfigureEsbConnection));
    }

    private static void ConfigureEsbConnection(ConnectionBuilder connectionBuilder)
    {
        const string queueName = "RabbitMqScaleTestInput";

        connectionBuilder.ConfigureRpc(builder =>
            builder.ConfigureRpcRequestHandlingDefault<Queue.ScaleTest.Input, Queue.ScaleTest.Output>(
                queueName));
    }

    public void Configure(
        IApplicationBuilder app,
        IServiceScopeFactory factory)
    { }
}