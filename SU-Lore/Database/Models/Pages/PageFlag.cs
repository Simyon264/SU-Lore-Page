namespace SU_Lore.Database.Models.Pages;

/// <summary>
/// A flag that can be set on a page to change its behavior. Be it displaying the title or well, not.
/// </summary>
public class PageFlag
{
    public int Id { get; set; }
    
    /// <summary>
    /// The type of flag.
    /// </summary>
    public required PageFlagType Type { get; set; }
    
    /// <summary>
    /// It's assigned value. Can be anything, really. It's up to the flag type to decide what to do with it.
    /// </summary>
    public required string Value { get; set; }

    /// <summary>
    /// Equality check. This only checks if the type is the same as you should not have two flags of the same type.
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is not PageFlag flag)
            return false;

        return flag.Type == Type;
    }

    /// <summary>
    /// As we only check for the type, we only hash the type.
    /// </summary>
    public override int GetHashCode()
    {
        return Type.GetHashCode();
    }
}

public static class PageFlagExtensions
{
    /// <summary>
    /// Checks if the page has a flag of the specified type.
    /// </summary>
    /// <param name="flags">The flags to check.</param>
    /// <param name="type">The type of flag to check for.</param>
    /// <returns>True if the flag is present, false otherwise.</returns>
    public static bool HasFlag(this HashSet<PageFlag> flags, PageFlagType type)
    {
        return flags.Any(f => f.Type == type);
    }

    /// <summary>
    /// Gets the value of a flag of the specified type.
    /// </summary>
    /// <param name="flags">The flags to check.</param>
    /// <param name="type">The type of flag to check for.</param>
    /// <returns>The value of the flag if found, null otherwise.</returns>
    public static string? GetFlagValue(this HashSet<PageFlag> flags, PageFlagType type)
    {
        return flags.FirstOrDefault(f => f.Type == type)?.Value;
    }
}