using System.Text.RegularExpressions;

namespace Altinn.Profile.Core;

/// <summary>
/// Extension class for <see cref="string"/> to add more members.
/// </summary>
public static partial class StringExtensions
{
    /// <summary>
    ///     Determines whether a given string consists of only digits.
    /// </summary>
    /// <param name="input">The string to check.</param>
    /// <returns>
    ///     <c>true</c> if the given string consists of only digits; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     This method checks if the provided string is not null or whitespace and matches the regex pattern for digits.
    ///     The regex pattern ensures that the string contains only numeric characters (0-9).
    /// </remarks>
    public static bool IsDigitsOnly(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        return DigitsOnlyRegex().IsMatch(input);
    }

    /// <summary>
    ///     Removes all whitespace characters from the given string.
    /// </summary>
    /// <param name="stringToClean">The string from which to remove whitespace characters.</param>
    /// <returns>
    ///     A new string with all whitespace characters removed. 
    ///     If the input is null, empty, or consists only of whitespace, the original input is returned.
    /// </returns>
    public static string? RemoveWhitespace(this string stringToClean)
    {
        if (string.IsNullOrWhiteSpace(stringToClean))
        {
            return stringToClean?.Trim();
        }

        return WhitespaceRegex().Replace(stringToClean, string.Empty);
    }

    /// <summary>
    ///     Generates a compiled regular expression for matching all whitespace characters in a string.
    /// </summary>
    /// <returns>
    ///     A <see cref="Regex"/> object that can be used to match all whitespace characters in a string.
    /// </returns>
    [GeneratedRegex(@"\s+", RegexOptions.Compiled)]
    private static partial Regex WhitespaceRegex();

    /// <summary>
    ///     Generates a compiled regular expression for validating that a string consists of only digits.
    /// </summary>
    /// <returns>
    ///     A <see cref="Regex"/> object that can be used to validate that a string contains only digits.
    /// </returns>
    [GeneratedRegex(@"^\d+$", RegexOptions.Compiled)]
    private static partial Regex DigitsOnlyRegex();
}
