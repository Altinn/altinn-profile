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
    private readonly Mock<IMapper> _mockMapper;
    private readonly RegisterService _registerService;

    public RegisterServiceTests()
    {
        _mockMapper = new Mock<IMapper>();
        _mockRegisterRepository = new Mock<IRegisterRepository>();
        _registerService = new RegisterService(_mockMapper.Object, _mockRegisterRepository.Object);
    }

    [Fact]
    public async Task GetUserContactInfoAsync_ShouldReturnContactInfo_WhenValidNationalIdentityNumber()
    {
        var nationalIdentityNumber = "123";
        var register = new Register { EmailAddress = "test@example.com" };
        var mockUserContactInfo = new Mock<IUserContactInfo>();
        mockUserContactInfo.SetupGet(u => u.EmailAddress).Returns("test@example.com");

        _mockRegisterRepository.Setup(repo => repo.GetUserContactInfoAsync(nationalIdentityNumber))
            .ReturnsAsync(register);
        _mockMapper.Setup(m => m.Map<IUserContactInfo>(register)).Returns(mockUserContactInfo.Object);

        var result = await _registerService.GetUserContactInfoAsync(nationalIdentityNumber);

        Assert.NotNull(result);
        Assert.Equal("test@example.com", result?.EmailAddress);
    }

    [Fact]
    public async Task GetUserContactInfoAsync_ShouldReturnNull_WhenInvalidNationalIdentityNumber()
    {
        _mockRegisterRepository.Setup(repo => repo.GetUserContactInfoAsync("invalid"))
            .ReturnsAsync((Register?)null);

        var result = await _registerService.GetUserContactInfoAsync("invalid");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserContactInfoAsync_ShouldReturnEmpty_WhenNoNationalIdentityNumbersProvided()
    {
        var result = await _registerService.GetUserContactInfoAsync(new List<string>());

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetUserContactInfoAsync_ShouldReturnCorrectResults_ForValidAndInvalidNumbers()
    {
        var validNationalIdentityNumber = "123";
        var invalidNationalIdentityNumber = "invalid";
        var register = new Register { EmailAddress = "test@example.com" };
        var mockUserContactInfo = new Mock<IUserContactInfo>();
        mockUserContactInfo.SetupGet(u => u.EmailAddress).Returns("test@example.com");

        _mockRegisterRepository.Setup(repo => repo.GetUserContactInfoAsync(validNationalIdentityNumber))
            .ReturnsAsync(register);
        _mockRegisterRepository.Setup(repo => repo.GetUserContactInfoAsync(invalidNationalIdentityNumber))
            .ReturnsAsync((Register?)null);
        _mockMapper.Setup(m => m.Map<IUserContactInfo>(register)).Returns(mockUserContactInfo.Object);

        var validResult = await _registerService.GetUserContactInfoAsync(validNationalIdentityNumber);
        var invalidResult = await _registerService.GetUserContactInfoAsync(invalidNationalIdentityNumber);

        // Assert valid result
        Assert.NotNull(validResult);
        Assert.Equal("test@example.com", validResult?.EmailAddress);

        // Assert invalid result
        Assert.Null(invalidResult);
    }

    [Fact]
    public async Task GetUserContactInfoAsync_ShouldReturnNull_WhenNationalIdentityNumberIsNull()
    {
        var result = await _registerService.GetUserContactInfoAsync(string.Empty);

        Assert.Null(result);
    }

}
