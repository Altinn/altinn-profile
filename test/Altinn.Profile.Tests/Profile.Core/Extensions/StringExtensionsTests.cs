using System.Reflection;

using Altinn.Profile.Core.Extensions;

using Xunit;

namespace Altinn.Profile.Tests.Core.Extensions;

public class StringExtensionsTests
{
    [Fact]
    public void CalculateControlDigits_WorksCorrectly()
    {
        // Arrange
        var firstNineDigits = "081190436";
        var expectedControlDigits = "98"; // Known correct control digits for this SSN

        // Act
        var method = typeof(StringExtensions).GetMethod("CalculateControlDigits", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var result = method.Invoke(null, [firstNineDigits]);

        // Assert
        Assert.Equal(expectedControlDigits, result);
    }

    [Theory]
    [InlineData("11113432278", 7)]
    [InlineData("08114231372", 7)]
    public void CalculateControlDigit_WorksCorrectly(string socialSecurityNumber, int expected)
    {
        // Arrange
        var firstNineDigits = socialSecurityNumber[..9];
        var method = typeof(StringExtensions).GetMethod("CalculateControlDigit", BindingFlags.NonPublic | BindingFlags.Static);
        var weightsFirst = new[] { 3, 7, 6, 1, 8, 9, 4, 5, 2 };

        // Act
        var result = method.Invoke(null, [firstNineDigits, weightsFirst]);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsValidSocialSecurityNumber_CacheReturnsCachedValue()
    {
        // Assign
        var ssn = "08119043698"; // Valid SSN
        var expectedFirstValidation = true; // Expected result for first validation

        // Act
        // First check: This will validate the SSN and store the result in the cache.
        var firstCheck = ssn.IsValidNationalIdentityNumber();

        // Second check: This should return the cached result.
        var secondCheck = ssn.IsValidNationalIdentityNumber();

        // Assert
        Assert.Equal(expectedFirstValidation, firstCheck);  // Verify first validation result
        Assert.Equal(expectedFirstValidation, secondCheck); // Verify cached result is returned
    }

    [Fact]
    public void IsValidSocialSecurityNumber_CachedResult_UsesCache()
    {
        // Arrange
        var ssn = "08119043698";

        // Act
        var firstCheck = ssn.IsValidNationalIdentityNumber(); // First call
        var secondCheck = ssn.IsValidNationalIdentityNumber(); // Should be cached

        // Assert
        Assert.True(firstCheck);
        Assert.True(secondCheck); // Cached result should match
    }

    [Fact]
    public void IsValidSocialSecurityNumber_InvalidFormat_ReturnsFalse()
    {
        // Arrange
        var invalidSsn = "0811a043698";

        // Act
        var result = invalidSsn.IsValidNationalIdentityNumber();

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("12345", false)]
    [InlineData("08119", false)]
    [InlineData("08119043698", true)]
    [InlineData("23017126016", true)]
    [InlineData("04045325356", true)]
    [InlineData("081190A3698", false)]
    [InlineData("081190 3698", false)]
    [InlineData("12345678900", false)]
    [InlineData("98765432100", false)]
    public void IsValidSocialSecurityNumber_ValidatesCorrectly(string socialSecurityNumber, bool expected)
    {
        // Act
        var result = socialSecurityNumber.IsValidNationalIdentityNumber();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("12345", true)]
    [InlineData("123A456", false)]
    [InlineData("123 456", false)]
    [InlineData("1234567890", true)]
    public void IsDigitsOnly_VariousInputs_ReturnsExpected(string input, bool expected)
    {
        // Act
        var result = input.IsDigitsOnly();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData(" ", "")]
    [InlineData("NoSpaces", "NoSpaces")]
    [InlineData("  Hello World  ", "HelloWorld")]
    public void RemoveWhitespace_VariousInputs_ReturnsExpected(string input, string expected)
    {
        // Act
        var result = input.RemoveWhitespace();

        // Assert
        Assert.Equal(expected, result);
    }
}
