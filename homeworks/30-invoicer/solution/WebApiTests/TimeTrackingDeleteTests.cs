using System.Net;

namespace WebApiTests;

public class TimeTrackingDeleteTests(WebApiTestFixture fixture) : IClassFixture<WebApiTestFixture>
{
    [Fact]
    public async Task DeleteTimeEntry_WithNonExistentId_ReturnsNotFound()
    {
        // Act
        var response = await fixture.HttpClient.DeleteAsync("/timeentries/999999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteTimeEntry_WithValidId_ReturnsNoContentOrNotFound()
    {
        // Act
        var response = await fixture.HttpClient.DeleteAsync("/timeentries/1");

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.NoContent || response.StatusCode == HttpStatusCode.NotFound,
            $"Expected NoContent or NotFound, got {response.StatusCode}"
        );
    }
}
