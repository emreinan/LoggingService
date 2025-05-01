using FluentValidation;
using LoggingBroker.Domain.Models;
using Serilog.Events;

namespace LoggingBroker.Application.Operations.SendLog;

public class SendLogCommandValidator : AbstractValidator<LogRequest>
{
    public SendLogCommandValidator()
    {

        RuleFor(x => x.Source)
            .NotEmpty().WithMessage("Source must not be empty.");

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message must not be empty.");

        RuleFor(x => x.LogLevel)
            .Must(level => Enum.IsDefined(typeof(LogEventLevel), level))
            .WithMessage("Invalid LogLevel value.");

        RuleFor(x => x.EventUnixTimeMs)
            .LessThanOrEqualTo(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
            .WithMessage("Event time must be less than or equal to current time.");
    }
}
