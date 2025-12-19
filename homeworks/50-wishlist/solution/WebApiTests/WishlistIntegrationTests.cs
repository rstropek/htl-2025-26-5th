using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace WebApiTests;

public class WishlistIntegrationTests(WebApiTestFixture fixture) : IClassFixture<WebApiTestFixture>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private const string WishlistName = "XMasAtStropeks";
    private const string ParentPin = "9JX7KM";
    private const string ChildPin = "TR4GQZ";

    [Fact]
    public async Task VerifyPin_WithParentPin_ReturnsParentRole()
    {
        var response = await fixture.HttpClient.PostAsJsonAsync($"/verify-pin/{WishlistName}", new VerifyPinRequestDto(ParentPin), JsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<VerifyPinResponseDto>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal("parent", body!.Role);
    }

    [Fact]
    public async Task GetGiftCategories_ReturnsCategories_WithoutPin()
    {
        var response = await fixture.HttpClient.GetAsync("/gift-categories");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var categories = await response.Content.ReadFromJsonAsync<List<string>>(JsonOptions);
        Assert.NotNull(categories);
        Assert.Contains("Books", categories!);
    }

    [Fact]
    public async Task GetGiftCategories_ReturnsCategoriesInAlphabeticalOrder()
    {
        var response = await fixture.HttpClient.GetAsync("/gift-categories");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var categories = await response.Content.ReadFromJsonAsync<List<string>>(JsonOptions);
        Assert.NotNull(categories);
        Assert.NotEmpty(categories!);
        
        // Verify categories are sorted alphabetically
        var sortedCategories = categories.OrderBy(c => c, StringComparer.Ordinal).ToList();
        Assert.Equal(sortedCategories, categories);
    }

    [Fact]
    public async Task VerifyPin_IsCaseInsensitive()
    {
        var response = await fixture.HttpClient.PostAsJsonAsync($"/verify-pin/{WishlistName}", new VerifyPinRequestDto(ParentPin.ToLowerInvariant()), JsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetItems_WithChildPin_IsForbidden()
    {
        var response = await fixture.HttpClient.PostAsJsonAsync($"/wishlist/{WishlistName}/items", new AuthRequestDto(ChildPin), JsonOptions);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AddItem_WithChildPin_AddsNewItem()
    {
        // Arrange
        var newItemName = $"Test Item {Guid.NewGuid():N}";

        // Act
        var addResponse = await fixture.HttpClient.PutAsJsonAsync(
            $"/wishlist/{WishlistName}/items",
            new AddItemRequestDto(ChildPin, newItemName, "Books"),
            JsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.Created, addResponse.StatusCode);
        var created = await addResponse.Content.ReadFromJsonAsync<AddItemResponseDto>(JsonOptions);
        Assert.NotNull(created);
        Assert.Equal(newItemName, created!.ItemName);
        Assert.Equal("Books", created.Category);
        Assert.False(created.Bought);

        var itemsResponse = await fixture.HttpClient.PostAsJsonAsync($"/wishlist/{WishlistName}/items", new AuthRequestDto(ParentPin), JsonOptions);
        Assert.Equal(HttpStatusCode.OK, itemsResponse.StatusCode);
        var items = await itemsResponse.Content.ReadFromJsonAsync<List<WishlistItemDto>>(JsonOptions);
        Assert.NotNull(items);
        Assert.Contains(items!, i => i.Id == created.Id && i.ItemName == newItemName);
    }

    [Fact]
    public async Task MarkAsBought_WithParentPin_MarksItemAsBought()
    {
        // Arrange: add an item first
        var newItemName = $"BuyMe {Guid.NewGuid():N}";
        var addResponse = await fixture.HttpClient.PutAsJsonAsync(
            $"/wishlist/{WishlistName}/items",
            new AddItemRequestDto(ParentPin, newItemName, "Toys"),
            JsonOptions);
        Assert.Equal(HttpStatusCode.Created, addResponse.StatusCode);
        var created = await addResponse.Content.ReadFromJsonAsync<AddItemResponseDto>(JsonOptions);
        Assert.NotNull(created);

        // Act
        var markResponse = await fixture.HttpClient.PostAsJsonAsync(
            $"/wishlist/{WishlistName}/items/{created!.Id}/mark-as-bought",
            new MarkAsBoughtRequestDto(ParentPin, true),
            JsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, markResponse.StatusCode);

        var itemsResponse = await fixture.HttpClient.PostAsJsonAsync($"/wishlist/{WishlistName}/items", new AuthRequestDto(ParentPin), JsonOptions);
        Assert.Equal(HttpStatusCode.OK, itemsResponse.StatusCode);
        var items = await itemsResponse.Content.ReadFromJsonAsync<List<WishlistItemDto>>(JsonOptions);
        Assert.NotNull(items);
        var updated = Assert.Single(items, i => i.Id == created.Id);
        Assert.True(updated.Bought);
    }

    [Fact]
    public async Task DeleteItem_WithParentPin_DeletesItem()
    {
        // Arrange: add an item first
        var newItemName = $"DeleteMe {Guid.NewGuid():N}";
        var addResponse = await fixture.HttpClient.PutAsJsonAsync(
            $"/wishlist/{WishlistName}/items",
            new AddItemRequestDto(ParentPin, newItemName, "Electronics"),
            JsonOptions);
        Assert.Equal(HttpStatusCode.Created, addResponse.StatusCode);
        var created = await addResponse.Content.ReadFromJsonAsync<AddItemResponseDto>(JsonOptions);
        Assert.NotNull(created);

        // Act
        var deleteResponse = await fixture.HttpClient.SendAsync(
            new HttpRequestMessage(HttpMethod.Delete, $"/wishlist/{WishlistName}/items/{created!.Id}")
            {
                Content = JsonContent.Create(new AuthRequestDto(ParentPin), options: JsonOptions)
            });

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var itemsResponse = await fixture.HttpClient.PostAsJsonAsync($"/wishlist/{WishlistName}/items", new AuthRequestDto(ParentPin), JsonOptions);
        Assert.Equal(HttpStatusCode.OK, itemsResponse.StatusCode);
        var items = await itemsResponse.Content.ReadFromJsonAsync<List<WishlistItemDto>>(JsonOptions);
        Assert.NotNull(items);
        Assert.DoesNotContain(items!, i => i.Id == created.Id);
    }
}

public record VerifyPinRequestDto(string Pin);
public record VerifyPinResponseDto(string Role);
public record AuthRequestDto(string Pin);
public record AddItemRequestDto(string Pin, string ItemName, string Category);
public record AddItemResponseDto(int Id, string ItemName, string Category, bool Bought);
public record WishlistItemDto(int Id, string ItemName, string Category, bool Bought);
public record MarkAsBoughtRequestDto(string Pin, bool Bought);

