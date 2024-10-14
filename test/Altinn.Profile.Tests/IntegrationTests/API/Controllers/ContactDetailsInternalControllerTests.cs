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
        Assert.Throws<ArgumentNullException>(() => new ContactDetailsInternalController(_loggerMock.Object, null));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        var contactDetailsRetrieverMock = new Mock<IContactDetailsRetriever>();
        Assert.Throws<ArgumentNullException>(() => new ContactDetailsInternalController(null, contactDetailsRetrieverMock.Object));
    }

    [Fact]
    public void Constructor_WithValidParameters_InitializesCorrectly()
    {
        var contactDetailsRetrieverMock = new Mock<IContactDetailsRetriever>();
        var controller = new ContactDetailsInternalController(_loggerMock.Object, contactDetailsRetrieverMock.Object);
        Assert.NotNull(controller);
    }

    [Fact]
    public async Task PostLookup_WhenRetrievalThrowsException_LogsErrorAndReturnsProblemResult()
    {
        var request = new UserContactPointLookup
        {
            NationalIdentityNumbers = ["05025308508"]
        };

        _mockContactDetailsRetriever.Setup(x => x.RetrieveAsync(request))
            .ThrowsAsync(new Exception("Some error occurred"));

        var response = await _controller.PostLookup(request);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An error occurred while retrieving contact details.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        var problemResult = Assert.IsType<ObjectResult>(response.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, problemResult.StatusCode);
        Assert.Equal("An unexpected error occurred.", Convert.ToString(((ProblemDetails)problemResult.Value).Detail));
    }

    [Fact]
    public async Task PostLookup_WithEmptyNationalIdentityNumbers_ReturnsBadRequest()
    {
        var request = new UserContactPointLookup { NationalIdentityNumbers = [] };
        var response = await _controller.PostLookup(request);
        AssertBadRequest(response, "National identity numbers cannot be null or empty.");
    }

    [Fact]
    public async Task PostLookup_WithInvalidModelState_ReturnsBadRequest()
    {
        var request = new UserContactPointLookup
        {
            NationalIdentityNumbers = ["17092037169"]
        };
        _controller.ModelState.AddModelError("TestError", "Invalid data model");

        var response = await _controller.PostLookup(request);
        AssertBadRequest(response);
    }

    [Fact]
    public async Task PostLookup_WithMixedNationalIdentityNumbers_ReturnsMixedResults()
    {
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

        var response = await _controller.PostLookup(request);

        var result = Assert.IsType<OkObjectResult>(response.Result);
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);

        var returnValue = Assert.IsType<ContactDetailsLookupResult>(result.Value);
        AssertContactDetailsLookupResult(lookupResult, returnValue);
    }

    [Fact]
    public async Task PostLookup_WithNullNationalIdentityNumbers_ReturnsBadRequest()
    {
        var request = new UserContactPointLookup { NationalIdentityNumbers = null };
        var response = await _controller.PostLookup(request);
        AssertBadRequest(response, "National identity numbers cannot be null or empty.");
    }

    private static void AssertBadRequest(ActionResult<ContactDetailsLookupResult> response, string expectedMessage = null)
    {
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(response.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        if (expectedMessage != null)
        {
            Assert.Equal(expectedMessage, badRequestResult.Value);
        }
    }

    private static void AssertContactDetailsLookupResult(ContactDetailsLookupResult expected, ContactDetailsLookupResult actual)
    {
        Assert.Equal(expected, actual);
        Assert.Single(actual.MatchedContactDetails);
        Assert.Single(actual.UnmatchedNationalIdentityNumbers);

        var matchedContactDetails = actual.MatchedContactDetails.FirstOrDefault();
        Assert.NotNull(matchedContactDetails);
        Assert.Equal(expected.MatchedContactDetails.First().Reservation, matchedContactDetails.Reservation);
        Assert.Equal(expected.MatchedContactDetails.First().EmailAddress, matchedContactDetails.EmailAddress);
        Assert.Equal(expected.MatchedContactDetails.First().LanguageCode, matchedContactDetails.LanguageCode);
        Assert.Equal(expected.MatchedContactDetails.First().MobilePhoneNumber, matchedContactDetails.MobilePhoneNumber);
        Assert.Equal(expected.MatchedContactDetails.First().NationalIdentityNumber, matchedContactDetails.NationalIdentityNumber);

        var unmatchedNationalIdentityNumber = actual.UnmatchedNationalIdentityNumbers.FirstOrDefault();
        Assert.Equal(expected.UnmatchedNationalIdentityNumbers.First(), unmatchedNationalIdentityNumber);
    }
}
