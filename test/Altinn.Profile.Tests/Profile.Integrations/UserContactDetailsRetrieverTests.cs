using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

using Altinn.Profile.Core;
using Altinn.Profile.Integrations.Entities;
using Altinn.Profile.Integrations.Services;
using Altinn.Profile.Models;
using Altinn.Profile.UseCases;

using Moq;

using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations;

public class UserContactDetailsRetrieverTests
{
    private readonly Mock<IPersonService> _mockRegisterService;
    private readonly ContactDetailsRetriever _retriever;

    public UserContactDetailsRetrieverTests()
    {
        _mockRegisterService = new Mock<IPersonService>();
        _retriever = new ContactDetailsRetriever(_mockRegisterService.Object);
    }

    [Fact]
    public async Task RetrieveAsync_ThrowsArgumentNullException_WhenLookupCriteriaIsNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _retriever.RetrieveAsync(null));
    }

    [Fact]
    public async Task RetrieveAsync_ReturnsFalse_WhenNationalIdentityNumbersIsEmpty()
    {
        // Arrange
        var lookupCriteria = new UserContactPointLookup { NationalIdentityNumbers = [] };

        // Act
        var result = await _retriever.RetrieveAsync(lookupCriteria);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task RetrieveAsync_ReturnsFalse_WhenNoContactDetailsFound()
    {
        // Arrange
        var lookupCriteria = new UserContactPointLookup
        {
            NationalIdentityNumbers = new List<string> { "08119043698" }
        };

        _mockRegisterService.Setup(s => s.GetUserContactAsync(lookupCriteria.NationalIdentityNumbers)).ReturnsAsync(false);

        // Act
        var result = await _retriever.RetrieveAsync(lookupCriteria);

        // Assert
        Assert.False(result.IsSuccess);
    }
}
