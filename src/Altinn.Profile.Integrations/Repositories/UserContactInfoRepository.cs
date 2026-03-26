using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Core.User.ContactInfo;
using Altinn.Profile.Integrations.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Altinn.Profile.Integrations.Repositories;

/// <inheritdoc/>
/// <summary>
/// Initializes a new instance of the <see cref="UserContactInfoRepository"/> class.
/// </summary>
/// <param name="contextFactory">A factory for creating instances of <see cref="ProfileDbContext"/></param>
public class UserContactInfoRepository(IDbContextFactory<ProfileDbContext> contextFactory) : IUserContactInfoRepository
{
    private readonly IDbContextFactory<ProfileDbContext> _contextFactory = contextFactory;

    /// <inheritdoc/>
    public Task<UserContactInfo?> UpdateMobileNumber(int userId, string phoneNumber, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
