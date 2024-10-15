using System;
using System.Text;
using System.Text.Json;

using Altinn.Profile.Integrations.Entities;
using Microsoft.Extensions.Logging;

namespace Altinn.Profile.Integrations;

/// <summary>
/// Implementation of <see cref="IContactDetailsHttpClient"/> to handle contact details via HTTP.
/// </summary>
public class ContactDetailsHttpClient : IContactDetailsHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ContactDetailsHttpClient> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContactDetailsHttpClient"/> class.
    /// </summary>
    /// <param name="logger">The logger instance used for logging.</param>
    /// <param name="httpClient">The service for retrieving the contact details.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the <paramref name="logger"/> or <paramref name="httpClient"/> is null.
    /// </exception>
    public ContactDetailsHttpClient(ILogger<ContactDetailsHttpClient> logger, HttpClient httpClient)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>
    /// Retrieves contact details changes from the specified endpoint.
    /// </summary>
    /// <param name="endpointUrl">The URL of the endpoint to retrieve contact details changes from.</param>
    /// <param name="margin">The starting index for retrieving contact details changes.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the HTTP response message.</returns>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="margin"/> is less than zero.</exception>
    public async Task<IEnumerable<IPersonContactPreferencesSnapshot>?> GetContactDetailsChangesAsync(string endpointUrl, string margin)
    {
        if (IsValidUrl(endpointUrl))
        {
            throw new ArgumentNullException(nameof(endpointUrl));
        }

        if (int.TryParse(margin, out var startIdentifier) || startIdentifier < 0)
        {
            throw new ArgumentNullException(nameof(margin));
        }

        var request = new HttpRequestMessage(HttpMethod.Post, endpointUrl)
        {
            Content = new StringContent($"{{\"fraEndringsId\": {startIdentifier}}}", Encoding.UTF8, "application/json")
        };

        try
        {
            var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            var responseData = await response.Content.ReadAsStringAsync();

            var responseObject = JsonSerializer.Deserialize<PersonChangesLog>(responseData);
            if (responseObject == null || responseObject.ContactDetailsList == null)
            {
                return null;
            }

            var contactDetails = new List<IPersonContactPreferencesSnapshot>();
            foreach (var person in responseObject.ContactDetailsList)
            {
                contactDetails.Add(new PersonContactPreferencesSnapshot
                {
                    PersonContactDetailsSnapshot = new PersonContactDetailsSnapshot
                    {
                        EmailAddress = person.PersonContactDetailsSnapshot?.EmailAddress,
                        MobilePhoneNumber = person.PersonContactDetailsSnapshot?.MobilePhoneNumber,
                        EmailAddressUpdated = person.PersonContactDetailsSnapshot?.EmailAddressUpdated,
                        MobilePhoneNumberUpdated = person.PersonContactDetailsSnapshot?.MobilePhoneNumberUpdated,
                        IsEmailAddressDuplicated = person.PersonContactDetailsSnapshot?.IsEmailAddressDuplicated,
                        EmailAddressLastVerified = person.PersonContactDetailsSnapshot?.EmailAddressLastVerified,
                        MobilePhoneNumberLastVerified = person.PersonContactDetailsSnapshot?.MobilePhoneNumberLastVerified,
                        IsMobilePhoneNumberDuplicated = person.PersonContactDetailsSnapshot?.IsMobilePhoneNumberDuplicated
                    },

                    Status = person.Status,
                    Language = person.Language,
                    Reservation = person.Reservation,
                    LanguageUpdated = person.LanguageUpdated,
                    PersonIdentifier = person.PersonIdentifier,
                    NotificationStatus = person.NotificationStatus
                });
            }

            return contactDetails;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving contact details changes.");

            throw;
        }
    }

    /// <summary>
    /// Validates the given URL.
    /// </summary>
    /// <param name="url">The URL to validate.</param>
    /// <returns>True if the URL is valid; otherwise, false.</returns>
    public static bool IsValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        try
        {
            var uri = new Uri(url);
            return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
        }
        catch (UriFormatException)
        {
            return false;
        }
    }
}
