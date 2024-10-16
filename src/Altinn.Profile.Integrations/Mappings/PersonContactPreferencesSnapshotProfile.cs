using Altinn.Profile.Integrations.Entities;

using AutoMapper;

namespace Altinn.Profile.Integrations.Mappings;

public class PersonContactPreferencesSnapshotProfile : AutoMapper.Profile
{
    public PersonContactPreferencesSnapshotProfile()
    {
        CreateMap<IPersonContactPreferencesSnapshot, PersonContactPreferencesSnapshot>()
            .ForMember(dest => dest.PersonContactDetailsSnapshot, opt => opt.MapFrom(src => src.PersonContactDetailsSnapshot))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.Language, opt => opt.MapFrom(src => src.Language))
            .ForMember(dest => dest.Reservation, opt => opt.MapFrom(src => src.Reservation))
            .ForMember(dest => dest.LanguageUpdated, opt => opt.MapFrom(src => src.LanguageUpdated))
            .ForMember(dest => dest.PersonIdentifier, opt => opt.MapFrom(src => src.PersonIdentifier))
            .ForMember(dest => dest.NotificationStatus, opt => opt.MapFrom(src => src.NotificationStatus));

        CreateMap<IPersonContactDetailsSnapshot, PersonContactDetailsSnapshot>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.EmailAddress))
            .ForMember(dest => dest.MobileNumber, opt => opt.MapFrom(src => src.MobilePhoneNumber))
            .ForMember(dest => dest.EmailLastUpdated, opt => opt.MapFrom(src => src.EmailAddressUpdated))
            .ForMember(dest => dest.MobileNumberLastUpdated, opt => opt.MapFrom(src => src.MobilePhoneNumberUpdated))
            .ForMember(dest => dest.IsEmailDuplicated, opt => opt.MapFrom(src => src.IsEmailAddressDuplicated))
            .ForMember(dest => dest.EmailLastVerified, opt => opt.MapFrom(src => src.EmailAddressLastVerified))
            .ForMember(dest => dest.MobileNumberLastVerified, opt => opt.MapFrom(src => src.MobilePhoneNumberLastVerified))
            .ForMember(dest => dest.IsMobileNumberDuplicated, opt => opt.MapFrom(src => src.IsMobilePhoneNumberDuplicated));
    }
}
