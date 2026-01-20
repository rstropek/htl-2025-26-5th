namespace AppServicesTests;

using AppServices;

public class ReimbursementCalculatorTests
{
    [Fact]
    public void PerDiem_UpTo3Hours_IsZero()
    {
        var travel = new Travel(
            Start: new DateTimeOffset(2026, 01, 20, 8, 0, 0, TimeSpan.Zero),
            End: new DateTimeOffset(2026, 01, 20, 11, 0, 0, TimeSpan.Zero),
            TravelerName: "Jane Doe",
            Purpose: "Customer meeting",
            Reimbursements: []);

        var calc = new ReimbursementCalculator();
        var result = calc.CalculateReimbursement(travel);

        Assert.Equal(0m, result.PerDiem);
    }

    [Fact]
    public void PerDiem_MoreThan3Hours_IsPerStartedHour()
    {
        var travel = new Travel(
            Start: new DateTimeOffset(2026, 01, 20, 8, 0, 0, TimeSpan.Zero),
            End: new DateTimeOffset(2026, 01, 20, 11, 0, 1, TimeSpan.Zero),
            TravelerName: "Jane Doe",
            Purpose: "Customer meeting",
            Reimbursements: []);

        var calc = new ReimbursementCalculator();
        var result = calc.CalculateReimbursement(travel);

        // 3h and 1s => 4 started hours
        Assert.Equal(10.0m, result.PerDiem);
    }

    [Fact]
    public void PerDiem_Exactly11Hours_IsNotFullRate()
    {
        var travel = new Travel(
            Start: new DateTimeOffset(2026, 01, 20, 8, 0, 0, TimeSpan.Zero),
            End: new DateTimeOffset(2026, 01, 20, 19, 0, 0, TimeSpan.Zero),
            TravelerName: "Jane Doe",
            Purpose: "Customer meeting",
            Reimbursements: []);

        var calc = new ReimbursementCalculator();
        var result = calc.CalculateReimbursement(travel);

        Assert.Equal(27.5m, result.PerDiem);
    }

    [Fact]
    public void PerDiem_MoreThan11Hours_IsFullRate()
    {
        var travel = new Travel(
            Start: new DateTimeOffset(2026, 01, 20, 8, 0, 0, TimeSpan.Zero),
            End: new DateTimeOffset(2026, 01, 20, 19, 0, 1, TimeSpan.Zero),
            TravelerName: "Jane Doe",
            Purpose: "Customer meeting",
            Reimbursements: []);

        var calc = new ReimbursementCalculator();
        var result = calc.CalculateReimbursement(travel);

        Assert.Equal(30m, result.PerDiem);
    }

    [Fact]
    public void PerDiem_SpanningMoreThan24Hours_Uses24HourBlocksFromStart()
    {
        // Matches the example from calculation-logic.md
        var travel = new Travel(
            Start: new DateTimeOffset(2026, 01, 19, 7, 0, 0, TimeSpan.Zero),
            End: new DateTimeOffset(2026, 01, 20, 14, 30, 0, TimeSpan.Zero),
            TravelerName: "John Doe",
            Purpose: "Training",
            Reimbursements: []);

        var calc = new ReimbursementCalculator();
        var result = calc.CalculateReimbursement(travel);

        Assert.Equal(50m, result.PerDiem);
    }

    [Fact]
    public void Mileage_IsCalculatedFromDriveEntries()
    {
        var reimbursements = new Reimbursement[]
        {
            new DriveWithPrivateCarReimbursement(75, "To airport"),
            new DriveWithPrivateCarReimbursement(75, "From airport"),
        };

        var travel = new Travel(
            Start: new DateTimeOffset(2026, 01, 19, 7, 30, 0, TimeSpan.Zero),
            End: new DateTimeOffset(2026, 01, 19, 10, 0, 0, TimeSpan.Zero),
            TravelerName: "John Doe",
            Purpose: "Trip",
            Reimbursements: reimbursements);

        var calc = new ReimbursementCalculator();
        var result = calc.CalculateReimbursement(travel);

        Assert.Equal(75m, result.Mileage);
    }

    [Fact]
    public void Expenses_AreSummed()
    {
        var reimbursements = new Reimbursement[]
        {
            new ExpenseReimbursement(498, "Flight"),
            new ExpenseReimbursement(120, "Hotel"),
        };

        var travel = new Travel(
            Start: new DateTimeOffset(2026, 01, 19, 7, 30, 0, TimeSpan.Zero),
            End: new DateTimeOffset(2026, 01, 19, 10, 0, 0, TimeSpan.Zero),
            TravelerName: "John Doe",
            Purpose: "Trip",
            Reimbursements: reimbursements);

        var calc = new ReimbursementCalculator();
        var result = calc.CalculateReimbursement(travel);

        Assert.Equal(618m, result.Expenses);
    }
}
