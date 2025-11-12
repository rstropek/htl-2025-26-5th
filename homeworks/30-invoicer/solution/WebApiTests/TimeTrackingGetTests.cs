using System.Net;
using System.Net.Http.Json;

namespace WebApiTests;

public class TimeTrackingGetTests(WebApiTestFixture fixture) : IClassFixture<WebApiTestFixture>
{
    [Fact]
    public async Task GetEmployees_ReturnsOk()
    {
        // Act
        var response = await fixture.HttpClient.GetAsync("/employees");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetProjects_ReturnsOk()
    {
        // Act
        var response = await fixture.HttpClient.GetAsync("/projects");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetTimeEntries_ReturnsOk()
    {
        // Act
        var response = await fixture.HttpClient.GetAsync("/timeentries");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetTimeEntries_WithEmployeeIdFilter_ReturnsOk()
    {
        // Act
        var response = await fixture.HttpClient.GetAsync("/timeentries?employeeId=1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetTimeEntries_WithProjectIdFilter_ReturnsOk()
    {
        // Act
        var response = await fixture.HttpClient.GetAsync("/timeentries?projectId=1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetTimeEntries_WithBothFilters_ReturnsOk()
    {
        // Act
        var response = await fixture.HttpClient.GetAsync("/timeentries?employeeId=1&projectId=1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
