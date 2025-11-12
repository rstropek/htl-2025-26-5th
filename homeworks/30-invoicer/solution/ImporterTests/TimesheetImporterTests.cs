using AppServices;
using AppServices.Importer;

namespace ImporterTests;

public class TimesheetImporterTests
{
    private readonly IFileReader fileReader;
    private readonly ITimesheetParser csvParser;
    private readonly ITimesheetImportDatabaseReaderWriter databaseWriter;
    private readonly TimesheetImporter importer;

    public TimesheetImporterTests()
    {
        fileReader = Substitute.For<IFileReader>();
        csvParser = Substitute.For<ITimesheetParser>();
        databaseWriter = Substitute.For<ITimesheetImportDatabaseReaderWriter>();
        importer = new TimesheetImporter(fileReader, csvParser, databaseWriter);
    }

    [Fact]
    public async Task ImportTimesheetsAsync_SuccessfulImport_ReturnsCount()
    {
        // Arrange
        var csvFilePath = "timesheet.txt";
        var csvContent = "EMP-ID: 12345\nEMP-NAME: John Doe\nTIMESHEETS: 2024-01-15\n08:00;12:00;\"Meeting\";PROJ1";
        var existingEmployees = new List<Employee>();
        var existingProjects = new List<Project>();
        var timeEntries = new List<TimeEntry>
        {
            new()
            {
                Date = new DateOnly(2024, 1, 15),
                StartTime = new TimeOnly(8, 0),
                EndTime = new TimeOnly(12, 0),
                Description = "Meeting",
                Employee = new Employee { EmplyeeId = "12345", EmployeeName = "John Doe" },
                Project = new Project { ProjectCode = "PROJ1" }
            }
        };

        fileReader.ReadAllTextAsync(csvFilePath).Returns(Task.FromResult(csvContent));
        databaseWriter.GetAllEmployeesAsync().Returns(Task.FromResult<IEnumerable<Employee>>(existingEmployees));
        databaseWriter.GetAllProjectsAsync().Returns(Task.FromResult<IEnumerable<Project>>(existingProjects));
        csvParser.ParseCsv(csvContent, existingEmployees, existingProjects).Returns(timeEntries);

        // Act
        var result = await importer.ImportTimesheetsAsync(csvFilePath, isDryRun: false);

        // Assert
        Assert.Equal(1, result);
        await databaseWriter.Received(1).BeginTransactionAsync();
        await databaseWriter.Received(1).GetAllEmployeesAsync();
        await databaseWriter.Received(1).GetAllProjectsAsync();
        await databaseWriter.Received(1).ClearDayAsync("12345", new DateOnly(2024, 1, 15));
        await databaseWriter.Received(1).WriteTimeEntriesAsync(Arg.Is<IEnumerable<TimeEntry>>(e => e.Count() == 1));
        await databaseWriter.Received(1).CommitTransactionAsync();
        await databaseWriter.DidNotReceive().RollbackTransactionAsync();
    }

    [Fact]
    public async Task ImportTimesheetsAsync_DryRun_RollsBackTransaction()
    {
        // Arrange
        var csvFilePath = "timesheet.txt";
        var csvContent = "EMP-ID: 12345\nEMP-NAME: John Doe\nTIMESHEETS: 2024-01-15\n08:00;12:00;\"Meeting\";PROJ1";
        var existingEmployees = new List<Employee>();
        var existingProjects = new List<Project>();
        var timeEntries = new List<TimeEntry>
        {
            new()
            {
                Date = new DateOnly(2024, 1, 15),
                StartTime = new TimeOnly(8, 0),
                EndTime = new TimeOnly(12, 0),
                Description = "Meeting",
                Employee = new Employee { EmplyeeId = "12345", EmployeeName = "John Doe" },
                Project = new Project { ProjectCode = "PROJ1" }
            }
        };

        fileReader.ReadAllTextAsync(csvFilePath).Returns(Task.FromResult(csvContent));
        databaseWriter.GetAllEmployeesAsync().Returns(Task.FromResult<IEnumerable<Employee>>(existingEmployees));
        databaseWriter.GetAllProjectsAsync().Returns(Task.FromResult<IEnumerable<Project>>(existingProjects));
        csvParser.ParseCsv(csvContent, existingEmployees, existingProjects).Returns(timeEntries);

        // Act
        var result = await importer.ImportTimesheetsAsync(csvFilePath, isDryRun: true);

        // Assert
        Assert.Equal(1, result);
        await databaseWriter.Received(1).BeginTransactionAsync();
        await databaseWriter.Received(1).RollbackTransactionAsync();
        await databaseWriter.DidNotReceive().CommitTransactionAsync();
    }

