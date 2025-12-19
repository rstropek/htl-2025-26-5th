using System.Net;
using System.Net.Http.Json;

namespace WebApiTests;

public class WishlistIntegrationTests(WebApiTestFixture fixture) : IClassFixture<WebApiTestFixture>
{
    [Fact]
    public async Task GetGiftCategories_ReturnsCategories_WithoutPin()
    {
        var response = await fixture.HttpClient.GetAsync("/gift-categories");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var categories = await response.Content.ReadFromJsonAsync<List<string>>();
        Assert.NotNull(categories);
        Assert.Contains("Books", categories!);
    }
}
