using MediatR;
using Microsoft.AspNetCore.Mvc;
using LoggingBroker.Application.Operations.GetLog;
using LoggingBroker.Application.Operations.SendLog;
using LoggingBroker.Domain.Models;

namespace LoggingBroker.WebApi.Extensions.Endpoints;

public static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapRoutes(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/v1/healthcheck", () => Results.Ok("Up and running"));

        endpoints.MapPost("/api/v1/logs", async (
                                                 [FromBody] LogRequest logRequest,
                                                 IMediator mediator,
                                                 CancellationToken cancellationToken) =>
        {
            await mediator.Send(new SendLogCommand(logRequest), cancellationToken);
            return Results.Ok("Log received and forwarded to Loki!");
        });

        endpoints.MapGet("/api/v1/logs", async (
                                                [AsParameters] LogQueryParameters queryParams,
                                                IMediator mediator,
                                                CancellationToken cancellationToken) =>
        {
            var logsJson = await mediator.Send(new GetLogsQuery(queryParams), cancellationToken);
            return Results.Ok(logsJson);
        });


        return endpoints;
    }
}
