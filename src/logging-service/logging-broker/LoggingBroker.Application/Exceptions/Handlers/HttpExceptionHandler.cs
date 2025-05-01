using LoggingBroker.Application.Exceptions.HttpProblemDetails;
using LoggingBroker.Application.Exceptions.Types;
using Microsoft.AspNetCore.Http;

namespace LoggingBroker.Application.Exceptions.Handlers;

public class HttpExceptionHandler : ExceptionHandler
{
    public HttpResponse Response
    {
        get => _response ?? throw new NullReferenceException(nameof(_response));
        set => _response = value;
    }

    private HttpResponse? _response;

    public override Task HandleException(BadRequestException businessException)
    {
        Response.StatusCode = StatusCodes.Status400BadRequest;
        string details = new BadRequestProblemDetails(businessException.Message).ToJson();
        return Response.WriteAsync(details);
    }

    public override Task HandleException(ValidationException validationException)
    {
        Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
        string details = new ValidationProblemDetails(validationException.Errors).ToJson();
        return Response.WriteAsync(details);
    }
    public override Task HandleException(Exception exception)
    {
        Response.StatusCode = StatusCodes.Status500InternalServerError;
        string details = new InternalServerErrorProblemDetails(exception.Message).ToJson();
        return Response.WriteAsync(details);
    }
}
