namespace LoggingBroker.Domain.Models;

public record LogQueryParameters
{
    public string[]? Sources { get; init; }
    public string[]? Levels { get; init; }
    public long? StartTime { get; init; }
    public long? EndTime { get; init; }
    public string? Contains { get; init; }
    public bool? CaseInsensitive { get; init; }
}