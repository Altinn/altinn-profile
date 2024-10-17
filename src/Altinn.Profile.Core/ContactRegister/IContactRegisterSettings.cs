using Altinn.ApiClients.Maskinporten.Config;

namespace Altinn.Profile.Core.ContactRegister;

/// <summary>
/// Represents settings used to communicate with the contact details register.
/// </summary>
public interface IContactRegisterSettings
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
