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

public class PersonContactDetailsInternalControllerTests
{
    private readonly PersonContactDetailsInternalController _controller;
    private readonly Mock<ILogger<PersonContactDetailsInternalController>> _loggerMock;
    private readonly Mock<IPersonContactDetailsRetriever> _mockContactDetailsRetriever;

    public PersonContactDetailsInternalControllerTests()
    {
        _loggerMock = new Mock<ILogger<PersonContactDetailsInternalController>>();
        _mockContactDetailsRetriever = new Mock<IPersonContactDetailsRetriever>();
        _controller = new PersonContactDetailsInternalController(_loggerMock.Object, _mockContactDetailsRetriever.Object);
    }

    [Fact]
    public void Constructor_WithNullContactDetailsRetriever_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new PersonContactDetailsInternalController(_loggerMock.Object, null));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        var contactDetailsRetrieverMock = new Mock<IPersonContactDetailsRetriever>();
        Assert.Throws<ArgumentNullException>(() => new PersonContactDetailsInternalController(null, contactDetailsRetrieverMock.Object));
    }

    [Fact]
    public void Constructor_WithValidParameters_InitializesCorrectly()
    {
        var contactDetailsRetrieverMock = new Mock<IPersonContactDetailsRetriever>();
        var controller = new PersonContactDetailsInternalController(_loggerMock.Object, contactDetailsRetrieverMock.Object);
        Assert.NotNull(controller);
    }

    [Fact]
    public async Task PostLookup_WhenRetrievalThrowsException_LogsErrorAndReturnsProblemResult()
    {
        var request = new PersonContactDetailsLookupCriteria
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
        var request = new PersonContactDetailsLookupCriteria { NationalIdentityNumbers = [] };
        var response = await _controller.PostLookup(request);
        AssertBadRequest(response, "National identity numbers cannot be null or empty.");
    }

    [Fact]
    public async Task PostLookup_WithInvalidModelState_ReturnsBadRequest()
    {
        var request = new PersonContactDetailsLookupCriteria
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
        var request = new PersonContactDetailsLookupCriteria
        {
            NationalIdentityNumbers = ["05025308508", "08110270527"]
        };

        var contactDetails = new PersonContactDetails
        {
            LanguageCode = "nb",
            IsReserved = false,
            MobilePhoneNumber = "12345678",
            EmailAddress = "test@example.com",
            NationalIdentityNumber = "05025308508"
        };

        var lookupResult = new PersonContactDetailsLookupResult(
            matchedPersonContactDetails: [contactDetails],
            unmatchedNationalIdentityNumbers: ["08110270527"]);

        _mockContactDetailsRetriever.Setup(x => x.RetrieveAsync(request))
            .ReturnsAsync(lookupResult);

        var response = await _controller.PostLookup(request);

        var result = Assert.IsType<OkObjectResult>(response.Result);
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);

        var returnValue = Assert.IsType<PersonContactDetailsLookupResult>(result.Value);
        AssertContactDetailsLookupResult(lookupResult, returnValue);
    }

    [Fact]
    public async Task PostLookup_WithNullNationalIdentityNumbers_ReturnsBadRequest()
    {
        var request = new PersonContactDetailsLookupCriteria { NationalIdentityNumbers = null };
        var response = await _controller.PostLookup(request);
        AssertBadRequest(response, "National identity numbers cannot be null or empty.");
    }

    private static void AssertBadRequest(ActionResult<PersonContactDetailsLookupResult> response, string expectedMessage = null)
    {
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(response.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        if (expectedMessage != null)
        {
            Assert.Equal(expectedMessage, badRequestResult.Value);
        }
    }

    private static void AssertContactDetailsLookupResult(PersonContactDetailsLookupResult expected, PersonContactDetailsLookupResult actual)
    {
        Assert.Equal(expected, actual);
        Assert.Single(actual.MatchedPersonContactDetails);
        Assert.Single(actual.UnmatchedNationalIdentityNumbers);

        var matchedContactDetails = actual.MatchedPersonContactDetails.FirstOrDefault();
        Assert.NotNull(matchedContactDetails);
        Assert.Equal(expected.MatchedPersonContactDetails.First().IsReserved, matchedContactDetails.IsReserved);
        Assert.Equal(expected.MatchedPersonContactDetails.First().EmailAddress, matchedContactDetails.EmailAddress);
        Assert.Equal(expected.MatchedPersonContactDetails.First().LanguageCode, matchedContactDetails.LanguageCode);
        Assert.Equal(expected.MatchedPersonContactDetails.First().MobilePhoneNumber, matchedContactDetails.MobilePhoneNumber);
        Assert.Equal(expected.MatchedPersonContactDetails.First().NationalIdentityNumber, matchedContactDetails.NationalIdentityNumber);

        var unmatchedNationalIdentityNumber = actual.UnmatchedNationalIdentityNumbers.FirstOrDefault();
        Assert.Equal(expected.UnmatchedNationalIdentityNumbers.First(), unmatchedNationalIdentityNumber);
    }
}
