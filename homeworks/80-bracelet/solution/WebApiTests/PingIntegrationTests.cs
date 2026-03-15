using System.Net;
using System.Net.Http.Json;

namespace WebApiTests;

public class PingIntegrationTests(WebApiTestFixture fixture) : IClassFixture<WebApiTestFixture>
{
    [Fact]
    public async Task Ping_ReturnsPong()
    {
        var response = await fixture.HttpClient.GetAsync("/api/ping");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PingResultDto>();
        Assert.NotNull(body);
        Assert.Equal("pong", body.Message);
    }

    private sealed record PingResultDto(string Message, DateTime Timestamp);
}
