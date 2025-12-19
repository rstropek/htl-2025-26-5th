using AppServices.Importer;

namespace ImporterTests;

public class WishlistJsonParserTests
{
    private readonly WishlistJsonParser parser = new();

    [Fact]
    public void ParseJson_ValidJson_ReturnsDto()
    {
        // Arrange
        var json = """
                   {
                     "wishlist": {
                       "name": "Hubers",
                       "parentPin": "ABCDEF",
                       "childPin": "GHIJKL"
                     },
                     "items": [
                       { "itemName": "Lego", "category": "Toys", "bought": false },
                       { "itemName": "Book", "category": "Books", "bought": true }
                     ]
                   }
                   """;

        // Act
        var dto = parser.ParseJson("file.json", json);

        // Assert
        Assert.Equal("Hubers", dto.Wishlist.Name);
        Assert.Equal("ABCDEF", dto.Wishlist.ParentPin);
        Assert.Equal("GHIJKL", dto.Wishlist.ChildPin);
        Assert.Equal(2, dto.Items.Count);
        Assert.Equal("Lego", dto.Items[0].ItemName);
        Assert.Equal("Toys", dto.Items[0].Category);
        Assert.False(dto.Items[0].Bought);
        Assert.True(dto.Items[1].Bought);
    }

    [Fact]
    public void ParseJson_InvalidJson_ThrowsWishlistParseExceptionWithInnerException()
    {
        // Arrange
        var invalidJson = "{ this is not json }";

        // Act
        var ex = Assert.Throws<WishlistParseException>(() => parser.ParseJson("broken.json", invalidJson));

        // Assert
        Assert.Contains("broken.json", ex.Message);
        Assert.NotNull(ex.InnerException);
    }

    [Fact]
    public void ParseJson_JsonNullResult_ThrowsWishlistParseException()
    {
        // Arrange
        var jsonNull = "null";

        // Act
        var ex = Assert.Throws<WishlistParseException>(() => parser.ParseJson("null.json", jsonNull));

        // Assert
        Assert.Contains("null.json", ex.Message);
    }
}

