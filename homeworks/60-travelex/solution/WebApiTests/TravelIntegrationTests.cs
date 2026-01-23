using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace WebApiTests;

public class TravelIntegrationTests(WebApiTestFixture fixture) : IClassFixture<WebApiTestFixture>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static string StrictUtcTimestamp(DateTimeOffset value)
        => value.ToUniversalTime().ToString("yyyy-MM-dd'T'HH':'mm':'ss'Z'", CultureInfo.InvariantCulture);

    [Fact]
    public async Task Upload_Then_List_ContainsCreatedTravel()
    {
        // Arrange
        var start = new DateTimeOffset(2026, 01, 20, 8, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2026, 01, 20, 18, 0, 0, TimeSpan.Zero);
        var header = $"{StrictUtcTimestamp(start)}|{StrictUtcTimestamp(end)}|Ada Lovelace|Conference";
        var fileText = string.Join("\n", new[]
        {
            header,
            "DRIVE|10|To customer",
            "EXPENSE|120|Hotel",
        });

        using var form = new MultipartFormDataContent();
        var bytes = Encoding.UTF8.GetBytes(fileText);
        var fileContent = new ByteArrayContent(bytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        form.Add(fileContent, "file", "travel.txt");

        // Act (upload)
        var uploadResponse = await fixture.HttpClient.PostAsync("/travels/upload", form);

        // Assert (upload)
        Assert.Equal(HttpStatusCode.Created, uploadResponse.StatusCode);
        var created = await uploadResponse.Content.ReadFromJsonAsync<TravelDetailsDto>(JsonOptions);
        Assert.NotNull(created);
        Assert.True(created!.Id > 0);
        Assert.Equal("Ada Lovelace", created.TravelerName);
        Assert.Equal("Conference", created.Purpose);
        Assert.Equal(2, created.Reimbursements.Count);

        // Act (list)
        var listResponse = await fixture.HttpClient.GetAsync("/travels");

        // Assert (list)
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var list = await listResponse.Content.ReadFromJsonAsync<List<TravelListItemDto>>(JsonOptions);
        Assert.NotNull(list);
        Assert.Contains(list!, t => t.Id == created.Id && t.TravelerName == "Ada Lovelace" && t.Purpose == "Conference");
    }

    [Fact]
    public async Task GetById_ReturnsDetailsAndReimbursementTotals()
    {
        // Arrange: create a travel via upload
        var start = new DateTimeOffset(2026, 01, 20, 8, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2026, 01, 20, 18, 0, 0, TimeSpan.Zero);
        var header = $"{StrictUtcTimestamp(start)}|{StrictUtcTimestamp(end)}|John Doe|Trip";
        var fileText = string.Join("\n", new[]
        {
            header,
            "DRIVE|75|To airport",
            "EXPENSE|120|Hotel",
        });

        using var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(fileText));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        form.Add(fileContent, "file", "travel.txt");

        var uploadResponse = await fixture.HttpClient.PostAsync("/travels/upload", form);
        Assert.Equal(HttpStatusCode.Created, uploadResponse.StatusCode);
        var created = await uploadResponse.Content.ReadFromJsonAsync<TravelDetailsDto>(JsonOptions);
        Assert.NotNull(created);

        // Act
        var detailsResponse = await fixture.HttpClient.GetAsync($"/travels/{created!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, detailsResponse.StatusCode);
        var details = await detailsResponse.Content.ReadFromJsonAsync<TravelDetailsDto>(JsonOptions);
        Assert.NotNull(details);

        Assert.Equal(created.Id, details!.Id);
        Assert.Equal(start, details.Start);
        Assert.Equal(end, details.End);
        Assert.Equal("John Doe", details.TravelerName);
        Assert.Equal("Trip", details.Purpose);

        // mileage: 75 km * 0.50
        Assert.Equal(37.5m, details.Mileage);
        // per diem: 10 hours => ceil(10) * 2.50 = 25
        Assert.Equal(25m, details.PerDiem);
        // expenses: mileage claimed => 0
        Assert.Equal(0m, details.Expenses);

        Assert.Contains(details.Reimbursements, r => r.Type == "DRIVE" && r.Km == 75 && r.Description == "To airport");
        Assert.Contains(details.Reimbursements, r => r.Type == "EXPENSE" && r.Amount == 120 && r.Description == "Hotel");
    }

    [Fact]
    public async Task GetById_Unknown_Returns404()
    {
        var response = await fixture.HttpClient.GetAsync($"/travels/{int.MaxValue}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

public record TravelListItemDto(int Id, string TravelerName, string Purpose);

public record TravelReimbursementDto(int Id, string Type, string Description, int? Km, int? Amount);

public record TravelDetailsDto(
    int Id,
    DateTimeOffset Start,
    DateTimeOffset End,
    string TravelerName,
    string Purpose,
    decimal Mileage,
    decimal PerDiem,
    decimal Expenses,
    List<TravelReimbursementDto> Reimbursements);
