namespace LoggingBroker.Application.Services.Validations;

public interface IValidationDispatcher
{
    Task ValidateAsync<T>(T model, CancellationToken cancellationToken);
}
