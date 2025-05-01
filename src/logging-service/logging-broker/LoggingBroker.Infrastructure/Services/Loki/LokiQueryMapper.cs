using LoggingBroker.Domain.Models;
using Microsoft.AspNetCore.Http;
using LoggingBroker.Application.Exceptions.Types;

namespace LoggingBroker.Infrastructure.Services.Loki;

public static class LokiQueryMapper
{
    public static List<string> MapToLabelFilters(LogQueryParameters queryParams, IQueryCollection query, bool caseInsensitive)
    {
        var labelFilters = new List<string>();

        if (queryParams.Sources is not null && queryParams.Sources.Length > 0)
        {
            var processedSources = queryParams.Sources
                .SelectMany(s => s.Split('|', StringSplitOptions.RemoveEmptyEntries))
                .Where(part => !string.IsNullOrWhiteSpace(part))
                .ToList();

            if (processedSources.Count > 0)
            {
                var joinedSources = $".*({string.Join("|", processedSources)}).*";
                labelFilters.Add(caseInsensitive
                    ? $"source=~\"(?i){joinedSources}\""
                    : $"source=~\"{joinedSources}\"");
            }
        }

        if (queryParams.Levels is not null && queryParams.Levels.Length > 0)
        {
            var processedLevels = queryParams.Levels
                .SelectMany(l => l.Split('|', StringSplitOptions.RemoveEmptyEntries))
                .Where(part => !string.IsNullOrWhiteSpace(part))
                .ToList();

            if (processedLevels.Count > 0)
            {
                var joinedLevels = $".*({string.Join("|", processedLevels)}).*";
                labelFilters.Add(caseInsensitive
                    ? $"level=~\"(?i){joinedLevels}\""
                    : $"level=~\"{joinedLevels}\"");
            }
            else { return labelFilters; }
        }

        foreach (var key in query.Keys)
        {
            if (key.StartsWith("param_", StringComparison.OrdinalIgnoreCase))
            {
                if (key.Length == "param_".Length)
                {
                    throw new BadRequestException("Query parameter name after 'param_' must not be empty.");
                }

                var val = query[key].ToString();
                if (string.IsNullOrWhiteSpace(val))
                {
                    throw new BadRequestException($"{key} must not be empty.");
                }
                var subParts = val.Split('|', StringSplitOptions.RemoveEmptyEntries);
                var processedParts = subParts.Where(part => !string.IsNullOrWhiteSpace(part)).ToList();
                var joinedParamValue = $".*({string.Join("|", processedParts)}).*";
                labelFilters.Add(caseInsensitive
                    ? $"{key}=~\"(?i){joinedParamValue}\""
                    : $"{key}=~\"{joinedParamValue}\"");
            }
        }

        return labelFilters;
    }
    public static LogQueryParameters ApplyDefaultTimes(LogQueryParameters queryParams)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        return queryParams switch
        {
            { StartTime: null, EndTime: null } => queryParams with
            {
                StartTime = now - 7L * 24 * 60 * 60 * 1000,
                EndTime = now
            },
            { StartTime: not null, EndTime: null } => queryParams with
            {
                EndTime = now
            },
            _ => queryParams
        };
    }
}


