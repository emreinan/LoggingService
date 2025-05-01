using FluentAssertions;
using LoggingBroker.Domain.Models;
using LoggingBroker.Infrastructure.Services.Loki;

namespace LoggingBrokerApi.UnitTests.Mappers;

public class LogQueryMapperUtilitiesTests
{
    [Fact]
    public void ApplyDefaultTimes_Should_ApplyBoth_When_StartTimeAndEndTimeAreNull()
    {
        var input = new LogQueryParameters();

        var result = LokiQueryMapper.ApplyDefaultTimes(input);

        result.StartTime.Should().NotBeNull();
        result.EndTime.Should().NotBeNull();
        result.EndTime.Value.Should().BeGreaterThan(result.StartTime.Value);
    }

    [Fact]
    public void ApplyDefaultTimes_Should_ApplyOnlyEndTime_When_OnlyStartTimeProvided()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var input = new LogQueryParameters
        {
            StartTime = now - 60_000
        };

        var result = LokiQueryMapper.ApplyDefaultTimes(input);

        result.StartTime.Should().Be(input.StartTime);
        result.EndTime.Should().NotBeNull();
    }

    [Fact]
    public void ApplyDefaultTimes_Should_KeepProvidedTimes_When_BothArePresent()
    {
        var input = new LogQueryParameters
        {
            StartTime = 1234567890,
            EndTime = 1234567999
        };

        var result = LokiQueryMapper.ApplyDefaultTimes(input);

        result.StartTime.Should().Be(input.StartTime);
        result.EndTime.Should().Be(input.EndTime);
    }
}
