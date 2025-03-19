using Altinn.Profile.Core.OrganizationNotificationAddresses;

namespace Altinn.Profile.Core.Integrations;

/// <summary>
/// Defines a repository for accessing organizationNotificationAddresses.
/// </summary>
public interface IOrganizationNotificationAddressRepository
{
    /// <summary>
    /// Getting the organizations notification addresses from the database
    /// </summary>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<Result<IEnumerable<Organization>, bool>> GetOrganizationsAsync(OrgContactPointLookup orgNumberLookup, CancellationToken cancellationToken);
}
