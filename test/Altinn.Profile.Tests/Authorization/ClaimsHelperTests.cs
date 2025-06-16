using System.Collections.Generic;
using System.Security.Claims;
using Altinn.Profile.Authorization;
using AltinnCore.Authentication.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Altinn.Profile.Tests.Authorization
{
    public class ClaimsHelperTests
    {
        [Fact]
        public void TryGetUserIdFromClaims_ValidUserId_ReturnsNullAndSetsUserId()
        {
            // Arrange
            var userId = 12345;
            var claims = new List<Claim>
            {
                new Claim(AltinnCoreClaimTypes.UserId, userId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);
            var context = new DefaultHttpContext { User = principal };

            // Act
            var result = ClaimsHelper.TryGetUserIdFromClaims(context, out int actualUserId);

            // Assert
            Assert.Null(result);
            Assert.Equal(userId, actualUserId);
        }

        [Fact]
        public void TryGetUserIdFromClaims_MissingUserIdClaim_ReturnsBadRequest()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim("someOtherClaim", "value")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);
            var context = new DefaultHttpContext { User = principal };

            // Act
            var result = ClaimsHelper.TryGetUserIdFromClaims(context, out int actualUserId);

            // Assert
            Assert.NotNull(result);
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid request context. UserId must be provided in claims.", badRequest.Value);
            Assert.Equal(0, actualUserId);
        }

        [Fact]
        public void TryGetUserIdFromClaims_InvalidUserIdFormat_ReturnsBadRequest()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim(AltinnCoreClaimTypes.UserId, "notAnInt")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);
            var context = new DefaultHttpContext { User = principal };

            // Act
            var result = ClaimsHelper.TryGetUserIdFromClaims(context, out int actualUserId);

            // Assert
            Assert.NotNull(result);
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid user ID format in claims.", badRequest.Value);
            Assert.Equal(0, actualUserId);
        }
    }
}
