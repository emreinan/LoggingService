using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LoggingBroker.Application.Exceptions.HttpProblemDetails;

public class BadRequestProblemDetails : ProblemDetails
{
    public BadRequestProblemDetails(string detail)
    {
        Title = "Rule violation";
        Detail = detail;
        Status = StatusCodes.Status400BadRequest;
    }
}
