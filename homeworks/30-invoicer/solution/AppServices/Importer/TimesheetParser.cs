namespace AppServices.Importer;

/// <summary>
/// Interface for parsing timesheet files into objects
/// </summary>
public interface ITimesheetParser
{
    /// <summary>
    /// Parses CSV content into a list of TimeEntry objects
    /// </summary>
    /// <param name="csvContent">CSV content as string</param>
    /// <param name="existingEmployees">Existing employees in the database</param>
    /// <param name="existingProjects">Existing projects in the database</param>
    /// <returns>List of parsed TimeEntry objects</returns>
    /// <exception cref="TimesheetParseException">
    /// Thrown when file content is invalid.
    /// </exception>
    /// <remarks>
    /// Note that this method must link TimeEntry objects to existing
    /// Employee and Project entities from the database where possible.
    /// 
    /// If an Employee (based on Employee ID) exists in the database, the
    /// employee's name will be updated if it differs from the CSV content.
    /// If a Project from the CSV does not exist in the database, a new
    /// entity must be created.
    /// 
    /// Multiple time entries with the same project code MUST reference 
    /// the same Project object. You must ensure that duplicate Project
    /// entities are not created for the same project code.
    /// </remarks>
    IEnumerable<TimeEntry> ParseCsv(string csvContent, IEnumerable<Employee> existingEmployees, IEnumerable<Project> existingProjects);
}

/// <summary>
/// Represents all possible validation errors for the time tracking import file format.
/// </summary>
public enum ImportFileError
{
    // Employee Identification Errors
    MissingEmployeeId,
    MissingEmployeeName,
    DuplicateEmployeeId,
    DuplicateEmployeeName,
    EmployeeIdTooLong,              // Max 5 characters
    EmployeeNameTooLong,            // Max 100 characters
    EmployeeIdNotNumeric,

    // Field Format Errors
    InvalidKeyValueFormat,          // Missing ": " separator
    LeadingWhitespace,
    TrailingWhitespace,
    UnknownKey,                     // e.g., DEPARTMENT, TIMESHEET (singular)
    EmptyValue,

    // Timesheet Section Errors
    MissingTimesheetSection,        // No TIMESHEETS sections found
    TimesheetSectionBeforeEmployeeData,
    EmptyTimesheetSection,          // TIMESHEETS section with no time entries

    // Date Errors
    InvalidDate,                    // Not YYYY-MM-DD or invalid date

    // Time Entry Field Errors
    IncorrectFieldCount,            // Not exactly 4 semicolon-delimited fields
    EmptyField,                     // One or more fields are empty

    // Time Format Errors
    InvalidTime,                    // Not HH:MM or invalid time
    EndTimeBeforeStartTime,         // Logical validation

    // Description Errors
    DescriptionNotQuoted,           // Missing opening or closing quotes
    DescriptionTooLong,             // Max 200 characters

    // Project Errors
    ProjectTooLong,                 // Max 20 characters
    ProjectQuoted,                  // Project should NOT be quoted
}

public class TimesheetParseException(ImportFileError errorCode)
    : Exception(ErrorMessages.TryGetValue(errorCode, out var message) ? message : "Unknown parsing error.")
{
    private static readonly Dictionary<ImportFileError, string> ErrorMessages = new()
    {
        { ImportFileError.MissingEmployeeId, "Employee ID is missing." },
        { ImportFileError.MissingEmployeeName, "Employee name is missing." },
        { ImportFileError.DuplicateEmployeeId, "Duplicate employee ID found." },
        { ImportFileError.DuplicateEmployeeName, "Duplicate employee name found." },
        { ImportFileError.EmployeeIdTooLong, "Employee ID exceeds maximum length of 5 characters." },
        { ImportFileError.EmployeeNameTooLong, "Employee name exceeds maximum length of 100 characters." },
        { ImportFileError.EmployeeIdNotNumeric, "Employee ID must be numeric." },
        { ImportFileError.InvalidKeyValueFormat, "Invalid key-value format; missing ': ' separator." },
        { ImportFileError.LeadingWhitespace, "Leading whitespace detected in field." },
        { ImportFileError.TrailingWhitespace, "Trailing whitespace detected in field." },
        { ImportFileError.UnknownKey, "Unknown key found in the file." },
        { ImportFileError.EmptyValue, "Field value cannot be empty." },
        { ImportFileError.MissingTimesheetSection, "No TIMESHEETS section found in the file." },
        { ImportFileError.TimesheetSectionBeforeEmployeeData, "TIMESHEETS section appears before employee data." },
        { ImportFileError.EmptyTimesheetSection, "TIMESHEETS section is empty." },
        { ImportFileError.InvalidDate, "Invalid date format; expected YYYY-MM-DD." },
        { ImportFileError.IncorrectFieldCount, "Incorrect number of fields in time entry; expected 4 fields." },
        { ImportFileError.EmptyField, "One or more fields in time entry are empty." },
        { ImportFileError.InvalidTime, "Invalid time format; expected HH:MM." },
        { ImportFileError.EndTimeBeforeStartTime, "End time is before start time." },
        { ImportFileError.DescriptionNotQuoted, "Description field must be enclosed in double quotes." },
        { ImportFileError.DescriptionTooLong, "Description exceeds maximum length of 200 characters." },
        { ImportFileError.ProjectTooLong, "Project code exceeds maximum length of 20 characters." },
        { ImportFileError.ProjectQuoted, "Project code must not be quoted." },
    };

    public ImportFileError ErrorCode { get; } = errorCode;
}

