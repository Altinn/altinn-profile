using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Register.Contracts;

using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace Altinn.Profile.Tests.IntegrationTests.API.Controllers;

internal static class RegisterHttpMessageHandlerHelpers
{
    internal static void SetupRegisterPartyIdLookup(ProfileWebApplicationFactory<Program> factory, Guid partyUuid, int partyId)
    {
        factory.RegisterHttpMessageHandler.ChangeHandlerFunction((request, token) =>
        {
            if (request.Method == HttpMethod.Get
                && request.RequestUri?.AbsolutePath.EndsWith("v1/parties/identifiers", StringComparison.Ordinal) == true
                && request.RequestUri.Query.Contains(partyUuid.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(new[]
                    {
                        new
                        {
                            partyId,
                            partyUuid,
                            orgNumber = string.Empty,
                        }
                    })
                });
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        });
    }

    internal static void SetupRegisterPartyLookupForAuthorization(ProfileWebApplicationFactory<Program> factory, Guid partyUuid, int identifiersPartyId, string organizationNumber, int partyQueryPartyId = 12345)
    {
        factory.RegisterHttpMessageHandler.ChangeHandlerFunction((request, token) =>
        {
            if (request.Method == HttpMethod.Get
                && request.RequestUri?.AbsolutePath.EndsWith("v1/parties/identifiers", StringComparison.Ordinal) == true)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(new[]
                    {
                        new
                        {
                            partyId = identifiersPartyId,
                            partyUuid,
                            orgNumber = organizationNumber,
                        }
                    })
                });
            }

