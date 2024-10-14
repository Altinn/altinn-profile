using System.Text;
using System.Text.Json;

using Altinn.Profile.Integrations.Entities;

namespace Altinn.Profile.Integrations;

/// <summary>
/// Implementation of <see cref="IContactDetailsHttpClient"/> to handle contact details via HTTP.
/// </summary>
public class ContactDetailsHttpClient : IContactDetailsHttpClient
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContactDetailsHttpClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client instance to be used for making requests.</param>
    /// <exception cref="ArgumentNullException">Thrown when the provided <paramref name="httpClient"/> is null.</exception>
    public ContactDetailsHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>
    /// Retrieves contact details changes from the specified endpoint.
    /// </summary>
    /// <param name="endpointUrl">The URL of the endpoint to retrieve contact details changes from.</param>
    /// <param name="startIndex">The starting index for retrieving contact details changes.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the HTTP response message.</returns>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="startIndex"/> is less than zero.</exception>
    public async Task<IEnumerable<IPersonNotificationStatusChangeLog>?> GetContactDetailsChangesAsync(string endpointUrl, string startIndex)
    {
        if (string.IsNullOrWhiteSpace(endpointUrl))
        {
            throw new ArgumentNullException(nameof(endpointUrl), "The endpoint address is missing.");
        }

        if (int.TryParse(startIndex, out var startIndexAt))
        {
            throw new ArgumentException(nameof(startIndex), "The start index cannot be less than zero.");
        }

        if (startIndexAt < 0)
        {
            throw new ArgumentException(nameof(startIndex), "The start index cannot be less than zero.");
        }

        var request = new HttpRequestMessage(HttpMethod.Post, endpointUrl)
        {
            Content = new StringContent($"{{\"fraEndringsId\": {startIndex}}}", Encoding.UTF8, "application/json")
        };

        try
        {
            var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            var responseData = await response.Content.ReadAsStringAsync();

            var responseObject = JsonSerializer.Deserialize<PersonContactDetailsListFromChangeLog>(responseData);
            if (responseObject == null || responseObject.ContactDetailsList == null)
            {
                return null;
            }

            var contactDetails = new List<IPersonNotificationStatusChangeLog>();
            foreach (var person in responseObject.ContactDetailsList)
            {
                contactDetails.Add(new PersonNotificationStatusChangeLog
                {
                    ContactInfoChangeLog = new PersonContactDetailsFromChangeLog
                    {
                        EmailAddress = person.ContactInfoChangeLog?.EmailAddress,
                        MobilePhoneNumber = person.ContactInfoChangeLog?.MobilePhoneNumber,
                        EmailAddressUpdated = person.ContactInfoChangeLog?.EmailAddressUpdated,
                        MobilePhoneNumberUpdated = person.ContactInfoChangeLog?.MobilePhoneNumberUpdated,
                        IsEmailAddressDuplicated = person.ContactInfoChangeLog?.IsEmailAddressDuplicated,
                        EmailAddressLastVerified = person.ContactInfoChangeLog?.EmailAddressLastVerified,
                        MobilePhoneNumberLastVerified = person.ContactInfoChangeLog?.MobilePhoneNumberLastVerified,
                        IsMobilePhoneNumberDuplicated = person.ContactInfoChangeLog?.IsMobilePhoneNumberDuplicated
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
        catch
        {
            throw;
        }
    }
}
