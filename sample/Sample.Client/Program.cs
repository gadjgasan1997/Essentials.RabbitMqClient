using NLog.Extensions.Logging;

namespace Sample.Client;

public static class Program
{
    public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host
            .CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(builder =>
                builder
                    .UseStartup<Startup>()
                    .ConfigureLogging(loggingBuilder => loggingBuilder.ClearProviders().AddNLog()));
}