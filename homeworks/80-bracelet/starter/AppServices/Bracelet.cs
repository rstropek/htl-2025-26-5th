namespace AppServices;

/// <summary>
/// Represents a validated friendship bracelet composed of alternating letter cubes and colored spacer beads.
/// </summary>
/// <remarks>
/// A bracelet follows the pattern: letter, spacer, letter, spacer, …, letter.
/// It always starts and ends with a letter. Instances are created via
/// <see cref="IBraceletSerializer.Parse"/> after successful validation.
/// </remarks>
public record Bracelet
{
    /// <summary>Gets the ordered parts of the bracelet (letters at even indices, color names at odd indices).</summary>
    public IReadOnlyList<string> Parts { get; }

    /// <summary>Gets the pipe-delimited serialized form of the bracelet (e.g. <c>H|pink|I</c>).</summary>
    public string Data { get; }

    /// <summary>Gets the total cost in EUR. Letters cost 1.00 each, spacers cost 0.50 each.</summary>
    public decimal Cost { get; }

    /// <summary>Gets whether the bracelet contains spacer beads of more than one color.</summary>
    public bool HasMixedColors { get; }

    /// <summary>
    /// Initializes a new bracelet from pre-validated parts.
    /// </summary>
    /// <param name="parts">The alternating list of letters and color names. 
    /// Must already be validated.</param>
    internal Bracelet(IReadOnlyList<string> parts)
    {
        // TODO: Implement the constructor to initialize the properties based on the provided parts.
        throw new NotImplementedException();
    }
}
