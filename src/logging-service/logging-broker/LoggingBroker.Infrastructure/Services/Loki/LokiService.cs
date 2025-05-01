using LoggingBroker.Application.Services.Loki;
using LoggingBroker.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using LoggingBroker.Application.Exceptions.Types;
using LoggingBroker.Infrastructure.Services.Loki.Models;

namespace LoggingBroker.Infrastructure.Services.Loki;

public class LokiService(IHttpClientFactory httpClientFactory,
                        ILogger<LokiService> fallbackFileLogger,
                        IHttpContextAccessor httpContextAccessor,
                        IConfiguration configuration
                        ) : ILokiService
{
    private readonly string lokiUrl = configuration.GetValue<string>("LokiUrl") ?? "http://loki:3100";

    private readonly HttpClient client = httpClientFactory.CreateClient("LokiClient");

    public async Task<JsonElement> GetLogsAsync(LogQueryParameters queryParams, CancellationToken cancellationToken)
    {
        queryParams = LokiQueryMapper.ApplyDefaultTimes(queryParams);

        bool caseInsensitive = queryParams.CaseInsensitive ?? true;

        var query = httpContextAccessor.HttpContext?.Request?.Query;

        var allowedParams = typeof(LogQueryParameters)
            .GetProperties()
            .Select(p => p.Name.ToLower())
            .ToList();

        foreach (var key in query!.Keys)
        {
            if (string.IsNullOrWhiteSpace(key) ||
            !allowedParams.Contains(key.ToLower()) && !key.StartsWith("param_", StringComparison.OrdinalIgnoreCase))
            {
                throw new BadRequestException($"Unknown query parameter: {key}");
            }
        }

        var labelFilters = LokiQueryMapper.MapToLabelFilters(queryParams, query, caseInsensitive);

        if (labelFilters.Count == 0)
        {
            labelFilters.Add("source=~\".+\"");
        }

        var labelSelector = "{" + string.Join(", ", labelFilters) + "}";
        var logQuery = labelSelector;

        if (!string.IsNullOrWhiteSpace(queryParams.Contains))
        {
            string ciPrefix = caseInsensitive ? "(?i)" : "";

            var parts = queryParams.Contains.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                    .Where(p => !string.IsNullOrWhiteSpace(p))
                                    .Select(Regex.Escape);
            var joined = string.Join("|", parts);

            logQuery += $" |~ \"{ciPrefix}({joined})\"";
        }

        var startNs = (queryParams.StartTime!.Value * 1_000_000L).ToString();
        var endNs = (queryParams.EndTime!.Value * 1_000_000L).ToString();

        var queryParamsList = new List<string>
        {
            $"query={Uri.EscapeDataString(logQuery)}",
            "limit=1000",
            "direction=backward",
            $"start={startNs}",
            $"end={endNs}"
        };

        string finalQueryString = string.Join("&", queryParamsList);
        var requestUri = $"{lokiUrl}/loki/api/v1/query_range?{finalQueryString}";

        var response = await client.GetAsync(requestUri, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            fallbackFileLogger.LogError("Loki query failed. Status: {StatusCode}, Content: {ErrorContent}",
                (int)response.StatusCode, responseContent);
            throw new BadRequestException($"Loki query failed: {response.StatusCode} - {responseContent}");
        }

        using var jsonDoc = JsonDocument.Parse(responseContent);
        if (jsonDoc.RootElement.TryGetProperty("data", out var dataElement)
            && dataElement.TryGetProperty("result", out var resultElement))
        {
            return dataElement.Clone();
        }

        throw new BadRequestException("Unexpected Loki response format: missing data.result");
    }

    public async Task SendLogAsync(LogRequest logRequest, CancellationToken cancellationToken)
    {
        var nanoTime = (logRequest.EventUnixTimeMs * 1_000_000L).ToString();
        var brokerReceivedTimeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var streamLabels = new Dictionary<string, string>
        {
                        { "source", logRequest.Source },
                        { "level", logRequest.LogLevel.ToString() },
                        { "brokerReceivedTimeMs", brokerReceivedTimeMs.ToString() },
                        { "originalEventTimeMs", logRequest.EventUnixTimeMs.ToString() }
        };

        if (logRequest.Parameters is not null)
        {
            foreach (var kv in logRequest.Parameters)
            {
                var safeKey = "param_" + kv.Key.Replace(" ", "_");
                streamLabels[safeKey] = kv.Value;
            }
        }

        var lokiPushRequest = new LokiPushRequest
        {
            Streams =
            [
                    new LokiStream
                            {
                                Stream = streamLabels,
                                Values =
                                [
                                    [nanoTime, logRequest.Message]
                                ]
                            }
                ]
        };
        var jsonOptions = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
        };

        var response = await client.PostAsJsonAsync(
                $"{lokiUrl}/loki/api/v1/push",
                lokiPushRequest,
                jsonOptions,
                cancellationToken
            );

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            fallbackFileLogger.LogError("HTTP push to Loki failed. Status: {StatusCode}, Content: {ErrorContent}",
                (int)response.StatusCode, content);
            // TODO: StatusCode'ı kontrol et ve uygun exception at
            throw new BadRequestException($"Failed to send log to Loki: {(int)response.StatusCode}");
        }
    }
}
