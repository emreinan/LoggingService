using LoggingBroker.Application.Exceptions.Types;

namespace LoggingBroker.Application.Exceptions.Handlers;

public abstract class ExceptionHandler
{
    public abstract Task HandleException(BadRequestException businessException);
    public abstract Task HandleException(ValidationException validationException);
    public abstract Task HandleException(Exception exception);
}
