namespace Altinn.Profile.Integrations.Entities;

/// <summary>
/// AutoMapper profile for configuring mappings between <see cref="Register"/> and a class implementing the <see cref="IUserContactInfo"/> interface.
/// </summary>
/// <remarks>
/// This profile defines the mapping rules required to transform a <see cref="Register"/> object into a class implementing the <see cref="IUserContactInfo"/> interface.
/// It is used by AutoMapper to facilitate the conversion of data between these two models.
/// </remarks>
public class UserContactInfoProfile : AutoMapper.Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserContactInfoProfile"/> class.
    /// </summary>
    /// <remarks>
    /// The constructor configures the mappings between the <see cref="Register"/> class and a class implementing the <see cref="IUserContactInfo"/> interface.
    /// </remarks>
    public UserContactInfoProfile()
    {
        CreateMap<Register, IUserContactInfo>()
            .ForMember(dest => dest.IsReserved, opt => opt.MapFrom(src => src.Reservation))
            .ForMember(dest => dest.EmailAddress, opt => opt.MapFrom(src => src.EmailAddress))
            .ForMember(dest => dest.LanguageCode, opt => opt.MapFrom(src => src.LanguageCode))
            .ForMember(dest => dest.NationalIdentityNumber, opt => opt.MapFrom(src => src.FnumberAk))
            .ForMember(dest => dest.MobilePhoneNumber, opt => opt.MapFrom(src => src.MobilePhoneNumber));
    }
}
