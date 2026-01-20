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
    private const string StrictUtcDateTimeFormat = "yyyy-MM-dd'T'HH':'mm':'ss'Z'";

    public Travel ParseTravel(string csvContent)
    {
        if (string.IsNullOrWhiteSpace(csvContent))
        {
            throw new TravelParseException(TravelParseError.EmptyFile);
        }

        var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToList();

        if (lines.Count == 0)
        {
            throw new TravelParseException(TravelParseError.EmptyFile);
        }

        // Parse header (first line)
        var headerFields = lines[0].Split('|');
        if (headerFields.Length != 4)
        {
            throw new TravelParseException(TravelParseError.InvalidHeaderFieldCount);
        }

        if (!TryParseStrictUtcDateTime(headerFields[0].Trim(), out var start))
        {
            throw new TravelParseException(TravelParseError.InvalidStartDateFormat);
        }

        if (!TryParseStrictUtcDateTime(headerFields[1].Trim(), out var end))
        {
            throw new TravelParseException(TravelParseError.InvalidEndDateFormat);
        }

        if (start > end)
        {
            throw new TravelParseException(TravelParseError.StartDateAfterEndDate);
        }

        var travelerName = headerFields[2].Trim();
        if (string.IsNullOrWhiteSpace(travelerName))
        {
            throw new TravelParseException(TravelParseError.EmptyTravelerName);
        }

        var purpose = headerFields[3].Trim();
        if (string.IsNullOrWhiteSpace(purpose))
        {
            throw new TravelParseException(TravelParseError.EmptyTripPurpose);
        }

        // Parse reimbursement entries
        var reimbursements = new List<Reimbursement>(lines.Count - 1);
        for (int i = 1; i < lines.Count; i++)
        {
            var entryFields = lines[i].Split('|');
            
            if (entryFields.Length < 1)
            {
                continue;
            }

            var entryType = entryFields[0].Trim().ToUpper();

            if (entryType == "DRIVE")
            {
                if (entryFields.Length != 3)
                {
                    throw new TravelParseException(TravelParseError.InvalidDriveFieldCount);
                }

                if (!int.TryParse(entryFields[1], CultureInfo.InvariantCulture, out var km) || km <= 0)
                {
                    throw new TravelParseException(TravelParseError.InvalidDriveDistance);
                }

                var description = entryFields[2].Trim();
                if (string.IsNullOrWhiteSpace(description))
                {
                    throw new TravelParseException(TravelParseError.EmptyDriveDescription);
                }

                reimbursements.Add(new DriveWithPrivateCarReimbursement(km, description));
            }
            else if (entryType == "EXPENSE")
            {
                if (entryFields.Length != 3)
                {
                    throw new TravelParseException(TravelParseError.InvalidExpenseFieldCount);
                }

                if (!int.TryParse(entryFields[1], CultureInfo.InvariantCulture, out var amount) || amount <= 0)
                {
                    throw new TravelParseException(TravelParseError.InvalidExpenseAmount);
                }

                var description = entryFields[2].Trim();
                if (string.IsNullOrWhiteSpace(description))
                {
                    throw new TravelParseException(TravelParseError.EmptyExpenseDescription);
                }

                reimbursements.Add(new ExpenseReimbursement(amount, description));
            }
            else
            {
                throw new TravelParseException(TravelParseError.InvalidEntryType);
            }
        }

        return new Travel(start, end, travelerName, purpose, reimbursements);
    }

    private static bool TryParseStrictUtcDateTime(string text, out DateTimeOffset value)
    {
        value = default;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        if (!DateTime.TryParseExact(
                text,
                StrictUtcDateTimeFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var dateTimeUtc))
        {
            return false;
        }

        value = new DateTimeOffset(DateTime.SpecifyKind(dateTimeUtc, DateTimeKind.Utc));
        return true;
    }
}
