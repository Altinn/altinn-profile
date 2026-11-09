using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

using Altinn.Common.AccessToken.Services;
using Altinn.Profile.Configuration;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Altinn.Profile.Authorization;

/// <summary>
/// Reads the public certificates used to validate platform access tokens from disk instead of Key Vault.
/// Only used when Key Vault is not configured, e.g. when running locally with docker compose.
/// </summary>
/// <remarks>
/// The certificate for an issuer is read from <c>{folder}/{issuer}-org.pem</c>. An issuer without a certificate
/// gets no signing keys, so tokens from that issuer fail validation.
/// </remarks>
public class FilePublicSigningKeyProvider : IPublicSigningKeyProvider
{
    private readonly string _folder;

    /// <summary>
    /// Initializes a new instance of the <see cref="FilePublicSigningKeyProvider"/> class.
    /// </summary>
    /// <param name="generalSettings">The general settings, holding the certificate folder.</param>
    public FilePublicSigningKeyProvider(IOptions<GeneralSettings> generalSettings)
    {
        _folder = generalSettings.Value.AccessTokenPublicCertsFolder ?? string.Empty;
    }

    /// <inheritdoc/>
    public Task<IEnumerable<SecurityKey>> GetSigningKeys(string issuer)
    {
        List<SecurityKey> signingKeys = [];

        string certPath = Path.Combine(_folder, $"{Path.GetFileName(issuer)}-org.pem");
        if (File.Exists(certPath))
        {
            X509Certificate2 cert = X509CertificateLoader.LoadCertificateFromFile(certPath);
            signingKeys.Add(new X509SecurityKey(cert));
        }

        return Task.FromResult<IEnumerable<SecurityKey>>(signingKeys);
    }
}
