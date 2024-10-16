using Altinn.ApiClients.Maskinporten.Config;

namespace Altinn.Profile.Core.Integrations;

/// <summary>
/// Represents the information used for managing contact details and reservation information for individuals.
/// </summary>
public interface IContactReservation
{
    /// <summary>
    /// Gets the endpoint URL used to retrieve updates in the contact information for one or more individuals.
    /// </summary>
    string? ChangesLogEndpoint { get; }

    /// <summary>
    /// Gets the settings required for Maskinporten authentication.
    /// </summary>
    MaskinportenSettings? MaskinportenSettings { get; }
}
