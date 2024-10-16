using Altinn.Profile.Integrations.Entities;
using Altinn.Profile.Integrations.Mappings;

using AutoMapper;

using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations;

public class PersonContactPreferencesProfileTests
{
    private readonly IMapper _mapper;

    public PersonContactPreferencesProfileTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<PersonContactPreferencesProfile>());
        _mapper = config.CreateMapper();
    }

    [Fact]
    public void Map_DifferentValues_CreatesCorrectMappings()
    {
        // Arrange
        var person = new Person
        {
            Reservation = true,
            LanguageCode = "no",
            FnumberAk = "24021633239",
            MobilePhoneNumber = "9876543210",
            EmailAddress = "test@example.com",
        };

        // Act
        var result = _mapper.Map<PersonContactPreferences>(person);

        // Assert
        Assert.True(result.IsReserved);
        Assert.Equal(person.EmailAddress, result.Email);
        Assert.Equal(person.LanguageCode, result.LanguageCode);
        Assert.Equal(person.FnumberAk, result.NationalIdentityNumber);
        Assert.Equal(person.MobilePhoneNumber, result.MobileNumber);
    }

    [Fact]
    public void Map_NullPerson_ReturnsNullPersonContactDetails()
    {
        // Arrange
        Person person = null;

        // Act
        var result = _mapper.Map<PersonContactPreferences>(person);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Map_OptionalProperties_WhenMissing_ReturnsDefaults()
    {
        // Arrange
        var person = new Person
        {
            Reservation = false,
            FnumberAk = "06082705358"
        };

        // Act
        var result = _mapper.Map<PersonContactPreferences>(person);

        // Assert
        Assert.Null(result.LanguageCode);
        Assert.Null(result.Email);
        Assert.Null(result.MobileNumber);
        Assert.Equal("06082705358", result.NationalIdentityNumber);
    }

    [Fact]
    public void Map_ReservationFalse_SetsIsReservedToFalse()
    {
        // Arrange
        var person = new Person { Reservation = false };

        // Act
        var result = _mapper.Map<PersonContactPreferences>(person);

        // Assert
        Assert.False(result.IsReserved);
    }

    [Fact]
    public void Map_ReservationTrue_SetsIsReservedToTrue()
    {
        // Arrange
        var person = new Person { Reservation = true };

        // Act
        var result = _mapper.Map<PersonContactPreferences>(person);

        // Assert
        Assert.True(result.IsReserved);
    }

    [Fact]
    public void Map_ValidPerson_ReturnsCorrectPersonContactDetails()
    {
        // Arrange
        var person = new Person
        {
            Reservation = false,
            LanguageCode = "en",
            FnumberAk = "17080227000",
            MobilePhoneNumber = "1234567890",
            EmailAddress = "test@example.com"
        };

        // Act
        var result = _mapper.Map<PersonContactPreferences>(person);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsReserved);
        Assert.Equal(person.EmailAddress, result.Email);
        Assert.Equal(person.LanguageCode, result.LanguageCode);
        Assert.Equal(person.FnumberAk, result.NationalIdentityNumber);
        Assert.Equal(person.MobilePhoneNumber, result.MobileNumber);
    }
}
