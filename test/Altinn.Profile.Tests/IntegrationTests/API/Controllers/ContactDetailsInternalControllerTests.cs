using System;
using System.Linq;
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
    public void Constructor_WithNullContactDetailsRetriever_ThrowsArgumentNullException()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ContactDetailsInternalController>>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ContactDetailsInternalController(loggerMock.Object, null));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        var contactDetailsRetrieverMock = new Mock<IContactDetailsRetriever>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ContactDetailsInternalController(null, contactDetailsRetrieverMock.Object));
    }

    [Fact]
    public void Constructor_WithValidParameters_InitializesCorrectly()
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
    public async Task PostLookup_WithInvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var request = new UserContactPointLookup
        {
            NationalIdentityNumbers = ["17092037169"]
        };

        _controller.ModelState.AddModelError("TestError", "Invalid data model");

        // Act
        var response = await _controller.PostLookup(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(response.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task PostLookup_WithMixedNationalIdentityNumbers_ReturnsMixedResults()
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

        var matchedContactDetails = returnValue.MatchedContactDetails.FirstOrDefault();
        Assert.NotNull(matchedContactDetails);
        Assert.Equal(contactDetails.Reservation, matchedContactDetails.Reservation);
        Assert.Equal(contactDetails.EmailAddress, matchedContactDetails.EmailAddress);
        Assert.Equal(contactDetails.LanguageCode, matchedContactDetails.LanguageCode);
        Assert.Equal(contactDetails.MobilePhoneNumber, matchedContactDetails.MobilePhoneNumber);
        Assert.Equal(contactDetails.NationalIdentityNumber, matchedContactDetails.NationalIdentityNumber);

        var unmatchedNationalIdentityNumber = returnValue.UnmatchedNationalIdentityNumbers.FirstOrDefault();
        Assert.Equal("08110270527", unmatchedNationalIdentityNumber);
    }
}
