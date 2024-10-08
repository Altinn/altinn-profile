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

public class ContactDetailsControllerTests
{
    private readonly ContactDetailsController _controller;
    private readonly Mock<ILogger<ContactDetailsController>> _loggerMock;
    private readonly Mock<IContactDetailsRetriever> _mockContactDetailsRetriever;

    public ContactDetailsControllerTests()
    {
        _loggerMock = new Mock<ILogger<ContactDetailsController>>();
        _mockContactDetailsRetriever = new Mock<IContactDetailsRetriever>();
        _controller = new ContactDetailsController(_loggerMock.Object, _mockContactDetailsRetriever.Object);
    }

    [Fact]
    public async Task PostLookup_ReturnsBadRequestResult_WhenRequestIsInvalid()
    {
        // Arrange
        var invalidRequest = new UserContactPointLookup
        {
            NationalIdentityNumbers = []
        };

        // Act
        var response = await _controller.PostLookup(invalidRequest);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestResult>(response.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task PostLookup_ReturnsInternalServerErrorResult_LogError_WhenExceptionOccurs()
    {
        // Arrange
        var request = new UserContactPointLookup
        {
            NationalIdentityNumbers = ["27038893837"]
        };

        _mockContactDetailsRetriever.Setup(x => x.RetrieveAsync(request))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var response = await _controller.PostLookup(request);

        // Assert
        var problemResult = Assert.IsType<ObjectResult>(response.Result);

        Assert.Equal(StatusCodes.Status500InternalServerError, problemResult.StatusCode);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An error occurred while retrieving contact details.")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task PostLookup_ReturnsMixedResults_WhenOneNumberMatchesAndOneDoesNot()
    {
        // Arrange
        var request = new UserContactPointLookup
        {
            NationalIdentityNumbers = ["10060339738", "16051327393"]
        };

        var contactDetails = new ContactDetails
        {
            LanguageCode = "nb",
            Reservation = false,
            MobilePhoneNumber = "12345678",
            EmailAddress = "test@example.com",
            NationalIdentityNumber = "10060339738"
        };

        var lookupResult = new ContactDetailsLookupResult(
            matchedContactDetails: [contactDetails],
            unmatchedNationalIdentityNumbers: ["16051327393"]);

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

    [Fact]
    public async Task PostLookup_ReturnsNotFoundResult_WhenNoContactDetailsFound()
    {
        // Arrange
        var request = new UserContactPointLookup
        {
            NationalIdentityNumbers = ["30083542175"]
        };

        var lookupResult = new ContactDetailsLookupResult(
            matchedContactDetails: [],
            unmatchedNationalIdentityNumbers: [request.NationalIdentityNumbers[0]]);

        _mockContactDetailsRetriever.Setup(x => x.RetrieveAsync(request))
            .ReturnsAsync(lookupResult);

        // Act
        var response = await _controller.PostLookup(request);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundResult>(response.Result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task PostLookup_ReturnsOkResult_WhenRequestIsValid()
    {
        // Arrange
        var request = new UserContactPointLookup
        {
            NationalIdentityNumbers = ["27038893837"]
        };

        var contactDetails = new ContactDetails
        {
            LanguageCode = "nb",
            Reservation = false,
            MobilePhoneNumber = "12345678",
            EmailAddress = "test@example.com",
            NationalIdentityNumber = "27038893837"
        };

        var lookupResult = new ContactDetailsLookupResult(
            matchedContactDetails: [contactDetails],
            unmatchedNationalIdentityNumbers: []);

        _mockContactDetailsRetriever.Setup(x => x.RetrieveAsync(request))
            .ReturnsAsync(lookupResult);

        // Act
        var response = await _controller.PostLookup(request);

        // Assert
        var result = Assert.IsType<OkObjectResult>(response.Result);
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);

        var returnValue = Assert.IsType<ContactDetailsLookupResult>(result.Value);
        Assert.Equal(lookupResult, returnValue);
    }
}
