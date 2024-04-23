namespace net.shonx.stocks;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

public class Program
{

    public static async Task Main(string[] args)
    {
        var host = new HostBuilder().ConfigureFunctionsWebApplication().ConfigureServices(services =>
            {
                services.AddApplicationInsightsTelemetryWorkerService();
                services.ConfigureFunctionsApplicationInsights();
            })
        .Build();

        await host.RunAsync();
    }

}