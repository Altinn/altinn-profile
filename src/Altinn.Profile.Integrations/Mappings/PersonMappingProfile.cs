using Altinn.Profile.Core.Person.ContactPreferences;
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
                .ForMember(dest => dest.EmailAddress, opt => opt.MapFrom(src => src.ContactDetailsSnapshot.Email))
                .ForMember(dest => dest.MobilePhoneNumber, opt => opt.MapFrom(src => src.ContactDetailsSnapshot.MobileNumber))
                .ForMember(dest => dest.EmailAddressLastUpdated, opt => opt.MapFrom(src => src.ContactDetailsSnapshot.EmailLastUpdated.HasValue ? src.ContactDetailsSnapshot.EmailLastUpdated.Value.ToUniversalTime() : (DateTime?)null))
                .ForMember(dest => dest.EmailAddressLastVerified, opt => opt.MapFrom(src => src.ContactDetailsSnapshot.EmailLastVerified.HasValue ? src.ContactDetailsSnapshot.EmailLastVerified.Value.ToUniversalTime() : (DateTime?)null))
                .ForMember(dest => dest.MobilePhoneNumberLastUpdated, opt => opt.MapFrom(src => src.ContactDetailsSnapshot.MobileNumberLastUpdated.HasValue ? src.ContactDetailsSnapshot.MobileNumberLastUpdated.Value.ToUniversalTime() : (DateTime?)null))
                .ForMember(dest => dest.MobilePhoneNumberLastVerified, opt => opt.MapFrom(src => src.ContactDetailsSnapshot.MobileNumberLastVerified.HasValue ? src.ContactDetailsSnapshot.MobileNumberLastVerified.Value.ToUniversalTime() : (DateTime?)null));
        }
    }
}
