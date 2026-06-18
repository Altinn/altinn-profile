using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Profile.Tests.IntegrationTests.Mocks;
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
        new RegisterHandlerBuilder()
            .OnJson(
                request => IsIdentifiersRequest(request)
                        && request.RequestUri.Query.Contains(partyUuid.ToString(), StringComparison.OrdinalIgnoreCase),
                new[] { new { partyId, partyUuid, orgNumber = string.Empty } })
            .Apply(factory);
    }

    internal static void SetupRegisterMainUnitLookup(ProfileWebApplicationFactory<Program> factory, string parentOrgNumber)
    {
        new RegisterHandlerBuilder()
            .OnJson(
                IsMainUnitsRequest,
                new { data = new[] { new { organizationIdentifier = parentOrgNumber } } })
            .Apply(factory);
    }

    internal static void SetupRegisterPartyLookupForAuthorization(
        ProfileWebApplicationFactory<Program> factory,
        Guid partyUuid,
        int identifiersPartyId,
        string organizationNumber,
        int partyQueryPartyId = 12345)
    {
        new RegisterHandlerBuilder()
            .OnJson(
                IsIdentifiersRequest,
                new[] { new { partyId = identifiersPartyId, partyUuid, orgNumber = organizationNumber } })
            .OnJson(
                IsPartyQueryRequest,
                new { data = new[] { new { partyId = partyQueryPartyId, partyUuid, organizationIdentifier = organizationNumber } } })
            .Apply(factory);
    }

    internal static void SetupRegisterUserPartyAndPartyIdLookup(
        ProfileWebApplicationFactory<Program> factory,
        Party userParty,
        Guid partyUuid,
        int partyId,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        new RegisterHandlerBuilder()
            .On(
                IsPartyQueryRequest,
                (request, cancellationToken) => HandlePartyQueryForSingleUserAsync(request, userParty, statusCode, cancellationToken))
            .OnJson(
                request => IsIdentifiersRequest(request)
                        && request.RequestUri.Query.Contains(partyUuid.ToString(), StringComparison.OrdinalIgnoreCase),
                new[] { new { partyId, partyUuid, orgNumber = string.Empty } })
            .Apply(factory);
    }

    internal static void SetupRegisterUserPartyLookup(ProfileWebApplicationFactory<Program> factory, Party userParty, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        new RegisterHandlerBuilder()
            .On(
                IsPartyQueryRequest,
                (request, cancellationToken) => HandlePartyQueryForSingleUserAsync(request, userParty, statusCode, cancellationToken))
            .Apply(factory);
    }

    internal static void SetupRegisterUserPartyByUserIdWithPartyQueryAndIdentifiersLookup(
        ProfileWebApplicationFactory<Program> factory,
        IReadOnlyDictionary<int, Party> userPartiesByUserId,
        IReadOnlyDictionary<Guid, string> organizationNumbersByPartyUuid,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        new RegisterHandlerBuilder()
            .On(
                IsPartyQueryRequest,
                (request, token) => HandlePartyQueryForMultipleUsersAsync(request, token, userPartiesByUserId, statusCode))
            .On(
                IsIdentifiersRequest,
                request => CreateIdentifiersResponse(request, organizationNumbersByPartyUuid))
            .Apply(factory);
    }

    internal static void SetupRegisterUserPartyWithPartyQuery(
        ProfileWebApplicationFactory<Program> factory,
        IReadOnlyDictionary<int, Party> userPartiesByUserId,
        IReadOnlyDictionary<Guid, string> organizationNumbersByPartyUuid,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        new RegisterHandlerBuilder()
            .On(
                request => IsPartyQueryRequest(request) && request.RequestUri.Query.Contains("user", StringComparison.OrdinalIgnoreCase),
                (request, token) => HandlePartyQueryForMultipleUsersAsync(request, token, userPartiesByUserId, statusCode))
            .On(
                request => IsPartyQueryRequest(request) && request.RequestUri.Query.Contains("org-id", StringComparison.OrdinalIgnoreCase),
                (request, token) => CreateOrgPartyQueryResponseAsync(request, organizationNumbersByPartyUuid, HttpStatusCode.OK, token))
            .Apply(factory);
    }

    internal static void SetupRegisterPartyQuery(
        ProfileWebApplicationFactory<Program> factory,
        IReadOnlyDictionary<Guid, string> organizationNumbersByPartyUuid,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        new RegisterHandlerBuilder()
            .On(
                IsPartyQueryRequest,
                (request, token) => CreateOrgPartyQueryResponseAsync(request, organizationNumbersByPartyUuid, HttpStatusCode.OK, token))
            .Apply(factory);
    }

    internal static void SetupRegisterPartyQueryLookup(ProfileWebApplicationFactory<Program> factory, Func<string[], List<Core.Unit.ContactPoints.Party>> responseFactory)
    {
        new RegisterHandlerBuilder()
            .On(
                IsPartyQueryRequest,
                async (request, token) =>
                {
                    if (request.Content == null)
                    {
                        return new HttpResponseMessage(HttpStatusCode.BadRequest);
                    }

                    string[] orgNumbers = await GetRequestedOrganizationNumbersFromQueryAsync(request, token);
                    List<Core.Unit.ContactPoints.Party> parties = responseFactory(orgNumbers);
                    return parties == null
                        ? new HttpResponseMessage(HttpStatusCode.InternalServerError)
                        : RegisterHandlerBuilder.OkJson(new { data = parties });
                })
            .Apply(factory);
    }

    internal static void SetupRegisterIdentifiersLookup(ProfileWebApplicationFactory<Program> factory, IReadOnlyDictionary<Guid, string> organizationNumbersByPartyUuid)
    {
        new RegisterHandlerBuilder()
            .On(
                IsIdentifiersRequest,
                (request, _) => Task.FromResult(CreateIdentifiersResponse(request, organizationNumbersByPartyUuid) ?? RegisterHandlerBuilder.NotFound()))
            .Apply(factory);
    }

    // Predicate helpers
    private static bool IsIdentifiersRequest(HttpRequestMessage request)
        => request.Method == HttpMethod.Get
        && request.RequestUri?.AbsolutePath.EndsWith(IdentifiersPath, StringComparison.Ordinal) == true;

    private static bool IsPartyQueryRequest(HttpRequestMessage request)
        => request.Method == HttpMethod.Post
        && request.RequestUri?.AbsolutePath.EndsWith(PartyQueryPath, StringComparison.Ordinal) == true;

    private static bool IsMainUnitsRequest(HttpRequestMessage request)
        => request.Method == HttpMethod.Post
        && request.RequestUri?.AbsolutePath.EndsWith(MainUnitsPath, StringComparison.Ordinal) == true;

    // Shared response-building logic 
    private static async Task<HttpResponseMessage> HandlePartyQueryForSingleUserAsync(
        HttpRequestMessage request,
        Party userParty,
        HttpStatusCode statusCode,
        CancellationToken cancellationToken)
    {
        if (statusCode != HttpStatusCode.OK)
        {
            return new HttpResponseMessage(statusCode);
        }

        if (request.Content == null)
        {
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }

        return RegisterHandlerBuilder.OkJson(new { data = userParty is null ? [] : new[] { userParty } });
    }

    private static async Task<HttpResponseMessage> HandlePartyQueryForMultipleUsersAsync(
        HttpRequestMessage request,
        CancellationToken token,
        IReadOnlyDictionary<int, Party> userPartiesByUserId,
        HttpStatusCode statusCode)
    {
        if (statusCode != HttpStatusCode.OK)
        {
            return new HttpResponseMessage(statusCode);
        }

        string[] requestedIdentifiers = await GetRequestedIdentifiersFromQueryAsync(request, token);

        string[] userIdIdentifiers = requestedIdentifiers
            .Where(id => id.StartsWith("urn:altinn:user:id:", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (userIdIdentifiers.Length > 0)
        {
            var userParties = userIdIdentifiers
                .Select(id => id.Split(':').Last())
                .Where(v => int.TryParse(v, out _))
                .Select(int.Parse)
                .Where(userPartiesByUserId.ContainsKey)
                .Select(userId => userPartiesByUserId[userId])
                .Where(p => p is not null)
                .ToArray();

            return RegisterHandlerBuilder.OkJson(new { data = userParties });
        }

        return RegisterHandlerBuilder.NotFound();
    }

    private static async Task<HttpResponseMessage> CreateOrgPartyQueryResponseAsync(
        HttpRequestMessage request,
        IReadOnlyDictionary<Guid, string> organizationNumbersByPartyUuid,
        HttpStatusCode partyQueryStatusCode,
        CancellationToken token)
    {
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

        return RegisterHandlerBuilder.OkJson(new { data = responseData });
    }

    private static HttpResponseMessage CreateIdentifiersResponse(HttpRequestMessage request, IReadOnlyDictionary<Guid, string> organizationNumbersByPartyUuid)
    {
        Guid[] uuids = GetUuidsFromIdentifiersQuery(request.RequestUri.Query);
        var responseData = uuids
            .Where(organizationNumbersByPartyUuid.ContainsKey)
            .Select((partyUuid, index) => new
            {
                partyId = index + 1,
                partyUuid,
                orgNumber = organizationNumbersByPartyUuid[partyUuid],
            })
            .ToArray();

        return RegisterHandlerBuilder.OkJson(responseData);
    }

    // Query parsing helpers
    internal static Guid[] GetUuidsFromIdentifiersQuery(string query)
    {
        Dictionary<string, StringValues> queryValues = QueryHelpers.ParseQuery(query);
        if (!queryValues.TryGetValue("uuids", out StringValues uuidQueryValues))
        {
            return [];
        }

        return [.. uuidQueryValues
            .SelectMany(value => value.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(value => Guid.TryParse(value, out Guid parsed) ? parsed : Guid.Empty)
            .Where(value => value != Guid.Empty)];
    }

    private static async Task<string[]> GetRequestedOrganizationNumbersFromQueryAsync(HttpRequestMessage request, CancellationToken token)
    {
        string[] requestedIdentifiers = await GetRequestedIdentifiersFromQueryAsync(request, token);
        return requestedIdentifiers.Select(ExtractOrganizationNumberFromUrn).ToArray();
    }

    private static async Task<string[]> GetRequestedIdentifiersFromQueryAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Content == null)
        {
            return [];
        }

        await using var stream = await request.Content.ReadAsStreamAsync(cancellationToken);
        using JsonDocument jsonDocument = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
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

    private static string ExtractOrganizationNumberFromUrn(string urn)
    {
        int separatorIndex = urn.LastIndexOf(':');
        return separatorIndex >= 0 ? urn[(separatorIndex + 1)..] : urn;
    }
}
