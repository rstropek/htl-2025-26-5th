using System.Net;
using System.Net.Http.Json;

namespace WebApiTests;

public class OrderIntegrationTests(WebApiTestFixture fixture) : IClassFixture<WebApiTestFixture>
{
    [Fact]
    public async Task PostValidOrder_ReturnsCreated()
    {
        var order = new
        {
            CustomerName = "Alice",
            CustomerAddress = "123 Main St",
            Bracelets = new[] { "A|pink|B|mint|C", "H|blue|I" }
        };

        var response = await fixture.HttpClient.PostAsJsonAsync("/api/orders", order);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<OrderDetailResponse>();
        Assert.NotNull(body);
        Assert.Equal("Alice", body.CustomerName);
        Assert.Equal(2, body.OrderItems.Count);
        // A|pink|B|mint|C = 3*1 + 2*0.5 = 4.0; H|blue|I = 2*1 + 1*0.5 = 2.5; total = 6.5
        Assert.Equal(6.5m, body.TotalCosts);
    }

    [Fact]
    public async Task PostInvalidBracelet_ReturnsBadRequest()
    {
        var order = new
        {
            CustomerName = "Bob",
            CustomerAddress = "456 Oak Ave",
            Bracelets = new[] { "A|invalid_color|B" }
        };

        var response = await fixture.HttpClient.PostAsJsonAsync("/api/orders", order);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetOrders_ReturnsList()
    {
        // Create an order first
        var order = new
        {
            CustomerName = "Charlie",
            CustomerAddress = "789 Elm St",
            Bracelets = new[] { "A" }
        };
        await fixture.HttpClient.PostAsJsonAsync("/api/orders", order);

        var response = await fixture.HttpClient.GetAsync("/api/orders");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var orders = await response.Content.ReadFromJsonAsync<List<OrderListResponse>>();
        Assert.NotNull(orders);
        Assert.True(orders.Count >= 1);
    }

    [Fact]
    public async Task GetOrderDetail_ReturnsOrder()
    {
        var order = new
        {
            CustomerName = "Diana",
            CustomerAddress = "101 Pine Rd",
            Bracelets = new[] { "X|sand|Y" }
        };
        var createResponse = await fixture.HttpClient.PostAsJsonAsync("/api/orders", order);
        var created = await createResponse.Content.ReadFromJsonAsync<OrderDetailResponse>();

        var response = await fixture.HttpClient.GetAsync($"/api/orders/{created!.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var detail = await response.Content.ReadFromJsonAsync<OrderDetailResponse>();
        Assert.NotNull(detail);
        Assert.Equal("Diana", detail.CustomerName);
        Assert.Single(detail.OrderItems);
        Assert.Equal("X|sand|Y", detail.OrderItems[0].BraceletData);
    }

    [Fact]
    public async Task PostOrder_MissingCustomerName_ReturnsBadRequest()
    {
        var order = new
        {
            CustomerName = "",
            CustomerAddress = "123 Main St",
            Bracelets = new[] { "A" }
        };

        var response = await fixture.HttpClient.PostAsJsonAsync("/api/orders", order);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostOrder_MissingCustomerAddress_ReturnsBadRequest()
    {
        var order = new
        {
            CustomerName = "Eve",
            CustomerAddress = "",
            Bracelets = new[] { "A" }
        };

        var response = await fixture.HttpClient.PostAsJsonAsync("/api/orders", order);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostOrder_EmptyBracelets_ReturnsBadRequest()
    {
        var order = new
        {
            CustomerName = "Frank",
            CustomerAddress = "999 Maple Dr",
            Bracelets = Array.Empty<string>()
        };

        var response = await fixture.HttpClient.PostAsJsonAsync("/api/orders", order);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetOrderDetail_NonExistentId_ReturnsNotFound()
    {
        var response = await fixture.HttpClient.GetAsync("/api/orders/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetOrders_MinTotalCostsFilter_FiltersResults()
    {
        // Create a cheap order (1 EUR)
        await fixture.HttpClient.PostAsJsonAsync("/api/orders", new
        {
            CustomerName = "Grace",
            CustomerAddress = "10 Low St",
            Bracelets = new[] { "A" }
        });

        // Create an expensive order (4 EUR)
        await fixture.HttpClient.PostAsJsonAsync("/api/orders", new
        {
            CustomerName = "Hank",
            CustomerAddress = "20 High St",
            Bracelets = new[] { "A|pink|B|pink|C|pink|D" }
        });

        var response = await fixture.HttpClient.GetAsync("/api/orders?minTotalCosts=3");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var orders = await response.Content.ReadFromJsonAsync<List<OrderListResponse>>();
        Assert.NotNull(orders);
        Assert.All(orders, o => Assert.True(o.TotalCosts >= 3));
    }

    private sealed record OrderListResponse(int Id, string CustomerName, DateTime OrderDate, decimal TotalCosts);
    private sealed record OrderDetailResponse(int Id, string CustomerName, string CustomerAddress, DateTime OrderDate, decimal TotalCosts, List<OrderItemResponse> OrderItems);
    private sealed record OrderItemResponse(int Id, string BraceletData, decimal Costs);
}
