using Serilog.Events;
using System.Text.Json.Serialization;

namespace LoggingBroker.Domain.Models;
public record LogRequest
{
    public string Source { get; init; } = default!;
    [JsonConverter(typeof(LogEventLevelJsonConverter))]
    public LogEventLevel LogLevel { get; init; }
    public string Message { get; init; } = default!;
    public Dictionary<string, string>? Parameters { get; init; }
    public long EventUnixTimeMs { get; init; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
}