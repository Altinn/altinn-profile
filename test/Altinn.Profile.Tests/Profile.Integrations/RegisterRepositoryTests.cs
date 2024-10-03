using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Altinn.Profile.Integrations.Entities;
using Altinn.Profile.Integrations.Persistence;
using Altinn.Profile.Integrations.Repositories;
using Microsoft.EntityFrameworkCore;

using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations;

/// <summary>
/// Contains unit tests for the <see cref="RegisterRepository"/> class.
/// </summary>
public class RegisterRepositoryTests : IDisposable
{
    private readonly ProfileDbContext _context;
    private readonly RegisterRepository _registerRepository;

    public RegisterRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ProfileDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ProfileDbContext(options);
        _registerRepository = new RegisterRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetUserContactInfoAsync_ShouldReturnContactInfo_WhenFound()
    {
        var register = new Register
        {
            Reservation = true,
            LanguageCode = "EN",
            FnumberAk = "21102709516",
            MobilePhoneNumber = "1234567890",
            Description = "Test Description",
            EmailAddress = "test@example.com",
            MailboxAddress = "Test Mailbox Address"
        };

        await _context.Registers.AddAsync(register);

        await _context.SaveChangesAsync();

        var result = await _registerRepository.GetUserContactInfoAsync("21102709516");

        Assert.NotNull(result);
        Assert.Equal(register.FnumberAk, result.FnumberAk);
        Assert.Equal(register.Description, result.Description);
        Assert.Equal(register.Reservation, result.Reservation);
        Assert.Equal(register.LanguageCode, result.LanguageCode);
        Assert.Equal(register.EmailAddress, result.EmailAddress);
        Assert.Equal(register.MailboxAddress, result.MailboxAddress);
        Assert.Equal(register.MobilePhoneNumber, result.MobilePhoneNumber);
    }

    [Fact]
    public async Task GetUserContactInfoAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _registerRepository.GetUserContactInfoAsync("nonexistent");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserContactInfoAsync_ShouldReturnMultipleContacts_WhenFound()
    {
        var registers = new List<Register>
        {
            new()
            {
                Reservation = false,
                LanguageCode = "NO",
                FnumberAk = "03062701187",
                MobilePhoneNumber = "1234567891",
                Description = "Test Description 1",
                EmailAddress = "test1@example.com",
                MailboxAddress = "Test Mailbox Address 1"
            },
            new()
            {
                Reservation = true,
                LanguageCode = "EN",
                FnumberAk = "02024333593",
                MobilePhoneNumber = "1234567892",
                Description = "Test Description 2",
                EmailAddress = "test2@example.com",
                MailboxAddress = "Test Mailbox Address 2"
            }
        };

        await _context.Registers.AddRangeAsync(registers);
        await _context.SaveChangesAsync();

        var result = await _registerRepository.GetUserContactInfoAsync(["03062701187", "02024333593"]);

        Assert.Equal(2, result.Count());

        var resultList = result.ToList();
        Assert.Contains(resultList, r => r.FnumberAk == "02024333593" && r.EmailAddress == "test2@example.com" && r.MobilePhoneNumber == "1234567892" && r.Description == "Test Description 2" && r.Reservation == true && r.MailboxAddress == "Test Mailbox Address 2" && r.LanguageCode == "EN");
        Assert.Contains(resultList, r => r.FnumberAk == "03062701187" && r.EmailAddress == "test1@example.com" && r.MobilePhoneNumber == "1234567891" && r.Description == "Test Description 1" && r.Reservation == false && r.MailboxAddress == "Test Mailbox Address 1" && r.LanguageCode == "NO");
    }

    [Fact]
    public async Task GetUserContactInfoAsync_ShouldReturnEmpty_WhenNoneFound()
    {
        var result = await _registerRepository.GetUserContactInfoAsync(new[] { "nonexistent1", "nonexistent2" });

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetUserContactInfoAsync_ShouldReturnCorrectResults_ForValidAndInvalidNumbers()
    {
        var validRegister = new Register
        {
            Reservation = true,
            LanguageCode = "EN",
            FnumberAk = "21102709516",
            MobilePhoneNumber = "1234567890",
            EmailAddress = "valid@example.com",
            Description = "Valid Test Description",
            MailboxAddress = "Valid Mailbox Address"
        };

        await _context.Registers.AddAsync(validRegister);
        await _context.SaveChangesAsync();

        var invalidResult = await _registerRepository.GetUserContactInfoAsync("invalid");
        var validResult = await _registerRepository.GetUserContactInfoAsync("21102709516");

        // Assert valid result
        Assert.NotNull(validResult);
        Assert.Equal(validRegister.FnumberAk, validResult.FnumberAk);
        Assert.Equal(validRegister.Description, validResult.Description);
        Assert.Equal(validRegister.Reservation, validResult.Reservation);
        Assert.Equal(validRegister.LanguageCode, validResult.LanguageCode);
        Assert.Equal(validRegister.EmailAddress, validResult.EmailAddress);
        Assert.Equal(validRegister.MailboxAddress, validResult.MailboxAddress);
        Assert.Equal(validRegister.MobilePhoneNumber, validResult.MobilePhoneNumber);

        // Assert invalid result
        Assert.Null(invalidResult);
    }
}
