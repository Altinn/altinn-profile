using System.Collections.Concurrent;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Altinn.Profile.Core.Extensions;

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
    ///     Determines whether a given string represents a valid format for a Norwegian Social Security Number (SSN).
    /// </summary>
    /// <param name="socialSecurityNumber">The Norwegian Social Security Number (SSN) to validate.</param>
    /// <param name="controlDigits">Indicates whether to validate the control digits.</param>
    /// <returns>
    ///     <c>true</c> if the given string represents a valid format for a Norwegian Social Security Number (SSN) and, if specified, the control digits are valid; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     A valid Norwegian Social Security Number (SSN) is an 11-digit number where:
    ///         - The first six digits represent the date of birth in the format DDMMYY.
    ///         - The next three digits are an individual number where the first digit indicates the century of birth.
    ///         - The last two digits are control digits.
    /// </remarks>
    /// <exception cref="FormatException">Thrown when the individual number part of the SSN cannot be parsed into an integer.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the parsed date is outside the range of DateTime.</exception>
    public static bool IsValidSocialSecurityNumber(this string socialSecurityNumber, bool controlDigits = true)
    {
        if (string.IsNullOrWhiteSpace(socialSecurityNumber) || socialSecurityNumber.Length != 11)
        {
            return false;
        }

        // Return the cached result if the given string has been checked once.
        if (CachedSocialSecurityNumber.TryGetValue(socialSecurityNumber, out var cachedResult))
        {
            return cachedResult;
        }

        ReadOnlySpan<char> socialSecurityNumberSpan = socialSecurityNumber.AsSpan();

        for (int i = 0; i < socialSecurityNumberSpan.Length; i++)
        {
            if (!char.IsDigit(socialSecurityNumberSpan[i]))
            {
                return false;
            }
        }

        // Extract parts of the Social Security Number (SSN) using slicing.
        ReadOnlySpan<char> datePart = socialSecurityNumberSpan[..6];
        ReadOnlySpan<char> controlDigitsPart = socialSecurityNumberSpan[9..11];
        ReadOnlySpan<char> individualNumberPart = socialSecurityNumberSpan[6..9];

        // If parsing the individual number part fails, return false.
        if (!int.TryParse(individualNumberPart, out _))
        {
            return false;
        }

        // Validate the date part.
        if (!DateTime.TryParseExact(datePart.ToString(), "ddMMyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
        {
            return false;
        }

        var isValidSocialSecurityNumber = !controlDigits || CalculateControlDigits(socialSecurityNumberSpan.Slice(0, 9).ToString()) == controlDigitsPart.ToString();

        CachedSocialSecurityNumber.TryAdd(socialSecurityNumber, isValidSocialSecurityNumber);

        return isValidSocialSecurityNumber;
    }

    /// <summary>
    ///     Calculates the control digits used to validate a Norwegian Social Security Number.
    /// </summary>
    /// <param name="firstNineDigits">The first nine digits of the Social Security Number.</param>
    /// <returns>A <see cref="string"/> represents the two control digits.</returns>
    private static string CalculateControlDigits(string firstNineDigits)
    {
        int[] weightsFirst = [3, 7, 6, 1, 8, 9, 4, 5, 2];

        int[] weightsSecond = [5, 4, 3, 2, 7, 6, 5, 4, 3, 2];

        int firstControlDigit = CalculateControlDigit(firstNineDigits, weightsFirst);

        int secondControlDigit = CalculateControlDigit(firstNineDigits + firstControlDigit, weightsSecond);

        return $"{firstControlDigit}{secondControlDigit}";
    }

    /// <summary>
    ///     Calculates a control digit using the specified weights.
    /// </summary>
    /// <param name="digits">The digits to use in the calculation.</param>
    /// <param name="weights">The weights for each digit.</param>
    /// <returns>An <see cref="int"/> represents the calculated control digit.</returns>
    private static int CalculateControlDigit(string digits, int[] weights)
    {
        int sum = 0;

        for (int i = 0; i < weights.Length; i++)
        {
            sum += (int)char.GetNumericValue(digits[i]) * weights[i];
        }

        int remainder = sum % 11;
        return remainder == 0 ? 0 : 11 - remainder;
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

    /// <summary>
    /// A cache for storing the validation results of Norwegian Social Security Numbers (SSNs).
    /// </summary>
    /// <remarks>
    /// This cache helps to avoid redundant validation checks for SSNs that have already been processed. 
    /// It maps the SSN as a string to a boolean indicating whether the SSN is valid (true) or not (false).
    /// Utilizing this cache can significantly improve performance for applications that frequently validate the same SSNs.
    /// </remarks>
    private static ConcurrentDictionary<string, bool> CachedSocialSecurityNumber => new();
}
