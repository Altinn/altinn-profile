﻿#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Altinn.Profile.Integrations.Entities;
using Altinn.Profile.Integrations.Services;
using Altinn.Profile.Models;
using Altinn.Profile.UseCases;

using Moq;

using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations;

public class UserContactDetailsRetrieverTests
{
    private readonly ContactDetailsRetriever _retriever;
    private readonly Mock<IPersonService> _mockPersonService;

    public UserContactDetailsRetrieverTests()
    {
        _mockPersonService = new Mock<IPersonService>();
        _retriever = new ContactDetailsRetriever(_mockPersonService.Object);
    }

    [Fact]
    public async Task RetrieveAsync_WhenLookupCriteriaIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _retriever.RetrieveAsync(null));
    }

    [Fact]
    public async Task RetrieveAsync_WhenNationalIdentityNumbersIsEmpty_ReturnsFalse()
    {
        // Arrange
        var lookupCriteria = new UserContactPointLookup { NationalIdentityNumbers = [] };

        // Act
        var result = await _retriever.RetrieveAsync(lookupCriteria);

        // Assert
        Assert.True(result.IsError);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task RetrieveAsync_WhenNoContactDetailsFound_ReturnsFalse()
    {
        // Arrange
        var lookupCriteria = new UserContactPointLookup
        {
            NationalIdentityNumbers = ["08119043698"]
        };

        _mockPersonService.Setup(s => s.GetContactDetailsAsync(lookupCriteria.NationalIdentityNumbers)).ReturnsAsync(false);

        // Act
        var result = await _retriever.RetrieveAsync(lookupCriteria);

        // Assert
        Assert.True(result.IsError);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task RetrieveAsync_WhenValidNationalIdentityNumbers_ReturnsExpectedContactDetailsLookupResult()
    {
        // Arrange
        var lookupCriteria = new UserContactPointLookup
        {
            NationalIdentityNumbers = ["08053414843"]
        };

        var personContactDetails = new PersonContactDetails
        {
            IsReserved = false,
            LanguageCode = "en",
            MobilePhoneNumber = "1234567890",
            EmailAddress = "test@example.com",
            NationalIdentityNumber = "08053414843"
        };

        var lookupResult = new PersonContactDetailsLookupResult
        {
            UnmatchedNationalIdentityNumbers = [],
            MatchedPersonContactDetails = [personContactDetails]
        };

        _mockPersonService
            .Setup(e => e.GetContactDetailsAsync(lookupCriteria.NationalIdentityNumbers))
            .ReturnsAsync(lookupResult);

        // Act
        var result = await _retriever.RetrieveAsync(lookupCriteria);

        // Assert
        Assert.True(result.IsSuccess);
        IEnumerable<string>? unmatchedNationalIdentityNumbers = [];
        IEnumerable<ContactDetails>? matchedPersonContactDetails = [];

        result.Match(
            success =>
            {
                matchedPersonContactDetails = success.MatchedContactDetails;
                unmatchedNationalIdentityNumbers = success.UnmatchedNationalIdentityNumbers;
            },
            failure =>
            {
                matchedPersonContactDetails = null;
            });

        Assert.NotNull(matchedPersonContactDetails);
        Assert.Single(matchedPersonContactDetails);

        var matchPersonContactDetails = matchedPersonContactDetails.FirstOrDefault();
        Assert.NotNull(matchPersonContactDetails);
        Assert.Equal(personContactDetails.IsReserved, matchPersonContactDetails.Reservation);
        Assert.Equal(personContactDetails.LanguageCode, matchPersonContactDetails.LanguageCode);
        Assert.Equal(personContactDetails.MobilePhoneNumber, matchPersonContactDetails.MobilePhoneNumber);
        Assert.Equal(personContactDetails.EmailAddress, matchPersonContactDetails.EmailAddress);
        Assert.Equal(personContactDetails.NationalIdentityNumber, matchPersonContactDetails.NationalIdentityNumber);

        Assert.Empty(unmatchedNationalIdentityNumbers);
    }
}
