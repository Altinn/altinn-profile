//#nullable enable

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//using Altinn.Profile.Integrations.Entities;
//using Altinn.Profile.Integrations.Services;
//using Altinn.Profile.Models;
//using Altinn.Profile.UseCases;

//using Moq;

//using Xunit;

//namespace Altinn.Profile.Tests.Profile.Integrations;

//public class PersonContactDetailsRetrieverTests
//{
//    private readonly PersonContactDetailsRetriever _retriever;
//    private readonly Mock<IPersonService> _mockPersonService;

//    public PersonContactDetailsRetrieverTests()
//    {
//        _mockPersonService = new Mock<IPersonService>();
//        _retriever = new PersonContactDetailsRetriever(_mockPersonService.Object);
//    }

//    [Fact]
//    public async Task RetrieveAsync_WhenLookupCriteriaIsNull_ThrowsArgumentNullException()
//    {
//        // Act & Assert
//        await Assert.ThrowsAsync<ArgumentNullException>(() => _retriever.RetrieveAsync(null));
//    }

//    [Fact]
//    public async Task RetrieveAsync_WhenNationalIdentityNumbersIsEmpty_ReturnsFalse()
//    {
//        // Arrange
//        var lookupCriteria = new PersonContactDetailsLookupCriteria { NationalIdentityNumbers = [] };

//        // Act
//        var result = await _retriever.RetrieveAsync(lookupCriteria);

//        // Assert
//        Assert.True(result.IsError);
//        Assert.False(result.IsSuccess);
//    }

//    [Fact]
//    public async Task RetrieveAsync_WhenNoContactDetailsFound_ReturnsFalse()
//    {
//        // Arrange
//        var lookupCriteria = new PersonContactDetailsLookupCriteria
//        {
//            NationalIdentityNumbers = ["08119043698"]
//        };

//        _mockPersonService.Setup(s => s.GetContactPreferencesAsync(lookupCriteria.NationalIdentityNumbers)).ReturnsAsync(false);

//        // Act
//        var result = await _retriever.RetrieveAsync(lookupCriteria);

//        // Assert
//        Assert.True(result.IsError);
//        Assert.False(result.IsSuccess);
//    }

//    [Fact]
//    public async Task RetrieveAsync_WhenValidNationalIdentityNumbers_ReturnsExpectedContactDetailsLookupResult()
//    {
//        // Arrange
//        var lookupCriteria = new PersonContactDetailsLookupCriteria
//        {
//            NationalIdentityNumbers = ["08053414843"]
//        };

//        var personContactDetails = new PersonContactPreferences
//        {
//            IsReserved = false,
//            LanguageCode = "en",
//            MobileNumber = "1234567890",
//            Email = "test@example.com",
//            NationalIdentityNumber = "08053414843"
//        };

//        var lookupResult = new PersonContactPreferencesLookupResult
//        {
//            UnmatchedNationalIdentityNumbers = [],
//            MatchedPersonContactPreferences = [personContactDetails]
//        };

//        _mockPersonService
//            .Setup(e => e.GetContactPreferencesAsync(lookupCriteria.NationalIdentityNumbers))
//            .ReturnsAsync(lookupResult);

//        // Act
//        var result = await _retriever.RetrieveAsync(lookupCriteria);

//        // Assert
//        Assert.True(result.IsSuccess);
//        IEnumerable<string>? unmatchedNationalIdentityNumbers = [];
//        IEnumerable<Models.PersonContactDetails>? matchedPersonContactDetails = [];

//        result.Match(
//            success =>
//            {
//                matchedPersonContactDetails = success.MatchedPersonContactDetails;
//                unmatchedNationalIdentityNumbers = success.UnmatchedNationalIdentityNumbers;
//            },
//            failure =>
//            {
//                matchedPersonContactDetails = null;
//            });

//        Assert.NotNull(matchedPersonContactDetails);
//        Assert.Single(matchedPersonContactDetails);

//        var matchPersonContactDetails = matchedPersonContactDetails.FirstOrDefault();
//        Assert.NotNull(matchPersonContactDetails);
//        Assert.Equal(personContactDetails.IsReserved, matchPersonContactDetails.IsReserved);
//        Assert.Equal(personContactDetails.LanguageCode, matchPersonContactDetails.LanguageCode);
//        Assert.Equal(personContactDetails.MobileNumber, matchPersonContactDetails.MobilePhoneNumber);
//        Assert.Equal(personContactDetails.Email, matchPersonContactDetails.EmailAddress);
//        Assert.Equal(personContactDetails.NationalIdentityNumber, matchPersonContactDetails.NationalIdentityNumber);

//        Assert.Empty(unmatchedNationalIdentityNumbers);
//    }
//}
