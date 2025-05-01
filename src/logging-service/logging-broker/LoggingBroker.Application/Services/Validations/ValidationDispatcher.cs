using FluentValidation;
using LoggingBroker.Application.Exceptions.Types;
using Microsoft.Extensions.DependencyInjection;
using ValidationException = LoggingBroker.Application.Exceptions.Types.ValidationException;

namespace LoggingBroker.Application.Services.Validations;

public class ValidationDispatcher(IServiceProvider serviceProvider) : IValidationDispatcher
{
    public async Task ValidateAsync<T>(T model, CancellationToken cancellationToken)
    {
        var validator = serviceProvider.GetService<IValidator<T>>();
        if (validator is null)
        {
            return;
        }

        var validationResult = await validator.ValidateAsync(model, cancellationToken);
        if (!validationResult.IsValid)
        {
            var validationErrors = validationResult.Errors
                .Select(error => new ValidationExceptionModel
                {
                    Property = error.PropertyName,
                    Errors = [error.ErrorMessage]
                });

            throw new ValidationException(validationErrors);
        }
    }
}

