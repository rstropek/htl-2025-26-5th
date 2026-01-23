using AppServices;
using Microsoft.EntityFrameworkCore;

namespace WebApi;

public static class TravelEndpoints
{
    public static IEndpointRouteBuilder MapTravelEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/travels")
            .WithTags("Travels");

        // TODO: Add additional endpoints here

        group.MapPost("/upload", UploadTravelFile)
            .Accepts<IFormFile>("multipart/form-data")
            .DisableAntiforgery()
            .Produces<TravelDetailsDto>(StatusCodes.Status201Created)
            .Produces<TravelUploadErrorDto>(StatusCodes.Status400BadRequest)
            .WithDescription("Uploads a travel .txt file, parses it, stores it, and returns the created travel.");

        // Why `.Accepts<IFormFile>("multipart/form-data")`?
        // - Uploading files via HTTP is typically done using the `multipart/form-data` content type.
        // - The client sends a "form" with fields; one of those fields is our `file`.
        // - In Swagger/OpenAPI, this tells tooling (e.g., Swagger UI) to render a file picker.
        //
        // Why `.DisableAntiforgery()`?
        // - ASP.NET Core can protect browser-based form posts with Anti-Forgery tokens (CSRF protection).
        // - For an API endpoint that is called from non-browser clients (or Swagger UI), this token is often
        //   not present. Disabling it avoids 400 errors during development.
        // - If this endpoint is used from a real browser app with cookies, you should revisit CSRF strategy.

        return app;
    }

    /// <summary>
    /// Receives a travel file upload and creates a corresponding travel record in the database.
    /// </summary>
    /// <remarks>
    /// High-level flow:
    /// 1) Validate that a file was uploaded.
    /// 2) Read uploaded file into a string.
    /// 3) Parse the domain model (<see cref="Travel"/>) using <see cref="ITravelFileParser"/>.
    /// 4) Calculate reimbursement totals using <see cref="IReimbursementCalculator"/>.
    /// 5) Map domain model to EF Core entities and persist them.
    /// 6) Reload the created entity (including child reimbursements) and return it.
    ///
    /// Notes on parameters (Minimal APIs):
    /// - <paramref name="file"/> comes from the multipart form field named "file".
    /// - <paramref name="db"/>, <paramref name="parser"/>, <paramref name="calculator"/> are resolved from
    ///   Dependency Injection (DI). They are NOT sent by the client.
    /// </remarks>
    private static async Task<IResult> UploadTravelFile(
        IFormFile file,
        ApplicationDataContext db,
        ITravelFileParser parser,
        IReimbursementCalculator calculator)
    {
        // `IFormFile` is an abstraction over an uploaded file.
        // - `Length` is the file size in bytes.
        // - The content is streamed; it is NOT automatically loaded into memory.
        if (file is null || file.Length == 0)
        {
            return Results.BadRequest(new TravelUploadErrorDto("EmptyFile", "No file uploaded or file is empty."));
        }

        string content;

        // Read the uploaded file stream into a string.
        // - `OpenReadStream()` gives us a readable stream for the uploaded content.
        // - `StreamReader` decodes bytes into text (UTF-8 by default).
        using (var stream = file.OpenReadStream())
        using (var reader = new StreamReader(stream))
        {
            content = await reader.ReadToEndAsync();
        }

        Travel parsed;
        try
        {
            parsed = parser.ParseTravel(content);
        }
        catch (TravelParseException ex)
        {
            return Results.BadRequest(new TravelUploadErrorDto(ex.ErrorCode.ToString(), ex.Message));
        }

        var reimbursement = calculator.CalculateReimbursement(parsed);

        // Map domain model -> EF Core entity model.
        // Domain model types (`Travel`, `ExpenseReimbursement`, ...) are optimized for business logic.
        // Entity types (`TravelEntity`, ...) are optimized for persistence (database tables, keys, relations).
        var entity = new TravelEntity
        {
            Start = parsed.Start,
            End = parsed.End,
            TravelerName = parsed.TravelerName,
            Purpose = parsed.Purpose,
            Mileage = reimbursement.Mileage,
            PerDiem = reimbursement.PerDiem,
            Expenses = reimbursement.Expenses,
        };

        // Create one child entity per reimbursement entry.
        // EF Core will store these in the related table and set up the foreign key to the travel record.
        foreach (var reimbursementEntry in parsed.Reimbursements)
        {
            switch (reimbursementEntry)
            {
                case DriveWithPrivateCarReimbursement drive:
                    entity.Reimbursements.Add(new DriveWithPrivateCarReimbursementEntity
                    {
                        Description = drive.Description,
                        KM = drive.KM
                    });
                    break;

                case ExpenseReimbursement expense:
                    entity.Reimbursements.Add(new ExpenseReimbursementEntity
                    {
                        Description = expense.Description,
                        Amount = expense.Amount
                    });
                    break;

                default:
                    // This should normally not happen unless parsing created an unknown reimbursement type.
                    return Results.BadRequest(new TravelUploadErrorDto("InvalidEntryType", "Unknown reimbursement type."));
            }
        }

        // Add the entity graph to the DbContext and persist it.
        // - `Add` marks the travel + its reimbursements as "to be inserted".
        // - `SaveChangesAsync` executes INSERT statements.
        // - After saving, EF Core populates generated keys (e.g., `entity.Id`).
        db.Travels.Add(entity);
        await db.SaveChangesAsync();

        // Reload including reimbursements to ensure:
        // - All child entities have their generated IDs
        // - We return a consistent "read model" for the response
        // We use `AsNoTracking()` because we only want to read and return data (no further updates).
        var created = await db.Travels
            .AsNoTracking()
            .Include(t => t.Reimbursements)
            .SingleAsync(t => t.Id == entity.Id);

        return Results.Created($"/travels/{entity.Id}", MapToDetailsDto(created));
    }

    private static TravelDetailsDto MapToDetailsDto(TravelEntity travel)
        => new(
            travel.Id,
            travel.Start,
            travel.End,
            travel.TravelerName,
            travel.Purpose,
            travel.Mileage,
            travel.PerDiem,
            travel.Expenses,
            [.. travel.Reimbursements
                .OrderBy(r => r.Id)
                .Select(r => r switch
                {
                    DriveWithPrivateCarReimbursementEntity drive => new TravelReimbursementDto(
                        Id: drive.Id,
                        Type: "DRIVE",
                        Description: drive.Description,
                        Km: drive.KM,
                        Amount: null),

                    ExpenseReimbursementEntity expense => new TravelReimbursementDto(
                        Id: expense.Id,
                        Type: "EXPENSE",
                        Description: expense.Description,
                        Km: null,
                        Amount: expense.Amount),

                    _ => new TravelReimbursementDto(
                        Id: r.Id,
                        Type: "UNKNOWN",
                        Description: r.Description,
                        Km: null,
                        Amount: null)
                })]);
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

public record TravelUploadErrorDto(string ErrorCode, string Message);
