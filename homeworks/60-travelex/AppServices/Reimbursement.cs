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
        ArgumentNullException.ThrowIfNull(travel);

        var mileageKm = travel.Reimbursements.OfType<DriveWithPrivateCarReimbursement>().Sum(d => d.KM);
        var mileage = mileageKm * 0.50m;

        var perDiem = CalculatePerDiem(travel.Start, travel.End);

        // If mileage allowance is claimed, it covers all expenses (incl. parking/tolls).
        var expenses = mileageKm > 0
            ? 0m
            : travel.Reimbursements.OfType<ExpenseReimbursement>().Sum(e => (decimal)e.Amount);

        return new ReimbursementResult(mileage, perDiem, expenses);
    }

    private static decimal CalculatePerDiem(DateTimeOffset start, DateTimeOffset end)
    {
        if (end <= start)
        {
            return 0m;
        }

        var duration = end - start;

        // Only trips longer than 3 hours qualify. If they qualify, all started hours count
        // (including the first 3 hours), subject to a max of 30€ per 24-hour block.
        if (duration <= TimeSpan.FromHours(3))
        {
            return 0m;
        }

        var fullDays = (int)(duration.Ticks / TimeSpan.TicksPerDay);
        var remainder = duration - TimeSpan.FromDays(fullDays);

        var result = fullDays * 30m;
        result += CalculatePerDiemRemainder(remainder);
        return result;
    }

    private static decimal CalculatePerDiemRemainder(TimeSpan remainder)
    {
        if (remainder > TimeSpan.FromHours(11))
        {
            return 30m;
        }

        var startedHours = (int)Math.Ceiling(remainder.TotalMinutes / 60d);
        var perHour = 2.50m;
        var amount = startedHours * perHour;
        return amount > 30m ? 30m : amount;
    }
}