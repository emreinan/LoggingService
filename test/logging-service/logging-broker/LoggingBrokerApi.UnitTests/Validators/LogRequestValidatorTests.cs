using FluentAssertions;
using FluentValidation.TestHelper;
using Serilog.Events;
using LoggingBroker.Application.Operations.SendLog;
using LoggingBroker.Domain.Models;

namespace LoggingBrokerApi.UnitTests.Validators;

public class LogRequestValidatorTests
{
    private readonly SendLogCommandValidator _validator = new();

    [Fact]
    public void Validate_Should_HaveError_When_EventTimeIsInFuture()
    {
        var futureTime = DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeMilliseconds();

        var model = new LogRequest
        {
            Source = "api",
            Message = "Future log",
            LogLevel = LogEventLevel.Warning,
            EventUnixTimeMs = futureTime
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EventUnixTimeMs);
    }


    [Theory]
    [InlineData("MySource", "Some message", LogEventLevel.Information, true)]
    [InlineData("", "Some message", LogEventLevel.Information, false)]
    [InlineData("MySource", "", LogEventLevel.Information, false)]
    public void LogRequest_Should_Validate_RequiredFields(string source, string message, LogEventLevel level, bool expected)
    {
        // Arrange
        var req = new LogRequest
        {
            Source = source,
            Message = message,
            LogLevel = level,
            EventUnixTimeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        // Act
        var result = _validator.Validate(req);

        // Assert
        result.IsValid.Should().Be(expected);
    }
}