/// <summary>
/// Implementation for parsing CSV content into TimeEntry objects
/// </summary>
public class TimesheetParser : ITimesheetParser
{
    /// <inheritdoc/>
    public IEnumerable<TimeEntry> ParseCsv(string csvContent, IEnumerable<Employee> existingEmployees, IEnumerable<Project> existingProjects)
    {
        var lines = csvContent.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries);
        var lineIndex = 0;

        // Parse employee section
        var (employeeId, employeeName) = ParseEmployeeSection(lines, ref lineIndex);

        // Validate that we have at least one TIMESHEETS section
        if (lineIndex >= lines.Length)
        {
            throw new TimesheetParseException(ImportFileError.MissingTimesheetSection);
        }

        // Find or create employee
        var employee = FindOrCreateEmployee(employeeId, employeeName, existingEmployees);

        // Create a cache for projects (both existing and newly created)
        var projectCache = existingProjects.ToDictionary(p => p.ProjectCode, p => p);

        // Parse timesheet sections
        var timeEntries = new List<TimeEntry>();
        while (lineIndex < lines.Length)
        {
            var date = ParseTimesheetHeader(lines, ref lineIndex);
            var sectionEntries = ParseTimeEntries(lines, ref lineIndex, date, employee, projectCache);
            
            if (sectionEntries.Count == 0)
            {
                throw new TimesheetParseException(ImportFileError.EmptyTimesheetSection);
            }

            timeEntries.AddRange(sectionEntries);
        }

        if (timeEntries.Count == 0)
        {
            throw new TimesheetParseException(ImportFileError.MissingTimesheetSection);
        }

