namespace AppServicesTests;

using AppServices;

public class ReimbursementCalculatorTests
{
    [Fact]
    public void CalculateReimbursement_NullTravel_Throws()
    {
        var calc = new ReimbursementCalculator();

        Assert.Throws<ArgumentNullException>(() => calc.CalculateReimbursement(null!));
    }

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
    public void PerDiem_Exactly3Hours_IsZero()
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
    public void PerDiem_JustUnder4Hours_IsFourStartedHours()
    {
        var travel = new Travel(
            Start: new DateTimeOffset(2026, 01, 20, 8, 0, 0, TimeSpan.Zero),
            End: new DateTimeOffset(2026, 01, 20, 11, 59, 59, TimeSpan.Zero),
            TravelerName: "Jane Doe",
            Purpose: "Customer meeting",
            Reimbursements: []);

        var calc = new ReimbursementCalculator();
        var result = calc.CalculateReimbursement(travel);

        // 3h 59m 59s => 4 started hours
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
    public void PerDiem_JustUnder11Hours_IsStillNotFullRate()
    {
        var travel = new Travel(
            Start: new DateTimeOffset(2026, 01, 20, 8, 0, 0, TimeSpan.Zero),
            End: new DateTimeOffset(2026, 01, 20, 18, 59, 59, TimeSpan.Zero),
            TravelerName: "Jane Doe",
            Purpose: "Customer meeting",
            Reimbursements: []);

        var calc = new ReimbursementCalculator();
        var result = calc.CalculateReimbursement(travel);

        // 10h 59m 59s => 11 started hours (still not full rate)
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
    public void PerDiem_EndBeforeStart_IsZero()
    {
        var travel = new Travel(
            Start: new DateTimeOffset(2026, 01, 20, 8, 0, 0, TimeSpan.Zero),
            End: new DateTimeOffset(2026, 01, 20, 7, 59, 59, TimeSpan.Zero),
            TravelerName: "Jane Doe",
            Purpose: "Customer meeting",
            Reimbursements: []);

        var calc = new ReimbursementCalculator();
        var result = calc.CalculateReimbursement(travel);

        Assert.Equal(0m, result.PerDiem);
    }

    [Fact]
    public void PerDiem_Exactly24Hours_IsFullDayRate()
    {
        var travel = new Travel(
            Start: new DateTimeOffset(2026, 01, 20, 8, 0, 0, TimeSpan.Zero),
            End: new DateTimeOffset(2026, 01, 21, 8, 0, 0, TimeSpan.Zero),
            TravelerName: "Jane Doe",
            Purpose: "Customer meeting",
            Reimbursements: []);

        var calc = new ReimbursementCalculator();
        var result = calc.CalculateReimbursement(travel);

        Assert.Equal(30m, result.PerDiem);
    }

    [Fact]
    public void PerDiem_24HoursPlus3Hours_CountsRemainderHours()
    {
        var travel = new Travel(
            Start: new DateTimeOffset(2026, 01, 20, 8, 0, 0, TimeSpan.Zero),
            End: new DateTimeOffset(2026, 01, 21, 11, 0, 0, TimeSpan.Zero),
            TravelerName: "Jane Doe",
            Purpose: "Customer meeting",
            Reimbursements: []);

        var calc = new ReimbursementCalculator();
        var result = calc.CalculateReimbursement(travel);

        // Once the overall trip is > 3 hours, all started hours count.
        // 1 full day (30) + remainder 3h => 3 * 2.50 = 7.50
        Assert.Equal(37.5m, result.PerDiem);
    }

    [Fact]
    public void PerDiem_24HoursPlus2Hours_CountsRemainderHours()
    {
        var travel = new Travel(
            Start: new DateTimeOffset(2026, 01, 20, 8, 0, 0, TimeSpan.Zero),
            End: new DateTimeOffset(2026, 01, 21, 10, 0, 0, TimeSpan.Zero),
            TravelerName: "Jane Doe",
            Purpose: "Customer meeting",
            Reimbursements: []);

        var calc = new ReimbursementCalculator();
        var result = calc.CalculateReimbursement(travel);

        Assert.Equal(35m, result.PerDiem);
    }

    [Fact]
    public void PerDiem_24HoursPlus3HoursAnd1Second_AddsFourStartedHours()
    {
        var travel = new Travel(
            Start: new DateTimeOffset(2026, 01, 20, 8, 0, 0, TimeSpan.Zero),
            End: new DateTimeOffset(2026, 01, 21, 11, 0, 1, TimeSpan.Zero),
            TravelerName: "Jane Doe",
            Purpose: "Customer meeting",
            Reimbursements: []);

        var calc = new ReimbursementCalculator();
        var result = calc.CalculateReimbursement(travel);

        // 1 full day (30) + (3h and 1s => 4 started hours => 10)
        Assert.Equal(40m, result.PerDiem);
    }

    [Fact]
    public void PerDiem_48Hours_IsTwoFullDays()
    {
        var travel = new Travel(
            Start: new DateTimeOffset(2026, 01, 20, 8, 0, 0, TimeSpan.Zero),
            End: new DateTimeOffset(2026, 01, 22, 8, 0, 0, TimeSpan.Zero),
            TravelerName: "Jane Doe",
            Purpose: "Customer meeting",
            Reimbursements: []);

        var calc = new ReimbursementCalculator();
        var result = calc.CalculateReimbursement(travel);

        Assert.Equal(60m, result.PerDiem);
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
    public void Expenses_AreSummed_EvenWhenMileageAllowanceIsClaimed()
    {
        var reimbursements = new Reimbursement[]
        {
            new DriveWithPrivateCarReimbursement(1, "Short drive"),
            new ExpenseReimbursement(100, "Parking"),
        };

        var travel = new Travel(
            Start: new DateTimeOffset(2026, 01, 19, 7, 30, 0, TimeSpan.Zero),
            End: new DateTimeOffset(2026, 01, 19, 10, 0, 0, TimeSpan.Zero),
            TravelerName: "John Doe",
            Purpose: "Trip",
            Reimbursements: reimbursements);

        var calc = new ReimbursementCalculator();
        var result = calc.CalculateReimbursement(travel);

        Assert.Equal(100m, result.Expenses);
    }

    [Fact]
    public void Expenses_AreNotZero_WhenDriveEntriesHaveZeroKm()
    {
        var reimbursements = new Reimbursement[]
        {
            new DriveWithPrivateCarReimbursement(0, "No distance"),
            new ExpenseReimbursement(100, "Parking"),
        };

        var travel = new Travel(
            Start: new DateTimeOffset(2026, 01, 19, 7, 30, 0, TimeSpan.Zero),
            End: new DateTimeOffset(2026, 01, 19, 10, 0, 0, TimeSpan.Zero),
            TravelerName: "John Doe",
            Purpose: "Trip",
            Reimbursements: reimbursements);

        var calc = new ReimbursementCalculator();
        var result = calc.CalculateReimbursement(travel);

        Assert.Equal(100m, result.Expenses);
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
