using System.Net;
using System.Net.Http.Json;

namespace WebApiTests;

public class ValidateBraceletIntegrationTests(WebApiTestFixture fixture) : IClassFixture<WebApiTestFixture>
{
    [Fact]
    public async Task ValidBracelet_ReturnsNoCostAndNoError()
    {
        var response = await fixture.HttpClient.PostAsJsonAsync("/api/validate-bracelet", new { BraceletData = "A|pink|B|mint|C" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ValidationResponse>();
        Assert.NotNull(result);
        Assert.Null(result.Error);
        // A|pink|B|mint|C = 3*1 + 2*0.5 = 4.0
        Assert.Equal(4.0m, result.Cost);
    }

    [Fact]
    public async Task ValidBracelet_SingleLetter_ReturnsCostOne()
    {
        var response = await fixture.HttpClient.PostAsJsonAsync("/api/validate-bracelet", new { BraceletData = "A" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ValidationResponse>();
        Assert.NotNull(result);
        Assert.Null(result.Error);
        Assert.Equal(1.0m, result.Cost);
    }

    [Fact]
    public async Task InvalidBracelet_ReturnsErrorAndNullCost()
    {
        var response = await fixture.HttpClient.PostAsJsonAsync("/api/validate-bracelet", new { BraceletData = "A|invalid_color|B" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ValidationResponse>();
        Assert.NotNull(result);
        Assert.NotNull(result.Error);
        Assert.Null(result.Cost);
    }

    [Fact]
    public async Task EmptyBracelet_ReturnsErrorAndNullCost()
    {
        var response = await fixture.HttpClient.PostAsJsonAsync("/api/validate-bracelet", new { BraceletData = "" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ValidationResponse>();
        Assert.NotNull(result);
        Assert.NotNull(result.Error);
        Assert.Null(result.Cost);
    }

    [Fact]
    public async Task MixedColors_ReturnsWarningAndCost()
    {
        var response = await fixture.HttpClient.PostAsJsonAsync("/api/validate-bracelet", new { BraceletData = "A|pink|B|mint|C" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ValidationResponse>();
        Assert.NotNull(result);
        Assert.Null(result.Error);
        Assert.True(result.MixedColorsWarning);
        Assert.NotNull(result.Cost);
    }

    [Fact]
    public async Task SameColors_ReturnsNoWarning()
    {
        var response = await fixture.HttpClient.PostAsJsonAsync("/api/validate-bracelet", new { BraceletData = "A|pink|B|pink|C" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ValidationResponse>();
        Assert.NotNull(result);
        Assert.Null(result.Error);
        Assert.False(result.MixedColorsWarning);
    }

    [Fact]
    public async Task BraceletEndingWithSpacer_ReturnsError()
    {
        var response = await fixture.HttpClient.PostAsJsonAsync("/api/validate-bracelet", new { BraceletData = "A|pink" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ValidationResponse>();
        Assert.NotNull(result);
        Assert.NotNull(result.Error);
        Assert.Null(result.Cost);
    }

    private sealed record ValidationResponse(string? Error, bool MixedColorsWarning, decimal? Cost);
}
