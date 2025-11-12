using AppServices;
using AppServices.Importer;

namespace ImporterTests;

public class TimesheetParserTests
{
    private readonly TimesheetParser _parser = new();
    private readonly List<Employee> _existingEmployees = [];
    private readonly List<Project> _existingProjects = [];

    [Fact]
    public void ParseCsv_Valid1_ReturnsTimeEntries()
    {
        var csvContent = """
            EMP-ID: 4711
            EMP-NAME: Rainer Stropek
            TIMESHEETS: 2025-10-01
            08:00;08:15;"Daily Standup Meeting";ADMIN
            08:15;09:00;"Importer Implementation";ACCOUNTING
            09:00;12:00;"Debugging CSS Bug";CRM
            13:00;17:00;"Importer Implementation";ACCOUNTING
            TIMESHEETS: 2025-10-02
            08:00;08:15;"Daily Standup Meeting";ADMIN
            08:15;12:00;"Open Tofu IaC Scripts";CRM
            12:45;16:30;"Importer Implementation";ACCOUNTING

            """;

        var result = _parser.ParseCsv(csvContent, _existingEmployees, _existingProjects);

        Assert.NotNull(result);
        Assert.Equal(7, result.Count());
    }

    [Fact]
    public void ParseCsv_Valid2_ReturnsTimeEntries()
    {
        var csvContent = """
            EMP-ID: 0815
            EMP-NAME: Max Mustermann
            TIMESHEETS: 2025-10-01
            08:00;08:30;"Team Coordination Call";ADMIN
            08:30;10:45;"Invoice Processing System";ACCOUNTING
            10:45;12:15;"Customer Portal Updates";CRM
            13:15;15:00;"Financial Report Review";ACCOUNTING
            15:00;17:30;"Client Dashboard Redesign";CRM
            TIMESHEETS: 2025-10-02
            08:00;08:20;"Sprint Planning Session";ADMIN
            08:20;11:30;"Payment Gateway Integration";ACCOUNTING
            11:30;12:00;"Documentation Updates";ADMIN
            13:00;16:45;"Contact Management Features";CRM

            """;

        var result = _parser.ParseCsv(csvContent, _existingEmployees, _existingProjects);

        Assert.NotNull(result);
        Assert.Equal(9, result.Count());
    }

    [Fact]
    public void ParseCsv_EmptyColumns_ThrowsEmptyFieldException()
    {
        var csvContent = """
            EMP-ID: 4711
            EMP-NAME: Rainer Stropek
            TIMESHEETS: 2025-10-01
            08:00;08:50;;

            """;

        var exception = Assert.Throws<TimesheetParseException>(() =>
            _parser.ParseCsv(csvContent, _existingEmployees, _existingProjects));

        Assert.Equal(ImportFileError.EmptyField, exception.ErrorCode);
    }

    [Fact]
    public void ParseCsv_InvalidDate1_ThrowsInvalidDateException()
    {
        var csvContent = """
            EMP-ID: 4711
            EMP-NAME: Rainer Stropek
            TIMESHEETS: 2025-13-01
            08:00;08:15;"Daily Standup Meeting";ADMIN

            """;

        var exception = Assert.Throws<TimesheetParseException>(() =>
            _parser.ParseCsv(csvContent, _existingEmployees, _existingProjects));

        Assert.Equal(ImportFileError.InvalidDate, exception.ErrorCode);
    }

    [Fact]
    public void ParseCsv_InvalidDate2_ThrowsInvalidDateException()
    {
        var csvContent = """
            EMP-ID: 4711
            EMP-NAME: Rainer Stropek
            TIMESHEETS: 2025-10-AB
            08:00;08:15;"Daily Standup Meeting";ADMIN

            """;

        var exception = Assert.Throws<TimesheetParseException>(() =>
            _parser.ParseCsv(csvContent, _existingEmployees, _existingProjects));

        Assert.Equal(ImportFileError.InvalidDate, exception.ErrorCode);
    }

    [Fact]
    public void ParseCsv_InvalidDate3_ThrowsInvalidDateException()
    {
        var csvContent = """
            EMP-ID: 4711
            EMP-NAME: Rainer Stropek
            TIMESHEETS: 2025-02-29
            08:00;08:15;"Daily Standup Meeting";ADMIN

            """;

        var exception = Assert.Throws<TimesheetParseException>(() =>
            _parser.ParseCsv(csvContent, _existingEmployees, _existingProjects));

        Assert.Equal(ImportFileError.InvalidDate, exception.ErrorCode);
    }

    [Fact]
    public void ParseCsv_InvalidTime1_ThrowsInvalidTimeException()
    {
        var csvContent = """
            EMP-ID: 4711
            EMP-NAME: Rainer Stropek
            TIMESHEETS: 2025-10-01
            08:00;08:15;"Daily Standup Meeting";ADMIN
            08:15;09:00;"Importer Implementation";ACCOUNTING
            09:00;12:00;"Debugging CSS Bug";CRM
            13:00;25:00;"Importer Implementation";ACCOUNTING

            """;

        var exception = Assert.Throws<TimesheetParseException>(() =>
            _parser.ParseCsv(csvContent, _existingEmployees, _existingProjects));

        Assert.Equal(ImportFileError.InvalidTime, exception.ErrorCode);
    }

