using Altinn.Profile.Core.OrganizationNotificationAddresses;
using Altinn.Profile.Integrations.Entities;

namespace Altinn.Profile.Integrations.Mappings;

/// <summary>
/// Provides mapping methods for converting data entities to core models related to organizations and their notification addresses.
/// </summary>
public static class OrganizationMapper
{
    /// <summary>
    /// Maps an <see cref="OrganizationDE"/> data entity to an <see cref="Organization"/> core model.
    /// </summary>
    /// <param name="dataEntity">The <see cref="OrganizationDE"/> instance to map from.</param>
    /// <returns>A mapped <see cref="Organization"/> instance.</returns>
    public static Organization? MapFromDataEntity(OrganizationDE? dataEntity)
    {
        if (dataEntity == null)
        {
            return null;
        }

        return new Organization
        {
            OrganizationNumber = dataEntity.RegistryOrganizationNumber,
            NotificationAddresses = [.. dataEntity.NotificationAddresses.Select(MapFromDataEntity)]
        };
    }

    /// <summary>
    /// Maps a <see cref="NotificationAddressDE"/> data entity to a <see cref="NotificationAddress"/> core model.
    /// </summary>
    /// <param name="dataEntity">The <see cref="NotificationAddressDE"/> instance to map from.</param>
    /// <returns>A mapped <see cref="NotificationAddress"/> instance.</returns>
    public static NotificationAddress MapFromDataEntity(NotificationAddressDE dataEntity)
    {
        return new NotificationAddress
        {
            NotificationAddressID = dataEntity.NotificationAddressID,
            AddressType = dataEntity.AddressType,
            Domain = dataEntity.Domain,
            Address = dataEntity.Address,
            FullAddress = dataEntity.FullAddress,
            NotificationName = dataEntity.NotificationName,
            RegistryID = dataEntity.RegistryID,
            RegistryUpdatedDateTime = dataEntity.RegistryUpdatedDateTime,
            IsSoftDeleted = dataEntity.IsSoftDeleted,
            HasRegistryAccepted = dataEntity.HasRegistryAccepted,
            IsSynthetic = dataEntity.UpdateSource == UpdateSource.Synthetic,
        };
    }
}
