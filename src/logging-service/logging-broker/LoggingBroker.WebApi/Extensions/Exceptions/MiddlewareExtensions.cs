using LoggingBroker.WebApi.Extensions.Exceptions.Middleware;
using Microsoft.AspNetCore.Builder;

namespace LoggingBroker.WebApi.Extensions.Exceptions;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
