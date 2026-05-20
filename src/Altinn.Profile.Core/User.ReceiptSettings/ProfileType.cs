using System.Text.Json.Serialization;

namespace Altinn.Profile.Core.User.ReceiptSettings;

/// <summary>
/// Specifies the type of user profile.
/// </summary>
/// 
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ProfileType
{
    /// <summary>
    /// A private user that represents him/herself as a citizen.
    /// </summary>
    Private,

    /// <summary>
    /// A professional user, acting on behalf of an organization.
    /// </summary>
    Professional
}
