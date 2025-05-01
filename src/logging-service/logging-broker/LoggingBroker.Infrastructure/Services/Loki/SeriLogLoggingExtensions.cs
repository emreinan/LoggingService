using Microsoft.AspNetCore.Builder;
using Serilog;
using Serilog.Events;

namespace LoggingBroker.Infrastructure.Services.Loki;

public static class SeriLogLoggingExtensions
{
    public static WebApplicationBuilder AddSerilogLogging(
            this WebApplicationBuilder builder)
    {
        // Root logger’ı SeriLog olarak ayarlar.
        // İleride SeriLog kullanılmak istenmez ise, sadece bu kısım değiştirilecektir.
        builder.Host.UseSerilog((ctx, services, config) =>
        {
            config
                .ReadFrom.Configuration(ctx.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .MinimumLevel.Debug()
                .WriteTo.File(
                    path: "logs/fallback-.log",
                    rollingInterval: RollingInterval.Day,
                    restrictedToMinimumLevel: LogEventLevel.Error);
        });

        return builder;
    }
}
