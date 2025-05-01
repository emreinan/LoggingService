namespace LoggingBroker.WebApi.Extensions.Services;

public record RateLimiterSettings
{
    public int PermitLimit { get; init; }
    public int WindowTime { get; init; }
    public int QueueLimit { get; init; }
}