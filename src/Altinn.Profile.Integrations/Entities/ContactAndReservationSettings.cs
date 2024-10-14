#nullable enable

using Altinn.ApiClients.Maskinporten.Config;

namespace Altinn.Profile.Integrations.Entities;

/// <summary>
/// Represents the settings for contact and reservation.
/// </summary>
public class ContactAndReservationSettings
{
    /// <summary>
    /// Gets or sets the endpoint to retrieve the contact information for one or more persons.
    /// </summary>
    public string? ContactRegisterEndpoint { get; init; }

    /// <summary>
    /// Gets or sets the endpoint to retrieve the changes in the contact information for one or more persons.
    /// </summary>
    public string? ContactInformationChangesEndpoint { get; init; }

    /// <summary>
    /// Gets or sets the Maskinporten settings.
    /// </summary>
    public MaskinportenSettings? Maskinporten { get; init; }
}
