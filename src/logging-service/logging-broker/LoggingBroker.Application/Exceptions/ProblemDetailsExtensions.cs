using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace LoggingBroker.Application.Exceptions;

public static class ProblemDetailsExtensions
{
    public static string ToJson<TProblemDetail>(this TProblemDetail details)
        where TProblemDetail : ProblemDetails
    {
        return JsonSerializer.Serialize(details);
    }
}