        return timeEntries;
    }

    private static (string employeeId, string employeeName) ParseEmployeeSection(string[] lines, ref int lineIndex)
    {
        string? employeeId = null;
        string? employeeName = null;

        while (lineIndex < lines.Length)
        {
            var line = lines[lineIndex];

            // Check if we've reached the TIMESHEETS section
            if (line.StartsWith("TIMESHEETS:", StringComparison.Ordinal))
            {
                break;
            }

            // Validate line format
            ValidateWhitespaces(line);

            if (!line.Contains(": ", StringComparison.Ordinal))
            {
                throw new TimesheetParseException(ImportFileError.InvalidKeyValueFormat);
            }

            var colonIndex = line.IndexOf(": ", StringComparison.Ordinal);
            var key = line[..colonIndex];
            var value = line[(colonIndex + 2)..];

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new TimesheetParseException(ImportFileError.EmptyValue);
            }

            switch (key)
            {
                case "EMP-ID":
                    if (employeeId != null)
                    {
                        throw new TimesheetParseException(ImportFileError.DuplicateEmployeeId);
                    }
                    ValidateEmployeeId(value);
                    employeeId = value;
                    break;

                case "EMP-NAME":
                    if (employeeName != null)
                    {
                        throw new TimesheetParseException(ImportFileError.DuplicateEmployeeName);
                    }
                    ValidateEmployeeName(value);
                    employeeName = value;
                    break;

                default:
                    throw new TimesheetParseException(ImportFileError.UnknownKey);
            }

            lineIndex++;
        }

        // Validate that both required fields are present
        if (employeeId == null)
        {
            throw new TimesheetParseException(ImportFileError.MissingEmployeeId);
        }

        if (employeeName == null)
        {
            throw new TimesheetParseException(ImportFileError.MissingEmployeeName);
        }

        return (employeeId, employeeName);
    }

    private static void ValidateWhitespaces(string line)
    {
        if (line.Length > 0 && char.IsWhiteSpace(line[0]))
        {
            throw new TimesheetParseException(ImportFileError.LeadingWhitespace);
        }

        if (line.Length > 0 && char.IsWhiteSpace(line[^1]))
        {
            throw new TimesheetParseException(ImportFileError.TrailingWhitespace);
        }
    }

    private static void ValidateEmployeeId(string employeeId)
    {
        if (employeeId.Length > 5)
        {
            throw new TimesheetParseException(ImportFileError.EmployeeIdTooLong);
        }

        if (!employeeId.All(char.IsDigit))
        {
            throw new TimesheetParseException(ImportFileError.EmployeeIdNotNumeric);
        }
    }

    private static void ValidateEmployeeName(string employeeName)
    {
        if (employeeName.Length > 100)
        {
            throw new TimesheetParseException(ImportFileError.EmployeeNameTooLong);
        }
    }

    private static DateOnly ParseTimesheetHeader(string[] lines, ref int lineIndex)
    {
        if (lineIndex >= lines.Length)
        {
            throw new TimesheetParseException(ImportFileError.MissingTimesheetSection);
        }

        var line = lines[lineIndex];
        ValidateWhitespaces(line);

        if (!line.StartsWith("TIMESHEETS: ", StringComparison.Ordinal))
        {
            throw new TimesheetParseException(ImportFileError.MissingTimesheetSection);
        }

        var dateStr = line["TIMESHEETS: ".Length..];
        if (!DateOnly.TryParseExact(dateStr, "yyyy-MM-dd", out var date))
        {
            throw new TimesheetParseException(ImportFileError.InvalidDate);
        }

        lineIndex++;
        return date;
    }

    private static List<TimeEntry> ParseTimeEntries(string[] lines, ref int lineIndex, DateOnly date, Employee employee, Dictionary<string, Project> projectCache)
    {
        var entries = new List<TimeEntry>();

        while (lineIndex < lines.Length)
        {
            var line = lines[lineIndex];

            // Check if we've reached the next TIMESHEETS section
            if (line.StartsWith("TIMESHEETS:", StringComparison.Ordinal))
            {
                break;
            }

            ValidateWhitespaces(line);

            var entry = ParseTimeEntry(line, date, employee, projectCache);
            entries.Add(entry);

            lineIndex++;
        }

        return entries;
    }

    private static TimeEntry ParseTimeEntry(string line, DateOnly date, Employee employee, Dictionary<string, Project> projectCache)
    {
        var fields = line.Split(';');

        if (fields.Length != 4)
        {
            throw new TimesheetParseException(ImportFileError.IncorrectFieldCount);
        }

        // Check for empty fields
        if (fields.Any(f => string.IsNullOrWhiteSpace(f)))
        {
            throw new TimesheetParseException(ImportFileError.EmptyField);
        }

        // Parse start time
        var startTime = ParseTime(fields[0]);

        // Parse end time
        var endTime = ParseTime(fields[1]);

        // Validate end time is after start time
        if (endTime <= startTime)
        {
            throw new TimesheetParseException(ImportFileError.EndTimeBeforeStartTime);
        }

        // Parse description
        var description = ParseDescription(fields[2]);

        // Parse project
        var projectCode = ParseProject(fields[3]);
        var project = FindOrCreateProject(projectCode, projectCache);

        return new TimeEntry
        {
            Date = date,
            StartTime = startTime,
            EndTime = endTime,
            Description = description,
            Employee = employee,
            EmployeeId = employee.Id,
            Project = project,
            ProjectId = project.Id
        };
    }

    private static TimeOnly ParseTime(string timeStr)
    {
        if (!TimeOnly.TryParseExact(timeStr, "HH:mm", out var time))
        {
            throw new TimesheetParseException(ImportFileError.InvalidTime);
        }

        return time;
    }

    private static string ParseDescription(string descriptionField)
    {
        if (!descriptionField.StartsWith('"') || !descriptionField.EndsWith('"'))
        {
            throw new TimesheetParseException(ImportFileError.DescriptionNotQuoted);
        }

        var description = descriptionField[1..^1];

        if (description.Length > 200)
        {
            throw new TimesheetParseException(ImportFileError.DescriptionTooLong);
        }

        return description;
    }

    private static string ParseProject(string projectField)
    {
        if (projectField.StartsWith('"') || projectField.EndsWith('"'))
        {
            throw new TimesheetParseException(ImportFileError.ProjectQuoted);
        }

        if (projectField.Length > 20)
        {
            throw new TimesheetParseException(ImportFileError.ProjectTooLong);
        }

        return projectField;
    }

    private static Employee FindOrCreateEmployee(string employeeId, string employeeName, IEnumerable<Employee> existingEmployees)
    {
        var employee = existingEmployees.FirstOrDefault(e => e.EmplyeeId == employeeId);
        if (employee != null)
        {
            // Update employee name if it has changed
            if (employee.EmployeeName != employeeName)
            {
                employee.EmployeeName = employeeName;
            }
        }
        else
        {
            employee = new Employee
            {
                EmplyeeId = employeeId,
                EmployeeName = employeeName
            };
        }

        return employee;
    }

    private static Project FindOrCreateProject(string projectCode, Dictionary<string, Project> projectCache)
    {
        if (!projectCache.TryGetValue(projectCode, out var project))
        {
            project = new Project
            {
                ProjectCode = projectCode
            };
            projectCache[projectCode] = project;
        }

        return project;
    }
}
