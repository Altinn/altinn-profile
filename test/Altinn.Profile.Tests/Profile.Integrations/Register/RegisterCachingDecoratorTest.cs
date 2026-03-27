using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Altinn.Authorization.ModelUtils;
using Altinn.Profile.Core;
using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Integrations.Register;
using Altinn.Register.Contracts;
using Altinn.Register.Contracts.Testing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using ContactPointParty = Altinn.Profile.Core.Unit.ContactPoints.Party;
using RegisterParty = Altinn.Register.Contracts.Party;

namespace Altinn.Profile.Tests.Profile.Integrations.Register;

public class RegisterCachingDecoratorTest
{
    private const string UserCacheKeyPrefix = "Party_User_UserId_";

    private readonly Mock<IRegisterClient> _decoratedServiceMock = new();
    private readonly Mock<IOptions<CoreSettings>> _coreSettingsOptions = new();

    public RegisterCachingDecoratorTest()
    {
        _decoratedServiceMock.Reset();
        _coreSettingsOptions
            .Setup(s => s.Value)
            .Returns(new CoreSettings { ProfileCacheLifetimeSeconds = 600 });
    }

    [Fact]
    public async Task GetMainUnit_MainUnitInCache_DecoratedServiceNotCalled()
    {
        const string orgNumber = "310494145";
        const string mainUnit = "910067494";
        MemoryCache memoryCache = CreateMemoryCache();
        memoryCache.Set($"MainUnit_{orgNumber}", mainUnit);

        RegisterCachingDecorator target = CreateTarget(memoryCache);

        string result = await target.GetMainUnit(orgNumber, CancellationToken.None);

        Assert.Equal(mainUnit, result);
        _decoratedServiceMock.Verify(service => service.GetMainUnit(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task GetMainUnit_MainUnitNotInCache_DecoratedServiceCalledAndCachePopulated()
    {
        const string orgNumber = "310494145";
        const string mainUnit = "910067494";
        MemoryCache memoryCache = CreateMemoryCache();

        _decoratedServiceMock
            .Setup(service => service.GetMainUnit(orgNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mainUnit);

        RegisterCachingDecorator target = CreateTarget(memoryCache);

        string result = await target.GetMainUnit(orgNumber, CancellationToken.None);

        Assert.Equal(mainUnit, result);
        _decoratedServiceMock.Verify(service => service.GetMainUnit(orgNumber, It.IsAny<CancellationToken>()), Times.Once());
        Assert.True(memoryCache.TryGetValue($"MainUnit_{orgNumber}", out string cachedMainUnit));
        Assert.Equal(mainUnit, cachedMainUnit);
    }

    [Fact]
    public async Task GetMainUnit_NullFromDecoratedService_CacheNotPopulated()
    {
        const string orgNumber = "310494145";
        MemoryCache memoryCache = CreateMemoryCache();

        _decoratedServiceMock
            .Setup(service => service.GetMainUnit(orgNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((string)null);

        RegisterCachingDecorator target = CreateTarget(memoryCache);

        string result = await target.GetMainUnit(orgNumber, CancellationToken.None);

        Assert.Null(result);
        _decoratedServiceMock.Verify(service => service.GetMainUnit(orgNumber, It.IsAny<CancellationToken>()), Times.Once());
        Assert.False(memoryCache.TryGetValue($"MainUnit_{orgNumber}", out string _));
    }

    [Fact]
    public async Task GetPartyId_PartyIdInCache_DecoratedServiceNotCalled()
    {
        Guid partyUuid = Guid.Parse("b1ad8a4a-54d8-4175-93db-f1dd1972c0a0");
        const int partyId = 51234567;
        MemoryCache memoryCache = CreateMemoryCache();
        memoryCache.Set($"PartyId_{partyUuid}", partyId);

        RegisterCachingDecorator target = CreateTarget(memoryCache);

        int? result = await target.GetPartyId(partyUuid, CancellationToken.None);

        Assert.Equal(partyId, result);
        _decoratedServiceMock.Verify(service => service.GetPartyId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task GetPartyId_PartyIdNotInCache_DecoratedServiceCalledAndCachePopulated()
    {
        Guid partyUuid = Guid.Parse("b1ad8a4a-54d8-4175-93db-f1dd1972c0a0");
        const int partyId = 51234567;
        MemoryCache memoryCache = CreateMemoryCache();

        _decoratedServiceMock
            .Setup(service => service.GetPartyId(partyUuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(partyId);

        RegisterCachingDecorator target = CreateTarget(memoryCache);

        int? result = await target.GetPartyId(partyUuid, CancellationToken.None);

        Assert.Equal(partyId, result);
        _decoratedServiceMock.Verify(service => service.GetPartyId(partyUuid, It.IsAny<CancellationToken>()), Times.Once());
        Assert.True(memoryCache.TryGetValue($"PartyId_{partyUuid}", out int cachedPartyId));
        Assert.Equal(partyId, cachedPartyId);
    }

    [Fact]
    public async Task GetPartyId_NullFromDecoratedService_CacheNotPopulated()
    {
        Guid partyUuid = Guid.Parse("b1ad8a4a-54d8-4175-93db-f1dd1972c0a0");
        MemoryCache memoryCache = CreateMemoryCache();

        _decoratedServiceMock
            .Setup(service => service.GetPartyId(partyUuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((int?)null);

        RegisterCachingDecorator target = CreateTarget(memoryCache);

        int? result = await target.GetPartyId(partyUuid, CancellationToken.None);

        Assert.Null(result);
        _decoratedServiceMock.Verify(service => service.GetPartyId(partyUuid, It.IsAny<CancellationToken>()), Times.Once());
        Assert.False(memoryCache.TryGetValue($"PartyId_{partyUuid}", out int _));
    }

    [Fact]
    public async Task GetPartyUuids_OrganizationsInCache_DecoratedServiceNotCalled()
    {
        ContactPointParty firstParty = CreateContactPointParty("310494145", 1);
        ContactPointParty secondParty = CreateContactPointParty("810494222", 2);
        string[] orgNumbers = [firstParty.OrganizationIdentifier, secondParty.OrganizationIdentifier];
        MemoryCache memoryCache = CreateMemoryCache();

        memoryCache.Set($"PartyUuid_{firstParty.OrganizationIdentifier}", firstParty);
        memoryCache.Set($"PartyUuid_{secondParty.OrganizationIdentifier}", secondParty);

        RegisterCachingDecorator target = CreateTarget(memoryCache);

        IReadOnlyList<ContactPointParty> result = await target.GetPartyUuids(orgNumbers, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, party => party.OrganizationIdentifier == firstParty.OrganizationIdentifier);
        Assert.Contains(result, party => party.OrganizationIdentifier == secondParty.OrganizationIdentifier);
        _decoratedServiceMock.Verify(service => service.GetPartyUuids(It.IsAny<string[]>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task GetPartyUuids_OrganizationsPartialInCache_DecoratedServiceCalledForMissingAndCachePopulated()
    {
        ContactPointParty cachedParty = CreateContactPointParty("310494145", 1);
        ContactPointParty fetchedParty = CreateContactPointParty("810494222", 2);
        string[] orgNumbers = [cachedParty.OrganizationIdentifier, fetchedParty.OrganizationIdentifier];
        string[] expectedMissingOrgNumbers = [fetchedParty.OrganizationIdentifier];
        MemoryCache memoryCache = CreateMemoryCache();
        memoryCache.Set($"PartyUuid_{cachedParty.OrganizationIdentifier}", cachedParty);

        _decoratedServiceMock
            .Setup(service => service.GetPartyUuids(
                It.Is<string[]>(numbers => numbers.SequenceEqual(expectedMissingOrgNumbers)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([fetchedParty]);

        RegisterCachingDecorator target = CreateTarget(memoryCache);

        IReadOnlyList<ContactPointParty> result = await target.GetPartyUuids(orgNumbers, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, party => party.OrganizationIdentifier == cachedParty.OrganizationIdentifier);
        Assert.Contains(result, party => party.OrganizationIdentifier == fetchedParty.OrganizationIdentifier);
        _decoratedServiceMock.Verify(
            service => service.GetPartyUuids(
                It.Is<string[]>(numbers => numbers.SequenceEqual(expectedMissingOrgNumbers)),
                It.IsAny<CancellationToken>()),
            Times.Once());
        Assert.True(memoryCache.TryGetValue($"PartyUuid_{fetchedParty.OrganizationIdentifier}", out ContactPointParty cachedFetchedParty));
        Assert.NotNull(cachedFetchedParty);
        Assert.Equal(fetchedParty.PartyUuid, cachedFetchedParty.PartyUuid);
    }

    [Fact]
    public async Task GetPartyUuids_NullFromDecoratedService_ReturnsCachedValuesOnly()
    {
        ContactPointParty cachedParty = CreateContactPointParty("310494145", 1);
        const string missingOrgNumber = "810494222";
        string[] orgNumbers = [cachedParty.OrganizationIdentifier, missingOrgNumber];
        string[] expectedMissingOrgNumbers = [missingOrgNumber];
        MemoryCache memoryCache = CreateMemoryCache();
        memoryCache.Set($"PartyUuid_{cachedParty.OrganizationIdentifier}", cachedParty);

        _decoratedServiceMock
            .Setup(service => service.GetPartyUuids(
                It.Is<string[]>(numbers => numbers.SequenceEqual(expectedMissingOrgNumbers)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<ContactPointParty>)null);

        RegisterCachingDecorator target = CreateTarget(memoryCache);

        IReadOnlyList<ContactPointParty> result = await target.GetPartyUuids(orgNumbers, CancellationToken.None);

        Assert.NotNull(result);
        ContactPointParty singleParty = Assert.Single(result);
        Assert.Equal(cachedParty.OrganizationIdentifier, singleParty.OrganizationIdentifier);
        Assert.False(memoryCache.TryGetValue($"PartyUuid_{missingOrgNumber}", out ContactPointParty _));
    }

    [Fact]
    public async Task GetOrganizationNumberByPartyUuid_OrganizationNumberInCache_DecoratedServiceNotCalled()
    {
        Guid partyUuid = Guid.Parse("21d53c0f-24e0-4f07-a397-7bfdb80d94f7");
        const string orgNumber = "310494145";
        MemoryCache memoryCache = CreateMemoryCache();
        memoryCache.Set($"OrgNumber_{partyUuid}", orgNumber);

        RegisterCachingDecorator target = CreateTarget(memoryCache);

        string result = await target.GetOrganizationNumberByPartyUuid(partyUuid, CancellationToken.None);

        Assert.Equal(orgNumber, result);
        _decoratedServiceMock.Verify(service => service.GetOrganizationNumberByPartyUuid(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task GetOrganizationNumberByPartyUuid_OrganizationNumberNotInCache_DecoratedServiceCalledAndCachePopulated()
    {
        Guid partyUuid = Guid.Parse("21d53c0f-24e0-4f07-a397-7bfdb80d94f7");
        const string orgNumber = "310494145";
        MemoryCache memoryCache = CreateMemoryCache();

        _decoratedServiceMock
            .Setup(service => service.GetOrganizationNumberByPartyUuid(partyUuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orgNumber);

        RegisterCachingDecorator target = CreateTarget(memoryCache);

        string result = await target.GetOrganizationNumberByPartyUuid(partyUuid, CancellationToken.None);

        Assert.Equal(orgNumber, result);
        _decoratedServiceMock.Verify(service => service.GetOrganizationNumberByPartyUuid(partyUuid, It.IsAny<CancellationToken>()), Times.Once());
        Assert.True(memoryCache.TryGetValue($"OrgNumber_{partyUuid}", out string cachedOrgNumber));
        Assert.Equal(orgNumber, cachedOrgNumber);
    }

    [Fact]
    public async Task GetOrganizationNumberByPartyUuid_NullFromDecoratedService_CacheNotPopulated()
    {
        Guid partyUuid = Guid.Parse("21d53c0f-24e0-4f07-a397-7bfdb80d94f7");
        MemoryCache memoryCache = CreateMemoryCache();

        _decoratedServiceMock
            .Setup(service => service.GetOrganizationNumberByPartyUuid(partyUuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((string)null);

        RegisterCachingDecorator target = CreateTarget(memoryCache);

        string result = await target.GetOrganizationNumberByPartyUuid(partyUuid, CancellationToken.None);

        Assert.Null(result);
        _decoratedServiceMock.Verify(service => service.GetOrganizationNumberByPartyUuid(partyUuid, It.IsAny<CancellationToken>()), Times.Once());
        Assert.False(memoryCache.TryGetValue($"OrgNumber_{partyUuid}", out string _));
    }

    [Fact]
    public async Task GetUserPartyUserUuid_UserInCache_DecoratedServiceNotCalled()
    {
        Guid userUuid = Guid.Parse("cc86d2c7-1695-44b0-8e82-e633243fdf31");
        MemoryCache memoryCache = CreateMemoryCache();
        Person party = CreatePersonParty("17902349936", userUuid);
        SeedUserCache(memoryCache, $"Party_UserId_UserUuid_{userUuid}", party);

        RegisterCachingDecorator target = CreateTarget(memoryCache);

        RegisterParty result = await target.GetUserParty(userUuid, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(party.Uuid, result.Uuid);
        _decoratedServiceMock.Verify(service => service.GetUserParty(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task GetUserPartyUserUuid_UserNotInCache_DecoratedServiceCalledAndCachePopulated()
    {
        Guid userUuid = Guid.Parse("cc86d2c7-1695-44b0-8e82-e633243fdf31");
        MemoryCache memoryCache = CreateMemoryCache();
        Person party = CreatePersonParty("17902349936", userUuid);

        _decoratedServiceMock
            .Setup(service => service.GetUserParty(userUuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(party);

        RegisterCachingDecorator target = CreateTarget(memoryCache);

        RegisterParty result = await target.GetUserParty(userUuid, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(party.Uuid, result.Uuid);
        _decoratedServiceMock.Verify(service => service.GetUserParty(userUuid, It.IsAny<CancellationToken>()), Times.Once());
        AssertUniqueAndUserCache(memoryCache, $"Party_UserId_UserUuid_{userUuid}", party);
    }

    [Fact]
    public async Task GetUserPartyUserUuid_NullFromDecoratedService_CacheNotPopulated()
    {
        Guid userUuid = Guid.Parse("cc86d2c7-1695-44b0-8e82-e633243fdf31");
        MemoryCache memoryCache = CreateMemoryCache();

        _decoratedServiceMock
            .Setup(service => service.GetUserParty(userUuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RegisterParty)null);

        RegisterCachingDecorator target = CreateTarget(memoryCache);

        RegisterParty result = await target.GetUserParty(userUuid, CancellationToken.None);

        Assert.Null(result);
        _decoratedServiceMock.Verify(service => service.GetUserParty(userUuid, It.IsAny<CancellationToken>()), Times.Once());
        Assert.False(memoryCache.TryGetValue($"Party_UserId_UserUuid_{userUuid}", out int _));
    }

    [Fact]
    public async Task GetUserPartyUserId_UserInCache_DecoratedServiceNotCalled()
    {
        MemoryCache memoryCache = CreateMemoryCache();
        Person party = CreatePersonParty("17902349936");
        int userId = GetUserId(party);
        SeedUserCache(memoryCache, party);

        RegisterCachingDecorator target = CreateTarget(memoryCache);

        RegisterParty result = await target.GetUserParty(userId, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(party.Uuid, result.Uuid);
        _decoratedServiceMock.Verify(service => service.GetUserParty(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task GetUserPartyUserId_UserNotInCache_DecoratedServiceCalledAndCachePopulated()
    {
        MemoryCache memoryCache = CreateMemoryCache();
        Person party = CreatePersonParty("17902349936");
        int userId = GetUserId(party);

        _decoratedServiceMock
            .Setup(service => service.GetUserParty(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(party);

        RegisterCachingDecorator target = CreateTarget(memoryCache);

        RegisterParty result = await target.GetUserParty(userId, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(party.Uuid, result.Uuid);
        _decoratedServiceMock.Verify(service => service.GetUserParty(userId, It.IsAny<CancellationToken>()), Times.Once());
        AssertUserCache(memoryCache, party);
    }

    [Fact]
    public async Task GetUserPartyUserId_NullFromDecoratedService_CacheNotPopulated()
    {
        const int userId = 2001607;
        MemoryCache memoryCache = CreateMemoryCache();

        _decoratedServiceMock
            .Setup(service => service.GetUserParty(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RegisterParty)null);

        RegisterCachingDecorator target = CreateTarget(memoryCache);

        RegisterParty result = await target.GetUserParty(userId, CancellationToken.None);

        Assert.Null(result);
        _decoratedServiceMock.Verify(service => service.GetUserParty(userId, It.IsAny<CancellationToken>()), Times.Once());
        Assert.False(memoryCache.TryGetValue(UserCacheKeyPrefix + userId, out RegisterParty _));
    }

    [Fact]
    public async Task GetUserPartyByUsername_UserInCache_DecoratedServiceNotCalled()
    {
        const string username = "testuser";
        MemoryCache memoryCache = CreateMemoryCache();
        SelfIdentifiedUser party = SelfIdentifiedUser.MinimalLegacy(username) with { User = new PartyUser(1, null, ImmutableValueArray<uint>.Empty.Add(1u)) };
        SeedUserCache(memoryCache, "Party_UserId_Username_" + username, party);

        RegisterCachingDecorator target = CreateTarget(memoryCache);

        RegisterParty result = await target.GetUserPartyByUsername(username, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(party.Uuid, result.Uuid);
        _decoratedServiceMock.Verify(service => service.GetUserPartyByUsername(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task GetUserPartyByUsername_UserNotInCache_DecoratedServiceCalledAndCachePopulated()
    {
        const string username = "testuser";
        MemoryCache memoryCache = CreateMemoryCache();
        SelfIdentifiedUser party = SelfIdentifiedUser.MinimalLegacy(username) with { User = new PartyUser(1, null, ImmutableValueArray<uint>.Empty.Add(1u)) };

        _decoratedServiceMock
            .Setup(service => service.GetUserPartyByUsername(username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(party);

        RegisterCachingDecorator target = CreateTarget(memoryCache);

        RegisterParty result = await target.GetUserPartyByUsername(username, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(party.Uuid, result.Uuid);
        _decoratedServiceMock.Verify(service => service.GetUserPartyByUsername(username, It.IsAny<CancellationToken>()), Times.Once());
        AssertUniqueAndUserCache(memoryCache, "Party_UserId_Username_" + username, party);
    }

    [Fact]
    public async Task GetUserPartyByUsername_NullFromDecoratedService_CacheNotPopulated()
    {
        const string username = "missing-user";
        MemoryCache memoryCache = CreateMemoryCache();

        _decoratedServiceMock
            .Setup(service => service.GetUserPartyByUsername(username, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RegisterParty)null);

        RegisterCachingDecorator target = CreateTarget(memoryCache);

        RegisterParty result = await target.GetUserPartyByUsername(username, CancellationToken.None);

        Assert.Null(result);
        _decoratedServiceMock.Verify(service => service.GetUserPartyByUsername(username, It.IsAny<CancellationToken>()), Times.Once());
        Assert.False(memoryCache.TryGetValue("Party_UserId_Username_" + username, out int _));
    }

    [Fact]
    public async Task GetUserPartyBySsn_UserInCache_DecoratedServiceNotCalled()
    {
        const string ssn = "17902349936";
        MemoryCache memoryCache = CreateMemoryCache();
        Person party = CreatePersonParty(ssn);
        SeedUserCache(memoryCache, "Party_UserId_SSN_" + ssn, party);

        RegisterCachingDecorator target = CreateTarget(memoryCache);

        RegisterParty result = await target.GetUserPartyBySsn(ssn, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(party.Uuid, result.Uuid);
        _decoratedServiceMock.Verify(service => service.GetUserPartyBySsn(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task GetUserPartyBySsn_UserNotInCache_DecoratedServiceCalledAndCachePopulated()
    {
        const string ssn = "30850399065";
        MemoryCache memoryCache = CreateMemoryCache();
        Person party = CreatePersonParty(ssn);

        _decoratedServiceMock
            .Setup(service => service.GetUserPartyBySsn(ssn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(party);

        RegisterCachingDecorator target = CreateTarget(memoryCache);

        RegisterParty result = await target.GetUserPartyBySsn(ssn, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(party.Uuid, result.Uuid);
        _decoratedServiceMock.Verify(service => service.GetUserPartyBySsn(ssn, It.IsAny<CancellationToken>()), Times.Once());
        AssertUniqueAndUserCache(memoryCache, "Party_UserId_SSN_" + ssn, party);
    }

    [Fact]
    public async Task GetUserPartyBySsn_NullFromDecoratedService_CacheNotPopulated()
    {
        const string ssn = "17902349936";
        MemoryCache memoryCache = CreateMemoryCache();

        _decoratedServiceMock
            .Setup(service => service.GetUserPartyBySsn(ssn, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RegisterParty)null);

        RegisterCachingDecorator target = CreateTarget(memoryCache);

        RegisterParty result = await target.GetUserPartyBySsn(ssn, CancellationToken.None);

        Assert.Null(result);
        _decoratedServiceMock.Verify(service => service.GetUserPartyBySsn(ssn, It.IsAny<CancellationToken>()), Times.Once());
        Assert.False(memoryCache.TryGetValue("Party_UserId_SSN_" + ssn, out int _));
    }

    [Fact]
    public async Task GetUserParties_UsersInCache_DecoratedServiceNotCalled()
    {
        Guid firstUserUuid = Guid.Parse("cc86d2c7-1695-44b0-8e82-e633243fdf31");
        Guid secondUserUuid = Guid.Parse("4c3b4909-eb17-45d5-bde1-256e065e196a");
        List<Guid> userUuids = [firstUserUuid, secondUserUuid];
        MemoryCache memoryCache = CreateMemoryCache();

        Person firstParty = CreatePersonParty("17902349936", firstUserUuid);
        Person secondParty = CreatePersonParty("45886700494", secondUserUuid);

        SeedUserCache(memoryCache, $"Party_UserId_UserUuid_{firstUserUuid}", firstParty);
        SeedUserCache(memoryCache, $"Party_UserId_UserUuid_{secondUserUuid}", secondParty);

        RegisterCachingDecorator target = CreateTarget(memoryCache);

        IReadOnlyList<RegisterParty> result = await target.GetUserParties(userUuids, CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, party => party.Uuid == firstUserUuid);
        Assert.Contains(result, party => party.Uuid == secondUserUuid);
        _decoratedServiceMock.Verify(service => service.GetUserParties(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task GetUserParties_UsersPartialInCache_DecoratedServiceCalledForMissingAndCachePopulated()
    {
        Guid cachedUserUuid = Guid.Parse("cc86d2c7-1695-44b0-8e82-e633243fdf31");
        Guid missingUserUuid = Guid.Parse("4c3b4909-eb17-45d5-bde1-256e065e196a");
        List<Guid> userUuids = [cachedUserUuid, missingUserUuid];
        List<Guid> expectedMissingUserUuids = [missingUserUuid];
        MemoryCache memoryCache = CreateMemoryCache();

        Person cachedParty = CreatePersonParty("17902349936", cachedUserUuid);
        Person fetchedParty = CreatePersonParty("45886700494", missingUserUuid);

        SeedUserCache(memoryCache, $"Party_UserId_UserUuid_{cachedUserUuid}", cachedParty);

        _decoratedServiceMock
            .Setup(service => service.GetUserParties(
                It.Is<List<Guid>>(guids => guids.SequenceEqual(expectedMissingUserUuids)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([fetchedParty]);

        RegisterCachingDecorator target = CreateTarget(memoryCache);

        IReadOnlyList<RegisterParty> result = await target.GetUserParties(userUuids, CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, party => party.Uuid == cachedUserUuid);
        Assert.Contains(result, party => party.Uuid == missingUserUuid);
        _decoratedServiceMock.Verify(
            service => service.GetUserParties(
                It.Is<List<Guid>>(guids => guids.SequenceEqual(expectedMissingUserUuids)),
                It.IsAny<CancellationToken>()),
            Times.Once());
        AssertUniqueAndUserCache(memoryCache, $"Party_UserId_UserUuid_{missingUserUuid}", fetchedParty);
    }

    [Fact]
    public async Task GetUserParties_EmptyListFromDecoratedService_CacheNotPopulated()
    {
        Guid firstUserUuid = Guid.Parse("cc86d2c7-1695-44b0-8e82-e633243fdf31");
        Guid secondUserUuid = Guid.Parse("4c3b4909-eb17-45d5-bde1-256e065e196a");
        List<Guid> userUuids = [firstUserUuid, secondUserUuid];
        MemoryCache memoryCache = CreateMemoryCache();

        _decoratedServiceMock
            .Setup(service => service.GetUserParties(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        RegisterCachingDecorator target = CreateTarget(memoryCache);

        IReadOnlyList<RegisterParty> result = await target.GetUserParties(userUuids, CancellationToken.None);

        Assert.Empty(result);
        _decoratedServiceMock.Verify(service => service.GetUserParties(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()), Times.Once());
        Assert.False(memoryCache.TryGetValue($"Party_UserId_UserUuid_{firstUserUuid}", out int _));
        Assert.False(memoryCache.TryGetValue($"Party_UserId_UserUuid_{secondUserUuid}", out int _));
    }

    private RegisterCachingDecorator CreateTarget(IMemoryCache memoryCache)
        => new(_decoratedServiceMock.Object, memoryCache, _coreSettingsOptions.Object);

    private static MemoryCache CreateMemoryCache()
        => new(new MemoryCacheOptions());

    private static ContactPointParty CreateContactPointParty(string orgNumber, int partyId)
        => new()
        {
            OrganizationIdentifier = orgNumber,
            PartyId = partyId,
            PartyUuid = Guid.NewGuid(),
        };

    private static Person CreatePersonParty(string ssn, Guid userUuid)
    {
        uint userId = (uint)(userUuid.GetHashCode() & int.MaxValue);
        userId = userId == 0 ? 1u : userId;

        return Person.Minimal(ssn, userUuid) with { User = new PartyUser(userId, null, ImmutableValueArray<uint>.Empty.Add(userId)) };
    }

    private static Person CreatePersonParty(string ssn)
        => Person.Minimal(ssn) with { User = new PartyUser(1, null, ImmutableValueArray<uint>.Empty.Add(1u)) };

    private static void SeedUserCache(IMemoryCache memoryCache, string uniqueCacheKey, RegisterParty party)
    {
        int userId = GetUserId(party);
        memoryCache.Set(uniqueCacheKey, userId);
        memoryCache.Set(UserCacheKeyPrefix + userId, party);
    }

    private static void SeedUserCache(IMemoryCache memoryCache, RegisterParty party)
    {
        int userId = GetUserId(party);
        memoryCache.Set(UserCacheKeyPrefix + userId, party);
    }

    private static void AssertUniqueAndUserCache(IMemoryCache memoryCache, string uniqueCacheKey, RegisterParty party)
    {
        Assert.True(memoryCache.TryGetValue(uniqueCacheKey, out int cachedUserId));
        Assert.Equal(GetUserId(party), cachedUserId);
        AssertUserCache(memoryCache, party);
    }

    private static void AssertUserCache(IMemoryCache memoryCache, RegisterParty party)
    {
        int userId = GetUserId(party);
        Assert.True(memoryCache.TryGetValue(UserCacheKeyPrefix + userId, out RegisterParty cachedParty));
        Assert.NotNull(cachedParty);
        Assert.Equal(party.Uuid, cachedParty.Uuid);
        Assert.Equal(party.PartyId, cachedParty.PartyId);
    }

    private static int GetUserId(RegisterParty party)
        => (int)party.User.Value.UserId.Value;
}
