using System.Globalization;

namespace AppServices;

/// <summary>
/// Interface for parsing a travel file
/// </summary>
public interface ITravelFileParser
{
    /// <summary>
    /// Parses travel file content into a <see cref="Travel"/> object 
    /// </summary>
    /// <param name="textContent">Travel file content as string</param>
    /// <returns>Parsed <see cref="Travel"/> object</returns>
    Travel ParseTravel(string csvContent);
}

public record Reimbursement();

public record DriveWithPrivateCarReimbursement(int KM, string Description) : Reimbursement();

public record ExpenseReimbursement(int Amount, string Description) : Reimbursement();

public record Travel(
    DateTimeOffset Start,
    DateTimeOffset End,
    string TravelerName,
    string Purpose,
    IEnumerable<Reimbursement> Reimbursements
);

public enum TravelParseError
{
    EmptyFile,
    InvalidHeaderFieldCount,
    InvalidStartDateFormat,
    InvalidEndDateFormat,
    StartDateAfterEndDate,
    EmptyTravelerName,
    EmptyTripPurpose,
    InvalidDriveFieldCount,
    InvalidDriveDistance,
    EmptyDriveDescription,
    InvalidExpenseFieldCount,
    InvalidExpenseAmount,
    EmptyExpenseDescription,
    InvalidEntryType
}

public class TravelParseException(TravelParseError errorCode)
    : Exception(ErrorMessages.TryGetValue(errorCode, out var message) ? message : "Unknown parsing error.")
{
    private static readonly Dictionary<TravelParseError, string> ErrorMessages = new()
    {
        { TravelParseError.EmptyFile, "The travel file is empty." },
        { TravelParseError.InvalidHeaderFieldCount, "Invalid number of fields in header." },
        { TravelParseError.InvalidStartDateFormat, "Invalid start date format." },
        { TravelParseError.InvalidEndDateFormat, "Invalid end date format." },
        { TravelParseError.StartDateAfterEndDate, "Start date is after end date." },
        { TravelParseError.EmptyTravelerName, "Traveler's name is empty." },
        { TravelParseError.EmptyTripPurpose, "Trip purpose is empty." },
        { TravelParseError.InvalidDriveFieldCount, "Invalid number of fields in DRIVE entry." },
        { TravelParseError.InvalidDriveDistance, "Invalid distance in DRIVE entry (not a positive integer)." },
        { TravelParseError.EmptyDriveDescription, "Empty description in DRIVE entry." },
        { TravelParseError.InvalidExpenseFieldCount, "Invalid number of fields in EXPENSE entry." },
        { TravelParseError.InvalidExpenseAmount, "Invalid amount in EXPENSE entry (not a positive integer)." },
        { TravelParseError.EmptyExpenseDescription, "Empty description in EXPENSE entry." },
        { TravelParseError.InvalidEntryType, "Invalid entry type (must be DRIVE or EXPENSE)." }
    };

    public TravelParseError ErrorCode { get; } = errorCode;
}

/// <summary>
/// Implementation for parsing CSV content into Dummy objects
/// </summary>
public class TravelFileParser : ITravelFileParser
{
    public Travel ParseTravel(string csvContent)
    {
        // TODO: Add your code here
        throw new NotImplementedException();
    }
}
