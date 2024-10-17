//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//using Altinn.Profile.Integrations.Entities;
//using Altinn.Profile.Integrations.Persistence;
//using Altinn.Profile.Integrations.Repositories;
//using Altinn.Profile.Tests.Testdata;

//using Microsoft.EntityFrameworkCore;

//using Xunit;

//namespace Altinn.Profile.Tests.Profile.Integrations;

///// <summary>
///// Contains unit tests for the <see cref="PersonRepository"/> class.
///// </summary>
//public class PersonRepositoryTests : IDisposable
//{
//    private readonly ProfileDbContext _context;
//    private readonly PersonRepository _registerRepository;
//    private readonly List<Person> _personContactAndReservationTestData;

//    public PersonRepositoryTests()
//    {
//        var options = new DbContextOptionsBuilder<ProfileDbContext>()
//            .UseInMemoryDatabase(Guid.NewGuid().ToString())
//            .Options;

//        _context = new ProfileDbContext(options);
//        _registerRepository = new PersonRepository(_context);

//        _personContactAndReservationTestData = new List<Person>(PersonTestData.GetContactAndReservationTestData());

//        _context.People.AddRange(_personContactAndReservationTestData);
//        _context.SaveChanges();
//    }

//    public void Dispose()
//    {
//        _context.Database.EnsureDeleted();
//        _context.Dispose();
//        GC.SuppressFinalize(this);
//    }

//    [Fact]
//    public async Task GetContactDetailsAsync_WhenFound_ReturnsContactInfo()
//    {
//        // Act
//        var result = await _registerRepository.GetContactDetailsAsync(["17111933790"]);

//        var actual = result.FirstOrDefault(e => e.FnumberAk == "17111933790");
//        var expected = _personContactAndReservationTestData.FirstOrDefault(e => e.FnumberAk == "17111933790");

//        // Assert
//        Assert.NotNull(actual);
//        AssertRegisterProperties(expected, actual);
//    }

//    [Fact]
//    public async Task GetContactDetailsAsync_WhenMultipleContactsFound_ReturnsMultipleContacts()
//    {
//        // Act
//        var result = await _registerRepository.GetContactDetailsAsync(["24064316776", "11044314101"]);
//        var expected = _personContactAndReservationTestData
//            .Where(e => e.FnumberAk == "24064316776" || e.FnumberAk == "11044314101");

//        // Assert
//        Assert.Equal(2, result.Count);

//        foreach (var register in result)
//        {
//            var foundRegister = expected.FirstOrDefault(r => r.FnumberAk == register.FnumberAk);
//            Assert.NotNull(foundRegister);
//            AssertRegisterProperties(register, foundRegister);
//        }
//    }

//    [Fact]
//    public async Task GetContactDetailsAsync_WhenNoNationalIdentityNumbersProvided_ReturnsEmpty()
//    {
//        // Act
//        var result = await _registerRepository.GetContactDetailsAsync([]);

//        // Assert
//        Assert.Empty(result);
//    }

//    [Fact]
//    public async Task GetContactDetailsAsync_WhenNoneFound_ReturnsEmpty()
//    {
//        // Act
//        var result = await _registerRepository.GetContactDetailsAsync(["nonexistent1", "nonexistent2"]);

//        // Assert
//        Assert.Empty(result);
//    }

//    [Fact]
//    public async Task GetContactDetailsAsync_WhenNotFound_ReturnsEmpty()
//    {
//        // Act
//        var result = await _registerRepository.GetContactDetailsAsync(["nonexistent", "11044314120"]);

//        // Assert
//        Assert.Empty(result);
//    }

//    [Fact]
//    public async Task GetContactDetailsAsync_WhenValidAndInvalidNumbers_ReturnsCorrectResults()
//    {
//        // Act
//        var result = _personContactAndReservationTestData.Where(e => e.FnumberAk == "28026698350");
//        var expected = await _registerRepository.GetContactDetailsAsync(["28026698350", "nonexistent2"]);

//        // Assert invalid result
//        Assert.Single(result);
//        AssertRegisterProperties(expected.FirstOrDefault(), result.FirstOrDefault());
//    }

//    private static void AssertRegisterProperties(Person expected, Person actual)
//    {
//        Assert.Equal(expected.FnumberAk, actual.FnumberAk);
//        Assert.Equal(expected.Description, actual.Description);
//        Assert.Equal(expected.Reservation, actual.Reservation);
//        Assert.Equal(expected.LanguageCode, actual.LanguageCode);
//        Assert.Equal(expected.EmailAddress, actual.EmailAddress);
//        Assert.Equal(expected.MailboxAddress, actual.MailboxAddress);
//        Assert.Equal(expected.MobilePhoneNumber, actual.MobilePhoneNumber);
//    }
//}
