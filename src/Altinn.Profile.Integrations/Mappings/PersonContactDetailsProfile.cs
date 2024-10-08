using Altinn.Profile.Integrations.Entities;

namespace Altinn.Profile.Integrations.Mappings;

/// <summary>
/// AutoMapper profile for mapping between <see cref="Person"/> and <see cref="PersonContactDetails"/>.
/// </summary>
/// <remarks>
/// This profile defines the mapping rules to convert a <see cref="Person"/> object into a <see cref="PersonContactDetails"/> instance.
/// </remarks>
public class PersonContactDetailsProfile : AutoMapper.Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PersonContactDetailsProfile"/> class and configures the mappings.
    /// </summary>
    public PersonContactDetailsProfile()
    {
        CreateMap<Person, PersonContactDetails>()
            .ForMember(dest => dest.IsReserved, opt => opt.MapFrom(src => src.Reservation))
            .ForMember(dest => dest.EmailAddress, opt => opt.MapFrom(src => src.EmailAddress))
            .ForMember(dest => dest.LanguageCode, opt => opt.MapFrom(src => src.LanguageCode))
            .ForMember(dest => dest.NationalIdentityNumber, opt => opt.MapFrom(src => src.FnumberAk))
            .ForMember(dest => dest.MobilePhoneNumber, opt => opt.MapFrom(src => src.MobilePhoneNumber));
    }
}
