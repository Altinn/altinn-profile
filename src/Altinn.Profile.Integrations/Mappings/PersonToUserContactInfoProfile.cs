using Altinn.Profile.Integrations.Entities;

namespace Altinn.Profile.Integrations.Mappings;

/// <summary>
/// AutoMapper profile for mapping between <see cref="Register"/> and <see cref="UserContactInfo"/>.
/// </summary>
/// <remarks>
/// This profile defines the mapping rules to convert a <see cref="Register"/> object into a <see cref="UserContactInfo"/> instance.
/// </remarks>
public class PersonToUserContactInfoProfile : AutoMapper.Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PersonToUserContactInfoProfile"/> class
    /// and configures the mappings.
    /// </summary>
    public PersonToUserContactInfoProfile()
    {
        CreateMap<Person, UserContactInfo>()
            .ForMember(dest => dest.IsReserved, opt => opt.MapFrom(src => src.Reservation))
            .ForMember(dest => dest.EmailAddress, opt => opt.MapFrom(src => src.EmailAddress))
            .ForMember(dest => dest.LanguageCode, opt => opt.MapFrom(src => src.LanguageCode))
            .ForMember(dest => dest.NationalIdentityNumber, opt => opt.MapFrom(src => src.FnumberAk))
            .ForMember(dest => dest.MobilePhoneNumber, opt => opt.MapFrom(src => src.MobilePhoneNumber));
    }
}
