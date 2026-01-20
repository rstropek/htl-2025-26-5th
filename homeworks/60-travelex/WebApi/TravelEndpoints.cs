using AppServices;
using Microsoft.EntityFrameworkCore;

namespace WebApi;

public static class TravelEndpoints
{
    public static IEndpointRouteBuilder MapTravelEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/travels")
            .WithTags("Travels");

        group.MapGet("", GetTravels)
            .Produces<List<TravelListItemDto>>(StatusCodes.Status200OK)
            .WithDescription("Gets a list of all travels (id, traveler, purpose).");

        group.MapGet("/{id:int}", GetTravelById)
            .Produces<TravelDetailsDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithDescription("Gets all details of a single travel by id.");

        group.MapPost("/upload", UploadTravelFile)
            .Accepts<IFormFile>("multipart/form-data")
            .DisableAntiforgery()
            .Produces<TravelDetailsDto>(StatusCodes.Status201Created)
            .Produces<TravelUploadErrorDto>(StatusCodes.Status400BadRequest)
            .WithDescription("Uploads a travel .txt file, parses it, stores it, and returns the created travel.");

        return app;
    }

    private static async Task<IResult> GetTravels(ApplicationDataContext db, CancellationToken ct)
    {
        var travels = await db.Travels
            .AsNoTracking()
            .OrderByDescending(t => t.Id)
            .Select(t => new TravelListItemDto(t.Id, t.TravelerName, t.Purpose))
            .ToListAsync(ct);

        return Results.Ok(travels);
    }

    private static async Task<IResult> GetTravelById(int id, ApplicationDataContext db, CancellationToken ct)
    {
        var travel = await db.Travels
            .AsNoTracking()
            .Include(t => t.Reimbursements)
            .SingleOrDefaultAsync(t => t.Id == id, ct);

        if (travel is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(MapToDetailsDto(travel));
    }

    private static async Task<IResult> UploadTravelFile(
        IFormFile file,
        ApplicationDataContext db,
        ITravelFileParser parser,
        IReimbursementCalculator calculator,
        CancellationToken ct)
    {
        if (file is null || file.Length == 0)
        {
            return Results.BadRequest(new TravelUploadErrorDto("EmptyFile", "No file uploaded or file is empty."));
        }

        string content;
        using (var stream = file.OpenReadStream())
        using (var reader = new StreamReader(stream))
        {
            content = await reader.ReadToEndAsync(ct);
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
                    return Results.BadRequest(new TravelUploadErrorDto("InvalidEntryType", "Unknown reimbursement type."));
            }
        }

        db.Travels.Add(entity);
        await db.SaveChangesAsync(ct);

        // Reload w/ reimbursements to ensure ids are present.
        var created = await db.Travels
            .AsNoTracking()
            .Include(t => t.Reimbursements)
            .SingleAsync(t => t.Id == entity.Id, ct);

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
            travel.Reimbursements
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
                })
                .ToList());
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
