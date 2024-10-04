using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Altinn.Profile.Models;

/// <summary>
/// Represents the response containing user notification preferences.
/// </summary>
public record UserNotificationPreferencesResponse
{
    /// <summary>
    /// Gets the list of user notification preferences that are matched.
    /// </summary>
    [JsonPropertyName("matched")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<UserNotificationPreferences> Matched { get; init; } = [];

    /// <summary>
    /// Gets the list of user notification preferences that are invalid or unmatched.
    /// </summary>
    [JsonPropertyName("Unmatched")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<UserNotificationPreferences> Unmatched { get; init; } = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="UserNotificationPreferencesResponse"/> record.
    /// </summary>
    /// <param name="matched">A list of user notification preferences that are matched.</param>
    /// <param name="unmatched">A list of user notification preferences that are invalid or unmatched.</param>
    public UserNotificationPreferencesResponse(
        List<UserNotificationPreferences> matched,
        List<UserNotificationPreferences> unmatched)
    {
        Matched = matched ?? [];
        Unmatched = unmatched ?? [];
    }
}
