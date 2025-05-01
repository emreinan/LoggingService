using FluentAssertions;
using MediatR;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Text.Json;
using LoggingBroker.Application.Exceptions.Types;
using LoggingBroker.Application.Operations.GetLog;
using LoggingBroker.Application.Operations.SendLog;
using LoggingBroker.Application.Services.Loki;
using LoggingBroker.Application.Services.Validations;
using LoggingBroker.Domain.Models;

namespace LoggingBrokerApi.UnitTests.Handlers;

public class LogCommandQueryHandlerTests
{
    private readonly ILokiService _mockLokiService = Substitute.For<ILokiService>();
    private readonly IValidationDispatcher _validationDispatcher = Substitute.For<IValidationDispatcher>();

    [Fact]
    public async Task SendLogCommand_Should_ThrowValidationException_When_RequestInvalid()
    {
        var logRequest = new LogRequest
        {
            Source = "invalid-source",
            LogLevel = Serilog.Events.LogEventLevel.Debug,
            Message = "test"
        };

        _validationDispatcher
            .ValidateAsync(logRequest, default)
            .Throws(new ValidationException("invalid source"));

        var handler = new SendLogCommandHandler(_mockLokiService, _validationDispatcher);

        var act = async () => await handler.Handle(new SendLogCommand(logRequest), default);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*invalid source*");
    }

    [Fact]
    public async Task SendLogCommand_Should_CallLokiService_When_RequestValid()
    {
        var logRequest = new LogRequest
        {
            Source = "api-service",
            LogLevel = Serilog.Events.LogEventLevel.Information,
            Message = "created",
            EventUnixTimeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        _validationDispatcher.ValidateAsync(logRequest, default).Returns(Task.CompletedTask);

        var handler = new SendLogCommandHandler(_mockLokiService, _validationDispatcher);

        var result = await handler.Handle(new SendLogCommand(logRequest), default);

        await _mockLokiService.Received(1).SendLogAsync(logRequest, default);
        result.Should().Be(Unit.Value);
    }

    [Fact]
    public async Task GetLogsQuery_Should_ThrowValidationException_When_InvalidParams()
    {
        var query = new LogQueryParameters { StartTime = 9999, EndTime = 100 };

        _validationDispatcher
            .ValidateAsync(query, default)
            .Throws(new ValidationException("invalid range"));

        var handler = new GetLogsQueryHandler(_mockLokiService, _validationDispatcher);

        var act = async () => await handler.Handle(new GetLogsQuery(query), default);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*invalid range*");
    }

    [Fact]
    public async Task GetLogsQuery_Should_ReturnLogs_When_ValidRequest()
    {
        var query = new LogQueryParameters
        {
            Sources = ["test"],
            StartTime = DateTimeOffset.UtcNow.AddMinutes(-5).ToUnixTimeMilliseconds(),
            EndTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        var json = JsonDocument.Parse("""
        {
          "result": [
            {
              "stream": { "source": "test" },
              "values": [["1234567890", "Log message"]]
            }
          ]
        }
        """).RootElement;

        _validationDispatcher.ValidateAsync(query, default).Returns(Task.CompletedTask);
        _mockLokiService.GetLogsAsync(query, default).Returns(json);

        var handler = new GetLogsQueryHandler(_mockLokiService, _validationDispatcher);

        var result = await handler.Handle(new GetLogsQuery(query), default);

        result.ValueKind.Should().Be(JsonValueKind.Object);
        result.GetProperty("result").EnumerateArray().Should().NotBeEmpty();
    }
}