    [Fact]
    public async Task ImportTimesheetsAsync_FileReaderThrows_RollsBackAndRethrows()
    {
        // Arrange
        var csvFilePath = "timesheet.txt";
        var expectedException = new FileNotFoundException("File not found");
        fileReader.ReadAllTextAsync(csvFilePath).Throws(expectedException);

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(
            async () => await importer.ImportTimesheetsAsync(csvFilePath));

        await databaseWriter.Received(1).BeginTransactionAsync();
        await databaseWriter.Received(1).RollbackTransactionAsync();
        await databaseWriter.DidNotReceive().CommitTransactionAsync();
    }

    [Fact]
    public async Task ImportTimesheetsAsync_CsvParserThrows_RollsBackAndRethrows()
    {
        // Arrange
        var csvFilePath = "timesheet.txt";
        var csvContent = "Invalid content";
        var existingEmployees = new List<Employee>();
        var existingProjects = new List<Project>();
        var expectedException = new TimesheetParseException(ImportFileError.InvalidKeyValueFormat);

        fileReader.ReadAllTextAsync(csvFilePath).Returns(Task.FromResult(csvContent));
        databaseWriter.GetAllEmployeesAsync().Returns(Task.FromResult<IEnumerable<Employee>>(existingEmployees));
        databaseWriter.GetAllProjectsAsync().Returns(Task.FromResult<IEnumerable<Project>>(existingProjects));
        csvParser.ParseCsv(csvContent, existingEmployees, existingProjects).Throws(expectedException);

        // Act & Assert
        await Assert.ThrowsAsync<TimesheetParseException>(
            async () => await importer.ImportTimesheetsAsync(csvFilePath));

        await databaseWriter.Received(1).BeginTransactionAsync();
        await databaseWriter.Received(1).RollbackTransactionAsync();
        await databaseWriter.DidNotReceive().CommitTransactionAsync();
    }

    [Fact]
    public async Task ImportTimesheetsAsync_DatabaseWriterThrows_RollsBackAndRethrows()
    {
        // Arrange
        var csvFilePath = "timesheet.txt";
        var csvContent = "EMP-ID: 12345\nEMP-NAME: John Doe\nTIMESHEETS: 2024-01-15\n08:00;12:00;\"Meeting\";PROJ1";
        var existingEmployees = new List<Employee>();
        var existingProjects = new List<Project>();
        var timeEntries = new List<TimeEntry>
        {
            new()
            {
                Date = new DateOnly(2024, 1, 15),
                StartTime = new TimeOnly(8, 0),
                EndTime = new TimeOnly(12, 0),
                Description = "Meeting",
                Employee = new Employee { EmplyeeId = "12345", EmployeeName = "John Doe" },
                Project = new Project { ProjectCode = "PROJ1" }
            }
        };
        var expectedException = new InvalidOperationException("Database error");

        fileReader.ReadAllTextAsync(csvFilePath).Returns(Task.FromResult(csvContent));
        databaseWriter.GetAllEmployeesAsync().Returns(Task.FromResult<IEnumerable<Employee>>(existingEmployees));
        databaseWriter.GetAllProjectsAsync().Returns(Task.FromResult<IEnumerable<Project>>(existingProjects));
        csvParser.ParseCsv(csvContent, existingEmployees, existingProjects).Returns(timeEntries);
        databaseWriter.WriteTimeEntriesAsync(Arg.Any<IEnumerable<TimeEntry>>()).Throws(expectedException);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await importer.ImportTimesheetsAsync(csvFilePath));

