using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Altinn.Profile.Core.Person.ContactPreferences;
using Altinn.Profile.Integrations.ContactRegister;
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

    [Fact]
    public async Task SyncPersonContactPreferencesAsync_AddsNewPerson_WhenNotExists()
    {
        // Arrange
        var snapshot = new PersonContactPreferencesSnapshot
        {
            PersonIdentifier = "99999999999",
            Language = "NO",
            Reservation = "JA",
            ContactDetailsSnapshot = new PersonContactDetailsSnapshot
            {
                Email = "newperson@example.com",
                MobileNumber = "12345678",
                EmailLastUpdated = DateTime.UtcNow.AddDays(-1),
                EmailLastVerified = DateTime.UtcNow,
                MobileNumberLastUpdated = DateTime.UtcNow.AddDays(-2),
                MobileNumberLastVerified = DateTime.UtcNow.AddDays(-1)
            }
        };

        var log = new ContactRegisterChangesLog
        {
            ContactPreferencesSnapshots = ImmutableList.Create(snapshot)
        };

        // Act
        var result = await _personRepository.SyncPersonContactPreferencesAsync(log);

        // Assert
        Assert.Equal(1, result);
        var person = _databaseContext.People.SingleOrDefault(p => p.FnumberAk == "99999999999");
        Assert.NotNull(person);
        Assert.Equal("NO", person.LanguageCode);
        Assert.Equal("newperson@example.com", person.EmailAddress);
        Assert.Equal("12345678", person.MobilePhoneNumber);
        Assert.True(person.Reservation);
    }

    [Fact]
    public async Task SyncPersonContactPreferencesAsync_UpdatesExistingPerson_WhenExists()
    {
        // Arrange
        var existing = new Person
        {
            FnumberAk = "88888888888",
            LanguageCode = "EN",
            Reservation = false,
            EmailAddress = "old@example.com",
            MobilePhoneNumber = "88888888"
        };
        _databaseContext.People.Add(existing);
        _databaseContext.SaveChanges();

        var snapshot = new PersonContactPreferencesSnapshot
        {
            PersonIdentifier = "88888888888",
            Language = "NO",
            Reservation = "JA",
            ContactDetailsSnapshot = new PersonContactDetailsSnapshot
            {
                Email = "updated@example.com",
                MobileNumber = "77777777",
                EmailLastUpdated = DateTime.UtcNow.AddDays(-1),
                EmailLastVerified = DateTime.UtcNow,
                MobileNumberLastUpdated = DateTime.UtcNow.AddDays(-2),
                MobileNumberLastVerified = DateTime.UtcNow.AddDays(-1)
            }
        };

        var log = new ContactRegisterChangesLog
        {
            ContactPreferencesSnapshots = ImmutableList.Create(snapshot)
        };

        // Act
        var result = await _personRepository.SyncPersonContactPreferencesAsync(log);

        // Assert
        Assert.Equal(1, result);
        var personList = await _personRepository.GetContactPreferencesAsync(["88888888888"]);
        var person = personList.FirstOrDefault();
        Assert.NotNull(person);
        Assert.Equal("NO", person.LanguageCode);
        Assert.Equal("updated@example.com", person.Email);
        Assert.Equal("77777777", person.MobileNumber);
        Assert.True(person.IsReserved);
    }

    [Fact]
    public async Task SyncPersonContactPreferencesAsync_DoesNothing_WhenSnapshotsIsEmpty()
    {
        // Arrange
        var log = new ContactRegisterChangesLog
        {
            ContactPreferencesSnapshots = ImmutableList<PersonContactPreferencesSnapshot>.Empty
        };

        // Act
        var result = await _personRepository.SyncPersonContactPreferencesAsync(log);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public async Task SyncPersonContactPreferencesAsync_ThrowsArgumentNullException_WhenLogIsNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _personRepository.SyncPersonContactPreferencesAsync(null!));
    }

    [Fact]
    public async Task SyncPersonContactPreferencesAsync_ThrowsArgumentNullException_WhenSnapshotsIsNull()
    {
        // Arrange
        var log = new ContactRegisterChangesLog
        {
            ContactPreferencesSnapshots = null
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _personRepository.SyncPersonContactPreferencesAsync(log));
    }
}
