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
                .ForMember(dest => dest.EmailAddress, opt => opt.MapFrom(src => src.PersonContactDetailsSnapshot.EmailAddress))
                .ForMember(dest => dest.MobilePhoneNumber, opt => opt.MapFrom(src => src.PersonContactDetailsSnapshot.MobilePhoneNumber))
                .ForMember(dest => dest.EmailAddressLastUpdated, opt => opt.MapFrom(src => src.PersonContactDetailsSnapshot.EmailAddressUpdated.HasValue ? src.PersonContactDetailsSnapshot.EmailAddressUpdated.Value.ToUniversalTime() : (DateTime?)null))
                .ForMember(dest => dest.EmailAddressLastVerified, opt => opt.MapFrom(src => src.PersonContactDetailsSnapshot.EmailAddressLastVerified.HasValue ? src.PersonContactDetailsSnapshot.EmailAddressLastVerified.Value.ToUniversalTime() : (DateTime?)null))
                .ForMember(dest => dest.MobilePhoneNumberLastUpdated, opt => opt.MapFrom(src => src.PersonContactDetailsSnapshot.MobilePhoneNumberUpdated.HasValue ? src.PersonContactDetailsSnapshot.MobilePhoneNumberUpdated.Value.ToUniversalTime() : (DateTime?)null))
                .ForMember(dest => dest.MobilePhoneNumberLastVerified, opt => opt.MapFrom(src => src.PersonContactDetailsSnapshot.MobilePhoneNumberLastVerified.HasValue ? src.PersonContactDetailsSnapshot.MobilePhoneNumberLastVerified.Value.ToUniversalTime() : (DateTime?)null));
        }
    }
}
