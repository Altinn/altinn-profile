using Altinn.Profile.Core.OrganizationNotificationAddresses;
using Altinn.Profile.Integrations.ContactRegister;
using Altinn.Profile.Integrations.Entities;

namespace Altinn.Profile.Integrations.Mappings;

/// <summary>
/// AutoMapper profile for mapping between db model <see cref="Organization"/> and core model <see cref="Organization"/>.
/// </summary>
public class OrganizationMappingProfile : AutoMapper.Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OrganizationMappingProfile"/> class and configures the mappings.
    /// </summary>
    public OrganizationMappingProfile()
    {
        CreateMap<Entities.Organization, Core.OrganizationNotificationAddresses.Organization>();

        CreateMap<OrganizationNotificationAddress, NotificationAddress>();
    }
}
