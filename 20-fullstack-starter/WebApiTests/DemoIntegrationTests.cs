using System.Net;

namespace WebApiTests;

public class DemoIntegrationTests(WebApiTestFixture fixture) : IClassFixture<WebApiTestFixture>
{
    [Fact]
    public async Task Ping_ReturnsPong()
    {
        // Act
        var response = await fixture.HttpClient.GetAsync("/ping");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal("pong", content);
    }
}