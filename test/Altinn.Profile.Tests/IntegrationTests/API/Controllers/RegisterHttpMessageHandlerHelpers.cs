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
    private const string IdentifiersPath = "v1/parties/identifiers";
    private const string PartyQueryPath = "v2/internal/parties/query";
    private const string MainUnitsPath = "v2/internal/parties/main-units";

    internal static void SetupRegisterPartyIdLookup(ProfileWebApplicationFactory<Program> factory, Guid partyUuid, int partyId)
    {
        factory.RegisterHttpMessageHandler.ChangeHandlerFunction((request, token) =>
        {
            if (IsIdentifiersRequest(request)
                && request.RequestUri.Query.Contains(partyUuid.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(OkJson(new[]
                {
                    new
                    {
                        partyId,
                        partyUuid,
                        orgNumber = string.Empty,
                    }
                }));
            }

            return Task.FromResult(NotFound());
        });
    }

    internal static void SetupRegisterUserPartyByUserIdAndPartyIdLookup(ProfileWebApplicationFactory<Program> factory, int userId, Party userParty, Guid partyUuid, int partyId, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        string expectedIdentifier = $"urn:altinn:user:id:{userId}";

        factory.RegisterHttpMessageHandler.ChangeHandlerFunction(async (request, token) =>
        {
            if (IsPartyQueryRequest(request))
            {
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
                    return NotFound();
                }

                return OkJson(new
                {
                    data = userParty is null ? [] : new[] { userParty }
                });
            }

            if (IsIdentifiersRequest(request)
                && request.RequestUri.Query.Contains(partyUuid.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return OkJson(new[]
                {
                    new
                    {
                        partyId,
                        partyUuid,
                        orgNumber = string.Empty,
                    }
                });
            }

            return NotFound();
        });
    }

    internal static void SetupRegisterPartyLookupForAuthorization(ProfileWebApplicationFactory<Program> factory, Guid partyUuid, int identifiersPartyId, string organizationNumber, int partyQueryPartyId = 12345)
    {
        factory.RegisterHttpMessageHandler.ChangeHandlerFunction((request, token) =>
        {
            if (IsIdentifiersRequest(request))
            {
                return Task.FromResult(OkJson(new[]
                {
                    new
                    {
                        partyId = identifiersPartyId,
                        partyUuid,
                        orgNumber = organizationNumber,
                    }
                }));
            }

            if (IsPartyQueryRequest(request))
            {
                return Task.FromResult(OkJson(new
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
                }));
            }

            return Task.FromResult(NotFound());
        });
    }

    internal static void SetupRegisterMainUnitLookup(ProfileWebApplicationFactory<Program> factory, string parentOrgNumber)
    {
        factory.RegisterHttpMessageHandler.ChangeHandlerFunction((request, token) =>
        {
            if (IsMainUnitsRequest(request))
            {
                return Task.FromResult(OkJson(new { data = new[] { new { organizationIdentifier = parentOrgNumber } } }));
            }

            return Task.FromResult(NotFound());
        });
    }

    internal static void SetupRegisterUserPartyByUserIdLookup(ProfileWebApplicationFactory<Program> factory, int userId, Party userParty, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        string expectedIdentifier = $"urn:altinn:user:id:{userId}";
        SetupRegisterUserPartyLookup(factory, expectedIdentifier, userParty, statusCode);
    }

    internal static void SetupRegisterUserPartyByUserIdWithPartyQueryAndIdentifiersLookup(ProfileWebApplicationFactory<Program> factory, IReadOnlyDictionary<int, Party> userPartiesByUserId, IReadOnlyDictionary<Guid, string> organizationNumbersByPartyUuid, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        factory.RegisterHttpMessageHandler.ChangeHandlerFunction(async (request, token) =>
        {
            if (IsPartyQueryRequest(request))
            {
                if (statusCode != HttpStatusCode.OK)
                {
                    return new HttpResponseMessage(statusCode);
                }

                string[] requestedIdentifiers = await GetRequestedIdentifiersFromQueryAsync(request, token);
                string[] requestedUserIdIdentifiers = requestedIdentifiers
                    .Where(identifier => identifier.StartsWith("urn:altinn:user:id:", StringComparison.OrdinalIgnoreCase))
                    .ToArray();

                if (requestedUserIdIdentifiers.Length > 0)
                {
                    var userParties = requestedUserIdIdentifiers
                        .Select(identifier => identifier.Split(':').Last())
                        .Where(value => int.TryParse(value, out _))
                        .Select(int.Parse)
                        .Where(userPartiesByUserId.ContainsKey)
                        .Select(userId => userPartiesByUserId[userId])
                        .Where(party => party is not null)
                        .ToArray();

                    return OkJson(new { data = userParties });
                }

                HttpResponseMessage partyQueryResponse = await TryCreatePartyQueryResponseAsync(request, organizationNumbersByPartyUuid, HttpStatusCode.OK, token);
                if (partyQueryResponse is not null)
                {
                    return partyQueryResponse;
                }
            }

            HttpResponseMessage identifiersResponse = TryCreateIdentifiersResponse(request, organizationNumbersByPartyUuid);
            if (identifiersResponse is not null)
            {
                return identifiersResponse;
            }

            return NotFound();
        });
    }

    internal static void SetupRegisterUserPartyByUserUuidLookup(ProfileWebApplicationFactory<Program> factory, Guid userUuid, Party userParty, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        string expectedIdentifier = $"urn:altinn:party:uuid:{userUuid}";
        SetupRegisterUserPartyLookup(factory, expectedIdentifier, userParty, statusCode);
    }

    internal static void SetupRegisterUserPartyByUsernameLookup(ProfileWebApplicationFactory<Program> factory, string username, Party userParty, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        string expectedIdentifier = $"urn:altinn:party:username:{username}";
        SetupRegisterUserPartyLookup(factory, expectedIdentifier, userParty, statusCode);
    }

    internal static void SetupRegisterUserPartyByUserSsnLookup(ProfileWebApplicationFactory<Program> factory, string ssn, Party userParty, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        string expectedIdentifier = $"urn:altinn:person:identifier-no:{ssn}";
        SetupRegisterUserPartyLookup(factory, expectedIdentifier, userParty, statusCode);
    }

    internal static void SetupRegisterPartyQueryLookup(ProfileWebApplicationFactory<Program> factory, IReadOnlyDictionary<Guid, string> organizationNumbersByPartyUuid)
    {
        factory.RegisterHttpMessageHandler.ChangeHandlerFunction(async (request, token) =>
        {
            HttpResponseMessage partyQueryResponse = await TryCreatePartyQueryResponseAsync(request, organizationNumbersByPartyUuid, HttpStatusCode.OK, token);
            return partyQueryResponse ?? NotFound();
        });
    }

    internal static void SetupRegisterIdentifiersLookup(ProfileWebApplicationFactory<Program> factory, IReadOnlyDictionary<Guid, string> organizationNumbersByPartyUuid)
    {
        factory.RegisterHttpMessageHandler.ChangeHandlerFunction((request, token) =>
        {
            HttpResponseMessage identifiersResponse = TryCreateIdentifiersResponse(request, organizationNumbersByPartyUuid);
            return Task.FromResult(identifiersResponse ?? NotFound());
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
            HttpResponseMessage partyQueryResponse = await TryCreatePartyQueryResponseAsync(request, organizationNumbersByPartyUuid, partyQueryStatusCode, token);
            if (partyQueryResponse is not null)
            {
                return partyQueryResponse;
            }

            HttpResponseMessage identifiersResponse = TryCreateIdentifiersResponse(request, organizationNumbersByPartyUuid);
            if (identifiersResponse is not null)
            {
                return identifiersResponse;
            }

            return NotFound();
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
            if (!IsPartyQueryRequest(request))
            {
                return NotFound();
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
                return NotFound();
            }

            return OkJson(new
            {
                data = userParty is null ? [] : new[] { userParty }
            });
        });
    }

    private static void SetupRegisterPartyQueryLookupCore<TParty>(ProfileWebApplicationFactory<Program> factory, Func<string[], List<TParty>> responseFactory)
    {
        factory.RegisterHttpMessageHandler.ChangeHandlerFunction(async (request, token) =>
        {
            if (!IsPartyQueryRequest(request))
            {
                return NotFound();
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

            return OkJson(new { data = parties });
        });
    }

    private static async Task<HttpResponseMessage> TryCreatePartyQueryResponseAsync(HttpRequestMessage request, IReadOnlyDictionary<Guid, string> organizationNumbersByPartyUuid, HttpStatusCode partyQueryStatusCode, CancellationToken token)
    {
        if (!IsPartyQueryRequest(request))
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

        return OkJson(new { data = responseData });
    }

    private static HttpResponseMessage TryCreateIdentifiersResponse(HttpRequestMessage request, IReadOnlyDictionary<Guid, string> organizationNumbersByPartyUuid)
    {
        if (!IsIdentifiersRequest(request))
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

        return OkJson(responseData);
    }

    private static async Task<string[]> GetRequestedOrganizationNumbersFromQueryAsync(HttpRequestMessage request, CancellationToken token)
    {
        string[] requestedIdentifiers = await GetRequestedIdentifiersFromQueryAsync(request, token);

        return requestedIdentifiers
            .Select(ExtractOrganizationNumberFromUrn)
            .ToArray();
    }

    private static async Task<string[]> GetRequestedIdentifiersFromQueryAsync(HttpRequestMessage request, CancellationToken token)
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
            .Select(value => value!)
            .ToArray();
    }

    private static bool IsIdentifiersRequest(HttpRequestMessage request)
    {
        return request.Method == HttpMethod.Get
            && request.RequestUri?.AbsolutePath.EndsWith(IdentifiersPath, StringComparison.Ordinal) == true;
    }

    private static bool IsPartyQueryRequest(HttpRequestMessage request)
    {
        return request.Method == HttpMethod.Post
            && request.RequestUri?.AbsolutePath.EndsWith(PartyQueryPath, StringComparison.Ordinal) == true;
    }

    private static bool IsMainUnitsRequest(HttpRequestMessage request)
    {
        return request.Method == HttpMethod.Post
            && request.RequestUri?.AbsolutePath.EndsWith(MainUnitsPath, StringComparison.Ordinal) == true;
    }

    private static HttpResponseMessage OkJson<T>(T payload)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(payload)
        };
    }

    private static HttpResponseMessage NotFound()
    {
        return new HttpResponseMessage(HttpStatusCode.NotFound);
    }

    private static string ExtractOrganizationNumberFromUrn(string urn)
    {
        int separatorIndex = urn.LastIndexOf(':');
        return separatorIndex >= 0
            ? urn[(separatorIndex + 1)..]
            : urn;
    }
}
