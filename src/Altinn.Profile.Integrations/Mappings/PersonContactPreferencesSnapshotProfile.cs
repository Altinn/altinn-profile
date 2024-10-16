using Altinn.Profile.Core.Person.ContactPreferences;
using Altinn.Profile.Integrations.Entities;

using AutoMapper;

namespace Altinn.Profile.Integrations.Mappings;

public class PersonContactPreferencesSnapshotProfile : AutoMapper.Profile
{
    public PersonContactPreferencesSnapshotProfile()
    {
        CreateMap<IPersonContactPreferencesSnapshot, PersonContactPreferencesSnapshot>()
            .ForMember(dest => dest.ContactDetailsSnapshot, opt => opt.MapFrom(src => src.ContactDetailsSnapshot))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.Language, opt => opt.MapFrom(src => src.Language))
            .ForMember(dest => dest.Reservation, opt => opt.MapFrom(src => src.Reservation))
            .ForMember(dest => dest.LanguageLastUpdated, opt => opt.MapFrom(src => src.LanguageLastUpdated))
            .ForMember(dest => dest.PersonIdentifier, opt => opt.MapFrom(src => src.PersonIdentifier))
            .ForMember(dest => dest.NotificationStatus, opt => opt.MapFrom(src => src.NotificationStatus));

        CreateMap<IPersonContactDetailsSnapshot, PersonContactDetailsSnapshot>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.MobileNumber, opt => opt.MapFrom(src => src.MobileNumber))
            .ForMember(dest => dest.EmailLastUpdated, opt => opt.MapFrom(src => src.EmailLastUpdated))
            .ForMember(dest => dest.MobileNumberLastUpdated, opt => opt.MapFrom(src => src.MobileNumberLastUpdated))
            .ForMember(dest => dest.IsEmailDuplicated, opt => opt.MapFrom(src => src.IsEmailDuplicated))
            .ForMember(dest => dest.EmailLastVerified, opt => opt.MapFrom(src => src.EmailLastVerified))
            .ForMember(dest => dest.MobileNumberLastVerified, opt => opt.MapFrom(src => src.MobileNumberLastVerified))
            .ForMember(dest => dest.IsMobileNumberDuplicated, opt => opt.MapFrom(src => src.IsMobileNumberDuplicated));
    }
}
