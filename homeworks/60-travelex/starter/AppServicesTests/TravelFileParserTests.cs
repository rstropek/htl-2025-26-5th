namespace AppServicesTests;

using System.Globalization;
using AppServices;

public class TravelFileParserTests
{
    private static string StrictUtcTimestamp(DateTimeOffset value)
        => value.ToUniversalTime().ToString("yyyy-MM-dd'T'HH':'mm':'ss'Z'", CultureInfo.InvariantCulture);

    private static string Header(DateTimeOffset start, DateTimeOffset end, string travelerName = "Jane Doe", string purpose = "Customer meeting")
        => $"{StrictUtcTimestamp(start)}|{StrictUtcTimestamp(end)}|{travelerName}|{purpose}";

    private static void AssertParseError(string csv, TravelParseError expectedError)
    {
        var parser = new TravelFileParser();
        var ex = Assert.Throws<TravelParseException>(() => parser.ParseTravel(csv));
        Assert.Equal(expectedError, ex.ErrorCode);
    }

    [Fact]
    public void ParseTravel_OnlyHeader_ReturnsTravelWithNoReimbursements()
    {
        var start = new DateTimeOffset(2026, 01, 20, 8, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2026, 01, 20, 18, 0, 0, TimeSpan.Zero);
        var csv = Header(start, end, "Ada Lovelace", "Conference");

        var parser = new TravelFileParser();
        var travel = parser.ParseTravel(csv);

        Assert.Equal(start, travel.Start);
        Assert.Equal(end, travel.End);
        Assert.Equal("Ada Lovelace", travel.TravelerName);
        Assert.Equal("Conference", travel.Purpose);
        Assert.Empty(travel.Reimbursements);
    }

    [Fact]
    public void ParseTravel_WithExpenseAndDrive_ParsesReimbursements()
    {
        var start = new DateTimeOffset(2026, 01, 20, 8, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2026, 01, 20, 18, 0, 0, TimeSpan.Zero);

        var csv = string.Join("\n", new[]
        {
            Header(start, end, "John Doe", "Trip"),
            "DRIVE|75|To airport",
            "EXPENSE|120|Hotel",
        });

        var parser = new TravelFileParser();
        var travel = parser.ParseTravel(csv);

        Assert.Equal("John Doe", travel.TravelerName);
        Assert.Equal("Trip", travel.Purpose);

        var reimbursements = travel.Reimbursements.ToList();
        Assert.Collection(
            reimbursements,
            r0 =>
            {
                var drive = Assert.IsType<DriveWithPrivateCarReimbursement>(r0);
                Assert.Equal(75, drive.KM);
                Assert.Equal("To airport", drive.Description);
            },
            r1 =>
            {
                var expense = Assert.IsType<ExpenseReimbursement>(r1);
                Assert.Equal(120, expense.Amount);
                Assert.Equal("Hotel", expense.Description);
            });
    }

    [Theory]
    [InlineData("")]
    [InlineData("\n")]
    [InlineData("   ")]
    public void ParseTravel_EmptyFile_Throws(string csv)
        => AssertParseError(csv, TravelParseError.EmptyFile);

    [Fact]
    public void ParseTravel_InvalidHeaderFieldCount_Throws()
    {
        var start = new DateTimeOffset(2026, 01, 20, 8, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2026, 01, 20, 18, 0, 0, TimeSpan.Zero);

        AssertParseError(
            $"{StrictUtcTimestamp(start)}|{StrictUtcTimestamp(end)}|Name\n",
            TravelParseError.InvalidHeaderFieldCount);

        AssertParseError(
            $"{StrictUtcTimestamp(start)}|{StrictUtcTimestamp(end)}|Name|Purpose|Extra\n",
            TravelParseError.InvalidHeaderFieldCount);
    }

    [Fact]
    public void ParseTravel_InvalidStartDateFormat_Throws()
    {
        var end = new DateTimeOffset(2026, 01, 20, 18, 0, 0, TimeSpan.Zero);
        var csv = $"not-a-date|{StrictUtcTimestamp(end)}|Name|Purpose";
        AssertParseError(csv, TravelParseError.InvalidStartDateFormat);
    }

    [Theory]
    [InlineData("2026-01-20T08:00:00.000Z")]
    [InlineData("2026-01-20T08:00:00+00:00")]
    [InlineData("2026-01-20T08:00:00")]
    [InlineData("2026-01-20 08:00:00Z")]
    [InlineData("2026-01-20T08:00:00z")]
    public void ParseTravel_StartDateMustBeStrictUtcFormat_Throws(string startText)
    {
        var end = new DateTimeOffset(2026, 01, 20, 18, 0, 0, TimeSpan.Zero);
        var csv = $"{startText}|{StrictUtcTimestamp(end)}|Name|Purpose";
        AssertParseError(csv, TravelParseError.InvalidStartDateFormat);
    }

    [Fact]
    public void ParseTravel_InvalidEndDateFormat_Throws()
    {
        var start = new DateTimeOffset(2026, 01, 20, 8, 0, 0, TimeSpan.Zero);
        var csv = $"{StrictUtcTimestamp(start)}|not-a-date|Name|Purpose";
        AssertParseError(csv, TravelParseError.InvalidEndDateFormat);
    }

