using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Altinn.Profile.Integrations.Entities;
using Altinn.Profile.Integrations.Persistence;
using Altinn.Profile.Integrations.Repositories;
using Altinn.Profile.Tests.Testdata;

using Microsoft.EntityFrameworkCore;

using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations;

/// <summary>
/// Contains unit tests for the <see cref="PersonRepository"/> class.
/// </summary>
public class RegisterRepositoryTests : IDisposable
{
    private readonly ProfileDbContext _context;
    private readonly PersonRepository _registerRepository;
    private readonly List<Register> _personContactAndReservationTestData;
    
    public RegisterRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ProfileDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ProfileDbContext(options);
        _registerRepository = new PersonRepository(_context);

        _personContactAndReservationTestData = [.. PersonTestData.GetContactAndReservationTestData()];

        _context.Registers.AddRange(_personContactAndReservationTestData);
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task GetUserContactInfoAsync_ReturnsContactInfo_WhenFound()
    {
        // Act
        var result = await _registerRepository.GetUserContactInfoAsync(["17111933790"]);

        var actual = result.FirstOrDefault(e => e.FnumberAk == "17111933790");
        var expected = _personContactAndReservationTestData.FirstOrDefault(e => e.FnumberAk == "17111933790");

        // Assert
        Assert.NotNull(actual);
        AssertRegisterProperties(expected, actual);
    }

    [Fact]
    public async Task GetUserContactInfoAsync_ReturnsCorrectResults_WhenValidAndInvalidNumbers()
    {
        // Act
        var result = _personContactAndReservationTestData.Where(e => e.FnumberAk == "28026698350");
        var expected = await _registerRepository.GetUserContactInfoAsync(["28026698350", "nonexistent2"]);

        // Assert invalid result
        Assert.Single(result);
        AssertRegisterProperties(expected.FirstOrDefault(), result.FirstOrDefault());
    }

    [Fact]
    public async Task GetUserContactInfoAsync_ReturnsEmpty_WhenNoneFound()
    {
        // Act
        var result = await _registerRepository.GetUserContactInfoAsync(["nonexistent1", "nonexistent2"]);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetUserContactInfoAsync_ReturnsMultipleContacts_WhenFound()
    {
        // Act
        var result = await _registerRepository.GetUserContactInfoAsync(["24064316776", "11044314101"]);
        var expected = _personContactAndReservationTestData.Where(e => e.FnumberAk == "24064316776" || e.FnumberAk == "11044314101");

        // Assert
        Assert.Equal(2, result.Count);

        foreach (var register in result)
        {
            var foundRegister = expected.FirstOrDefault(r => r.FnumberAk == register.FnumberAk);
            Assert.NotNull(foundRegister);
            AssertRegisterProperties(register, foundRegister);
        }
    }

    [Fact]
    public async Task GetUserContactInfoAsync_ReturnsEmpty_WhenNotFound()
    {
        // Act
        var result = await _registerRepository.GetUserContactInfoAsync(["nonexistent", "11044314120"]);

        // Assert
        Assert.Empty(result);
    }

    private static void AssertRegisterProperties(Register expected, Register actual)
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
