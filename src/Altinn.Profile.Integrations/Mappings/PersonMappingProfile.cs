using Altinn.Profile.Core.Person.ContactPreferences;
using Altinn.Profile.Integrations.Entities;

namespace Altinn.Profile.Integrations.Mappings;

/// <summary>
/// AutoMapper profile for mapping between <see cref="PersonContactPreferencesSnapshot"/> and <see cref="Person"/>.
/// </summary>
/// <remarks>
/// This profile defines the mapping rules to convert a <see cref="PersonContactPreferencesSnapshot"/> object into a <see cref="Person"/> instance.
/// </remarks>
public class PersonMappingProfile : AutoMapper.Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PersonMappingProfile"/> class and configures the mappings.
    /// </summary>
    public PersonMappingProfile()
    {
        CreateMap<PersonContactPreferencesSnapshot, Person>()
            .ForMember(dest => dest.LanguageCode, opt => opt.MapFrom(src => src.Language))
            .ForMember(dest => dest.FnumberAk, opt => opt.MapFrom(src => src.PersonIdentifier))
            .ForMember(dest => dest.Reservation, opt => opt.MapFrom(src => src.Reservation == "JA"))
            .ForMember(dest => dest.EmailAddress, opt => opt.MapFrom(src => GetContactDetail(src, detail => detail.Email)))
            .ForMember(dest => dest.MobilePhoneNumber, opt => opt.MapFrom(src => GetContactDetail(src, detail => detail.MobileNumber)))
            .ForMember(dest => dest.EmailAddressLastUpdated, opt => opt.MapFrom(src => GetContactDetailDate(src, detail => detail.EmailLastUpdated)))
            .ForMember(dest => dest.EmailAddressLastVerified, opt => opt.MapFrom(src => GetContactDetailDate(src, detail => detail.EmailLastVerified)))
            .ForMember(dest => dest.MobilePhoneNumberLastUpdated, opt => opt.MapFrom(src => GetContactDetailDate(src, detail => detail.MobileNumberLastUpdated)))
            .ForMember(dest => dest.MobilePhoneNumberLastVerified, opt => opt.MapFrom(src => GetContactDetailDate(src, detail => detail.MobileNumberLastVerified)));
    }

    private static string? GetContactDetail(PersonContactPreferencesSnapshot src, Func<PersonContactDetailsSnapshot, string?> selector)
    {
        return src.ContactDetailsSnapshot != null ? selector(src.ContactDetailsSnapshot) : null;
    }

    private static DateTime? GetContactDetailDate(PersonContactPreferencesSnapshot src, Func<PersonContactDetailsSnapshot, DateTime?> selector)
    {
        if (src.ContactDetailsSnapshot != null)
        {
            var date = selector(src.ContactDetailsSnapshot);

            if (date.HasValue)
            {
                return date.Value.ToUniversalTime();
            }
        }

        return null;
    }
}
