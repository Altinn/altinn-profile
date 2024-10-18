#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using Altinn.Profile.Core.ContactRegister;
using Altinn.Profile.Core.Person.ContactPreferences;
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
    private readonly PersonService _personService;
    private readonly Mock<IPersonRepository> _personRepositoryMock;
    private readonly Mock<IMetadataRepository> _metadataRepositoryMock;
    private readonly Mock<IContactRegisterService> _contactRegisterServiceMock;
    private readonly Mock<INationalIdentityNumberChecker> _nationalIdentityNumberCheckerMock;

    public PersonServiceTests()
    {
        _mapperMock = new Mock<IMapper>();
        _personRepositoryMock = new Mock<IPersonRepository>();
        _metadataRepositoryMock = new Mock<IMetadataRepository>();
        _contactRegisterServiceMock = new Mock<IContactRegisterService>();
        _nationalIdentityNumberCheckerMock = new Mock<INationalIdentityNumberChecker>();
        _personService = new PersonService(_mapperMock.Object, _personRepositoryMock.Object, _contactRegisterServiceMock.Object, _metadataRepositoryMock.Object, _nationalIdentityNumberCheckerMock.Object);
    }

    /// <summary>
    /// Tests that <see cref="PersonService.GetContactDetailsAsync"/> returns all contacts when all numbers are valid and all contacts are found.
    /// </summary>
    [Fact]
    public async Task GetContactDetailsAsync_WhenAllNumbersValidAndAllContactsFound_ReturnsAllContacts()
    {
        // Arrange
        var nationalIdentityNumbers = new List<string> { "17092037169", "17033112912" };

        var firstRandomPerson = CreatePerson(
            fnumberAk: "17092037169",
            reservation: false,
            emailAddress: "test@example.com",
            languageCode: "nb",
            mobilePhoneNumber: "+4791234567");

        var secondRandomPerson = CreatePerson(
            fnumberAk: "17033112912",
            reservation: true,
            emailAddress: "test@example.com",
            languageCode: "nb",
            mobilePhoneNumber: "+4791234567");

        var personList = new List<Person> { firstRandomPerson, secondRandomPerson }.ToImmutableList();

        var firstMappedContactDetails = CreatePersonContactPreferences(firstRandomPerson);
        var secondMappedContactDetails = CreatePersonContactPreferences(secondRandomPerson);

        _mapperMock.Setup(x => x.Map<PersonContactPreferences>(firstRandomPerson))
                   .Returns(firstMappedContactDetails);
        _mapperMock.Setup(x => x.Map<PersonContactPreferences>(secondRandomPerson))
                   .Returns(secondMappedContactDetails);

        _personRepositoryMock
            .Setup(x => x.GetContactDetailsAsync(nationalIdentityNumbers))
            .ReturnsAsync(personList);

        _nationalIdentityNumberCheckerMock
            .Setup(x => x.GetValid(nationalIdentityNumbers))
            .Returns(nationalIdentityNumbers.ToImmutableList());

        // Act
        var result = await _personService.GetContactPreferencesAsync(nationalIdentityNumbers);

        // Assert
        IEnumerable<string>? unmatchedNationalIdentityNumbers = null;
        IEnumerable<IPersonContactPreferences>? matchedPersonContactDetails = null;

        result.Match(
            success =>
            {
                matchedPersonContactDetails = success.MatchedPersonContactPreferences;
                unmatchedNationalIdentityNumbers = success.UnmatchedNationalIdentityNumbers;
            },
            failure =>
            {
                matchedPersonContactDetails = null;
            });

        Assert.NotNull(matchedPersonContactDetails);
        Assert.Equal(2, matchedPersonContactDetails.Count());

        // Validate first contact details
        AssertContactDetails(firstMappedContactDetails, matchedPersonContactDetails);

        // Validate second contact details
        AssertContactDetails(secondMappedContactDetails, matchedPersonContactDetails);

        Assert.Null(unmatchedNationalIdentityNumbers);
    }

    /// <summary>
    /// Tests that <see cref="PersonService.GetContactDetailsAsync"/> returns the correct result when multiple national identity numbers are provided.
    /// </summary>
    [Fact]
    public async Task GetContactDetailsAsync_WhenMultipleNationalIdentityNumbersAreProvided_ReturnsCorrectResult()
    {
        // Arrange
        var validNationalIdentityNumbers = new List<string> { "12028193007", "01091235338" };
        var nationalIdentityNumbers = new List<string> { "12028193007", "01091235338", "invalid_number" };

        var firstRandomPerson = CreatePerson(
            fnumberAk: "12028193007",
            reservation: false,
            emailAddress: "test@example.com",
            languageCode: "nb",
            mobilePhoneNumber: "+4791234567");

        var secondRandomPerson = CreatePerson(
            fnumberAk: "01091235338",
            reservation: true,
            emailAddress: "test@example.com",
            languageCode: "nb",
            mobilePhoneNumber: "+4791234567");

        var personList = new List<Person> { firstRandomPerson, secondRandomPerson }.ToImmutableList();

        var firstMappedContactDetails = CreatePersonContactPreferences(firstRandomPerson);
        var secondMappedContactDetails = CreatePersonContactPreferences(secondRandomPerson);

        _mapperMock.Setup(x => x.Map<PersonContactPreferences>(firstRandomPerson))
                   .Returns(firstMappedContactDetails);
        _mapperMock.Setup(x => x.Map<IPersonContactPreferences>(secondRandomPerson))
                   .Returns(secondMappedContactDetails);

        _personRepositoryMock
            .Setup(x => x.GetContactDetailsAsync(validNationalIdentityNumbers))
            .ReturnsAsync(personList);

        _nationalIdentityNumberCheckerMock
            .Setup(x => x.GetValid(nationalIdentityNumbers))
            .Returns(validNationalIdentityNumbers.ToImmutableList());

        // Act
        var result = await _personService.GetContactPreferencesAsync(nationalIdentityNumbers);

        // Assert
        IEnumerable<string>? unmatchedNationalIdentityNumbers = null;
        IEnumerable<IPersonContactPreferences>? matchedPersonContactDetails = null;

        result.Match(
            success =>
            {
                matchedPersonContactDetails = success.MatchedPersonContactPreferences;
                unmatchedNationalIdentityNumbers = success.UnmatchedNationalIdentityNumbers;
            },
            failure =>
            {
                matchedPersonContactDetails = null;
            });

        Assert.NotNull(matchedPersonContactDetails);
        Assert.Equal(2, matchedPersonContactDetails.Count());

        // Validate first contact details
        AssertContactDetails(firstMappedContactDetails, matchedPersonContactDetails);

        // Validate second contact details
        AssertContactDetails(secondMappedContactDetails, matchedPersonContactDetails);

        Assert.NotNull(unmatchedNationalIdentityNumbers);
        Assert.Single(unmatchedNationalIdentityNumbers);
        Assert.Contains("invalid_number", unmatchedNationalIdentityNumbers);
    }

    /// <summary>
    /// Tests that <see cref="PersonService.GetContactDetailsAsync"/> returns null when the national identity number is invalid.
    /// </summary>
    [Fact]
    public async Task GetContactDetailsAsync_WhenNationalIdentityNumberIsInvalid_ReturnsNull()
    {
        // Arrange
        var nationalIdentityNumber = "invalid_number";
        var nationalIdentityNumbers = new List<string> { nationalIdentityNumber };

        _nationalIdentityNumberCheckerMock
            .Setup(x => x.IsValid(nationalIdentityNumber))
            .Returns(false);

        // Act
        var result = await _personService.GetContactPreferencesAsync(nationalIdentityNumbers);

        // Assert
        IEnumerable<string>? unmatchedNationalIdentityNumbers = null;
        IEnumerable<IPersonContactPreferences>? matchedPersonContactDetails = null;
        result.Match(
          success =>
          {
              matchedPersonContactDetails = success.MatchedPersonContactPreferences;
              unmatchedNationalIdentityNumbers = success.UnmatchedNationalIdentityNumbers;
          },
          failure =>
          {
              matchedPersonContactDetails = null;
          });

        Assert.Null(matchedPersonContactDetails);
        Assert.Null(unmatchedNationalIdentityNumbers);
        _personRepositoryMock.Verify(x => x.GetContactDetailsAsync(It.IsAny<IEnumerable<string>>()), Times.Never);
    }

    /// <summary>
    /// Tests that <see cref="PersonService.GetContactDetailsAsync"/> returns the contact details when the national identity number is valid.
    /// </summary>
    [Fact]
    public async Task GetContactDetailsAsync_WhenNationalIdentityNumberIsValid_ReturnsContactDetails()
    {
        // Arrange
        var nationalIdentityNumbers = new List<string> { "12028193007" };

        var randomPerson = CreatePerson(
            fnumberAk: "12028193007",
            reservation: false,
            emailAddress: "test@example.com",
            languageCode: "nb",
            mobilePhoneNumber: "+4791234567");

        var personList = new List<Person> { randomPerson }.ToImmutableList();

        var mappedContactDetails = CreatePersonContactPreferences(randomPerson);

        _mapperMock.Setup(x => x.Map<PersonContactPreferences>(randomPerson))
                   .Returns(mappedContactDetails);

        _personRepositoryMock
            .Setup(x => x.GetContactDetailsAsync(nationalIdentityNumbers))
            .ReturnsAsync(personList);

        _nationalIdentityNumberCheckerMock
            .Setup(x => x.GetValid(nationalIdentityNumbers))
            .Returns(nationalIdentityNumbers.ToImmutableList());

        // Act
        var result = await _personService.GetContactPreferencesAsync(nationalIdentityNumbers);

        // Assert
        IEnumerable<string>? unmatchedNationalIdentityNumbers = null;
        IEnumerable<IPersonContactPreferences>? matchedPersonContactDetails = null;

        result.Match(
            success =>
            {
                matchedPersonContactDetails = success.MatchedPersonContactPreferences;
                unmatchedNationalIdentityNumbers = success.UnmatchedNationalIdentityNumbers;
            },
            failure =>
            {
                matchedPersonContactDetails = null;
            });

        Assert.NotNull(matchedPersonContactDetails);
        Assert.Single(matchedPersonContactDetails);

        // Validate contact details
        AssertContactDetails(mappedContactDetails, matchedPersonContactDetails);

        Assert.Null(unmatchedNationalIdentityNumbers);
    }

    /// <summary>
    /// Asserts that the contact details match the expected values.
    /// </summary>
    /// <param name="expected">The expected contact preferences.</param>
    /// <param name="actualContactDetails">The actual contact details.</param>
    private static void AssertContactDetails(PersonContactPreferences expected, IEnumerable<IPersonContactPreferences> actualContactDetails)
    {
        var contactDetails = actualContactDetails.FirstOrDefault(detail => detail.NationalIdentityNumber == expected.NationalIdentityNumber);

        Assert.NotNull(contactDetails);
        Assert.Equal(expected.Email, contactDetails.Email);
        Assert.Equal(expected.IsReserved, contactDetails.IsReserved);
        Assert.Equal(expected.MobileNumber, contactDetails.MobileNumber);
        Assert.Equal(expected.LanguageCode, contactDetails.LanguageCode);
        Assert.Equal(expected.NationalIdentityNumber, contactDetails.NationalIdentityNumber);
    }

    /// <summary>
    /// Creates a new instance of <see cref="Person"/>.
    /// </summary>
    /// <param name="fnumberAk">The national identity number.</param>
    /// <param name="reservation">The reservation status.</param>
    /// <param name="emailAddress">The email address.</param>
    /// <param name="languageCode">The language code.</param>
    /// <param name="mobilePhoneNumber">The mobile phone number.</param>
    /// <returns>A new instance of <see cref="Person"/>.</returns>
    private static Person CreatePerson(string fnumberAk, bool reservation, string emailAddress, string languageCode, string mobilePhoneNumber)
    {
        return new Person
        {
            FnumberAk = fnumberAk,
            Reservation = reservation,
            EmailAddress = emailAddress,
            LanguageCode = languageCode,
            MobilePhoneNumber = mobilePhoneNumber,
            MailboxAddress = "1234 Test St",
            X509Certificate = "certificate_data"
        };
    }

    /// <summary>
    /// Creates a new instance of <see cref="PersonContactPreferences"/>.
    /// </summary>
    /// <param name="person">The person entity.</param>
    /// <returns>A new instance of <see cref="PersonContactPreferences"/>.</returns>
    private static PersonContactPreferences CreatePersonContactPreferences(Person person)
    {
        return new PersonContactPreferences
        {
            Email = person.EmailAddress,
            IsReserved = person.Reservation,
            LanguageCode = person.LanguageCode,
            MobileNumber = person.MobilePhoneNumber,
            NationalIdentityNumber = person.FnumberAk
        };
    }
}
