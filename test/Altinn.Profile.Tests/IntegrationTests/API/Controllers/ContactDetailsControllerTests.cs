﻿using System;
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
    public void Constructor_WithNullContactDetailsRetriever_ThrowsArgumentNullException()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ContactDetailsController>>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ContactDetailsController(loggerMock.Object, null));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        var contactDetailsRetrieverMock = new Mock<IContactDetailsRetriever>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ContactDetailsController(null, contactDetailsRetrieverMock.Object));
    }

    [Fact]
    public void Constructor_WithValidParameters_InitializesCorrectly()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ContactDetailsController>>();
        var contactDetailsRetrieverMock = new Mock<IContactDetailsRetriever>();

        // Act
        var controller = new ContactDetailsController(loggerMock.Object, contactDetailsRetrieverMock.Object);

        // Assert
        Assert.NotNull(controller);
    }

    [Fact]
    public async Task PostLookup_WhenExceptionOccurs_ReturnsInternalServerError_And_LogsError()
    {
        // Arrange
        var request = new UserContactPointLookup { NationalIdentityNumbers = { "27038893837" } };
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
        UserContactPointLookup request = null;

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
        var request = new UserContactPointLookup { NationalIdentityNumbers = { "05053423096" } };
        var contactDetails = new ContactDetails
        {
            LanguageCode = "en",
            Reservation = false,
            MobilePhoneNumber = "98765432",
            EmailAddress = "user@example.com",
            NationalIdentityNumber = "05053423096"
        };
        var lookupResult = new ContactDetailsLookupResult(matchedContactDetails: [contactDetails], unmatchedNationalIdentityNumbers: []);

        _mockContactDetailsRetriever.Setup(x => x.RetrieveAsync(request)).ReturnsAsync(lookupResult);

        // Act
        var response = await _controller.PostLookup(request);

        // Assert
        var result = Assert.IsType<OkObjectResult>(response.Result);
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);

        var returnValue = Assert.IsType<ContactDetailsLookupResult>(result.Value);
        Assert.NotNull(returnValue);
        Assert.Single(returnValue.MatchedContactDetails);
        Assert.Empty(returnValue.UnmatchedNationalIdentityNumbers);
    }

    [Fact]
    public async Task PostLookup_WhenNationalIdentityNumbersIsNull_ReturnsBadRequest()
    {
        // Arrange
        var request = new UserContactPointLookup { NationalIdentityNumbers = null };

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
        var request = new UserContactPointLookup { NationalIdentityNumbers = { "30083542175" } };
        var lookupResult = new ContactDetailsLookupResult(matchedContactDetails: [], unmatchedNationalIdentityNumbers: ["30083542175"]);

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
        var request = new UserContactPointLookup { NationalIdentityNumbers = { "99020312345" } };
        var lookupResult = new ContactDetailsLookupResult(matchedContactDetails: [], unmatchedNationalIdentityNumbers: ["99020312345"]);

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
        var request = new UserContactPointLookup { NationalIdentityNumbers = { "02038112735" } };
        var contactDetails = new ContactDetails
        {
            LanguageCode = "nb",
            Reservation = false,
            MobilePhoneNumber = "12345678",
            EmailAddress = "test@example.com",
            NationalIdentityNumber = "02038112735"
        };
        var lookupResult = new ContactDetailsLookupResult(matchedContactDetails: [contactDetails], unmatchedNationalIdentityNumbers: []);

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
        Assert.Equal(lookupResult, Assert.IsType<ContactDetailsLookupResult>(result.Value));
    }

    [Fact]
    public async Task PostLookup_WithInvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var invalidRequest = new UserContactPointLookup { NationalIdentityNumbers = { "14078112078" } };
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
        var invalidRequest = new UserContactPointLookup { NationalIdentityNumbers = { "invalid_format" } };

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
        var request = new UserContactPointLookup { NationalIdentityNumbers = { "10060339738", "16051327393" } };
        var contactDetails = new ContactDetails
        {
            LanguageCode = "nb",
            Reservation = false,
            MobilePhoneNumber = "12345678",
            EmailAddress = "test@example.com",
            NationalIdentityNumber = "10060339738"
        };
        var lookupResult = new ContactDetailsLookupResult(matchedContactDetails: [contactDetails], unmatchedNationalIdentityNumbers: ["16051327393"]);

        _mockContactDetailsRetriever.Setup(x => x.RetrieveAsync(request)).ReturnsAsync(lookupResult);

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
    public async Task PostLookup_WithNoNationalIdentityNumbers_ReturnsBadRequest()
    {
        // Arrange
        var invalidRequest = new UserContactPointLookup { NationalIdentityNumbers = { } };

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
        var request = new UserContactPointLookup { NationalIdentityNumbers = { "27038893837" } };
        var contactDetails = new ContactDetails
        {
            LanguageCode = "nb",
            Reservation = false,
            MobilePhoneNumber = "12345678",
            EmailAddress = "test@example.com",
            NationalIdentityNumber = "27038893837"
        };
        var lookupResult = new ContactDetailsLookupResult(matchedContactDetails: [contactDetails], unmatchedNationalIdentityNumbers: []);

        _mockContactDetailsRetriever.Setup(x => x.RetrieveAsync(request)).ReturnsAsync(lookupResult);

        // Act
        var response = await _controller.PostLookup(request);

        // Assert
        var result = Assert.IsType<OkObjectResult>(response.Result);
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        var returnValue = Assert.IsType<ContactDetailsLookupResult>(result.Value);
        Assert.Equal(lookupResult, returnValue);
    }
}
