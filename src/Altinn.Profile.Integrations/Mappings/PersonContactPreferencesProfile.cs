using Altinn.Profile.Integrations.Entities;

namespace Altinn.Profile.Integrations.Mappings;

/// <summary>
/// AutoMapper profile for mapping between <see cref="Person"/> and <see cref="PersonContactPreferences"/>.
/// </summary>
/// <remarks>
/// This profile defines the mapping rules to convert a <see cref="Person"/> object into a <see cref="PersonContactPreferences"/> instance.
/// </remarks>
public class PersonContactPreferencesProfile : AutoMapper.Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PersonContactPreferencesProfile"/> class and configures the mappings.
    /// </summary>
    public PersonContactPreferencesProfile()
    {
        CreateMap<Person, PersonContactPreferences>()
            .ForMember(dest => dest.EmailAddress, opt => opt.MapFrom(src => src.EmailAddress))
            .ForMember(dest => dest.IsReserved, opt => opt.MapFrom(src => src.Reservation))
            .ForMember(dest => dest.LanguageCode, opt => opt.MapFrom(src => src.LanguageCode))
            .ForMember(dest => dest.MobilePhoneNumber, opt => opt.MapFrom(src => src.MobilePhoneNumber))
            .ForMember(dest => dest.NationalIdentityNumber, opt => opt.MapFrom(src => src.FnumberAk));
    }
}
