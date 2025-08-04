using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Altinn.Profile.Core.Person.ContactPreferences;
using Altinn.Profile.Integrations.Entities;
using Altinn.Profile.Integrations.Persistence;
using Altinn.Profile.Integrations.Repositories;
using Altinn.Profile.Tests.Profile.Integrations.Extensions;
using Altinn.Profile.Tests.Testdata;

using Microsoft.EntityFrameworkCore;

using Moq;

using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations;

/// <summary>
/// Contains unit tests for the <see cref="PersonRepository"/> class.
/// </summary>
public class PersonRepositoryTests : IDisposable
{
    private bool _isDisposed;
    private readonly ProfileDbContext _databaseContext;
    private readonly PersonRepository _personRepository;
    private readonly List<Person> _personContactAndReservationTestData;
    private readonly Mock<IDbContextFactory<ProfileDbContext>> _databaseContextFactory;

    public PersonRepositoryTests()
    {
        var databaseContextOptions = new DbContextOptionsBuilder<ProfileDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _databaseContextFactory = new Mock<IDbContextFactory<ProfileDbContext>>();

        _databaseContextFactory.Setup(f => f.CreateDbContext())
            .Returns(new ProfileDbContext(databaseContextOptions));

        _databaseContextFactory.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new ProfileDbContext(databaseContextOptions));

        _personRepository = new PersonRepository(_databaseContextFactory.Object, null);

        _personContactAndReservationTestData = new List<Person>(PersonTestData.GetContactAndReservationTestData());

        _databaseContext = _databaseContextFactory.Object.CreateDbContext();
        _databaseContext.People.AddRange(_personContactAndReservationTestData);
        _databaseContext.SaveChanges();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                _databaseContext.Database.EnsureDeleted();
                _databaseContext.Dispose();
            }

            _isDisposed = true;
        }
    }

    [Fact]
    public async Task GetContactDetailsAsync_WhenFound_ReturnsContactInfo()
    {
        // Act
        var matches = await _personRepository.GetContactPreferencesAsync(["17111933790"]);
        var matchedPersonContactPreferences = matches[0];

        var expectedPerson = _personContactAndReservationTestData
            .Find(p => p.FnumberAk == "17111933790");

        // Assert
        Assert.NotNull(matchedPersonContactPreferences);
        AssertRegisterProperties(expectedPerson.AsPersonContactPreferences(), matchedPersonContactPreferences);
    }

    [Fact]
    public async Task GetContactDetailsAsync_WhenMultipleContactsFound_ReturnsMultipleContacts()
    {
        // Act
        var matchedPersonContactPreferences = await _personRepository.GetContactPreferencesAsync(["24064316776", "11044314101"]);

        var expectedPersons = _personContactAndReservationTestData
            .Where(e => e.FnumberAk == "24064316776" || e.FnumberAk == "11044314101")
            .ToList();

        // Assert
        Assert.Equal(2, matchedPersonContactPreferences.Count);

        foreach (var person in matchedPersonContactPreferences)
        {
            var expectedPerson = expectedPersons.Find(r => r.FnumberAk == person.NationalIdentityNumber);

            Assert.NotNull(expectedPerson);
            AssertRegisterProperties(expectedPerson.AsPersonContactPreferences(), person);
        }
    }

    [Fact]
    public async Task GetContactDetailsAsync_WhenNoNationalIdentityNumbersProvided_ReturnsEmpty()
    {
        // Act
        var matchedPersonContactPreferences = await _personRepository.GetContactPreferencesAsync([]);

        // Assert
        Assert.Empty(matchedPersonContactPreferences);
    }

    [Fact]
    public async Task GetContactDetailsAsync_WhenNoneFound_ReturnsEmpty()
    {
        // Act
        var matchedPersonContactPreferences = await _personRepository.GetContactPreferencesAsync(["nonexistent1", "nonexistent2"]);

        // Assert
        Assert.Empty(matchedPersonContactPreferences);
    }

    [Fact]
    public async Task GetContactDetailsAsync_WhenValidAndInvalidNumbers_ReturnsCorrectResults()
    {
        // Act
        var expectedPerson = _personContactAndReservationTestData.Find(e => e.FnumberAk == "28026698350");

        var matchedPersonContactPreferences = await _personRepository.GetContactPreferencesAsync(["28026698350", "nonexistent2"]);

        // Assert invalid result
        Assert.Single(matchedPersonContactPreferences);
        AssertRegisterProperties(expectedPerson.AsPersonContactPreferences(), matchedPersonContactPreferences.FirstOrDefault());
    }

    private static void AssertRegisterProperties(PersonContactPreferences expected, PersonContactPreferences actual)
    {
        Assert.Equal(expected.NationalIdentityNumber, actual.NationalIdentityNumber);
        Assert.Equal(expected.IsReserved, actual.IsReserved);
        Assert.Equal(expected.LanguageCode, actual.LanguageCode);
        Assert.Equal(expected.Email, actual.Email);
        Assert.Equal(expected.MobileNumber, actual.MobileNumber);
    }
}