            if (request.Method == HttpMethod.Post
                && request.RequestUri?.AbsolutePath.EndsWith("v2/internal/parties/query", StringComparison.Ordinal) == true)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(new
                    {
                        data = new[]
                        {
                            new
                            {
                                partyId = partyQueryPartyId,
                                partyUuid,
                                organizationIdentifier = organizationNumber,
                            }
                        }
                    })
                });
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        });
    }

    internal static void SetupRegisterMainUnitLookup(ProfileWebApplicationFactory<Program> factory, string parentOrgNumber)
    {
        factory.RegisterHttpMessageHandler.ChangeHandlerFunction((request, token) =>
        {
            if (request.Method == HttpMethod.Post
                && request.RequestUri?.AbsolutePath.EndsWith("v2/internal/parties/main-units", StringComparison.Ordinal) == true)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(new { data = new[] { new { organizationIdentifier = parentOrgNumber } } })
                });
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        });
    }

    internal static void SetupRegisterUserPartyByUserIdLookup(ProfileWebApplicationFactory<Program> factory, int userId, Party userParty, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        string expectedIdentifier = $"urn:altinn:user:id:{userId}";
        SetupRegisterUserPartyLookup(factory, expectedIdentifier, userParty, statusCode);
    }

    internal static void SetupRegisterUserPartyByUsernameLookup(ProfileWebApplicationFactory<Program> factory, string username, Party userParty, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        string expectedIdentifier = $"urn:altinn:party:username:{username}";
        SetupRegisterUserPartyLookup(factory, expectedIdentifier, userParty, statusCode);
    }

    internal static void SetupRegisterPartyQueryLookup(ProfileWebApplicationFactory<Program> factory, IReadOnlyDictionary<Guid, string> organizationNumbersByPartyUuid)
    {
        factory.RegisterHttpMessageHandler.ChangeHandlerFunction(async (request, token) =>
        {
            HttpResponseMessage partyQueryResponse = await TryCreatePartyQueryResponseAsync(request, token, organizationNumbersByPartyUuid, HttpStatusCode.OK);
            return partyQueryResponse ?? new HttpResponseMessage(HttpStatusCode.NotFound);
        });
    }

    internal static void SetupRegisterIdentifiersLookup(ProfileWebApplicationFactory<Program> factory, IReadOnlyDictionary<Guid, string> organizationNumbersByPartyUuid)
    {
        factory.RegisterHttpMessageHandler.ChangeHandlerFunction((request, token) =>
        {
            HttpResponseMessage identifiersResponse = TryCreateIdentifiersResponse(request, organizationNumbersByPartyUuid);
            return Task.FromResult(identifiersResponse ?? new HttpResponseMessage(HttpStatusCode.NotFound));
        });
    }

    internal static Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> CreateCombinedRegisterPartyQueryAndIdentifiersHandler(IReadOnlyDictionary<Guid, string> organizationNumbersByPartyUuid)
    {
        return CreateCombinedRegisterPartyQueryAndIdentifiersHandler(organizationNumbersByPartyUuid, HttpStatusCode.OK);
    }

    internal static Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> CreateCombinedRegisterPartyQueryAndIdentifiersHandler(IReadOnlyDictionary<Guid, string> organizationNumbersByPartyUuid, HttpStatusCode partyQueryStatusCode)
    {
        return async (request, token) =>
        {
            HttpResponseMessage partyQueryResponse = await TryCreatePartyQueryResponseAsync(request, token, organizationNumbersByPartyUuid, partyQueryStatusCode);
            if (partyQueryResponse is not null)
            {
                return partyQueryResponse;
            }

            HttpResponseMessage identifiersResponse = TryCreateIdentifiersResponse(request, organizationNumbersByPartyUuid);
            if (identifiersResponse is not null)
            {
                return identifiersResponse;
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        };
    }

    internal static void SetupRegisterOrganizationNumberLookup(ProfileWebApplicationFactory<Program> factory, IReadOnlyDictionary<Guid, string> organizationNumbersByPartyUuid)
    {
        SetupRegisterIdentifiersLookup(factory, organizationNumbersByPartyUuid);
    }

    internal static void SetupRegisterPartyQueryLookup(ProfileWebApplicationFactory<Program> factory, Func<string[], List<Party>> responseFactory)
    {
        SetupRegisterPartyQueryLookupCore(factory, responseFactory);
    }

    internal static void SetupRegisterPartyQueryLookup(ProfileWebApplicationFactory<Program> factory, Func<string[], List<Core.Unit.ContactPoints.Party>> responseFactory)
    {
        SetupRegisterPartyQueryLookupCore(factory, responseFactory);
    }

    internal static Guid[] GetUuidsFromIdentifiersQuery(string query)
    {
        Dictionary<string, StringValues> queryValues = QueryHelpers.ParseQuery(query);
        if (!queryValues.TryGetValue("uuids", out StringValues uuidQueryValues))
        {
            return [];
        }

        return uuidQueryValues
            .SelectMany(value => value.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(value => Guid.TryParse(value, out Guid parsed) ? parsed : Guid.Empty)
            .Where(value => value != Guid.Empty)
            .ToArray();
    }

    private static void SetupRegisterUserPartyLookup(ProfileWebApplicationFactory<Program> factory, string expectedIdentifier, Party userParty, HttpStatusCode statusCode)
    {
        factory.RegisterHttpMessageHandler.ChangeHandlerFunction(async (request, token) =>
        {
            if (request.Method != HttpMethod.Post
                || request.RequestUri?.AbsolutePath.EndsWith("v2/internal/parties/query", StringComparison.Ordinal) != true)
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            if (statusCode != HttpStatusCode.OK)
            {
                return new HttpResponseMessage(statusCode);
            }

            if (request.Content == null)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            await using var stream = await request.Content.ReadAsStreamAsync(token);
            using JsonDocument jsonDocument = await JsonDocument.ParseAsync(stream, cancellationToken: token);

            bool hasMatch = false;
            if (jsonDocument.RootElement.TryGetProperty("data", out JsonElement dataElement) && dataElement.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement element in dataElement.EnumerateArray())
                {
                    if (element.ValueKind == JsonValueKind.String && string.Equals(element.GetString(), expectedIdentifier, StringComparison.Ordinal))
                    {
                        hasMatch = true;
                        break;
                    }
                }
            }

            if (!hasMatch)
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new
                {
                    data = userParty is null ? [] : new[] { userParty }
                })
            };
        });
    }

    private static void SetupRegisterPartyQueryLookupCore<TParty>(ProfileWebApplicationFactory<Program> factory, Func<string[], List<TParty>> responseFactory)
    {
        factory.RegisterHttpMessageHandler.ChangeHandlerFunction(async (request, token) =>
        {
            if (request.Method != HttpMethod.Post
                || request.RequestUri?.AbsolutePath.EndsWith("v2/internal/parties/query", StringComparison.Ordinal) != true)
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            if (request.Content == null)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            string[] orgNumbers = await GetRequestedOrganizationNumbersFromQueryAsync(request, token);
            List<TParty> parties = responseFactory(orgNumbers);
            if (parties == null)
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new { data = parties })
            };
        });
    }

    private static async Task<HttpResponseMessage> TryCreatePartyQueryResponseAsync(HttpRequestMessage request, CancellationToken token, IReadOnlyDictionary<Guid, string> organizationNumbersByPartyUuid, HttpStatusCode partyQueryStatusCode)
    {
        if (request.Method != HttpMethod.Post
            || request.RequestUri?.AbsolutePath.EndsWith("v2/internal/parties/query", StringComparison.Ordinal) != true)
        {
            return null;
        }

        if (partyQueryStatusCode != HttpStatusCode.OK)
        {
            return new HttpResponseMessage(partyQueryStatusCode);
        }

        string[] requestedOrgNumbers = await GetRequestedOrganizationNumbersFromQueryAsync(request, token);
        var responseData = organizationNumbersByPartyUuid
            .Where(entry => requestedOrgNumbers.Length == 0 || requestedOrgNumbers.Contains(entry.Value, StringComparer.Ordinal))
            .Select((entry, index) => new
            {
                partyId = index + 1,
                partyUuid = entry.Key,
                organizationIdentifier = entry.Value,
            })
            .ToArray();

        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new { data = responseData })
        };
    }

    private static HttpResponseMessage TryCreateIdentifiersResponse(HttpRequestMessage request, IReadOnlyDictionary<Guid, string> organizationNumbersByPartyUuid)
    {
        if (request.Method != HttpMethod.Get
            || request.RequestUri?.AbsolutePath.EndsWith("v1/parties/identifiers", StringComparison.Ordinal) != true)
        {
            return null;
        }

        Guid[] uuids = GetUuidsFromIdentifiersQuery(request.RequestUri.Query);
        var responseData = uuids
            .Where(partyUuid => organizationNumbersByPartyUuid.ContainsKey(partyUuid))
            .Select((partyUuid, index) => new
            {
                partyId = index + 1,
                partyUuid,
                orgNumber = organizationNumbersByPartyUuid[partyUuid],
            })
            .ToArray();

        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseData)
        };
    }

    private static async Task<string[]> GetRequestedOrganizationNumbersFromQueryAsync(HttpRequestMessage request, CancellationToken token)
    {
        if (request.Content == null)
        {
            return [];
        }

        await using var stream = await request.Content.ReadAsStreamAsync(token);
        using JsonDocument jsonDocument = await JsonDocument.ParseAsync(stream, cancellationToken: token);
        if (!jsonDocument.RootElement.TryGetProperty("data", out JsonElement dataElement) || dataElement.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return dataElement
            .EnumerateArray()
            .Where(element => element.ValueKind == JsonValueKind.String)
            .Select(element => element.GetString())
            .Where(value => !string.IsNullOrEmpty(value))
            .Select(value => ExtractOrganizationNumberFromUrn(value!))
            .ToArray();
    }

    private static string ExtractOrganizationNumberFromUrn(string urn)
    {
        int separatorIndex = urn.LastIndexOf(':');
        return separatorIndex >= 0
            ? urn[(separatorIndex + 1)..]
            : urn;
    }
}
