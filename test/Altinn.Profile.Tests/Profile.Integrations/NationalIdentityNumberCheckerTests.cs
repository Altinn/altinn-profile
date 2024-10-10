using System;
using System.Collections.Generic;

using Altinn.Profile.Integrations.Services;

using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations;

public class NationalIdentityNumberCheckerTests
{
    private readonly NationalIdentityNumberChecker _checker;

    public NationalIdentityNumberCheckerTests()
    {
        _checker = new NationalIdentityNumberChecker();
    }

    [Fact]
    public void Categorize_NullInput_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _checker.Categorize(null));
    }

    [Fact]
    public void Categorize_ValidAndInvalidNumbers_ReturnsCorrectCategorization()
    {
        var input = new List<string> { "26050711071", "invalid_number", "06010190515" };
        var (valid, invalid) = _checker.Categorize(input);

        Assert.Single(invalid);
        Assert.Equal(2, valid.Count);
        Assert.Contains("26050711071", valid);
        Assert.Contains("06010190515", valid);
        Assert.Contains("invalid_number", invalid);
    }

    [Fact]
    public void GetValid_NullInput_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _checker.GetValid(null));
    }

    [Fact]
    public void GetValid_ValidAndInvalidNumbers_ReturnsOnlyValidNumbers()
    {
        var input = new List<string> { "05112908325", "031IN0918959", "03110918959" };
        var result = _checker.GetValid(input);

        Assert.Equal(2, result.Count);
        Assert.Contains("05112908325", result);
        Assert.Contains("03110918959", result);
    }

    [Fact]
    public void IsValid_ValidNumber_ReturnsTrue()
    {
        var result = _checker.IsValid("08033201398");
        Assert.True(result);
    }

    [Fact]
    public void IsValid_InvalidNumber_ReturnsFalse()
    {
        var result = _checker.IsValid("080332Q1398");
        Assert.False(result);
    }
}
