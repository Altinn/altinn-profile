using System.Text;
using System.Text.Json;
using Altinn.Profile.Integrations.Entities;

namespace Altinn.Profile.Integrations;

/// <summary>
/// Interface for HTTP client to handle contact details.
/// </summary>
public interface IContactDetailsHttpClient
{
    /// <summary>
    /// Retrieves contact details changes from the specified endpoint.
    /// </summary>
    /// <param name="endpointUrl">The URL of the endpoint to retrieve contact details changes from.</param>
    /// <param name="startIndex">The starting index for retrieving contact details changes.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the HTTP response message.</returns>
    Task<IEnumerable<IPersonContactDetailsFromChangeLog>> GetContactDetailsChangesAsync(string endpointUrl, int startIndex);
}

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
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="endpointUrl"/> is null or empty.</exception>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="startIndex"/> is less than zero.</exception>
    public async Task<IEnumerable<IPersonContactDetailsFromChangeLog>> GetContactDetailsChangesAsync(string endpointUrl, int startIndex)
    {
        if (string.IsNullOrWhiteSpace(endpointUrl))
        {
            throw new ArgumentNullException(nameof(endpointUrl), "The endpoint address is missing.");
        }

        if (startIndex < 0)
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

            var responseObject = JsonSerializer.Deserialize<PersonContactDetailsFromChangeLog>(responseData);

            return null;
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}
