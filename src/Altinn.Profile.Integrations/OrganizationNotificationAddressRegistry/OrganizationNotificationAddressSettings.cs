using Altinn.ApiClients.Maskinporten.Config;

namespace Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry;

/// <summary>
/// Represents the settings for managing contact details and reservation information for individuals.
/// </summary>
public class OrganizationNotificationAddressSettings
{
    /// <summary>
    /// The endpoint URL used to retrieve updates in the contact information for one or more organizations.
    /// </summary>
    public string? ChangesLogEndpoint { get; init; }

    /// <summary>
    /// The maximum number of entries to retrieve from the changelog
    /// </summary>
    public int ChangesLogPageSize { get; init; }

    /// <summary>
    /// The endpoint URL used to send updates in the contact information for one or more organizations.
    /// </summary>
    public string? UpdateEndpoint { get; init; }

    /// <summary>
    /// The settings required for Maskinporten authentication.
    /// </summary>
    public MaskinportenSettings? MaskinportenSettings { get; init; }
}