    [Fact]
    public void ParseCsv_InvalidTime2_ThrowsInvalidTimeException()
    {
        var csvContent = """
            EMP-ID: 4711
            EMP-NAME: Rainer Stropek
            TIMESHEETS: 2025-10-01
            08:00;ABC;"Daily Standup Meeting";ADMIN

            """;

        var exception = Assert.Throws<TimesheetParseException>(() =>
            _parser.ParseCsv(csvContent, _existingEmployees, _existingProjects));

        Assert.Equal(ImportFileError.InvalidTime, exception.ErrorCode);
    }

    [Fact]
    public void ParseCsv_MissingColumns_ThrowsIncorrectFieldCountException()
    {
        var csvContent = """
            EMP-ID: 4711
            EMP-NAME: Rainer Stropek
            TIMESHEETS: 2025-10-01
            08:00;08:15;"Daily Standup Meeting"

            """;

        var exception = Assert.Throws<TimesheetParseException>(() =>
            _parser.ParseCsv(csvContent, _existingEmployees, _existingProjects));

        Assert.Equal(ImportFileError.IncorrectFieldCount, exception.ErrorCode);
    }

    [Fact]
    public void ParseCsv_MissingEmployee1_ThrowsMissingEmployeeIdException()
    {
        var csvContent = """
            TIMESHEETS: 2025-10-01
            08:00;08:15;"Daily Standup Meeting";ADMIN
            08:15;09:00;"Importer Implementation";ACCOUNTING
            09:00;12:00;"Debugging CSS Bug";CRM
            13:00;17:00;"Importer Implementation";ACCOUNTING

            """;

        var exception = Assert.Throws<TimesheetParseException>(() =>
            _parser.ParseCsv(csvContent, _existingEmployees, _existingProjects));

        Assert.Equal(ImportFileError.MissingEmployeeId, exception.ErrorCode);
    }

    [Fact]
    public void ParseCsv_MissingEmployee2_ThrowsMissingEmployeeIdException()
    {
        var csvContent = """
            EMP-NAME: Rainer Stropek
            TIMESHEETS: 2025-10-01
            08:00;08:15;"Daily Standup Meeting";ADMIN
            08:15;09:00;"Importer Implementation";ACCOUNTING
            09:00;12:00;"Debugging CSS Bug";CRM
            13:00;17:00;"Importer Implementation";ACCOUNTING
            TIMESHEETS: 2025-10-02
            08:00;08:15;"Daily Standup Meeting";ADMIN
            08:15;12:00;"Open Tofu IaC Scripts";CRM
            12:45;16:30;"Importer Implementation";ACCOUNTING

            """;

        var exception = Assert.Throws<TimesheetParseException>(() =>
            _parser.ParseCsv(csvContent, _existingEmployees, _existingProjects));

        Assert.Equal(ImportFileError.MissingEmployeeId, exception.ErrorCode);
    }

    [Fact]
    public void ParseCsv_MissingQuotes_ThrowsDescriptionNotQuotedException()
    {
        var csvContent = """
            EMP-ID: 4711
            EMP-NAME: Rainer Stropek
            TIMESHEETS: 2025-10-01
            08:00;08:15;Daily Standup Meeting;ADMIN

            """;

        var exception = Assert.Throws<TimesheetParseException>(() =>
            _parser.ParseCsv(csvContent, _existingEmployees, _existingProjects));

        Assert.Equal(ImportFileError.DescriptionNotQuoted, exception.ErrorCode);
    }

    [Fact]
    public void ParseCsv_MissingTimesheets1_ThrowsMissingTimesheetSectionException()
    {
        var csvContent = """
            EMP-ID: 4711
            EMP-NAME: Rainer Stropek

            """;

        var exception = Assert.Throws<TimesheetParseException>(() =>
            _parser.ParseCsv(csvContent, _existingEmployees, _existingProjects));

        Assert.Equal(ImportFileError.MissingTimesheetSection, exception.ErrorCode);
    }

    [Fact]
    public void ParseCsv_MissingTimesheets2_ThrowsEmptyTimesheetSectionException()
    {
        var csvContent = """
            EMP-ID: 4711
            EMP-NAME: Rainer Stropek
            TIMESHEETS: 2025-10-01

            """;

        var exception = Assert.Throws<TimesheetParseException>(() =>
            _parser.ParseCsv(csvContent, _existingEmployees, _existingProjects));

        Assert.Equal(ImportFileError.EmptyTimesheetSection, exception.ErrorCode);
    }

