using FluentAssertions;
using Polly;
using System.Net;
using System.Net.Http.Json;
using LoggingBroker.Application.Exceptions.HttpProblemDetails;

namespace LoggingBrokerWebApi.IntegrationTests.LoggingBrokerTest;

public class LoggingBrokerTests : IClassFixture<LoggingBrokerTestFixture>
{
    private readonly string baseUrl;
    private readonly HttpClient client;
    private readonly LoggingBrokerTestFixture loggingBrokerTestFixture;
    public LoggingBrokerTests(LoggingBrokerTestFixture loggingBrokerTestFixture)
    {
        baseUrl = $"http://localhost:{loggingBrokerTestFixture.LoggingBrokerPort}";
        client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        this.loggingBrokerTestFixture = loggingBrokerTestFixture;
    }

    [Fact]
    public async Task Log_Should_Be_Queryable_When_ValidLogIsPosted()
    {
        // Arrange
        var logRequest = new
        {
            Source = "TestSource",
            LogLevel = "Information",
            Message = "HelloFromCombinedTest",
            Parameters = new Dictionary<string, string> { { "Key", "Value" } },
            EventUnixTimeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        // Act 1: POST
        var postResponse = await client.PostAsJsonAsync($"{baseUrl}/api/v1/logs", logRequest);
        postResponse.IsSuccessStatusCode.Should().BeTrue("Log POST isteği başarılı olmalı");

        // Act 2: GET → Polly ile retry
        var getUrl = $"{baseUrl}/api/v1/logs?contains=Hello";
        HttpResponseMessage? getResponse = null;

        // log sisteminde henüz indexlenmemişse, tekrar istek yapıyoruz. await Task.Delay ile pasif bekleme çözüm olmuyor.
        // Mikroservis mimarilerinde, dış hizmetlere yapılan çağrılar için (Loki) retry mekanizması kullanılmalıdır.
        var result = await Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .WaitAndRetryAsync(
                retryCount: 1,
                sleepDurationProvider: _ => TimeSpan.FromSeconds(1))
            .ExecuteAsync(async () =>
            {
                getResponse = await client.GetAsync(getUrl);
                return getResponse;
            });

        // Assert
        getResponse.Should().NotBeNull();
        getResponse!.IsSuccessStatusCode.Should().BeTrue("GET isteği logu başarılı sorgulamalı");

        var content = await getResponse.Content.ReadAsStringAsync();
        content.Should().Contain("Hello");
    }

    [Fact]
    public async Task InvalidLogRequest_Should_Return422_And_WriteToFallbackLogFile()
    {
        // Arrange
        var invalidLogRequest = new
        {
            Source = "",
            Message = ""
        };

        // Act
        var response = await client.PostAsJsonAsync($"{baseUrl}/api/v1/logs", invalidLogRequest);

        // Assert
        response.StatusCode.Should().Be((HttpStatusCode)422);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem.Should().NotBeNull();
        problem.Errors.Should().Contain(e => e.Property == "Source");
        problem.Errors.Should().Contain(e => e.Property == "Message");

        // Fallback log dosyasını kontrol et
        await Task.Delay(1000);

        var logFileName = $"fallback-{DateTime.UtcNow:yyyyMMdd}.log";
        var logPath = $"/app/logs/{logFileName}";

        var execResult = await loggingBrokerTestFixture.LoggingBrokerContainer.ExecAsync(["cat", logPath]);

        execResult.ExitCode.Should().Be(0, "fallback log dosyası okunabilir olmalı");

        execResult.Stdout.Should().Contain("ValidationException")
        .And.Contain("Source must not be empty.")
        .And.Contain("Message must not be empty.");
    }
}
