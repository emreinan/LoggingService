using System.Text.Json.Serialization;

namespace LoggingBroker.Infrastructure.Services.Loki.Models;
public record LokiPushRequest
{
    [JsonPropertyName("streams")]
    public LokiStream[] Streams { get; set; } = [];
}
