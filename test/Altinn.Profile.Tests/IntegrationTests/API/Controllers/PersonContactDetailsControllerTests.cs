using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

using Altinn.Profile.Controllers;
using Altinn.Profile.Models;
using Altinn.Profile.Tests.IntegrationTests.Utils;
using Altinn.Profile.UseCases;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

namespace Altinn.Profile.Tests.IntegrationTests.API.Controllers;

public class PersonContactDetailsControllerTests
{
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly PersonContactDetailsController _controller;
    private readonly Mock<ILogger<PersonContactDetailsController>> _loggerMock;
    private readonly Mock<IPersonContactDetailsRetriever> _mockContactDetailsRetriever;
    private readonly WebApplicationFactorySetup<PersonContactDetailsController> _webApplicationFactorySetup;

    public PersonContactDetailsControllerTests()
    {
        _loggerMock = new Mock<ILogger<PersonContactDetailsController>>();
        _mockContactDetailsRetriever = new Mock<IPersonContactDetailsRetriever>();
        _serializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        _controller = new PersonContactDetailsController(_loggerMock.Object, _mockContactDetailsRetriever.Object);
        _webApplicationFactorySetup = new WebApplicationFactorySetup<PersonContactDetailsController>(new WebApplicationFactory<PersonContactDetailsController>());
    }

