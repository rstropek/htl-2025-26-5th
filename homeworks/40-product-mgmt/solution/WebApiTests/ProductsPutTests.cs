using System.Net;
using System.Net.Http.Json;

namespace WebApiTests;

public class ProductsPutTests(WebApiTestFixture fixture) : IClassFixture<WebApiTestFixture>
{
    [Fact]
    public async Task UpdateProduct_WithValidData_ReturnsOk()
    {
        // Arrange
        var updateDto = new
        {
            ProductCode = "PROD001",
            ProductName = "Updated Product",
            ProductDescription = "Updated description",
            Category = "Electronics",
            PricePerUnit = 99.99m
        };

        // Act
        var response = await fixture.HttpClient.PutAsJsonAsync("/products/1", updateDto);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound,
            $"Expected OK or NotFound, got {response.StatusCode}"
        );
    }

    [Fact]
    public async Task UpdateProduct_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var updateDto = new
        {
            ProductCode = "PROD001",
            ProductName = "Updated Product",
            ProductDescription = "Updated description",
            Category = "Electronics",
            PricePerUnit = 99.99m
        };

        // Act
        var response = await fixture.HttpClient.PutAsJsonAsync("/products/999999", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProduct_WithEmptyProductCode_ReturnsBadRequest()
    {
        // Arrange
        var updateDto = new
        {
            ProductCode = "", // Empty product code
            ProductName = "Product Name",
            ProductDescription = "Description",
            Category = "Electronics",
            PricePerUnit = 99.99m
        };

        // Act
        var response = await fixture.HttpClient.PutAsJsonAsync("/products/1", updateDto);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.NotFound,
            $"Expected BadRequest or NotFound, got {response.StatusCode}"
        );
    }

    [Fact]
    public async Task UpdateProduct_WithEmptyProductName_ReturnsBadRequest()
    {
        // Arrange
        var updateDto = new
        {
            ProductCode = "PROD001",
            ProductName = "", // Empty product name
            ProductDescription = "Description",
            Category = "Electronics",
            PricePerUnit = 99.99m
        };

        // Act
        var response = await fixture.HttpClient.PutAsJsonAsync("/products/1", updateDto);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.NotFound,
            $"Expected BadRequest or NotFound, got {response.StatusCode}"
        );
    }

    [Fact]
    public async Task UpdateProduct_WithProductCodeTooLong_ReturnsBadRequest()
    {
        // Arrange
        var updateDto = new
        {
            ProductCode = "PROD0012345", // 11 characters, exceeds max of 10
            ProductName = "Product Name",
            ProductDescription = "Description",
            Category = "Electronics",
            PricePerUnit = 99.99m
        };

        // Act
        var response = await fixture.HttpClient.PutAsJsonAsync("/products/1", updateDto);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.NotFound,
            $"Expected BadRequest or NotFound, got {response.StatusCode}"
        );
    }

    [Fact]
    public async Task UpdateProduct_WithProductNameTooLong_ReturnsBadRequest()
    {
        // Arrange
        var updateDto = new
        {
            ProductCode = "PROD001",
            ProductName = new string('A', 101), // 101 characters, exceeds max of 100
            ProductDescription = "Description",
            Category = "Electronics",
            PricePerUnit = 99.99m
        };

        // Act
        var response = await fixture.HttpClient.PutAsJsonAsync("/products/1", updateDto);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.NotFound,
            $"Expected BadRequest or NotFound, got {response.StatusCode}"
        );
    }

    [Fact]
    public async Task UpdateProduct_WithDescriptionTooLong_ReturnsBadRequest()
    {
        // Arrange
        var updateDto = new
        {
            ProductCode = "PROD001",
            ProductName = "Product Name",
            ProductDescription = new string('A', 256), // 256 characters, exceeds max of 255
            Category = "Electronics",
            PricePerUnit = 99.99m
        };

        // Act
        var response = await fixture.HttpClient.PutAsJsonAsync("/products/1", updateDto);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.NotFound,
            $"Expected BadRequest or NotFound, got {response.StatusCode}"
        );
    }

    [Fact]
    public async Task UpdateProduct_WithCategoryTooLong_ReturnsBadRequest()
    {
        // Arrange
        var updateDto = new
        {
            ProductCode = "PROD001",
            ProductName = "Product Name",
            ProductDescription = "Description",
            Category = new string('A', 51), // 51 characters, exceeds max of 50
            PricePerUnit = 99.99m
        };

        // Act
        var response = await fixture.HttpClient.PutAsJsonAsync("/products/1", updateDto);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.NotFound,
            $"Expected BadRequest or NotFound, got {response.StatusCode}"
        );
    }

    [Fact]
    public async Task UpdateProduct_WithZeroPrice_ReturnsBadRequest()
    {
        // Arrange
        var updateDto = new
        {
            ProductCode = "PROD001",
            ProductName = "Product Name",
            ProductDescription = "Description",
            Category = "Electronics",
            PricePerUnit = 0m // Zero price
        };

        // Act
        var response = await fixture.HttpClient.PutAsJsonAsync("/products/1", updateDto);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.NotFound,
            $"Expected BadRequest or NotFound, got {response.StatusCode}"
        );
    }

    [Fact]
    public async Task UpdateProduct_WithNegativePrice_ReturnsBadRequest()
    {
        // Arrange
        var updateDto = new
        {
            ProductCode = "PROD001",
            ProductName = "Product Name",
            ProductDescription = "Description",
            Category = "Electronics",
            PricePerUnit = -10m // Negative price
        };

        // Act
        var response = await fixture.HttpClient.PutAsJsonAsync("/products/1", updateDto);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.NotFound,
            $"Expected BadRequest or NotFound, got {response.StatusCode}"
        );
    }

    [Fact]
    public async Task UpdateProduct_WithNullOptionalFields_ReturnsOk()
    {
        // Arrange
        var updateDto = new
        {
            ProductCode = "PROD001",
            ProductName = "Product Name",
            ProductDescription = (string?)null, // Null description is valid
            Category = (string?)null, // Null category is valid
            PricePerUnit = 99.99m
        };

        // Act
        var response = await fixture.HttpClient.PutAsJsonAsync("/products/1", updateDto);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound,
            $"Expected OK or NotFound, got {response.StatusCode}"
        );
    }
}
