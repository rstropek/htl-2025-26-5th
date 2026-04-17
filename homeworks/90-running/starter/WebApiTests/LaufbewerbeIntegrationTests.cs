using Aspire.Hosting.Testing;
using System.Net.Http.Json;
using Xunit;

namespace WebApiTests;

public class LaufbewerbeIntegrationTests(WebApiTestFixture fixture) : IClassFixture<WebApiTestFixture>
{
    // Example record type for deserializing API responses — define your own as needed.
    private record LaufkategorieResponse(int Id, string Bezeichnung);

    /// <summary>
    /// Example integration test: verifies that GET /laufkategorien returns the seeded categories.
    /// Use this as a template for your own tests.
    /// </summary>
    [Fact]
    public async Task Get_Laufkategorien_ReturnsSeededData()
    {
        var client = fixture.App.CreateHttpClient("webapi");

        var kategorien = await client.GetFromJsonAsync<List<LaufkategorieResponse>>("/laufkategorien");

        Assert.NotNull(kategorien);
        Assert.True(kategorien.Count >= 4);
        Assert.Contains(kategorien, k => k.Bezeichnung == "Straßenlauf");
    }

    // TODO: Add at least 3 integration tests for the Laufbewerbe Web API endpoints.
    // Use the test above as a template. Define private record types for request/response DTOs as needed.
    //
    // Suggested test scenarios:
    // 1. Insert a Laufbewerb (POST), then list (GET) and verify it appears
    // 2. Insert a Laufbewerb, delete it (DELETE), then verify GET by id returns 404
    // 3. Insert a Laufbewerb, patch it (PATCH), then verify changes are persisted and other fields unchanged
}
