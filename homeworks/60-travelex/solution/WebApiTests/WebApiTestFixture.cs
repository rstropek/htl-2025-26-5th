using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace WebApiTests;

public class WebApiTestFixture : IAsyncLifetime
{
    public DistributedApplication App { get; private set; } = null!;
    public HttpClient HttpClient { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.AppHost>();

        builder.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        App = await builder.BuildAsync();
        await App.StartAsync();

        HttpClient = App.CreateHttpClient("webapi");

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        await App.ResourceNotifications.WaitForResourceHealthyAsync(
            "webapi",
            cts.Token);
    }

    public async Task DisposeAsync()
    {
        HttpClient?.Dispose();
        if (App != null)
        {
            await App.DisposeAsync();
        }
    }
}
