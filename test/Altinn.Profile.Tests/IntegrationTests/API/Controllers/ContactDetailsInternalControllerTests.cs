using System;
using System.Threading.Tasks;

using Altinn.Profile.Controllers;
using Altinn.Profile.Models;
using Altinn.Profile.UseCases;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

namespace Altinn.Profile.Tests.IntegrationTests.API.Controllers;

public class ContactDetailsInternalControllerTests
{
    private readonly ContactDetailsInternalController _controller;
    private readonly Mock<ILogger<ContactDetailsInternalController>> _loggerMock;
    private readonly Mock<IContactDetailsRetriever> _mockContactDetailsRetriever;

    public ContactDetailsInternalControllerTests()
    {
        _loggerMock = new Mock<ILogger<ContactDetailsInternalController>>();
        _mockContactDetailsRetriever = new Mock<IContactDetailsRetriever>();
        _controller = new ContactDetailsInternalController(_loggerMock.Object, _mockContactDetailsRetriever.Object);
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        var contactDetailsRetrieverMock = new Mock<IContactDetailsRetriever>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ContactDetailsInternalController(null, contactDetailsRetrieverMock.Object));
    }

    [Fact]
    public void Constructor_NullContactDetailsRetriever_ThrowsArgumentNullException()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ContactDetailsInternalController>>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ContactDetailsInternalController(loggerMock.Object, null));
    }

    [Fact]
    public void Constructor_ValidParameters_InitializesCorrectly()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ContactDetailsInternalController>>();
        var contactDetailsRetrieverMock = new Mock<IContactDetailsRetriever>();

        // Act
        var controller = new ContactDetailsInternalController(loggerMock.Object, contactDetailsRetrieverMock.Object);

        // Assert
        Assert.NotNull(controller);
    }

    [Fact]
    public async Task PostLookup_ReturnsMixedResults_WhenOneNumberMatchesAndOneDoesNot()
    {
        // Arrange
        var request = new UserContactPointLookup
        {
            NationalIdentityNumbers = ["05025308508", "08110270527"]
        };

        var contactDetails = new ContactDetails
        {
            LanguageCode = "nb",
            Reservation = false,
            MobilePhoneNumber = "12345678",
            EmailAddress = "test@example.com",
            NationalIdentityNumber = "05025308508"
        };

        var lookupResult = new ContactDetailsLookupResult(
            matchedContactDetails: [contactDetails],
            unmatchedNationalIdentityNumbers: ["08110270527"]);

        _mockContactDetailsRetriever.Setup(x => x.RetrieveAsync(request))
            .ReturnsAsync(lookupResult);

        // Act
        var response = await _controller.PostLookup(request);

        // Assert
        var result = Assert.IsType<OkObjectResult>(response.Result);
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);

        var returnValue = Assert.IsType<ContactDetailsLookupResult>(result.Value);
        Assert.Equal(lookupResult, returnValue);
        Assert.Single(returnValue.MatchedContactDetails);
        Assert.Single(returnValue.UnmatchedNationalIdentityNumbers);
    }
}
