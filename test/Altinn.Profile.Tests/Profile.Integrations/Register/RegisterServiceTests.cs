#nullable enable

using Altinn.Profile.Integrations.Repositories;
using Altinn.Profile.Integrations.Services;

using AutoMapper;

using Moq;

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
}
