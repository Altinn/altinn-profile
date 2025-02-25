#nullable enable

using Altinn.ApiClients.Maskinporten.Config;

namespace Altinn.Profile.Integrations.OfficialAddressRegister;

/// <summary>
/// Represents the settings for managing contact details and reservation information for individuals.
/// </summary>
public class OfficialAddressRegisterSettings
{
    /// <summary>
    /// Gets the endpoint URL used to retrieve updates in the contact information for one or more organizations.
    /// </summary>
    public string? ChangesLogEndpoint { get; init; }

    /// <summary>
    /// The maximum number of entries to retrieve from the changelog
    /// </summary>
    public int ChangesLogPageSize { get; init; }

    /// <summary>
    /// Gets the settings required for Maskinporten authentication.
    /// </summary>
    public MaskinportenSettings? MaskinportenSettings { get; init; }
}
