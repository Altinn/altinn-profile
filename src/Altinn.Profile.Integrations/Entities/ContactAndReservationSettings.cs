#nullable enable

using Altinn.ApiClients.Maskinporten.Config;
using Altinn.Profile.Core.ContactRegsiter;

namespace Altinn.Profile.Integrations.Entities;

/// <summary>
/// Represents the settings for managing contact details and reservation information for individuals.
/// </summary>
public class ContactAndReservationSettings : IContactRegisterSettings
{
    /// <summary>
    /// Gets the endpoint URL used to retrieve updates in the contact information for one or more individuals.
    /// </summary>
    public string? ChangesLogEndpoint { get; init; }

    /// <summary>
    /// Gets the settings required for Maskinporten authentication.
    /// </summary>
    public MaskinportenSettings? MaskinportenSettings { get; init; }
}
