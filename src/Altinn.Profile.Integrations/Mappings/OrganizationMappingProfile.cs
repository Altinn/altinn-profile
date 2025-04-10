﻿using Altinn.Profile.Core.OrganizationNotificationAddresses;
using Altinn.Profile.Integrations.ContactRegister;
using Altinn.Profile.Integrations.Entities;

namespace Altinn.Profile.Integrations.Mappings;

/// <summary>
/// AutoMapper profile for mapping between db model <see cref="OrganizationDE"/> and core model <see cref="Organization"/>.
/// </summary>
public class OrganizationMappingProfile : AutoMapper.Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OrganizationMappingProfile"/> class and configures the mappings.
    /// </summary>
    public OrganizationMappingProfile()
    {
        CreateMap<OrganizationDE, Organization>()
            .ForMember(dest => dest.OrganizationNumber, opt => opt.MapFrom(src => src.RegistryOrganizationNumber));

        CreateMap<NotificationAddressDE, NotificationAddress>();
    }
}
