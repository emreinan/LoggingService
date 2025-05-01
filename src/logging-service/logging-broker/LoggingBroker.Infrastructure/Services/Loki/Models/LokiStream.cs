using System.Text.Json.Serialization;

namespace LoggingBroker.Infrastructure.Services.Loki.Models;

public record LokiStream
{
    [JsonPropertyName("stream")]
    public Dictionary<string, string> Stream { get; set; } = [];
    
    [JsonPropertyName("values")]
    public string[][] Values { get; set; } = [];
}
