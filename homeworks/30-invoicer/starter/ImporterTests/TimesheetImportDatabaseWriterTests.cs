using AppServices;
using AppServices.Importer;
using Microsoft.EntityFrameworkCore;
using TestInfrastructure;

namespace ImporterTests;

public class TimesheetImportDatabaseWriterTests(DatabaseFixture fixture)
    : IClassFixture<DatabaseFixture>
{
    [Fact]
    public async Task ClearDayAsync_RemovesTimeEntriesForSpecificEmployeeAndDate()
    {
        // Arrange
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var employee1 = new Employee { EmplyeeId = "E001", EmployeeName = "John Doe" };
            var employee2 = new Employee { EmplyeeId = "E002", EmployeeName = "Jane Smith" };
            var project = new Project { ProjectCode = "PRJ001" };
            
            context.Employees.AddRange(employee1, employee2);
            context.Projects.Add(project);
            await context.SaveChangesAsync();

            var date1 = new DateOnly(2025, 11, 12);
            var date2 = new DateOnly(2025, 11, 13);

            context.TimeEntries.AddRange(
                new TimeEntry 
                { 
                    EmployeeId = employee1.Id, 
                    ProjectId = project.Id, 
                    Date = date1, 
                    StartTime = new TimeOnly(9, 0), 
                    EndTime = new TimeOnly(17, 0),
                    Description = "Work"
                },
                new TimeEntry 
                { 
                    EmployeeId = employee1.Id, 
                    ProjectId = project.Id, 
                    Date = date2, 
                    StartTime = new TimeOnly(9, 0), 
                    EndTime = new TimeOnly(17, 0),
                    Description = "Work"
                },
                new TimeEntry 
                { 
                    EmployeeId = employee2.Id, 
                    ProjectId = project.Id, 
                    Date = date1, 
                    StartTime = new TimeOnly(9, 0), 
                    EndTime = new TimeOnly(17, 0),
                    Description = "Work"
                }
            );
            await context.SaveChangesAsync();
        }

        // Act
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var writer = new TimesheetImportDatabaseReaderWriter(context);
            await writer.ClearDayAsync("E001", new DateOnly(2025, 11, 12));
        }

        // Assert
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var entries = await context.TimeEntries.Include(te => te.Employee).ToListAsync();
            Assert.Equal(2, entries.Count);
            Assert.DoesNotContain(entries, e => e.Employee!.EmplyeeId == "E001" && e.Date == new DateOnly(2025, 11, 12));
            Assert.Contains(entries, e => e.Employee!.EmplyeeId == "E001" && e.Date == new DateOnly(2025, 11, 13));
            Assert.Contains(entries, e => e.Employee!.EmplyeeId == "E002" && e.Date == new DateOnly(2025, 11, 12));
        }
    }

    [Fact]
    public async Task WriteTimeEntriesAsync_CreatesNewEmployeeAndProject()
    {
        // Arrange
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            // Clear existing data
            context.TimeEntries.RemoveRange(context.TimeEntries);
            context.Employees.RemoveRange(context.Employees);
            context.Projects.RemoveRange(context.Projects);
            await context.SaveChangesAsync();
        }

        var entries = new List<TimeEntry>
        {
            new() 
            { 
                Employee = new Employee { EmplyeeId = "E001", EmployeeName = "John Doe" },
                Project = new Project { ProjectCode = "PRJ001" },
                Date = new DateOnly(2025, 11, 12),
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(17, 0),
                Description = "Development work"
            }
        };

        // Act
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var writer = new TimesheetImportDatabaseReaderWriter(context);
            await writer.WriteTimeEntriesAsync(entries);
        }

        // Assert
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var employeeCount = await context.Employees.CountAsync();
            var projectCount = await context.Projects.CountAsync();
            var timeEntryCount = await context.TimeEntries.CountAsync();

            Assert.Equal(1, employeeCount);
            Assert.Equal(1, projectCount);
            Assert.Equal(1, timeEntryCount);

            var employee = await context.Employees.FirstAsync();
            Assert.Equal("E001", employee.EmplyeeId);
            Assert.Equal("John Doe", employee.EmployeeName);

            var project = await context.Projects.FirstAsync();
            Assert.Equal("PRJ001", project.ProjectCode);
        }
    }

    [Fact]
    public async Task GetAllEmployeesAsync_ReturnsAllEmployees()
    {
        // Arrange
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            context.Employees.RemoveRange(context.Employees);
            context.Employees.AddRange(
                new Employee { EmplyeeId = "E001", EmployeeName = "John Doe" },
                new Employee { EmplyeeId = "E002", EmployeeName = "Jane Smith" }
            );
            await context.SaveChangesAsync();
        }

        // Act
        IEnumerable<Employee> employees;
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var writer = new TimesheetImportDatabaseReaderWriter(context);
            employees = await writer.GetAllEmployeesAsync();
        }

        // Assert
        Assert.Equal(2, employees.Count());
        Assert.Contains(employees, e => e.EmplyeeId == "E001");
        Assert.Contains(employees, e => e.EmplyeeId == "E002");
    }

    [Fact]
    public async Task GetAllProjectsAsync_ReturnsAllProjects()
    {
        // Arrange
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            context.Projects.RemoveRange(context.Projects);
            context.Projects.AddRange(
                new Project { ProjectCode = "PRJ001" },
                new Project { ProjectCode = "PRJ002" }
            );
            await context.SaveChangesAsync();
        }

        // Act
        IEnumerable<Project> projects;
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var writer = new TimesheetImportDatabaseReaderWriter(context);
            projects = await writer.GetAllProjectsAsync();
        }

        // Assert
        Assert.Equal(2, projects.Count());
        Assert.Contains(projects, p => p.ProjectCode == "PRJ001");
        Assert.Contains(projects, p => p.ProjectCode == "PRJ002");
    }

    [Fact]
    public async Task TransactionMethods_CommitSucceeds()
    {
        // Arrange
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            context.TimeEntries.RemoveRange(context.TimeEntries);
            context.Employees.RemoveRange(context.Employees);
            context.Projects.RemoveRange(context.Projects);
            await context.SaveChangesAsync();
        }

        var entries = new List<TimeEntry>
        {
            new() 
            { 
                Employee = new Employee { EmplyeeId = "E001", EmployeeName = "John Doe" },
                Project = new Project { ProjectCode = "PRJ001" },
                Date = new DateOnly(2025, 11, 12),
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(17, 0),
                Description = "Work"
            }
        };

        // Act
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var writer = new TimesheetImportDatabaseReaderWriter(context);
            await writer.BeginTransactionAsync();
            await writer.WriteTimeEntriesAsync(entries);
            await writer.CommitTransactionAsync();
        }

        // Assert
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var count = await context.TimeEntries.CountAsync();
            Assert.Equal(1, count);
        }
    }

    [Fact]
    public async Task TransactionMethods_RollbackSucceeds()
    {
        // Arrange
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            context.TimeEntries.RemoveRange(context.TimeEntries);
            context.Employees.RemoveRange(context.Employees);
            context.Projects.RemoveRange(context.Projects);
            await context.SaveChangesAsync();
        }

        var entries = new List<TimeEntry>
        {
            new() 
            { 
                Employee = new Employee { EmplyeeId = "E001", EmployeeName = "John Doe" },
                Project = new Project { ProjectCode = "PRJ001" },
                Date = new DateOnly(2025, 11, 12),
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(17, 0),
                Description = "Work"
            }
        };

        // Act
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var writer = new TimesheetImportDatabaseReaderWriter(context);
            await writer.BeginTransactionAsync();
            await writer.WriteTimeEntriesAsync(entries);
            await writer.RollbackTransactionAsync();
        }

        // Assert
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var count = await context.TimeEntries.CountAsync();
            Assert.Equal(0, count);
        }
    }
}
