namespace AppServices;

public class TravelEntity
{
    public int Id { get; set; }

    public DateTimeOffset Start { get; set; }
    public DateTimeOffset End { get; set; }
    public string TravelerName { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;

    /// <summary>
    /// Persisted reimbursement calculation result.
    /// </summary>
    public decimal Mileage { get; set; }
    public decimal PerDiem { get; set; }
    public decimal Expenses { get; set; }

    public List<TravelReimbursementEntity> Reimbursements { get; set; } = [];
}

public enum TravelReimbursementType
{
    DriveWithPrivateCar = 1,
    Expense = 2
}

public abstract class TravelReimbursementEntity
{
    public int Id { get; set; }

    public int TravelId { get; set; }
    public TravelEntity? Travel { get; set; }

    public string Description { get; set; } = string.Empty;
}

public sealed class DriveWithPrivateCarReimbursementEntity : TravelReimbursementEntity
{
    public int KM { get; set; }
}

public sealed class ExpenseReimbursementEntity : TravelReimbursementEntity
{
    public int Amount { get; set; }
}