    [Fact]
    public void ParseCsv_UnknownKey1_ThrowsUnknownKeyException()
    {
        var csvContent = """
            EMP-ID: 4711
            EMP-NAME: Rainer Stropek
            DEPARTMENT: Research and Development
            TIMESHEETS: 2025-10-01
            08:00;08:15;"Daily Standup Meeting";ADMIN
            08:15;09:00;"Importer Implementation";ACCOUNTING
            09:00;12:00;"Debugging CSS Bug";CRM
            13:00;17:00;"Importer Implementation";ACCOUNTING
            TIMESHEETS: 2025-10-02
            08:00;08:15;"Daily Standup Meeting";ADMIN
            08:15;12:00;"Open Tofu IaC Scripts";CRM
            12:45;16:30;"Importer Implementation";ACCOUNTING

            """;

        var exception = Assert.Throws<TimesheetParseException>(() =>
            _parser.ParseCsv(csvContent, _existingEmployees, _existingProjects));

        Assert.Equal(ImportFileError.UnknownKey, exception.ErrorCode);
    }

    [Fact]
    public void ParseCsv_UnknownKey2_ThrowsUnknownKeyException()
    {
        var csvContent = """
            EMP-ID: 4711
            EMP-NAME: Rainer Stropek
            DEPARTMENT: Research and Development
            TIMESHEETS: 2025-10-01
            08:00;08:15;"Daily Standup Meeting";ADMIN
            08:15;09:00;"Importer Implementation";ACCOUNTING
            09:00;12:00;"Debugging CSS Bug";CRM
            13:00;17:00;"Importer Implementation";ACCOUNTING
            TIMESHEET: 2025-10-02
            08:00;08:15;"Daily Standup Meeting";ADMIN
            08:15;12:00;"Open Tofu IaC Scripts";CRM
            12:45;16:30;"Importer Implementation";ACCOUNTING

            """;

        var exception = Assert.Throws<TimesheetParseException>(() =>
            _parser.ParseCsv(csvContent, _existingEmployees, _existingProjects));

        Assert.Equal(ImportFileError.UnknownKey, exception.ErrorCode);
    }

    [Fact]
    public void ParseCsv_ExistingEmployee_UpdatesEmployeeName()
    {
        // Arrange: Add an existing employee with a different name
        var existingEmployee = new Employee
        {
            Id = 1,
            EmplyeeId = "4711",
            EmployeeName = "Old Name"
        };
        var existingEmployees = new List<Employee> { existingEmployee };

        var csvContent = """
            EMP-ID: 4711
            EMP-NAME: Rainer Stropek
            TIMESHEETS: 2025-10-01
            08:00;08:15;"Daily Standup Meeting";ADMIN

            """;

        // Act
        var result = _parser.ParseCsv(csvContent, existingEmployees, _existingProjects);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        var timeEntry = result.First();
        Assert.Equal(existingEmployee, timeEntry.Employee);
        Assert.Equal("Rainer Stropek", existingEmployee.EmployeeName);
    }

    [Fact]
    public void ParseCsv_SameProjectMultipleTimes_ReferencesSameProjectObject()
    {
        // Arrange: CSV content with multiple time entries using the same project code
        var csvContent = """
            EMP-ID: 4711
            EMP-NAME: Rainer Stropek
            TIMESHEETS: 2025-10-01
            08:00;09:00;"Task 1";ACCOUNTING
            09:00;10:00;"Task 2";CRM
            10:00;11:00;"Task 3";ACCOUNTING
            TIMESHEETS: 2025-10-02
            08:00;09:00;"Task 4";ACCOUNTING
            09:00;10:00;"Task 5";CRM

            """;

        // Act
        var result = _parser.ParseCsv(csvContent, _existingEmployees, _existingProjects);

        // Assert
        Assert.NotNull(result);
        var timeEntries = result.ToList();
        Assert.Equal(5, timeEntries.Count);

        // Get all time entries for ACCOUNTING project
        var accountingEntries = timeEntries.Where(te => te.Project?.ProjectCode == "ACCOUNTING").ToList();
        Assert.Equal(3, accountingEntries.Count);

        // Verify all ACCOUNTING entries reference the same Project object
        var firstAccountingProject = accountingEntries[0].Project;
        Assert.NotNull(firstAccountingProject);
        Assert.All(accountingEntries, te => Assert.Same(firstAccountingProject, te.Project));

        // Get all time entries for CRM project
        var crmEntries = timeEntries.Where(te => te.Project?.ProjectCode == "CRM").ToList();
        Assert.Equal(2, crmEntries.Count);

        // Verify all CRM entries reference the same Project object
        var firstCrmProject = crmEntries[0].Project;
        Assert.NotNull(firstCrmProject);
        Assert.All(crmEntries, te => Assert.Same(firstCrmProject, te.Project));

        // Verify ACCOUNTING and CRM are different objects
        Assert.NotSame(firstAccountingProject, firstCrmProject);
    }
}
