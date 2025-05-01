using LoggingBroker.Domain.Models;
using System.Text.Json;

namespace LoggingBroker.Application.Services.Loki;

public interface ILokiService
{
    Task<JsonElement> GetLogsAsync(LogQueryParameters logQuery, CancellationToken cancellationToken);
    Task SendLogAsync(LogRequest logRequest, CancellationToken cancellationToken);
}
