using Altinn.Profile.Integrations.Entities;

using AutoMapper;

namespace Altinn.Profile.Integrations.Mappings
{
    public class PersonMappingProfile : AutoMapper.Profile
    {
        public PersonMappingProfile()
        {
            CreateMap<PersonContactPreferencesSnapshot, Person>()
                .ForMember(dest => dest.LanguageCode, opt => opt.MapFrom(src => src.Language))
                .ForMember(dest => dest.FnumberAk, opt => opt.MapFrom(src => src.PersonIdentifier))
                .ForMember(dest => dest.Reservation, opt => opt.MapFrom(src => src.Reservation == "JA"))
                .ForMember(dest => dest.EmailAddress, opt => opt.MapFrom(src => src.PersonContactDetailsSnapshot.Email))
                .ForMember(dest => dest.MobilePhoneNumber, opt => opt.MapFrom(src => src.PersonContactDetailsSnapshot.MobileNumber))
                .ForMember(dest => dest.EmailAddressLastUpdated, opt => opt.MapFrom(src => src.PersonContactDetailsSnapshot.EmailLastUpdated.HasValue ? src.PersonContactDetailsSnapshot.EmailLastUpdated.Value.ToUniversalTime() : (DateTime?)null))
                .ForMember(dest => dest.EmailAddressLastVerified, opt => opt.MapFrom(src => src.PersonContactDetailsSnapshot.EmailLastVerified.HasValue ? src.PersonContactDetailsSnapshot.EmailLastVerified.Value.ToUniversalTime() : (DateTime?)null))
                .ForMember(dest => dest.MobilePhoneNumberLastUpdated, opt => opt.MapFrom(src => src.PersonContactDetailsSnapshot.MobileNumberLastUpdated.HasValue ? src.PersonContactDetailsSnapshot.MobileNumberLastUpdated.Value.ToUniversalTime() : (DateTime?)null))
                .ForMember(dest => dest.MobilePhoneNumberLastVerified, opt => opt.MapFrom(src => src.PersonContactDetailsSnapshot.MobileNumberLastVerified.HasValue ? src.PersonContactDetailsSnapshot.MobileNumberLastVerified.Value.ToUniversalTime() : (DateTime?)null));
        }
    }
}
