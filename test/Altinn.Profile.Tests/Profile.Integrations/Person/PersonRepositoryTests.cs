using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Profile.Integrations.Entities;
using Altinn.Profile.Integrations.Persistence;
using Altinn.Profile.Integrations.Repositories;
using Altinn.Profile.Tests.Testdata;

using AutoMapper;

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
    private readonly Mock<IMapper> _mapperMock;
    private readonly ProfileDbContext _databaseContext;
    private readonly PersonRepository _personRepository;
    private readonly List<Person> _personContactAndReservationTestData;
    private readonly Mock<IDbContextFactory<ProfileDbContext>> _databaseContextFactory;

    public PersonRepositoryTests()
    {
        _mapperMock = new Mock<IMapper>();

        var databaseContextOptions = new DbContextOptionsBuilder<ProfileDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _databaseContextFactory = new Mock<IDbContextFactory<ProfileDbContext>>();

        _databaseContextFactory.Setup(f => f.CreateDbContext())
            .Returns(new ProfileDbContext(databaseContextOptions));

        _databaseContextFactory.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new ProfileDbContext(databaseContextOptions));

        _personRepository = new PersonRepository(_mapperMock.Object, _databaseContextFactory.Object);

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
        var matchedPerson = (await _personRepository.GetContactDetailsAsync(["17111933790"]))
            .Match(
                e => e.Find(p => p.FnumberAk == "17111933790"),
                _ => null);

        var expectedPerson = _personContactAndReservationTestData
            .Find(e => e.FnumberAk == "17111933790");

        // Assert
        Assert.NotNull(matchedPerson);
        AssertRegisterProperties(expectedPerson, matchedPerson);
    }

    [Fact]
    public async Task GetContactDetailsAsync_WhenMultipleContactsFound_ReturnsMultipleContacts()
    {
        // Act
        var contactDetailsGetter = await _personRepository.GetContactDetailsAsync(["24064316776", "11044314101"]);

        var matchedPersons = contactDetailsGetter.Match(
            e => e,
            _ => Enumerable.Empty<Person>());

        var expectedPersons = _personContactAndReservationTestData
            .Where(e => e.FnumberAk == "24064316776" || e.FnumberAk == "11044314101")
            .ToList();

        // Assert
        Assert.Equal(2, matchedPersons.Count());

        foreach (var person in matchedPersons)
        {
            var expectedPerson = expectedPersons.Find(r => r.FnumberAk == person.FnumberAk);

            Assert.NotNull(expectedPerson);
            AssertRegisterProperties(expectedPerson, person);
        }
    }

    [Fact]
    public async Task GetContactDetailsAsync_WhenNoNationalIdentityNumbersProvided_ReturnsEmpty()
    {
        // Act
        var contactDetailsGetter = await _personRepository.GetContactDetailsAsync([]);

        var matchedPersons = contactDetailsGetter.Match(
            e => e,
            _ => Enumerable.Empty<Person>());

        // Assert
        Assert.Empty(matchedPersons);
    }

    [Fact]
    public async Task GetContactDetailsAsync_WhenNoneFound_ReturnsEmpty()
    {
        // Act
        var contactDetailsGetter = await _personRepository.GetContactDetailsAsync(["nonexistent1", "nonexistent2"]);

        var matchedPersons = contactDetailsGetter.Match(
            e => e,
            _ => Enumerable.Empty<Person>());

        // Assert
        Assert.Empty(matchedPersons);
    }

    [Fact]
    public async Task GetContactDetailsAsync_WhenValidAndInvalidNumbers_ReturnsCorrectResults()
    {
        // Act
        var result = _personContactAndReservationTestData.Find(e => e.FnumberAk == "28026698350");

        var contactDetailsGetter = await _personRepository.GetContactDetailsAsync(["28026698350", "nonexistent2"]);

        var matchedPersons = contactDetailsGetter.Match(
            e => e,
            _ => Enumerable.Empty<Person>());

        // Assert invalid result
        Assert.Single(matchedPersons);
        AssertRegisterProperties(matchedPersons.FirstOrDefault(), result);
    }

    private static void AssertRegisterProperties(Person expected, Person actual)
    {
        Assert.Equal(expected.FnumberAk, actual.FnumberAk);
        Assert.Equal(expected.Description, actual.Description);
        Assert.Equal(expected.Reservation, actual.Reservation);
        Assert.Equal(expected.LanguageCode, actual.LanguageCode);
        Assert.Equal(expected.EmailAddress, actual.EmailAddress);
        Assert.Equal(expected.MailboxAddress, actual.MailboxAddress);
        Assert.Equal(expected.MobilePhoneNumber, actual.MobilePhoneNumber);
    }
}
