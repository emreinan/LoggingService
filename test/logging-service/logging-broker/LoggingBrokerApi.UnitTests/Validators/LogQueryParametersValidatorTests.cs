using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using LoggingBroker.Application.Operations.GetLog;
using LoggingBroker.Domain.Models;

namespace LoggingBrokerApi.UnitTests.Validators;

public class LogQueryParametersValidatorTests
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LogQueryParametersValidatorTests()
    {
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
    }

    private GetLogsQueryValidator CreateValidatorWithQuery(IQueryCollection query)
    {
        var context = new DefaultHttpContext();
        context.Request.Query = query;
        _httpContextAccessor.HttpContext.Returns(context);
        return new GetLogsQueryValidator(_httpContextAccessor);
    }

    [Theory]
    [InlineData(null, true)] 
    [InlineData(new string[] { }, true)] 
    [InlineData(new string[] { "source1", "" }, false)] 
    public void Sources_Should_Validate_EmptyItems_WhenGivenArray(string[]? sources, bool expectedIsValid)
    {
        // Arrange
        var validator = CreateValidatorWithQuery(new QueryCollection());
        var model = new LogQueryParameters { Sources = sources };

        // Act
        var result = validator.Validate(model);

        // Assert
        result.IsValid.Should().Be(expectedIsValid);
    }

    [Theory]
    [InlineData(-1, 0, false)]
    [InlineData(0, 0, false)]
    public void StartTime_Should_Fail_When_NonPositive(long startTime, long endTime, bool expectedIsValid)
    {
        // Arrange
        var validator = CreateValidatorWithQuery(new QueryCollection());
        var model = new LogQueryParameters
        {
            StartTime = startTime,
            EndTime = endTime
        };

        // Act
        var result = validator.Validate(model);

        // Assert
        result.IsValid.Should().Be(expectedIsValid);
    }

    [Fact]
    public void StartTime_ShouldBeBeforeEndTime_WhenBothInValidRange()
    {
        // Arrange
        var validator = CreateValidatorWithQuery(new QueryCollection());

        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var start = now - TimeSpan.FromHours(1).TotalMilliseconds;
        var end = now;

        var model = new LogQueryParameters
        {
            StartTime = (long)start,
            EndTime = end
        };

        // Act
        var result = validator.Validate(model);

        // Assert
        result.IsValid.Should().BeTrue();
    }


    [Theory]
    [InlineData(null, true)]    
    [InlineData("   ", false)]   
    [InlineData("someText", true)]
    public void Contains_Should_NotBeEmpty_WhenProvided(string? text, bool expectedIsValid)
    {
        // Arrange
        var validator = CreateValidatorWithQuery(new QueryCollection());
        var model = new LogQueryParameters { Contains = text };

        // Act
        var result = validator.Validate(model);

        // Assert
        result.IsValid.Should().Be(expectedIsValid);
    }

    [Theory]
    [InlineData(new string[] { "Information" }, true)]
    [InlineData(new string[] { "info", "warn" }, true)]
    [InlineData(new string[] { "debug", "ERROR", "fatal" }, true)]
    [InlineData(new string[] { "INVALID" }, true)]
    [InlineData(new string[] { "", "warn" }, false)]
    public void Levels_Should_BeValid_When_TheyAreRecognized(string[] levels, bool expectedIsValid)
    {
        // Arrange
        var validator = CreateValidatorWithQuery(new QueryCollection());
        var model = new LogQueryParameters { Levels = levels };

        // Act
        var result = validator.Validate(model);

        // Assert
        result.IsValid.Should().Be(expectedIsValid);
    }
}
