using System.Diagnostics.CodeAnalysis;

namespace SU.LorePage;

public static class Helpers
{
    /// <summary>
    /// Get the current date in the format "yyyy-MM-dd".
    /// </summary>
    /// <returns>
    /// A string representing the current date in the format "yyyy-MM-dd".
    /// It is offset by +200 years.
    /// </returns>
    public static string GetCurrentDate()
    {
        return DateTime.Now.AddYears(200).ToString("yyyy-MM-dd");
    }
}