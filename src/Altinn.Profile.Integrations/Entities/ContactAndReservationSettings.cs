#nullable enable

using Altinn.ApiClients.Maskinporten.Config;

namespace Altinn.Profile.Integrations.Entities;

/// <summary>
/// Represents the settings for managing contact details and reservation information for individuals.
/// </summary>
public class ContactAndReservationSettings : IContactAndReservationSettings
{
    /// <summary>
    /// Gets the endpoint URL used to retrieve updates in the contact information for one or more individuals.
    /// </summary>
    public string? ContactDetailsChangesEndpoint { get; init; }

    /// <summary>
    /// Gets the settings required for Maskinporten authentication.
    /// </summary>
    public MaskinportenSettings? Maskinporten { get; init; }
}
