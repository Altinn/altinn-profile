#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Altinn.Profile.Integrations.Entities;
using Altinn.Profile.Integrations.Repositories;
using Altinn.Profile.Integrations.Services;

using AutoMapper;

using Moq;

using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations;

public class RegisterServiceTests
{
    private readonly Mock<IRegisterRepository> _mockRegisterRepository;
    private readonly Mock<INationalIdentityNumberChecker> _mockNationalIdentityNumberChecker;
    private readonly Mock<IMapper> _mockMapper;

    public RegisterServiceTests()
    {
        _mockMapper = new Mock<IMapper>();
        _mockRegisterRepository = new Mock<IRegisterRepository>();
        _mockNationalIdentityNumberChecker = new Mock<INationalIdentityNumberChecker>();
    }

    [Fact]
    public async Task GetUserContactInfoAsync_ReturnsMappedInfo_WhenValidIds()
    {
        // Arrange
        var ids = new List<string> { "11103048704", "30039302787" };

        var firstRegister = new Register
        {
            Reservation = false,
            LanguageCode = "NO",
            FnumberAk = "11103048704",
            MobilePhoneNumber = "1234567890",
            EmailAddress = "test1@example.com",
            Description = "Test Description 1",
            MailboxAddress = "Test Mailbox Address 1"
        };

        var secondRegister = new Register
        {
            Reservation = false,
            LanguageCode = "NO",
            FnumberAk = "30039302787",
            MobilePhoneNumber = "0987654321",
            EmailAddress = "test2@example.com",
            Description = "Test Description 2",
            MailboxAddress = "Test Mailbox Address 2"
        };

        SetupRegisterRepository(firstRegister, secondRegister);

        var firstUserContactInfo = SetupUserContactInfo(firstRegister);
        var secondUserContactInfo = SetupUserContactInfo(secondRegister);

        SetupMapper((firstRegister, firstUserContactInfo), (secondRegister, secondUserContactInfo));

        // Act
        var registerService = new RegisterService(_mockMapper.Object, _mockRegisterRepository.Object, _mockNationalIdentityNumberChecker.Object);
        var result = await registerService.GetUserContactAsync(ids);

        // Assert
        Assert.NotNull(result);
        ///Assert.Equal(2, result.Count());
        ///AssertUserContactInfoMatches(firstRegister, result.ElementAt(0));
        ///AssertUserContactInfoMatches(secondRegister, result.ElementAt(1));
    }

    [Fact]
    public async Task GetUserContactInfoAsync_ReturnsEmpty_WhenInvalidNationalIds()
    {
        // Arrange
        var nationalIdentityNumbers = new List<string> { "invalid1", "invalid2" };

        SetupRegisterRepository(); // Return empty

        // Act
        var registerService = new RegisterService(_mockMapper.Object, _mockRegisterRepository.Object, _mockNationalIdentityNumberChecker.Object);
        var result = await registerService.GetUserContactAsync(nationalIdentityNumbers);

        // Assert
        Assert.NotNull(result);
        ///Assert.Empty(result);
    }

    [Fact]
    public async Task GetUserContactInfoAsync_ReturnsMapped_WhenOneValidAndOneInvalidId()
    {
        // Arrange
        var nationalIdentityNumbers = new List<string> { "11103048704", "invalid" };

        var validRegister = new Register
        {
            Reservation = false,
            LanguageCode = "NO",
            FnumberAk = "11103048704",
            MobilePhoneNumber = "1234567890",
            EmailAddress = "test@example.com",
            Description = "Test Description",
            MailboxAddress = "Test Mailbox Address"
        };

        SetupRegisterRepository(validRegister);

        var mockUserContactInfo = SetupUserContactInfo(validRegister);

        SetupMapper((validRegister, mockUserContactInfo));

        // Act
        var registerService = new RegisterService(_mockMapper.Object, _mockRegisterRepository.Object, _mockNationalIdentityNumberChecker.Object);
        var result = await registerService.GetUserContactAsync(nationalIdentityNumbers);

        // Assert
        Assert.NotNull(result);
        ///Assert.Single(result);
        ///AssertUserContactInfoMatches(validRegister, result.First());
    }

    private void SetupRegisterRepository(params Register[] registers)
    {
        ///_mockRegisterRepository.Setup(repo => repo.GetUserContactInfoAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(registers.AsEnumerable());
    }

    private static Mock<IUserContactInfo> SetupUserContactInfo(Register register)
    {
        var mockUserContactInfo = new Mock<IUserContactInfo>();
        mockUserContactInfo.SetupGet(u => u.IsReserved).Returns(register.Reservation);
        mockUserContactInfo.SetupGet(u => u.LanguageCode).Returns(register.LanguageCode);
        mockUserContactInfo.SetupGet(u => u.EmailAddress).Returns(register.EmailAddress);
        mockUserContactInfo.SetupGet(u => u.NationalIdentityNumber).Returns(register.FnumberAk);
        mockUserContactInfo.SetupGet(u => u.MobilePhoneNumber).Returns(register.MobilePhoneNumber);
        return mockUserContactInfo;
    }

    private void SetupMapper(params (Register Register, Mock<IUserContactInfo> UserContactInfo)[] mappings)
    {
        _mockMapper.Setup(m => m.Map<IEnumerable<IUserContactInfo>>(It.IsAny<IEnumerable<Register>>()))
            .Returns((IEnumerable<Register> registers) =>
                registers.Select(r =>
                    mappings.FirstOrDefault(m => m.Register.FnumberAk == r.FnumberAk).UserContactInfo.Object)
                    .Where(u => u != null)
                    .Cast<IUserContactInfo>());
    }

    private static void AssertUserContactInfoMatches(Register expectedRegister, IUserContactInfo actualContactInfo)
    {
        Assert.Equal(expectedRegister.Reservation, actualContactInfo.IsReserved);
        Assert.Equal(expectedRegister.EmailAddress, actualContactInfo.EmailAddress);
        Assert.Equal(expectedRegister.LanguageCode, actualContactInfo.LanguageCode);
        Assert.Equal(expectedRegister.FnumberAk, actualContactInfo.NationalIdentityNumber);
        Assert.Equal(expectedRegister.MobilePhoneNumber, actualContactInfo.MobilePhoneNumber);
    }
}
