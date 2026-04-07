// filepath: test/Altinn.Profile.Tests/Profile.Integrations/Notifications/UserNotifierTest.cs
using Altinn.Profile.Integrations.Notifications;

using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations.Notifications;

/// <summary>
/// Tests for <see cref="UserNotifier"/>.
/// </summary>
public class UserNotifierTest
{
    /// <summary>
    /// Tests that null input returns null.
    /// </summary>
    [Fact]
    public void EnsureCountryCodeIfValidNumber_NullInput_ReturnsNull()
    {
        // Act
        string result = UserNotifier.EnsureCountryCodeIfValidNumber(null);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Tests that empty string input returns empty string.
    /// </summary>
    [Fact]
    public void EnsureCountryCodeIfValidNumber_EmptyString_ReturnsEmpty()
    {
        // Act
        string result = UserNotifier.EnsureCountryCodeIfValidNumber(string.Empty);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    /// <summary>
    /// Tests that a number starting with "00" gets "00" replaced by "+".
    /// </summary>
    [Theory]
    [InlineData("0047123456789", "+47123456789")]
    [InlineData("004512345678", "+4512345678")]
    [InlineData("001234567890", "+1234567890")]
    public void EnsureCountryCodeIfValidNumber_StartsWithDoubleZero_ReplacesWithPlus(string input, string expected)
    {
        // Act
        string result = UserNotifier.EnsureCountryCodeIfValidNumber(input);

        // Assert
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// Tests that an 8-digit Norwegian mobile number starting with 9 gets +47 prepended.
    /// </summary>
    [Theory]
    [InlineData("91234567", "+4791234567")]
    [InlineData("99999999", "+4799999999")]
    public void EnsureCountryCodeIfValidNumber_EightDigitStartingWithNine_PrependsPlusFortySevenNorway(string input, string expected)
    {
        // Act
        string result = UserNotifier.EnsureCountryCodeIfValidNumber(input);

        // Assert
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// Tests that an 8-digit Norwegian mobile number starting with 4 gets +47 prepended.
    /// </summary>
    [Theory]
    [InlineData("41234567", "+4741234567")]
    [InlineData("48888888", "+4748888888")]
    public void EnsureCountryCodeIfValidNumber_EightDigitStartingWithFour_PrependsPlusFortySevenNorway(string input, string expected)
    {
        // Act
        string result = UserNotifier.EnsureCountryCodeIfValidNumber(input);

        // Assert
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// Tests that a number already containing a country code starting with "+" is returned unchanged.
    /// </summary>
    [Theory]
    [InlineData("+4791234567")]
    [InlineData("+4512345678")]
    [InlineData("+12025550100")]
    public void EnsureCountryCodeIfValidNumber_AlreadyHasCountryCode_ReturnsUnchanged(string input)
    {
        // Act
        string result = UserNotifier.EnsureCountryCodeIfValidNumber(input);

        // Assert
        Assert.Equal(input, result);
    }

    /// <summary>
    /// Tests that an 8-digit number not starting with 4 or 9 is returned unchanged.
    /// </summary>
    [Theory]
    [InlineData("51234567")]
    [InlineData("31234567")]
    [InlineData("81234567")]
    public void EnsureCountryCodeIfValidNumber_EightDigitNotStartingWithFourOrNine_ReturnsUnchanged(string input)
    {
        // Act
        string result = UserNotifier.EnsureCountryCodeIfValidNumber(input);

        // Assert
        Assert.Equal(input, result);
    }

    /// <summary>
    /// Tests that a number with length other than 8 not starting with "00" or "+" is returned unchanged.
    /// </summary>
    [Theory]
    [InlineData("9123456")]
    [InlineData("412345678")]
    [InlineData("123")]
    public void EnsureCountryCodeIfValidNumber_NonEightDigitWithoutPrefix_ReturnsUnchanged(string input)
    {
        // Act
        string result = UserNotifier.EnsureCountryCodeIfValidNumber(input);

        // Assert
        Assert.Equal(input, result);
    }
}
