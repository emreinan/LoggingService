using LoggingBroker.Application.Services.Loki;
using LoggingBroker.Application.Services.Validations;
using LoggingBroker.Domain.Models;
using MediatR;
using System.Text.Json;

namespace LoggingBroker.Application.Operations.GetLog;

public record GetLogsQuery(LogQueryParameters QueryParams) : IRequest<JsonElement>;

// Handler
public class GetLogsQueryHandler(ILokiService lokiService,
                           IValidationDispatcher validation) : IRequestHandler<GetLogsQuery, JsonElement>
{
    public async Task<JsonElement> Handle(GetLogsQuery request, CancellationToken cancellationToken)
    {
        // 1) Validasyon
        await validation.ValidateAsync(request.QueryParams, cancellationToken);

        // 2) BusinessRules

        // 3) Call Loki service
        var logs = await lokiService.GetLogsAsync(request.QueryParams, cancellationToken);
        return logs;
    }
}