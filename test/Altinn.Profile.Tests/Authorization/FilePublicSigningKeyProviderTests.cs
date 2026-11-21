using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Altinn.Profile.Authorization;
using Altinn.Profile.Configuration;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using Xunit;

namespace Altinn.Profile.Tests.Authorization
{
    public class FilePublicSigningKeyProviderTests
    {
        [Fact]
        public async Task GetSigningKeys_CertificateExistsForIssuer_ReturnsKeyFromCertificate()
        {
            // Arrange
            var provider = CreateProvider(AppContext.BaseDirectory);

            // Act
            var keys = (await provider.GetSigningKeys("ttd")).ToList();

            // Assert
            var key = Assert.Single(keys);
            Assert.IsType<X509SecurityKey>(key);
        }

        [Fact]
        public async Task GetSigningKeys_NoCertificateForIssuer_ReturnsNoKeys()
        {
            // Arrange
            var provider = CreateProvider(AppContext.BaseDirectory);

            // Act
            var keys = await provider.GetSigningKeys("unknown-issuer");

            // Assert
            Assert.Empty(keys);
        }

        [Fact]
        public async Task GetSigningKeys_IssuerContainsPath_DoesNotReadOutsideFolder()
        {
            // Arrange
            string folder = Path.Combine(AppContext.BaseDirectory, "Authorization");
            var provider = CreateProvider(folder);

            // Act
            var keys = await provider.GetSigningKeys("../ttd");

            // Assert
            Assert.Empty(keys);
        }

        [Fact]
        public async Task GetSigningKeys_FolderNotConfigured_ReturnsNoKeys()
        {
            // Arrange
            var provider = CreateProvider(null);

            // Act
            var keys = await provider.GetSigningKeys("unknown-issuer");

            // Assert
            Assert.Empty(keys);
        }

        private static FilePublicSigningKeyProvider CreateProvider(string folder)
        {
            return new FilePublicSigningKeyProvider(Options.Create(new GeneralSettings { AccessTokenPublicCertsFolder = folder }));
        }
    }
}
