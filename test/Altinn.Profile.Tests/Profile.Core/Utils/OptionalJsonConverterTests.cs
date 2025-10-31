using System.Text.Json;
using Altinn.Profile.Core.Utils;
using Xunit;

namespace Altinn.Profile.Tests.Profile.Core.Utils;

public class OptionalJsonConverterTests
{
    private readonly JsonSerializerOptions _options;

    public OptionalJsonConverterTests()
    {
        _options = new JsonSerializerOptions();
        _options.Converters.Add(new OptionalJsonConverterFactory());
    }

    [Fact]
    public void Deserialize_ExplicitNull_ReturnsOptionalWithValueNull()
    {
        var json = "null";
        var result = JsonSerializer.Deserialize<Optional<string>>(json, _options);
        Assert.True(result.HasValue);
        Assert.Null(result.Value);
    }

    [Fact]
    public void Deserialize_Value_ReturnsOptionalWithValue()
    {
        var json = "\"hello\"";
        var result = JsonSerializer.Deserialize<Optional<string>>(json, _options);
        Assert.True(result.HasValue);
        Assert.Equal("hello", result.Value);
    }

    [Fact]
    public void Serialize_OptionalWithValue_WritesValue()
    {
        var optional = new Optional<string>("hello");
        var json = JsonSerializer.Serialize(optional, _options);
        Assert.Equal("\"hello\"", json);
    }

    [Fact]
    public void Serialize_OptionalWithoutValue_WritesNull()
    {
        var optional = new Optional<string>();
        var json = JsonSerializer.Serialize(optional, _options);
        Assert.Equal("null", json);
    }
}