        await databaseWriter.Received(1).BeginTransactionAsync();
        await databaseWriter.Received(1).RollbackTransactionAsync();
        await databaseWriter.DidNotReceive().CommitTransactionAsync();
    }

    [Fact]
    public async Task ImportTimesheetsAsync_EmptyFile_ReturnsZero()
    {
        // Arrange
        var csvFilePath = "timesheet.txt";
        var csvContent = "EMP-ID: 12345\nEMP-NAME: John Doe\n";
        var existingEmployees = new List<Employee>();
        var existingProjects = new List<Project>();
        var timeEntries = new List<TimeEntry>();

        fileReader.ReadAllTextAsync(csvFilePath).Returns(Task.FromResult(csvContent));
        databaseWriter.GetAllEmployeesAsync().Returns(Task.FromResult<IEnumerable<Employee>>(existingEmployees));
        databaseWriter.GetAllProjectsAsync().Returns(Task.FromResult<IEnumerable<Project>>(existingProjects));
        csvParser.ParseCsv(csvContent, existingEmployees, existingProjects).Returns(timeEntries);

        // Act
        var result = await importer.ImportTimesheetsAsync(csvFilePath, isDryRun: false);

        // Assert
        Assert.Equal(0, result);
        await databaseWriter.Received(1).WriteTimeEntriesAsync(Arg.Is<IEnumerable<TimeEntry>>(e => e.Count() == 0));
        await databaseWriter.Received(1).CommitTransactionAsync();
    }

    [Fact]
    public async Task ImportTimesheetsAsync_MultipleEntriesForSameDay_ClearsOnlyOnce()
    {
        // Arrange
        var csvFilePath = "timesheet.txt";
        var csvContent = "EMP-ID: 12345\nEMP-NAME: John Doe\nTIMESHEETS: 2024-01-15\n08:00;12:00;\"Meeting\";PROJ1\n13:00;17:00;\"Development\";PROJ2";
        var existingEmployees = new List<Employee>();
        var existingProjects = new List<Project>();
        var employee = new Employee { EmplyeeId = "12345", EmployeeName = "John Doe" };
        var timeEntries = new List<TimeEntry>
        {
            new()
            {
                Date = new DateOnly(2024, 1, 15),
                StartTime = new TimeOnly(8, 0),
                EndTime = new TimeOnly(12, 0),
                Description = "Meeting",
                Employee = employee,
                Project = new Project { ProjectCode = "PROJ1" }
            },
            new()
            {
                Date = new DateOnly(2024, 1, 15),
                StartTime = new TimeOnly(13, 0),
                EndTime = new TimeOnly(17, 0),
                Description = "Development",
                Employee = employee,
                Project = new Project { ProjectCode = "PROJ2" }
            }
        };

        fileReader.ReadAllTextAsync(csvFilePath).Returns(Task.FromResult(csvContent));
        databaseWriter.GetAllEmployeesAsync().Returns(Task.FromResult<IEnumerable<Employee>>(existingEmployees));
        databaseWriter.GetAllProjectsAsync().Returns(Task.FromResult<IEnumerable<Project>>(existingProjects));
        csvParser.ParseCsv(csvContent, existingEmployees, existingProjects).Returns(timeEntries);

        // Act
        var result = await importer.ImportTimesheetsAsync(csvFilePath, isDryRun: false);

        // Assert
        Assert.Equal(2, result);
        await databaseWriter.Received(1).ClearDayAsync("12345", new DateOnly(2024, 1, 15));
        await databaseWriter.Received(1).WriteTimeEntriesAsync(Arg.Is<IEnumerable<TimeEntry>>(e => e.Count() == 2));
    }

    [Fact]
    public async Task ImportTimesheetsAsync_MultipleDays_ClearsEachDay()
    {
        // Arrange
        var csvFilePath = "timesheet.txt";
        var csvContent = "EMP-ID: 12345\nEMP-NAME: John Doe\nTIMESHEETS: 2024-01-15\n08:00;12:00;\"Meeting\";PROJ1\nTIMESHEETS: 2024-01-16\n09:00;13:00;\"Development\";PROJ2";
        var existingEmployees = new List<Employee>();
        var existingProjects = new List<Project>();
        var employee = new Employee { EmplyeeId = "12345", EmployeeName = "John Doe" };
        var timeEntries = new List<TimeEntry>
        {
            new()
            {
                Date = new DateOnly(2024, 1, 15),
                StartTime = new TimeOnly(8, 0),
                EndTime = new TimeOnly(12, 0),
                Description = "Meeting",
                Employee = employee,
                Project = new Project { ProjectCode = "PROJ1" }
            },
            new()
            {
                Date = new DateOnly(2024, 1, 16),
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(13, 0),
                Description = "Development",
                Employee = employee,
                Project = new Project { ProjectCode = "PROJ2" }
            }
        };

        fileReader.ReadAllTextAsync(csvFilePath).Returns(Task.FromResult(csvContent));
        databaseWriter.GetAllEmployeesAsync().Returns(Task.FromResult<IEnumerable<Employee>>(existingEmployees));
        databaseWriter.GetAllProjectsAsync().Returns(Task.FromResult<IEnumerable<Project>>(existingProjects));
        csvParser.ParseCsv(csvContent, existingEmployees, existingProjects).Returns(timeEntries);

        // Act
        var result = await importer.ImportTimesheetsAsync(csvFilePath, isDryRun: false);

        // Assert
        Assert.Equal(2, result);
        await databaseWriter.Received(1).ClearDayAsync("12345", new DateOnly(2024, 1, 15));
        await databaseWriter.Received(1).ClearDayAsync("12345", new DateOnly(2024, 1, 16));
        await databaseWriter.Received(1).WriteTimeEntriesAsync(Arg.Is<IEnumerable<TimeEntry>>(e => e.Count() == 2));
    }

    [Fact]
    public async Task ImportTimesheetsAsync_ClearDayThrows_RollsBackAndRethrows()
    {
        // Arrange
        var csvFilePath = "timesheet.txt";
        var csvContent = "EMP-ID: 12345\nEMP-NAME: John Doe\nTIMESHEETS: 2024-01-15\n08:00;12:00;\"Meeting\";PROJ1";
        var existingEmployees = new List<Employee>();
        var existingProjects = new List<Project>();
        var timeEntries = new List<TimeEntry>
        {
            new()
            {
                Date = new DateOnly(2024, 1, 15),
                StartTime = new TimeOnly(8, 0),
                EndTime = new TimeOnly(12, 0),
                Description = "Meeting",
                Employee = new Employee { EmplyeeId = "12345", EmployeeName = "John Doe" },
                Project = new Project { ProjectCode = "PROJ1" }
            }
        };
        var expectedException = new InvalidOperationException("Clear day error");

        fileReader.ReadAllTextAsync(csvFilePath).Returns(Task.FromResult(csvContent));
        databaseWriter.GetAllEmployeesAsync().Returns(Task.FromResult<IEnumerable<Employee>>(existingEmployees));
        databaseWriter.GetAllProjectsAsync().Returns(Task.FromResult<IEnumerable<Project>>(existingProjects));
        csvParser.ParseCsv(csvContent, existingEmployees, existingProjects).Returns(timeEntries);
        databaseWriter.ClearDayAsync(Arg.Any<string>(), Arg.Any<DateOnly>()).Throws(expectedException);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await importer.ImportTimesheetsAsync(csvFilePath));

        await databaseWriter.Received(1).BeginTransactionAsync();
        await databaseWriter.Received(1).RollbackTransactionAsync();
        await databaseWriter.DidNotReceive().CommitTransactionAsync();
    }

    [Fact]
    public async Task ImportTimesheetsAsync_CallsServicesInCorrectOrder()
    {
        // Arrange
        var csvFilePath = "timesheet.txt";
        var csvContent = "EMP-ID: 12345\nEMP-NAME: John Doe\nTIMESHEETS: 2024-01-15\n08:00;12:00;\"Meeting\";PROJ1";
        var existingEmployees = new List<Employee>();
        var existingProjects = new List<Project>();
        var timeEntries = new List<TimeEntry>
        {
            new()
            {
                Date = new DateOnly(2024, 1, 15),
                StartTime = new TimeOnly(8, 0),
                EndTime = new TimeOnly(12, 0),
                Description = "Meeting",
                Employee = new Employee { EmplyeeId = "12345", EmployeeName = "John Doe" },
                Project = new Project { ProjectCode = "PROJ1" }
            }
        };

        fileReader.ReadAllTextAsync(csvFilePath).Returns(Task.FromResult(csvContent));
        databaseWriter.GetAllEmployeesAsync().Returns(Task.FromResult<IEnumerable<Employee>>(existingEmployees));
        databaseWriter.GetAllProjectsAsync().Returns(Task.FromResult<IEnumerable<Project>>(existingProjects));
        csvParser.ParseCsv(csvContent, existingEmployees, existingProjects).Returns(timeEntries);

        // Act
        await importer.ImportTimesheetsAsync(csvFilePath, isDryRun: false);

        // Assert - Verify order of calls
        Received.InOrder(async () =>
        {
            await databaseWriter.BeginTransactionAsync();
            await fileReader.ReadAllTextAsync(csvFilePath);
            await databaseWriter.GetAllEmployeesAsync();
            await databaseWriter.GetAllProjectsAsync();
            csvParser.ParseCsv(csvContent, Arg.Any<IEnumerable<Employee>>(), Arg.Any<IEnumerable<Project>>());
            await databaseWriter.ClearDayAsync("12345", new DateOnly(2024, 1, 15));
            await databaseWriter.WriteTimeEntriesAsync(Arg.Any<IEnumerable<TimeEntry>>());
            await databaseWriter.CommitTransactionAsync();
        });
    }

    [Fact]
    public async Task ImportTimesheetsAsync_PassesEmployeesAndProjectsToParser()
    {
        // Arrange
        var csvFilePath = "timesheet.txt";
        var csvContent = "EMP-ID: 12345\nEMP-NAME: John Doe\nTIMESHEETS: 2024-01-15\n08:00;12:00;\"Meeting\";PROJ1";
        var existingEmployees = new List<Employee>
        {
            new() { Id = 1, EmplyeeId = "99999", EmployeeName = "Jane Smith" }
        };
        var existingProjects = new List<Project>
        {
            new() { Id = 1, ProjectCode = "EXISTINGPROJ" }
        };
        var timeEntries = new List<TimeEntry>
        {
            new()
            {
                Date = new DateOnly(2024, 1, 15),
                StartTime = new TimeOnly(8, 0),
                EndTime = new TimeOnly(12, 0),
                Description = "Meeting",
                Employee = new Employee { EmplyeeId = "12345", EmployeeName = "John Doe" },
                Project = new Project { ProjectCode = "PROJ1" }
            }
        };

        fileReader.ReadAllTextAsync(csvFilePath).Returns(Task.FromResult(csvContent));
        databaseWriter.GetAllEmployeesAsync().Returns(Task.FromResult<IEnumerable<Employee>>(existingEmployees));
        databaseWriter.GetAllProjectsAsync().Returns(Task.FromResult<IEnumerable<Project>>(existingProjects));
        csvParser.ParseCsv(csvContent, existingEmployees, existingProjects).Returns(timeEntries);

        // Act
        await importer.ImportTimesheetsAsync(csvFilePath, isDryRun: false);

        // Assert
        csvParser.Received(1).ParseCsv(
            csvContent,
            Arg.Is<IEnumerable<Employee>>(e => e.Count() == 1 && e.First().EmplyeeId == "99999"),
            Arg.Is<IEnumerable<Project>>(p => p.Count() == 1 && p.First().ProjectCode == "EXISTINGPROJ")
        );
    }
}
