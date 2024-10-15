using Altinn.ApiClients.Maskinporten.Config;

namespace Altinn.Profile.Integrations.Entities;

/// <summary>
/// Represents the settings for managing contact details and reservation information for individuals.
/// </summary>
public interface IContactAndReservationSettings
{
    /// <summary>
    /// Gets the endpoint URL used to retrieve updates in the contact information for one or more individuals.
    /// </summary>
    string? ContactDetailsChangesEndpoint { get; }

    /// <summary>
    /// Gets the settings required for Maskinporten authentication.
    /// </summary>
    MaskinportenSettings? Maskinporten { get; }
}
