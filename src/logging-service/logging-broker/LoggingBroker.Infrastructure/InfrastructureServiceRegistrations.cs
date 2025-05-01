using LoggingBroker.Application.Services.Loki;
using LoggingBroker.Infrastructure.Services.Loki;
using Microsoft.Extensions.DependencyInjection;

namespace LoggingBroker.Infrastructure;

public static class InfrastructureServiceRegistrations
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services)
    {
        services.AddHttpClient("LokiClient", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        services.AddScoped<ILokiService, LokiService>();
        services.AddHttpContextAccessor();

        return services;
    }
}