    [Fact]
    public void Constructor_WithNullContactDetailsRetriever_ThrowsArgumentNullException()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<PersonContactDetailsController>>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new PersonContactDetailsController(loggerMock.Object, null));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        var contactDetailsRetrieverMock = new Mock<IPersonContactDetailsRetriever>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new PersonContactDetailsController(null, contactDetailsRetrieverMock.Object));
    }

    [Fact]
    public void Constructor_WithValidParameters_InitializesCorrectly()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<PersonContactDetailsController>>();
        var contactDetailsRetrieverMock = new Mock<IPersonContactDetailsRetriever>();

        // Act
        var controller = new PersonContactDetailsController(loggerMock.Object, contactDetailsRetrieverMock.Object);

        // Assert
        Assert.NotNull(controller);
    }

    [Fact]
    public async Task PostLookup_WhenExceptionOccurs_ReturnsInternalServerError_And_LogsError()
    {
        // Arrange
        var request = new PersonContactDetailsLookupCriteria { NationalIdentityNumbers = { "27038893837" } };
        _mockContactDetailsRetriever.Setup(x => x.RetrieveAsync(request)).ThrowsAsync(new Exception("Test exception"));

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
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task PostLookup_WhenLookupCriteriaIsNull_ReturnsBadRequest()
    {
        // Arrange
        PersonContactDetailsLookupCriteria request = null;

        // Act
        var response = await _controller.PostLookup(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(response.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        Assert.Equal("National identity numbers cannot be null or empty.", badRequestResult.Value);
    }

    [Fact]
    public async Task PostLookup_WhenMatchedContactDetailsAreFound_ReturnsOk()
    {
        // Arrange
        var request = new PersonContactDetailsLookupCriteria { NationalIdentityNumbers = { "05053423096" } };
        var contactDetails = new PersonContactDetails
        {
            LanguageCode = "en",
            IsReserved = false,
            MobilePhoneNumber = "98765432",
            EmailAddress = "user@example.com",
            NationalIdentityNumber = "05053423096"
        };
        var lookupResult = new PersonContactDetailsLookupResult(matchedPersonContactDetails: [contactDetails], unmatchedNationalIdentityNumbers: []);

        _mockContactDetailsRetriever.Setup(x => x.RetrieveAsync(request)).ReturnsAsync(lookupResult);

        // Act
        var response = await _controller.PostLookup(request);

        // Assert
        var result = Assert.IsType<OkObjectResult>(response.Result);
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);

        var returnValue = Assert.IsType<PersonContactDetailsLookupResult>(result.Value);
        Assert.NotNull(returnValue);
        Assert.Single(returnValue.MatchedPersonContactDetails);
        Assert.Empty(returnValue.UnmatchedNationalIdentityNumbers);
    }

    [Fact]
    public async Task PostLookup_WhenNationalIdentityNumbersIsNull_ReturnsBadRequest()
    {
        // Arrange
        var request = new PersonContactDetailsLookupCriteria { NationalIdentityNumbers = null };

        // Act
        var response = await _controller.PostLookup(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(response.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        Assert.Equal("National identity numbers cannot be null or empty.", badRequestResult.Value);
    }

    [Fact]
    public async Task PostLookup_WhenNoContactDetailsFound_ReturnsNotFound()
    {
        // Arrange
        var request = new PersonContactDetailsLookupCriteria { NationalIdentityNumbers = { "30083542175" } };
        var lookupResult = new PersonContactDetailsLookupResult(matchedPersonContactDetails: [], unmatchedNationalIdentityNumbers: ["30083542175"]);

        _mockContactDetailsRetriever.Setup(x => x.RetrieveAsync(request)).ReturnsAsync(lookupResult);

        // Act
        var response = await _controller.PostLookup(request);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundResult>(response.Result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task PostLookup_WhenNoMatchedContactDetailsAreFound_ReturnsNotFound()
    {
        // Arrange
        var request = new PersonContactDetailsLookupCriteria { NationalIdentityNumbers = { "99020312345" } };
        var lookupResult = new PersonContactDetailsLookupResult(matchedPersonContactDetails: [], unmatchedNationalIdentityNumbers: ["99020312345"]);

        _mockContactDetailsRetriever.Setup(x => x.RetrieveAsync(request)).ReturnsAsync(lookupResult);

        // Act
        var response = await _controller.PostLookup(request);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundResult>(response.Result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task PostLookup_WhenServiceCallIsLongRunning_DoesNotBlock()
    {
        // Arrange
        var request = new PersonContactDetailsLookupCriteria { NationalIdentityNumbers = { "02038112735" } };
        var contactDetails = new PersonContactDetails
        {
            LanguageCode = "nb",
            IsReserved = false,
            MobilePhoneNumber = "12345678",
            EmailAddress = "test@example.com",
            NationalIdentityNumber = "02038112735"
        };
        var lookupResult = new PersonContactDetailsLookupResult(matchedPersonContactDetails: [contactDetails], unmatchedNationalIdentityNumbers: []);

        _mockContactDetailsRetriever.Setup(x => x.RetrieveAsync(request)).ReturnsAsync(() =>
        {
            Task.Delay(5000).Wait(); // Simulate long-running task
            return lookupResult;
        });

        // Act
        var response = await _controller.PostLookup(request);

        // Assert
        var result = Assert.IsType<OkObjectResult>(response.Result);
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.Equal(lookupResult, Assert.IsType<PersonContactDetailsLookupResult>(result.Value));
    }

    [Fact]
    public async Task PostLookup_WithInvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var invalidRequest = new PersonContactDetailsLookupCriteria { NationalIdentityNumbers = { "14078112078" } };
        _controller.ModelState.AddModelError("InvalidKey", "Invalid error message");

        // Act
        var response = await _controller.PostLookup(invalidRequest);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(response.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task PostLookup_WithInvalidSingleNationalIdentityNumber_ReturnsBadRequest()
    {
        // Arrange
        var invalidRequest = new PersonContactDetailsLookupCriteria { NationalIdentityNumbers = { "invalid_format" } };

        // Act
        var response = await _controller.PostLookup(invalidRequest);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundResult>(response.Result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task PostLookup_WithMixedNationalIdentityNumbers_ReturnsMixedResults()
    {
        // Arrange
        var request = new PersonContactDetailsLookupCriteria { NationalIdentityNumbers = { "10060339738", "16051327393" } };
        var contactDetails = new PersonContactDetails
        {
            LanguageCode = "nb",
            IsReserved = false,
            MobilePhoneNumber = "12345678",
            EmailAddress = "test@example.com",
            NationalIdentityNumber = "10060339738"
        };
        var lookupResult = new PersonContactDetailsLookupResult(matchedPersonContactDetails: [contactDetails], unmatchedNationalIdentityNumbers: ["16051327393"]);

        _mockContactDetailsRetriever.Setup(x => x.RetrieveAsync(request)).ReturnsAsync(lookupResult);

        // Act
        var response = await _controller.PostLookup(request);

        // Assert
        var result = Assert.IsType<OkObjectResult>(response.Result);
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        var returnValue = Assert.IsType<PersonContactDetailsLookupResult>(result.Value);
        Assert.Equal(lookupResult, returnValue);
        Assert.Single(returnValue.MatchedPersonContactDetails);
        Assert.Single(returnValue.UnmatchedNationalIdentityNumbers);
    }

    [Fact]
    public async Task PostLookup_WithNoNationalIdentityNumbers_ReturnsBadRequest()
    {
        // Arrange
        var invalidRequest = new PersonContactDetailsLookupCriteria { NationalIdentityNumbers = { } };

        // Act
        var response = await _controller.PostLookup(invalidRequest);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(response.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        Assert.Equal("National identity numbers cannot be null or empty.", badRequestResult.Value);
    }

    [Fact]
    public async Task PostLookup_WithValidRequest_ReturnsOk()
    {
        // Arrange
        var request = new PersonContactDetailsLookupCriteria { NationalIdentityNumbers = { "27038893837" } };
        var contactDetails = new PersonContactDetails
        {
            LanguageCode = "nb",
            IsReserved = false,
            MobilePhoneNumber = "12345678",
            EmailAddress = "test@example.com",
            NationalIdentityNumber = "27038893837"
        };
        var lookupResult = new PersonContactDetailsLookupResult(matchedPersonContactDetails: [contactDetails], unmatchedNationalIdentityNumbers: []);

        _mockContactDetailsRetriever.Setup(x => x.RetrieveAsync(request)).ReturnsAsync(lookupResult);

        // Act
        var response = await _controller.PostLookup(request);

        // Assert
        var result = Assert.IsType<OkObjectResult>(response.Result);
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        var returnValue = Assert.IsType<PersonContactDetailsLookupResult>(result.Value);
        Assert.Equal(lookupResult, returnValue);
    }

    ////[Fact]
    ////public async Task PostLookup_WithValidNationalIdentityNumbers_ReturnsValidResults_IntegrationTest()
    ////{
    ////    // Arrange
    ////    HttpClient client = _webApplicationFactorySetup.GetTestServerClient();
    ////    var lookupCriteria = new PersonContactDetailsLookupCriteria
    ////    {
    ////        NationalIdentityNumbers = ["02018090573", "03070100664", "03074500217"]
    ////    };

    ////    HttpRequestMessage httpRequestMessage = CreatePostRequest("/profile/api/v1/person/contact/details/lookup");
    ////    httpRequestMessage.Content = JsonContent.Create(lookupCriteria);

    ////    // Act
    ////    HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

    ////    // Assert
    ////    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    ////    string responseContent = await response.Content.ReadAsStringAsync();
    ////    PersonContactDetailsLookupResult lookupResult = JsonSerializer.Deserialize<PersonContactDetailsLookupResult>(responseContent, _serializerOptions);

    ////    Assert.NotNull(lookupResult);
    ////    Assert.NotNull(lookupResult.MatchedPersonContactDetails);
    ////    Assert.Null(lookupResult.UnmatchedNationalIdentityNumbers);
    ////    Assert.Equal(3, lookupResult.MatchedPersonContactDetails.Count);
    ////}

    [Fact]
    public async Task PostLookup_WhenNationalIdentityNumbersIsNull_ReturnsBadRequest_IntegrationTest()
    {
        // Arrange
        var client = _webApplicationFactorySetup.GetTestServerClient();
        var lookupCriteria = new PersonContactDetailsLookupCriteria
        {
            NationalIdentityNumbers = null
        };
        HttpRequestMessage httpRequestMessage = CreatePostRequest("/profile/api/v1/person/contact/details/lookup");
        httpRequestMessage.Content = JsonContent.Create(lookupCriteria);

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostLookup_WhenNoContactDetailsFound_ReturnsNotFound_IntegrationTest()
    {
        // Arrange
        var client = _webApplicationFactorySetup.GetTestServerClient();
        var lookupCriteria = new PersonContactDetailsLookupCriteria
        {
            NationalIdentityNumbers = ["false"]
        };
        HttpRequestMessage httpRequestMessage = CreatePostRequest("/profile/api/v1/person/contact/details/lookup");
        httpRequestMessage.Content = JsonContent.Create(lookupCriteria);

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    ////[Fact]
    ////public async Task PostLookup_WithMixedNationalIdentityNumbers_ReturnsMixedResults_IntegrationTest()
    ////{
    ////    // Arrange
    ////    HttpClient client = _webApplicationFactorySetup.GetTestServerClient();
    ////    var lookupCriteria = new PersonContactDetailsLookupCriteria
    ////    {
    ////        NationalIdentityNumbers = ["07875499461", "none", "07844998311"]
    ////    };

    ////    HttpRequestMessage httpRequestMessage = CreatePostRequest("/profile/api/v1/person/contact/details/lookup");
    ////    httpRequestMessage.Content = JsonContent.Create(lookupCriteria);

    ////    // Act
    ////    HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

    ////    // Assert
    ////    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    ////    string responseContent = await response.Content.ReadAsStringAsync();
    ////    PersonContactDetailsLookupResult lookupResult = JsonSerializer.Deserialize<PersonContactDetailsLookupResult>(responseContent, _serializerOptions);

    ////    Assert.NotNull(lookupResult);
    ////    Assert.NotNull(lookupResult.MatchedPersonContactDetails);
    ////    Assert.Single(lookupResult.UnmatchedNationalIdentityNumbers);
    ////    Assert.NotNull(lookupResult.UnmatchedNationalIdentityNumbers);
    ////    Assert.Equal(2, lookupResult.MatchedPersonContactDetails.Count);
    ////}

    private static HttpRequestMessage CreatePostRequest(string requestUri)
    {
        int userId = 2516356;
        HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, requestUri);
        string token = PrincipalUtil.GetToken(userId);
        httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return httpRequestMessage;
    }
}
