using System.Net;
using System.Net.Http.Json;

namespace WebApiTests;

public class ProductsGetTests(WebApiTestFixture fixture) : IClassFixture<WebApiTestFixture>
{
    [Fact]
    public async Task GetCategories_ReturnsOk()
    {
        // Act
        var response = await fixture.HttpClient.GetAsync("/categories");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetProducts_ReturnsOk()
    {
        // Act
        var response = await fixture.HttpClient.GetAsync("/products");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetProducts_WithCategoryFilter_ReturnsOk()
    {
        // Act
        var response = await fixture.HttpClient.GetAsync("/products?category=Electronics");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetProducts_WithMaxUnitPriceFilter_ReturnsOk()
    {
        // Act
        var response = await fixture.HttpClient.GetAsync("/products?maxUnitPrice=100");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetProducts_WithBothFilters_ReturnsOk()
    {
        // Act
        var response = await fixture.HttpClient.GetAsync("/products?category=Electronics&maxUnitPrice=100");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetProductById_WithNonExistentId_ReturnsNotFound()
    {
        // Act
        var response = await fixture.HttpClient.GetAsync("/products/999999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetProductById_WithValidId_ReturnsOkOrNotFound()
    {
        // Act
        var response = await fixture.HttpClient.GetAsync("/products/1");

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound,
            $"Expected OK or NotFound, got {response.StatusCode}"
        );
    }
}
