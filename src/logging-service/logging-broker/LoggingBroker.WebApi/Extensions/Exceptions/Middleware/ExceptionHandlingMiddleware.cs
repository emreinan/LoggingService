using System.Net.Mime;
using LoggingBroker.Application.Exceptions.Handlers;

namespace LoggingBroker.WebApi.Extensions.Exceptions.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> fallbackLogger)
{
    private readonly HttpExceptionHandler _httpExceptionHandler = new();

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            fallbackLogger.LogError(ex, "Unhandled exception on {Path} [{Method}]: {Message}",
                           context.Request.Path, context.Request.Method, ex.Message);

            await HandleExceptionAsync(context.Response, ex);
        }
    }
    protected virtual Task HandleExceptionAsync(HttpResponse response, dynamic exception)
    {
        response.ContentType = MediaTypeNames.Application.Json;
        _httpExceptionHandler.Response = response;

        return _httpExceptionHandler.HandleException(exception);
    }
}
