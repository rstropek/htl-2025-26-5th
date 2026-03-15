namespace AppServices;

/// <summary>
/// Result of parsing and validating a bracelet data string.
/// </summary>
public enum BraceletValidationResult
{
    /// <summary>The bracelet data is valid.</summary>
    Ok,
    /// <summary>The bracelet data string is null or empty.</summary>
    Empty,
    /// <summary>A letter at an even position is not a recognized letter or symbol.</summary>
    InvalidLetter,
    /// <summary>A color name at an odd position is not a recognized spacer color.</summary>
    InvalidColor,
    /// <summary>The bracelet contains more than the maximum of 10 letters.</summary>
    TooManyLetters,
    /// <summary>The bracelet ends with a spacer instead of a letter.</summary>
    EndsWithSpacer
}

/// <summary>
/// Parses and validates pipe-delimited bracelet data strings into <see cref="Bracelet"/> instances.
/// </summary>
public interface IBraceletSerializer
{
    /// <summary>
    /// Parses a pipe-delimited bracelet data string and validates its contents.
    /// </summary>
    /// <param name="data">The pipe-delimited bracelet string (e.g. <c>H|pink|I</c>).</param>
    /// <param name="bracelet">
    /// When the method returns <see cref="BraceletValidationResult.Ok"/>, contains the parsed bracelet;
    /// otherwise <c>null</c>.
    /// </param>
    /// <returns>A <see cref="BraceletValidationResult"/> indicating success or the specific validation failure.</returns>
    BraceletValidationResult Parse(string data, out Bracelet? bracelet);
}

/// <summary>
/// Default implementation of <see cref="IBraceletSerializer"/> that validates letters (A–Z, ♥, ★)
/// and spacer colors against a fixed set of allowed values.
/// </summary>
public class BraceletSerializer : IBraceletSerializer
{
    private readonly HashSet<string> ValidColorNames =
        ["pink", "peach", "mint", "blue", "purple", "rose", "cyan", "lime", "sand"];

    private readonly HashSet<string> ValidLetters;

    /// <summary>
    /// Initializes valid letters (A–Z) and symbols (♥, ★).
    /// </summary>
    public BraceletSerializer()
    {
        ValidLetters = [];
        for (char c = 'A'; c <= 'Z'; c++)
        {
            ValidLetters.Add(c.ToString());
        }
        ValidLetters.Add("\u2665"); // ♥
        ValidLetters.Add("\u2605"); // ★
    }

    /// <inheritdoc />
    public BraceletValidationResult Parse(string data, out Bracelet? bracelet)
    {
        bracelet = null;

        if (string.IsNullOrEmpty(data))
        {
            return BraceletValidationResult.Empty;
        }

        var parts = data.Split('|');

        int letterCount = 0;

        for (int i = 0; i < parts.Length; i++)
        {
            bool isEvenPosition = i % 2 == 0;

            if (isEvenPosition)
            {
                if (!ValidLetters.Contains(parts[i]))
                {
                    return BraceletValidationResult.InvalidLetter;
                }
                letterCount++;
            }
            else
            {
                if (!ValidColorNames.Contains(parts[i]))
                {
                    return BraceletValidationResult.InvalidColor;
                }
            }
        }

        if (letterCount > 10)
        {
            return BraceletValidationResult.TooManyLetters;
        }

        if (parts.Length % 2 == 0)
        {
            return BraceletValidationResult.EndsWithSpacer;
        }

        bracelet = new Bracelet(parts);
        return BraceletValidationResult.Ok;
    }
}
