using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using LoggingBroker.Application.Exceptions.Types;
using LoggingBroker.Domain.Models;
using LoggingBroker.Infrastructure.Services.Loki;

namespace LoggingBrokerApi.UnitTests.Mappers;

public class LokiQueryMapperTests
{
    [Fact]
    public void MapToLabelFilters_Should_MapSources_Correctly()
    {
        var queryParams = new LogQueryParameters
        {
            Sources = ["api|auth"],
            CaseInsensitive = true
        };

        var query = new QueryCollection();

        var result = LokiQueryMapper.MapToLabelFilters(queryParams, query, caseInsensitive: true);

        result.Should().ContainSingle()
              .Which.Should().Be("source=~\"(?i).*(api|auth).*\"");
    }

    [Fact]
    public void MapToLabelFilters_Should_MapLevels_Correctly()
    {
        var queryParams = new LogQueryParameters
        {
            Levels = ["Error"],
            CaseInsensitive = false
        };

        var query = new QueryCollection();

        var result = LokiQueryMapper.MapToLabelFilters(queryParams, query, caseInsensitive: false);

        result.Should().ContainSingle()
              .Which.Should().Be("level=~\".*(Error).*\"");
    }

    [Fact]
    public void MapToLabelFilters_Should_MapCustomParameters_Correctly()
    {
        var queryParams = new LogQueryParameters
        {
            CaseInsensitive = true
        };

        var query = new QueryCollection(new Dictionary<string, StringValues>
        {
            { "param_userId", new StringValues("123|456") }
        });

        var result = LokiQueryMapper.MapToLabelFilters(queryParams, query, caseInsensitive: true);

        result.Should().ContainSingle()
              .Which.Should().Be("param_userId=~\"(?i).*(123|456).*\"");
    }

    [Fact]
    public void MapToLabelFilters_Should_ReturnEmpty_When_NoParams()
    {
        var queryParams = new LogQueryParameters();
        var query = new QueryCollection();

        var result = LokiQueryMapper.MapToLabelFilters(queryParams, query, caseInsensitive: true);

        result.Should().BeEmpty();
    }

    [Fact]
    public void MapToLabelFilters_Should_ReturnEmpty_When_OnlyInvalidCustomParams()
    {
        var queryParams = new LogQueryParameters();
        var query = new QueryCollection(new Dictionary<string, StringValues>
        {
            { "param_invalid", "" }
        });

        var act = () => LokiQueryMapper.MapToLabelFilters(queryParams, query, caseInsensitive: true);

        act.Should().Throw<BadRequestException>()
        .WithMessage("param_invalid must not be empty.");
    }

}
