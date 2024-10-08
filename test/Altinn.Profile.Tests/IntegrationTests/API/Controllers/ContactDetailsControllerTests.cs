using System.Threading.Tasks;

using Altinn.Profile.Controllers;
using Altinn.Profile.Models;
using Altinn.Profile.UseCases;

using Microsoft.AspNetCore.Mvc;

using Moq;

using Xunit;

namespace Altinn.Profile.Tests.IntegrationTests.API.Controllers;

public class ContactDetailsControllerTests
{
    private readonly Mock<IContactDetailsRetriever> _mockContactDetailsRetriever;
    private readonly ContactDetailsController _controller;

    public ContactDetailsControllerTests()
    {
        _mockContactDetailsRetriever = new Mock<IContactDetailsRetriever>();
        _controller = new ContactDetailsController(_mockContactDetailsRetriever.Object);
    }

    [Fact]
    public async Task PostLookup_ReturnsOkResult_WhenSuccessful()
    {
        // Arrange
        var request = new UserContactPointLookup
        {
            NationalIdentityNumbers = ["27038893837"]
        };

        var contactDetails = new ContactDetails
        {
            LanguageCode = "nb",
            Reservation = false,
            MobilePhoneNumber = "12345678",
            EmailAddress = "test@example.com",
            NationalIdentityNumber = "27038893837",
        };

        var result = new ContactDetailsLookupResult(matchedContactDetails: [contactDetails], unmatchedNationalIdentityNumbers: null);

        _mockContactDetailsRetriever.Setup(x => x.RetrieveAsync(request)).ReturnsAsync(result);

        // Act
        var response = await _controller.PostLookup(request);

        // Assert
        var resultOk = Assert.IsType<OkObjectResult>(response.Result);
        var returnValue = Assert.IsType<ContactDetailsLookupResult>(resultOk.Value);
        Assert.Equal(result, returnValue);
    }
}
