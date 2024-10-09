#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using Altinn.Profile.Integrations.Entities;
using Altinn.Profile.Integrations.Repositories;
using Altinn.Profile.Integrations.Services;

using AutoMapper;

using Moq;

using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations;

public class PersonServiceTests
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IPersonRepository> _personRepositoryMock;
    private readonly Mock<INationalIdentityNumberChecker> _nationalIdentityNumberCheckerMock;

    private readonly PersonService _personService;

    public PersonServiceTests()
    {
        _mapperMock = new Mock<IMapper>();
        _personRepositoryMock = new Mock<IPersonRepository>();
        _nationalIdentityNumberCheckerMock = new Mock<INationalIdentityNumberChecker>();
        _personService = new PersonService(_mapperMock.Object, _personRepositoryMock.Object, _nationalIdentityNumberCheckerMock.Object);
    }

    [Fact]
    public async Task GetContactDetailsAsync_WhenAllNumbersValidAndAllContactsFound_ReturnsAllContacts()
    {
        // Arrange
        var nationalIdentityNumbers = new List<string> { "17092037169", "17033112912" };

        var firstRandomPerson = new Person
        {
            LanguageCode = "nb",
            FnumberAk = "17092037169",
            MailboxAddress = "1234 Test St",
            MobilePhoneNumber = "+4791234567",
            EmailAddress = "test@example.com",
            X509Certificate = "certificate_data"
        };

        var secondRandomPerson = new Person
        {
            LanguageCode = "nb",
            FnumberAk = "17033112912",
            MailboxAddress = "1234 Test St",
            MobilePhoneNumber = "+4791234567",
            EmailAddress = "test@example.com",
            X509Certificate = "certificate_data"
        };

        var personList = new List<Person> { firstRandomPerson, secondRandomPerson }.ToImmutableList();

        var firstMappedContactDetails = new Mock<IPersonContactDetails>();
        firstMappedContactDetails.SetupGet(x => x.IsReserved).Returns(firstRandomPerson.Reservation);
        firstMappedContactDetails.SetupGet(x => x.EmailAddress).Returns(firstRandomPerson.EmailAddress);
        firstMappedContactDetails.SetupGet(x => x.LanguageCode).Returns(firstRandomPerson.LanguageCode);
        firstMappedContactDetails.SetupGet(x => x.NationalIdentityNumber).Returns(firstRandomPerson.FnumberAk);
        firstMappedContactDetails.SetupGet(x => x.MobilePhoneNumber).Returns(firstRandomPerson.MobilePhoneNumber);

        _mapperMock.Setup(x => x.Map<IPersonContactDetails>(firstRandomPerson))
            .Returns(firstMappedContactDetails.Object);

        var secondMappedContactDetails = new Mock<IPersonContactDetails>();
        secondMappedContactDetails.SetupGet(x => x.IsReserved).Returns(secondRandomPerson.Reservation);
        secondMappedContactDetails.SetupGet(x => x.EmailAddress).Returns(secondRandomPerson.EmailAddress);
        secondMappedContactDetails.SetupGet(x => x.LanguageCode).Returns(secondRandomPerson.LanguageCode);
        secondMappedContactDetails.SetupGet(x => x.NationalIdentityNumber).Returns(secondRandomPerson.FnumberAk);
        secondMappedContactDetails.SetupGet(x => x.MobilePhoneNumber).Returns(secondRandomPerson.MobilePhoneNumber);

        _mapperMock.Setup(x => x.Map<IPersonContactDetails>(secondRandomPerson))
            .Returns(secondMappedContactDetails.Object);

        _personRepositoryMock
            .Setup(x => x.GetContactDetailsAsync(nationalIdentityNumbers))
            .ReturnsAsync(personList);

        _nationalIdentityNumberCheckerMock
            .Setup(x => x.GetValid(nationalIdentityNumbers))
            .Returns(nationalIdentityNumbers.ToImmutableList());

        // Act
        var result = await _personService.GetContactDetailsAsync(nationalIdentityNumbers);

        // Assert
        IEnumerable<string>? unmatchedNationalIdentityNumbers = [];
        IEnumerable<IPersonContactDetails>? matchedPersonContactDetails = [];

        result.Match(
            success =>
            {
                matchedPersonContactDetails = success.MatchedPersonContactDetails;
                unmatchedNationalIdentityNumbers = success.UnmatchedNationalIdentityNumbers;
            },
            failure =>
            {
                matchedPersonContactDetails = null;
            });

        Assert.Equal(2, matchedPersonContactDetails.Count());

        Assert.Contains(matchedPersonContactDetails, detail => detail == firstMappedContactDetails.Object);
        var firstContactDetails = matchedPersonContactDetails.FirstOrDefault(detail => detail.NationalIdentityNumber == firstRandomPerson.FnumberAk);

        Assert.NotNull(firstContactDetails);
        Assert.Equal(firstRandomPerson.Reservation, firstContactDetails.IsReserved);
        Assert.Equal(firstRandomPerson.EmailAddress, firstContactDetails.EmailAddress);
        Assert.Equal(firstRandomPerson.LanguageCode, firstContactDetails.LanguageCode);
        Assert.Equal(firstRandomPerson.FnumberAk, firstContactDetails.NationalIdentityNumber);
        Assert.Equal(firstRandomPerson.MobilePhoneNumber, firstContactDetails.MobilePhoneNumber);

        Assert.Contains(matchedPersonContactDetails, detail => detail == secondMappedContactDetails.Object);
        var secondContactDetails = matchedPersonContactDetails.FirstOrDefault(detail => detail.NationalIdentityNumber == secondRandomPerson.FnumberAk);
        Assert.NotNull(secondContactDetails);
        Assert.Equal(secondRandomPerson.Reservation, secondContactDetails.IsReserved);
        Assert.Equal(secondRandomPerson.EmailAddress, secondContactDetails.EmailAddress);
        Assert.Equal(secondRandomPerson.LanguageCode, secondContactDetails.LanguageCode);
        Assert.Equal(secondRandomPerson.FnumberAk, secondContactDetails.NationalIdentityNumber);
        Assert.Equal(secondRandomPerson.MobilePhoneNumber, secondContactDetails.MobilePhoneNumber);

        Assert.Empty(unmatchedNationalIdentityNumbers);
    }

    [Fact]
    public async Task GetContactDetailsAsync_WhenMultipleNationalIdentityNumbersAreProvided_ReturnsCorrectResult()
    {
        // Arrange
        var validNationalIdentityNumbers = new List<string> { "12028193007", "01091235338" };
        var nationalIdentityNumbers = new List<string> { "12028193007", "01091235338", "invalid_number" };

        var firstRandomPerson = new Person
        {
            LanguageCode = "nb",
            FnumberAk = "12028193007",
            MailboxAddress = "1234 Test St",
            MobilePhoneNumber = "+4791234567",
            EmailAddress = "test@example.com",
            X509Certificate = "certificate_data"
        };

        var secondRandomPerson = new Person
        {
            LanguageCode = "nb",
            FnumberAk = "01091235338",
            MailboxAddress = "1234 Test St",
            MobilePhoneNumber = "+4791234567",
            EmailAddress = "test@example.com",
            X509Certificate = "certificate_data"
        };

        var personList = new List<Person> { firstRandomPerson, secondRandomPerson }.ToImmutableList();

        var firstMappedContactDetails = new Mock<PersonContactDetails>();
        firstMappedContactDetails.SetupGet(x => x.IsReserved).Returns(firstRandomPerson.Reservation);
        firstMappedContactDetails.SetupGet(x => x.EmailAddress).Returns(firstRandomPerson.EmailAddress);
        firstMappedContactDetails.SetupGet(x => x.LanguageCode).Returns(firstRandomPerson.LanguageCode);
        firstMappedContactDetails.SetupGet(x => x.NationalIdentityNumber).Returns(firstRandomPerson.FnumberAk);
        firstMappedContactDetails.SetupGet(x => x.MobilePhoneNumber).Returns(firstRandomPerson.MobilePhoneNumber);

        _mapperMock.Setup(x => x.Map<PersonContactDetails>(firstRandomPerson))
            .Returns(firstMappedContactDetails.Object);

        var secondMappedContactDetails = new Mock<IPersonContactDetails>();
        secondMappedContactDetails.SetupGet(x => x.IsReserved).Returns(secondRandomPerson.Reservation);
        secondMappedContactDetails.SetupGet(x => x.EmailAddress).Returns(secondRandomPerson.EmailAddress);
        secondMappedContactDetails.SetupGet(x => x.LanguageCode).Returns(secondRandomPerson.LanguageCode);
        secondMappedContactDetails.SetupGet(x => x.NationalIdentityNumber).Returns(secondRandomPerson.FnumberAk);
        secondMappedContactDetails.SetupGet(x => x.MobilePhoneNumber).Returns(secondRandomPerson.MobilePhoneNumber);

        _mapperMock.Setup(x => x.Map<IPersonContactDetails>(secondRandomPerson))
            .Returns(secondMappedContactDetails.Object);

        _personRepositoryMock
            .Setup(x => x.GetContactDetailsAsync(validNationalIdentityNumbers))
            .ReturnsAsync(personList);

        _nationalIdentityNumberCheckerMock
            .Setup(x => x.GetValid(nationalIdentityNumbers))
            .Returns(validNationalIdentityNumbers.ToImmutableList());

        // Act
        var result = await _personService.GetContactDetailsAsync(nationalIdentityNumbers);

        // Assert
        IEnumerable<string>? unmatchedNationalIdentityNumbers = [];
        IEnumerable<IPersonContactDetails>? matchedPersonContactDetails = [];

        result.Match(
            success =>
            {
                matchedPersonContactDetails = success.MatchedPersonContactDetails;
                unmatchedNationalIdentityNumbers = success.UnmatchedNationalIdentityNumbers;
            },
            failure =>
            {
                matchedPersonContactDetails = null;
            });

        Assert.Equal(2, matchedPersonContactDetails.Count());

        var firstContactDetails = matchedPersonContactDetails.FirstOrDefault(detail => detail.NationalIdentityNumber == firstRandomPerson.FnumberAk);

        Assert.NotNull(firstContactDetails);
        Assert.Equal(firstRandomPerson.Reservation, firstContactDetails.IsReserved);
        Assert.Equal(firstRandomPerson.EmailAddress, firstContactDetails.EmailAddress);
        Assert.Equal(firstRandomPerson.LanguageCode, firstContactDetails.LanguageCode);
        Assert.Equal(firstRandomPerson.FnumberAk, firstContactDetails.NationalIdentityNumber);
        Assert.Equal(firstRandomPerson.MobilePhoneNumber, firstContactDetails.MobilePhoneNumber);

        Assert.Contains(matchedPersonContactDetails, detail => detail == secondMappedContactDetails.Object);
        var secondContactDetails = matchedPersonContactDetails.FirstOrDefault(detail => detail.NationalIdentityNumber == secondRandomPerson.FnumberAk);
        Assert.NotNull(secondContactDetails);
        Assert.Equal(secondRandomPerson.Reservation, secondContactDetails.IsReserved);
        Assert.Equal(secondRandomPerson.EmailAddress, secondContactDetails.EmailAddress);
        Assert.Equal(secondRandomPerson.LanguageCode, secondContactDetails.LanguageCode);
        Assert.Equal(secondRandomPerson.FnumberAk, secondContactDetails.NationalIdentityNumber);
        Assert.Equal(secondRandomPerson.MobilePhoneNumber, secondContactDetails.MobilePhoneNumber);

        Assert.Single(unmatchedNationalIdentityNumbers);
        Assert.Contains("invalid_number", unmatchedNationalIdentityNumbers);
    }

    [Fact]
    public async Task GetContactDetailsAsync_WhenNationalIdentityNumberIsInvalid_ReturnsNull()
    {
        // Arrange
        var nationalIdentityNumber = "invalid_number";

        _nationalIdentityNumberCheckerMock
            .Setup(x => x.IsValid(nationalIdentityNumber))
            .Returns(false);

        // Act
        var result = await _personService.GetContactDetailsAsync(nationalIdentityNumber);

        // Assert
        Assert.Null(result);
        _personRepositoryMock.Verify(x => x.GetContactDetailsAsync(It.IsAny<IEnumerable<string>>()), Times.Never);
    }

    [Fact]
    public async Task GetContactDetailsAsync_WhenNationalIdentityNumberIsValid_ReturnsContactDetails()
    {
        // Arrange
        var nationalIdentityNumber = "23080188641";
        var randomPerson = new Person
        {
            LanguageCode = "nb",
            MailboxAddress = "1234 Test St",
            MobilePhoneNumber = "+4791234567",
            EmailAddress = "test@example.com",
            FnumberAk = nationalIdentityNumber,
            X509Certificate = "certificate_data"
        };
        var randomPersons = new List<Person> { randomPerson }.ToImmutableList();

        var personContactDetails = new Mock<PersonContactDetails>();
        personContactDetails.SetupGet(x => x.IsReserved).Returns(randomPerson.Reservation);
        personContactDetails.SetupGet(x => x.EmailAddress).Returns(randomPerson.EmailAddress);
        personContactDetails.SetupGet(x => x.LanguageCode).Returns(randomPerson.LanguageCode);
        personContactDetails.SetupGet(x => x.NationalIdentityNumber).Returns(nationalIdentityNumber);
        personContactDetails.SetupGet(x => x.MobilePhoneNumber).Returns(randomPerson.MobilePhoneNumber);

        _mapperMock
            .Setup(x => x.Map<PersonContactDetails>(randomPerson))
            .Returns(personContactDetails.Object);

        _personRepositoryMock
            .Setup(x => x.GetContactDetailsAsync(It.Is<IEnumerable<string>>(e => e.Contains(nationalIdentityNumber))))
            .ReturnsAsync(randomPersons);

        _nationalIdentityNumberCheckerMock
            .Setup(x => x.IsValid(nationalIdentityNumber))
            .Returns(true);

        // Act
        var result = await _personService.GetContactDetailsAsync(nationalIdentityNumber);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(personContactDetails.Object.IsReserved, result.IsReserved);
        Assert.Equal(personContactDetails.Object.EmailAddress, result.EmailAddress);
        Assert.Equal(personContactDetails.Object.LanguageCode, result.LanguageCode);
        Assert.Equal(personContactDetails.Object.MobilePhoneNumber, result.MobilePhoneNumber);
        Assert.Equal(personContactDetails.Object.NationalIdentityNumber, result.NationalIdentityNumber);
    }

    [Fact]
    public async Task GetContactDetailsAsync_WhenNationalIdentityNumberIsValidAndNoContactFound_ReturnsNull()
    {
        // Arrange
        var nationalIdentityNumber = "23080188641";

        _nationalIdentityNumberCheckerMock
            .Setup(x => x.IsValid(nationalIdentityNumber))
            .Returns(true);

        _personRepositoryMock
            .Setup(x => x.GetContactDetailsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync([]);

        // Act
        var result = await _personService.GetContactDetailsAsync(nationalIdentityNumber);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetContactDetailsAsync_WhenNoValidNumbersProvided_ReturnsEmptyResult()
    {
        // Arrange
        var nationalIdentityNumbers = new List<string> { "invalid1", "invalid2" };

        _nationalIdentityNumberCheckerMock
            .Setup(x => x.GetValid(nationalIdentityNumbers))
            .Returns(nationalIdentityNumbers.ToImmutableList());

        // Act
        var result = await _personService.GetContactDetailsAsync(nationalIdentityNumbers);

        // Assert
        IEnumerable<string>? unmatchedNationalIdentityNumbers = [];
        IEnumerable<IPersonContactDetails>? matchedPersonContactDetails = [];

        result.Match(
            success =>
            {
                matchedPersonContactDetails = success.MatchedPersonContactDetails;
                unmatchedNationalIdentityNumbers = success.UnmatchedNationalIdentityNumbers;
            },
            failure =>
            {
                matchedPersonContactDetails = null;
            });

        Assert.NotNull(matchedPersonContactDetails);
        Assert.Empty(matchedPersonContactDetails);

        Assert.NotNull(unmatchedNationalIdentityNumbers);
        Assert.Equal(2, unmatchedNationalIdentityNumbers.Count());
    }

    [Fact]
    public async Task GetContactDetailsAsync_WhenRepositoryThrowsException_HandlesGracefully()
    {
        // Arrange
        var nationalIdentityNumber = "26050711071";
        _personRepositoryMock
            .Setup(repo => repo.GetByIdAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Repository failure"));

        // Act
        var result = await _personService.GetContactDetailsAsync(nationalIdentityNumber);

        // Assert
        Assert.Null(result);
    }
}
