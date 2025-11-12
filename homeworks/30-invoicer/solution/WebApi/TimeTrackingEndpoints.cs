using AppServices;
using Microsoft.EntityFrameworkCore;

namespace WebApi;

public static class TimeTrackingEndpoints
{
    public static IEndpointRouteBuilder MapTimeTrackingEndpoints(this IEndpointRouteBuilder app)
    {
        // Get all employees
        app.MapGet("/employees", async (ApplicationDataContext db) => db.Employees)
            .Produces<List<Employee>>(StatusCodes.Status200OK)
            .WithDescription("Gets all employees.");

        // Get all projects
        app.MapGet("/projects", async (ApplicationDataContext db) => db.Projects)
            .Produces<List<Project>>(StatusCodes.Status200OK)
            .WithDescription("Gets all projects.");

        // Get time entries with optional filters
        app.MapGet("/timeentries", async (ApplicationDataContext db, int? employeeId, int? projectId) =>
            {
                var query = db.TimeEntries
                    .Include(te => te.Employee)
                    .Include(te => te.Project)
                    .AsQueryable();

                if (employeeId.HasValue)
                {
                    query = query.Where(te => te.EmployeeId == employeeId.Value);
                }

                if (projectId.HasValue)
                {
                    query = query.Where(te => te.ProjectId == projectId.Value);
                }

                var timeEntries = await query.ToListAsync();
                return timeEntries.Select(te => new TimeEntryDto(
                    te.Id,
                    te.Date,
                    te.StartTime,
                    te.EndTime,
                    te.Description,
                    te.EmployeeId,
                    te.Employee!.EmployeeName,
                    te.ProjectId,
                    te.Project!.ProjectCode
                )).ToList();
            })
            .Produces<List<TimeEntryDto>>(StatusCodes.Status200OK)
            .WithDescription("Gets all time entries with optional filters for employeeId and projectId.");

        // Get a single time entry by ID
        app.MapGet("/timeentries/{id}", async (int id, ApplicationDataContext db) =>
            {
                var timeEntry = await db.TimeEntries
                    .Include(te => te.Employee)
                    .Include(te => te.Project)
                    .FirstOrDefaultAsync(te => te.Id == id);

                if (timeEntry == null)
                {
                    return Results.NotFound();
                }

                var result = new TimeEntryDto(
                    timeEntry.Id,
                    timeEntry.Date,
                    timeEntry.StartTime,
                    timeEntry.EndTime,
                    timeEntry.Description,
                    timeEntry.EmployeeId,
                    timeEntry.Employee!.EmployeeName,
                    timeEntry.ProjectId,
                    timeEntry.Project!.ProjectCode
                );

                return Results.Ok(result);
            })
            .Produces<TimeEntryDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithDescription("Gets a single time entry by ID.");

        // Update a time entry
        app.MapPut("/timeentries/{id}", async (int id, TimeEntryUpdateDto dto, ApplicationDataContext db) =>
            {
                var timeEntry = await db.TimeEntries.FindAsync(id);
                if (timeEntry == null)
                {
                    return Results.NotFound("Time entry not found.");
                }

                // Validate end time is after start time
                if (dto.EndTime <= dto.StartTime)
                {
                    return Results.BadRequest("End time must be after start time.");
                }

                // Validate description is not empty
                if (string.IsNullOrWhiteSpace(dto.Description))
                {
                    return Results.BadRequest("Description must not be empty.");
                }

                // Validate employee exists
                var employeeExists = await db.Employees.AnyAsync(e => e.Id == dto.EmployeeId);
                if (!employeeExists)
                {
                    return Results.BadRequest($"Employee with ID {dto.EmployeeId} does not exist.");
                }

                // Validate project exists
                var projectExists = await db.Projects.AnyAsync(p => p.Id == dto.ProjectId);
                if (!projectExists)
                {
                    return Results.BadRequest($"Project with ID {dto.ProjectId} does not exist.");
                }

                timeEntry.Date = dto.Date;
                timeEntry.StartTime = dto.StartTime;
                timeEntry.EndTime = dto.EndTime;
                timeEntry.Description = dto.Description;
                timeEntry.EmployeeId = dto.EmployeeId;
                timeEntry.ProjectId = dto.ProjectId;

                await db.SaveChangesAsync();
                
                // Load related entities for response
                await db.Entry(timeEntry).Reference(te => te.Employee).LoadAsync();
                await db.Entry(timeEntry).Reference(te => te.Project).LoadAsync();
                
                var result = new TimeEntryDto(
                    timeEntry.Id,
                    timeEntry.Date,
                    timeEntry.StartTime,
                    timeEntry.EndTime,
                    timeEntry.Description,
                    timeEntry.EmployeeId,
                    timeEntry.Employee!.EmployeeName,
                    timeEntry.ProjectId,
                    timeEntry.Project!.ProjectCode
                );
                
                return Results.Ok(result);
            })
            .Produces<TimeEntryDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithDescription("Updates an existing time entry.");

        // Delete a time entry
        app.MapDelete("/timeentries/{id}", async (int id, ApplicationDataContext db) =>
            {
                var timeEntry = await db.TimeEntries.FindAsync(id);
                if (timeEntry == null)
                {
                    return Results.NotFound();
                }

                db.TimeEntries.Remove(timeEntry);
                await db.SaveChangesAsync();
                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithDescription("Deletes a time entry.");

        return app;
    }
}

public record TimeEntryDto(
    int Id,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string Description,
    int EmployeeId,
    string EmployeeName,
    int ProjectId,
    string ProjectCode);

public record TimeEntryUpdateDto(
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string Description,
    int EmployeeId,
    int ProjectId);
