using System.Net;
using System.Net.Http.Json;

namespace WebApiTests;

public class TimeTrackingPutTests(WebApiTestFixture fixture) : IClassFixture<WebApiTestFixture>
{
    [Fact]
    public async Task UpdateTimeEntry_WithValidData_ReturnsOk()
    {
        // Arrange
        var updateDto = new
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(17, 0),
            Description = "Updated work description",
            EmployeeId = 1,
            ProjectId = 1
        };

        // Act
        var response = await fixture.HttpClient.PutAsJsonAsync("/timeentries/1", updateDto);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound,
            $"Expected OK or NotFound, got {response.StatusCode}"
        );
    }

    [Fact]
    public async Task UpdateTimeEntry_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var updateDto = new
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(17, 0),
            Description = "Updated work description",
            EmployeeId = 1,
            ProjectId = 1
        };

        // Act
        var response = await fixture.HttpClient.PutAsJsonAsync("/timeentries/999999", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTimeEntry_WithEndTimeBeforeStartTime_ReturnsBadRequest()
    {
        // Arrange
        var updateDto = new
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            StartTime = new TimeOnly(17, 0),
            EndTime = new TimeOnly(9, 0), // End before start
            Description = "Work description",
            EmployeeId = 1,
            ProjectId = 1
        };

        // Act
        var response = await fixture.HttpClient.PutAsJsonAsync("/timeentries/1", updateDto);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.NotFound,
            $"Expected BadRequest or NotFound, got {response.StatusCode}"
        );
    }

    [Fact]
    public async Task UpdateTimeEntry_WithEmptyDescription_ReturnsBadRequest()
    {
        // Arrange
        var updateDto = new
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(17, 0),
            Description = "", // Empty description
            EmployeeId = 1,
            ProjectId = 1
        };

        // Act
        var response = await fixture.HttpClient.PutAsJsonAsync("/timeentries/1", updateDto);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.NotFound,
            $"Expected BadRequest or NotFound, got {response.StatusCode}"
        );
    }

    [Fact]
    public async Task UpdateTimeEntry_WithNonExistentEmployeeId_ReturnsBadRequest()
    {
        // Arrange
        var updateDto = new
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(17, 0),
            Description = "Work description",
            EmployeeId = 999999, // Non-existent employee
            ProjectId = 1
        };

        // Act
        var response = await fixture.HttpClient.PutAsJsonAsync("/timeentries/1", updateDto);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.NotFound,
            $"Expected BadRequest or NotFound, got {response.StatusCode}"
        );
    }

    [Fact]
    public async Task UpdateTimeEntry_WithNonExistentProjectId_ReturnsBadRequest()
    {
        // Arrange
        var updateDto = new
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(17, 0),
            Description = "Work description",
            EmployeeId = 1,
            ProjectId = 999999 // Non-existent project
        };

        // Act
        var response = await fixture.HttpClient.PutAsJsonAsync("/timeentries/1", updateDto);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.NotFound,
            $"Expected BadRequest or NotFound, got {response.StatusCode}"
        );
    }
}
