using FluentValidation;
using LoggingBroker.Domain.Models;
using Microsoft.AspNetCore.Http;
namespace LoggingBroker.Application.Operations.GetLog;

public class GetLogsQueryValidator : AbstractValidator<LogQueryParameters>
{
    public GetLogsQueryValidator(IHttpContextAccessor httpContextAccessor)
    {
        RuleForEach(x => x.Sources)
             .Must(s => !string.IsNullOrWhiteSpace(s))
             .WithMessage("Sources must not contain empty or whitespace strings.")
             .When(x => x.Sources is not null && x.Sources.Length > 0);

        RuleFor(x => x.Contains)
            .Must(c => !string.IsNullOrWhiteSpace(c))
            .When(x => x.Contains is not null)
            .WithMessage("'contains' query parameter cannot be empty.");

        RuleFor(x => x.Levels)
            .Must(levels => levels is null || levels.All(l => !string.IsNullOrWhiteSpace(l)))
            .WithMessage("Levels cannot contain empty values.");

        RuleFor(x => x.StartTime)
            .Must(x => x >= DateTimeOffset.UtcNow.AddDays(-30).ToUnixTimeMilliseconds())
            .WithMessage("StartTime cannot be older than 30 days ago.")
            .When(x => x.StartTime.HasValue);

        RuleFor(x => x.EndTime)
            .Must(x => x <= DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
            .WithMessage("EndTime cannot be in the future.")
            .When(x => x.EndTime.HasValue);

        RuleFor(x => x)
            .Must(x => !x.StartTime.HasValue || !x.EndTime.HasValue || x.StartTime <= x.EndTime)
            .WithMessage("StartTime must be less than or equal to EndTime.");


        RuleFor(x => x.CaseInsensitive).Custom((caseInsensitive, context) =>
        {
            if (caseInsensitive != true)
                return;

            var model = context.InstanceToValidate;
            var hasKnownFields =
                !string.IsNullOrWhiteSpace(model?.Contains) ||
                model?.Sources is { Length: > 0 } ||
                model?.Levels is { Length: > 0 };

            var httpContext = httpContextAccessor.HttpContext;
            var queryParams = httpContext?.Request?.Query;

            var hasValidCustomParam = queryParams != null &&
                queryParams.Keys.Any(k =>
                    k.StartsWith("param_", StringComparison.OrdinalIgnoreCase) &&
                    k.Length > "param_".Length);

            if (!hasKnownFields && !hasValidCustomParam)
            {
                context.AddFailure("CaseInsensitive", "'caseInsensitive' can only be used with 'contains', 'sources', 'levels', or 'param_' prefixed parameters.");
            }
        });

        RuleFor(x => x)
            .Custom((_, context) =>
            {
                var httpContext = httpContextAccessor.HttpContext;
                var queryParams = httpContext?.Request?.Query;

                if (queryParams is null) return;

                foreach (var kvp in queryParams)
                {
                    var key = kvp.Key;
                    var value = kvp.Value;

                    if (key.StartsWith("param_") && key.Length == "param_".Length)
                    {
                        continue;
                    }

                    if (key.StartsWith("param_") && string.IsNullOrWhiteSpace(value))
                    {
                        context.AddFailure(key, $"Query parameter '{key}' cannot be empty.");
                    }
                }
            });


    }
}

