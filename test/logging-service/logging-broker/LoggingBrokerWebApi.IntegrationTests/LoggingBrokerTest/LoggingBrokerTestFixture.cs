using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;

namespace LoggingBrokerWebApi.IntegrationTests.LoggingBrokerTest;

public class LoggingBrokerTestFixture : IAsyncLifetime
{
    public IContainer _lokiContainer = default!;
    public IContainer _loggingBrokerContainer = default!;
    private INetwork _testNetwork = default!;

    public IContainer LoggingBrokerContainer => _loggingBrokerContainer;

    public int LokiPort { get; private set; }
    public int LoggingBrokerPort { get; private set; }

    public async Task InitializeAsync()
    {
        _testNetwork = new NetworkBuilder()
             .WithName($"loki-broker-network-{Guid.NewGuid():N}")
             .Build();
        await _testNetwork.CreateAsync();

        _lokiContainer = new ContainerBuilder()
            .WithName("loki-test-container")
            .WithImage("grafana/loki:2.9.4")
            .WithPortBinding(3100, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(3100))
            .WithNetwork(_testNetwork)
            .WithNetworkAliases("loki")         // Logging broker "http://loki:3100" diyebilmesi için
            .Build();
        await _lokiContainer.StartAsync();

        // önemli: image oluşması için sln ve Dockerfile aynı dizinde olmalı!
        var dockerImage = new ImageFromDockerfileBuilder()
             .WithName("loggingbroker-test-image")
             .WithDockerfileDirectory(
              Path.GetFullPath("../../../../../../../src/logging-service/logging-broker"))
             .WithDockerfile("Dockerfile")
             .Build();

        //var dockerfilePath = FindSolutionRootWithDockerfile();
        //var dockerImage = new ImageFromDockerfileBuilder()
        //    .WithName("loggingbroker-test-image")
        //    .WithDockerfileDirectory(dockerfilePath)
        //    .WithDockerfile("Dockerfile")
        //    .Build();

        await dockerImage.CreateAsync().ConfigureAwait(false);

        _loggingBrokerContainer = new ContainerBuilder()
            .WithName("logging-broker-test-container")
            .WithImage(dockerImage.FullName)
            .WithPortBinding(8080, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(8080))
            .WithNetwork(_testNetwork)
            .WithEnvironment("LokiUrl", "http://loki:3100")
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
            .WithEnvironment("RateLimiterSettings__PermitLimit", "100")
            .WithEnvironment("RateLimiterSettings__WindowTime", "15")
            .WithEnvironment("RateLimiterSettings__QueueLimit", "0")
            .Build();

        await _loggingBrokerContainer.StartAsync();

        // Host üzerinde random maplenen portları öğrenelim
        LokiPort = _lokiContainer.GetMappedPublicPort(3100);
        LoggingBrokerPort = _loggingBrokerContainer.GetMappedPublicPort(8080);
    }

    public async Task DisposeAsync()
    {
        await _loggingBrokerContainer.DisposeAsync();
        await _lokiContainer.DisposeAsync();
        await _testNetwork.DeleteAsync();
    }

    public static string FindSolutionRootWithDockerfile()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "Dockerfile")))
            {
                return dir.FullName;
            }
            dir = dir.Parent;
        }
        throw new FileNotFoundException("Dockerfile not found in parent directories.");
    }

}


