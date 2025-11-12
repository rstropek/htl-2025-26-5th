using System.ComponentModel.DataAnnotations;

namespace AppServices;

public class Employee
{
    public int Id { get; set; }

    [MaxLength(5)]
    public string EmplyeeId { get; set; } = string.Empty;

    [MaxLength(100)]
    public string EmployeeName { get; set; } = string.Empty;

    public List<TimeEntry> TimeEntries { get; set; } = [];
}

public class Project
{
    public int Id { get; set; }

    [MaxLength(20)]
    public string ProjectCode { get; set; } = string.Empty;

    public List<TimeEntry> TimeEntries { get; set; } = [];
}

public class TimeEntry
{
    public int Id { get; set; }

    public DateOnly Date { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    [MaxLength(200)]
    public string Description { get; set; } = string.Empty;

    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public int ProjectId { get; set; }
    public Project? Project { get; set; }
}
