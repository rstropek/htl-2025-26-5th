namespace AppServices;

public interface IReimbursementCalculator
{
    ReimbursementResult CalculateReimbursement(Travel travel);
}

public record ReimbursementResult(
    decimal Mileage,
    decimal PerDiem,
    decimal Expenses
);

public class ReimbursementCalculator : IReimbursementCalculator
{
    public ReimbursementResult CalculateReimbursement(Travel travel)
    {
        // TODO: Add your code here
        throw new NotImplementedException();
    }
}