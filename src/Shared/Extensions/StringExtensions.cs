namespace RunSuggestion.Shared.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Returns the last 5 characters of the passed string if the length is greater than 5 chars long,
    /// or the original string if 5 or less characters long
    /// </summary>
    /// <param name="fullString">the full string to extract from</param>
    /// <returns>the last 5 characters of the passed string, or the original string if the length is too short</returns>
    /// <exception cref="ArgumentNullException">Thrown if fullString is null</exception>
    public static string LastFiveChars(this string fullString)
    {
        const int substringLength = 5;

        ArgumentNullException.ThrowIfNull(fullString);

        return fullString.Length <= substringLength
            ? fullString
            : fullString[^substringLength..];
    }
}
