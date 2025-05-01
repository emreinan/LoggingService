using Microsoft.Extensions.Options;
using System.Threading.RateLimiting;

namespace LoggingBroker.WebApi.Extensions.Services;
public static class BrokerServicesExtensions
{
    public static IServiceCollection AddBrokerServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<RateLimiterSettings>()
                .Bind(configuration.GetSection("RateLimiterSettings"))
                .Validate(settings =>
                    settings.PermitLimit > 0 &&
                    settings.WindowTime > 0 &&
                    settings.QueueLimit >= 0,
                    "RateLimiter ayarları geçersiz: PermitLimit > 0, WindowTime > 0, QueueLimit >= 0 olmalı.")
                .ValidateOnStart();

        services.AddRateLimiter(options =>
        {
            var serviceProvider = services.BuildServiceProvider();
            var limiterSettings = serviceProvider.GetRequiredService<IOptions<RateLimiterSettings>>().Value;

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter("global", _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = limiterSettings.PermitLimit,
                    Window = TimeSpan.FromSeconds(limiterSettings.WindowTime),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = limiterSettings.QueueLimit
                }));

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });

        return services;
    }
}
