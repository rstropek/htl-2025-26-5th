using System.Net;

namespace WebApiTests;

public class ProductsDeleteTests(WebApiTestFixture fixture) : IClassFixture<WebApiTestFixture>
{
    [Fact]
    public async Task DeleteProduct_WithNonExistentId_ReturnsNotFound()
    {
        // Act
        var response = await fixture.HttpClient.DeleteAsync("/products/999999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteProduct_WithValidId_ReturnsNoContentOrNotFound()
    {
        // Act
        var response = await fixture.HttpClient.DeleteAsync("/products/1");

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.NoContent || response.StatusCode == HttpStatusCode.NotFound,
            $"Expected NoContent or NotFound, got {response.StatusCode}"
        );
    }
}