    [Theory]
    [InlineData("2026-01-20T18:00:00.000Z")]
    [InlineData("2026-01-20T18:00:00+00:00")]
    [InlineData("2026-01-20T18:00:00")]
    [InlineData("2026-01-20 18:00:00Z")]
    [InlineData("2026-01-20T18:00:00z")]
    public void ParseTravel_EndDateMustBeStrictUtcFormat_Throws(string endText)
    {
        var start = new DateTimeOffset(2026, 01, 20, 8, 0, 0, TimeSpan.Zero);
        var csv = $"{StrictUtcTimestamp(start)}|{endText}|Name|Purpose";
        AssertParseError(csv, TravelParseError.InvalidEndDateFormat);
    }

    [Fact]
    public void ParseTravel_StartDateAfterEndDate_Throws()
    {
        var start = new DateTimeOffset(2026, 01, 20, 18, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2026, 01, 20, 8, 0, 0, TimeSpan.Zero);
        var csv = Header(start, end, "Name", "Purpose");
        AssertParseError(csv, TravelParseError.StartDateAfterEndDate);
    }

    [Fact]
    public void ParseTravel_EmptyTravelerName_Throws()
    {
        var start = new DateTimeOffset(2026, 01, 20, 8, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2026, 01, 20, 18, 0, 0, TimeSpan.Zero);
        var csv = Header(start, end, "   ", "Purpose");
        AssertParseError(csv, TravelParseError.EmptyTravelerName);
    }

    [Fact]
    public void ParseTravel_EmptyTripPurpose_Throws()
    {
        var start = new DateTimeOffset(2026, 01, 20, 8, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2026, 01, 20, 18, 0, 0, TimeSpan.Zero);
        var csv = Header(start, end, "Name", "\t");
        AssertParseError(csv, TravelParseError.EmptyTripPurpose);
    }

    [Fact]
    public void ParseTravel_InvalidDriveFieldCount_Throws()
    {
        var start = new DateTimeOffset(2026, 01, 20, 8, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2026, 01, 20, 18, 0, 0, TimeSpan.Zero);

        AssertParseError(string.Join("\n", new[] { Header(start, end), "DRIVE|10" }), TravelParseError.InvalidDriveFieldCount);
        AssertParseError(string.Join("\n", new[] { Header(start, end), "DRIVE|10|Desc|Extra" }), TravelParseError.InvalidDriveFieldCount);
    }

    [Fact]
    public void ParseTravel_InvalidDriveDistance_Throws()
    {
        var start = new DateTimeOffset(2026, 01, 20, 8, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2026, 01, 20, 18, 0, 0, TimeSpan.Zero);

        AssertParseError(string.Join("\n", new[] { Header(start, end), "DRIVE|0|Desc" }), TravelParseError.InvalidDriveDistance);
        AssertParseError(string.Join("\n", new[] { Header(start, end), "DRIVE|abc|Desc" }), TravelParseError.InvalidDriveDistance);
    }

    [Fact]
    public void ParseTravel_EmptyDriveDescription_Throws()
    {
        var start = new DateTimeOffset(2026, 01, 20, 8, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2026, 01, 20, 18, 0, 0, TimeSpan.Zero);

        AssertParseError(string.Join("\n", new[] { Header(start, end), "DRIVE|10|   " }), TravelParseError.EmptyDriveDescription);
    }

    [Fact]
    public void ParseTravel_InvalidExpenseFieldCount_Throws()
    {
        var start = new DateTimeOffset(2026, 01, 20, 8, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2026, 01, 20, 18, 0, 0, TimeSpan.Zero);

        AssertParseError(string.Join("\n", new[] { Header(start, end), "EXPENSE|10" }), TravelParseError.InvalidExpenseFieldCount);
        AssertParseError(string.Join("\n", new[] { Header(start, end), "EXPENSE|10|Desc|Extra" }), TravelParseError.InvalidExpenseFieldCount);
    }

    [Fact]
    public void ParseTravel_InvalidExpenseAmount_Throws()
    {
        var start = new DateTimeOffset(2026, 01, 20, 8, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2026, 01, 20, 18, 0, 0, TimeSpan.Zero);

        AssertParseError(string.Join("\n", new[] { Header(start, end), "EXPENSE|0|Desc" }), TravelParseError.InvalidExpenseAmount);
        AssertParseError(string.Join("\n", new[] { Header(start, end), "EXPENSE|abc|Desc" }), TravelParseError.InvalidExpenseAmount);
    }

    [Fact]
    public void ParseTravel_EmptyExpenseDescription_Throws()
    {
        var start = new DateTimeOffset(2026, 01, 20, 8, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2026, 01, 20, 18, 0, 0, TimeSpan.Zero);

        AssertParseError(string.Join("\n", new[] { Header(start, end), "EXPENSE|10|  " }), TravelParseError.EmptyExpenseDescription);
    }

    [Fact]
    public void ParseTravel_InvalidEntryType_Throws()
    {
        var start = new DateTimeOffset(2026, 01, 20, 8, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2026, 01, 20, 18, 0, 0, TimeSpan.Zero);

        AssertParseError(string.Join("\n", new[] { Header(start, end), "UNKNOWN" }), TravelParseError.InvalidEntryType);
    }
}
