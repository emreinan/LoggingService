using System.Text.Json;
using System.Text.Json.Serialization;
using Serilog.Events;

public class LogEventLevelJsonConverter : JsonConverter<LogEventLevel>
{
    public static readonly Dictionary<string, LogEventLevel> Mappings = new(StringComparer.OrdinalIgnoreCase)
    {
        ["verbose"] = LogEventLevel.Verbose,
        ["debug"] = LogEventLevel.Debug,
        ["info"] = LogEventLevel.Information,
        ["information"] = LogEventLevel.Information,
        ["warn"] = LogEventLevel.Warning,
        ["warning"] = LogEventLevel.Warning,
        ["error"] = LogEventLevel.Error,
        ["err"] = LogEventLevel.Error,
        ["fatal"] = LogEventLevel.Fatal,
        ["critical"] = LogEventLevel.Fatal,
    };

    private const LogEventLevel InvalidSentinel = (LogEventLevel)(-1);

    public override LogEventLevel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            return InvalidSentinel;
        }

        var enumText = reader.GetString();

        if (string.IsNullOrWhiteSpace(enumText))
        {
            return InvalidSentinel;
        }

        if (Mappings.TryGetValue(enumText, out var mappedLevel))
        {
            return mappedLevel;
        }

        if (Enum.TryParse<LogEventLevel>(enumText, ignoreCase: true, out var parsedLevel))
        {
            return parsedLevel;
        }

        return InvalidSentinel;
    }

    public override void Write(Utf8JsonWriter writer, LogEventLevel value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
