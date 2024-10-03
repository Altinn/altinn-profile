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
        var options = new DbContextOptionsBuilder<ProfileDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        _context = new ProfileDbContext(options);
        _registerRepository = new RegisterRepository(_context);
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
        // Arrange
        var register = CreateRegister("21102709516", "Test Description", "test@example.com", "Test Mailbox Address");

        await AddRegisterToContext(register);

        // Act
        var result = await _registerRepository.GetUserContactInfoAsync(["21102709516"]);

        // Assert
        AssertSingleRegister(register, result.First());
    }

    [Fact]
    public async Task GetUserContactInfoAsync_ReturnsCorrectResults_WhenValidAndInvalidNumbers()
    {
        // Arrange
        var validRegister = CreateRegister("21102709516", "Valid Test Description", "valid@example.com", "Valid Mailbox Address");

        await AddRegisterToContext(validRegister);

        // Act
        var result = await _registerRepository.GetUserContactInfoAsync(["21102709516", "nonexistent2"]);

        // Assert valid result
        AssertSingleRegister(validRegister, result.FirstOrDefault(r => r.FnumberAk == "21102709516"));

        // Assert invalid result
        Assert.Null(result.FirstOrDefault(r => r.FnumberAk == "nonexistent2"));
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
        // Arrange
        var registers = new List<Register>
        {
            CreateRegister("03062701187", "Test Description 1", "test1@example.com", "Test Mailbox Address 1", false),
            CreateRegister("02024333593", "Test Description 2", "test2@example.com", "Test Mailbox Address 2", true)
        };

        await _context.Registers.AddRangeAsync(registers);
        await _context.SaveChangesAsync();

        // Act
        var result = await _registerRepository.GetUserContactInfoAsync(["03062701187", "02024333593"]);

        // Assert
        Assert.Equal(2, result.Count());

        foreach (var register in registers)
        {
            var foundRegister = result.FirstOrDefault(r => r.FnumberAk == register.FnumberAk);
            Assert.NotNull(foundRegister);
            AssertRegisterProperties(register, foundRegister);
        }
    }

    [Fact]
    public async Task GetUserContactInfoAsync_ReturnsEmpty_WhenNotFound()
    {
        // Act
        var result = await _registerRepository.GetUserContactInfoAsync(["nonexistent"]);

        // Assert
        Assert.Empty(result);
    }

    private async Task AddRegisterToContext(Register register)
    {
        await _context.Registers.AddAsync(register);
        await _context.SaveChangesAsync();
    }

    private static void AssertSingleRegister(Register expected, Register actual)
    {
        Assert.NotNull(actual);
        AssertRegisterProperties(expected, actual);
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

    private static Register CreateRegister(string fnumberAk, string description, string emailAddress, string mailboxAddress, bool reservation = true)
    {
        return new Register
        {
            LanguageCode = "EN",
            FnumberAk = fnumberAk,
            Reservation = reservation,
            Description = description,
            EmailAddress = emailAddress,
            MailboxAddress = mailboxAddress,
            MobilePhoneNumber = "1234567890",
        };
    }
}
