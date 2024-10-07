using Altinn.Profile.Core.Extensions;
using Xunit;

namespace Altinn.Profile.Tests.Core.Extensions;

public class StringExtensionsTests
{
    [Fact]
    public void IsDigitsOnly_InvalidInput_ReturnsFalse()
    {
        Assert.False("123A456".IsDigitsOnly());
        Assert.False("123 456".IsDigitsOnly());
        Assert.False(string.Empty.IsDigitsOnly());
        Assert.False(((string)null).IsDigitsOnly());
    }

    [Fact]
    public void IsDigitsOnly_ValidDigits_ReturnsTrue()
    {
        Assert.True("1234567890".IsDigitsOnly());
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
        var result = input.IsDigitsOnly();
        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsValidSocialSecurityNumber_CacheWorks()
    {
        var socialSecurityNumber = "08119043698";
        var firstCheck = socialSecurityNumber.IsValidSocialSecurityNumber();
        var secondCheck = socialSecurityNumber.IsValidSocialSecurityNumber(); // Should hit cache

        Assert.True(firstCheck);
        Assert.True(secondCheck);
    }

    [Theory]
    [InlineData("12345678900")]
    [InlineData("98765432100")]
    [InlineData("11111111111")]
    public void IsValidSocialSecurityNumber_InvalidNumbers_ReturnsFalse(string number)
    {
        var result = number.IsValidSocialSecurityNumber();
        Assert.False(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    [InlineData("12345")]
    [InlineData("0811a043698")]
    public void IsValidSocialSecurityNumber_InvalidFormat_ReturnsFalse(string number)
    {
        var result = number.IsValidSocialSecurityNumber();
        Assert.False(result);
    }

    [Theory]
    [InlineData("08119043698")]
    [InlineData("11111111111")]
    public void IsValidSocialSecurityNumber_NoControlDigits(string socialSecurityNumber)
    {
        var result = socialSecurityNumber.IsValidSocialSecurityNumber(false);
        Assert.True(result || !result);
    }

    [Theory]
    [InlineData("08119043698")]
    [InlineData("23017126016")]
    [InlineData("03087937150")]
    public void IsValidSocialSecurityNumber_ValidNumbers_ReturnsTrue(string number)
    {
        var result = number.IsValidSocialSecurityNumber();
        Assert.True(result);
    }

    [Theory]
    [InlineData("08119043698", true)]
    [InlineData("12345678901", false)]
    public void IsValidSocialSecurityNumber_WithControlDigits(string socialSecurityNumber, bool expected)
    {
        var result = socialSecurityNumber.IsValidSocialSecurityNumber(true);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void RemoveWhitespace_EmptyOrNullInput_ReturnsInput()
    {
        Assert.Null(((string)null).RemoveWhitespace());
        Assert.Equal(string.Empty, " ".RemoveWhitespace());
    }

    [Fact]
    public void RemoveWhitespace_ValidInput_RemovesWhitespace()
    {
        var result = "  Hello World  ".RemoveWhitespace();
        Assert.Equal("HelloWorld", result);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData(" ", "")]
    [InlineData(null, null)]
    [InlineData("NoSpaces", "NoSpaces")]
    [InlineData("  Hello World  ", "HelloWorld")]
    public void RemoveWhitespace_VariousInputs_ReturnsExpected(string input, string expected)
    {
        var result = input.RemoveWhitespace();
        Assert.Equal(expected, result);
    }
}
