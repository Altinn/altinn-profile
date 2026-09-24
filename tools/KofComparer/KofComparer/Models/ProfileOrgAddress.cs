namespace KofComparer.Models;

public record ProfileOrgAddress(
    string RegistryOrganizationNumber,
    string RegistryID,
    int AddressType,
    DateTime? RegistryUpdatedDateTime
);
