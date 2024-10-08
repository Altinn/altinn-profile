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
    public async Task GetContactDetailsAsync_InvalidNationalIdentityNumber_ReturnsNull()
    {
        var invalidNIN = "invalid_number";

        _nationalIdentityNumberCheckerMock
            .Setup(x => x.IsValid(invalidNIN))
            .Returns(false);

        var result = await _personService.GetContactDetailsAsync(invalidNIN);

        Assert.Null(result);
        _personRepositoryMock.Verify(x => x.GetContactDetailsAsync(It.IsAny<IEnumerable<string>>()), Times.Never);
    }

    [Fact]
    public async Task GetContactDetailsAsync_ValidNationalIdentityNumber_ReturnsContactDetails()
    {
        var validNIN = "12345678901";
        var mockPerson = new Person { FnumberAk = validNIN };
        var personList = new List<Person> { mockPerson }.ToImmutableList();
        var mappedContactDetail = new Mock<IPersonContactDetails>();

        _nationalIdentityNumberCheckerMock
            .Setup(x => x.IsValid(validNIN))
            .Returns(true);
        _personRepositoryMock
            .Setup(x => x.GetContactDetailsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(personList);

        _mapperMock
            .Setup(x => x.Map<IPersonContactDetails>(It.IsAny<Person>()))
            .Returns(mappedContactDetail.Object);

        var result = await _personService.GetContactDetailsAsync(validNIN);

        Assert.NotNull(result);
        Assert.Equal(mappedContactDetail.Object, result);
    }

    [Fact]
    public async Task GetContactDetailsAsync_MultipleNationalIdentityNumbers_ReturnsCorrectResult()
    {
        var nationalIdentityNumbers = new List<string> { "12028193007", "01091235338", "invalid_number" };
        var validNationalIdentityNumbers = new List<string> { "12028193007", "01091235338" };

        var mockPerson1 = new Person { FnumberAk = "12028193007" };
        var mockPerson2 = new Person { FnumberAk = "01091235338" };

        var personList = new List<Person> { mockPerson1, mockPerson2 }.ToImmutableList();

        var mappedContactDetail1 = new Mock<IPersonContactDetails>();
        var mappedContactDetail2 = new Mock<IPersonContactDetails>();

        _nationalIdentityNumberCheckerMock
            .Setup(x => x.GetValid(nationalIdentityNumbers))
            .Returns(validNationalIdentityNumbers.ToImmutableList());
        _personRepositoryMock
            .Setup(x => x.GetContactDetailsAsync(validNationalIdentityNumbers))
            .ReturnsAsync(personList);

        _mapperMock.Setup(x => x.Map<IPersonContactDetails>(mockPerson1))
            .Returns(mappedContactDetail1.Object);
        _mapperMock.Setup(x => x.Map<IPersonContactDetails>(mockPerson2))
            .Returns(mappedContactDetail2.Object);

        var result = await _personService.GetContactDetailsAsync(nationalIdentityNumbers);
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

        Assert.Contains(matchedPersonContactDetails, detail => detail == mappedContactDetail1.Object);
        Assert.Contains(matchedPersonContactDetails, detail => detail == mappedContactDetail2.Object);
        Assert.Empty(unmatchedNationalIdentityNumbers);
    }
}
