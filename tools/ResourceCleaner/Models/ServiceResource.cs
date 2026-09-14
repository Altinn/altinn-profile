using System.Text.Json.Serialization;

namespace ResourceCleaner.Models;

/// <summary>
/// Model describing a resource from the resource registry, in the slimmed-down shape
/// produced by the jq transformations (resourcelist.transformed.json and
/// resourcelist.rest.json). Note that this is not the shape of
/// original_resourcelist.json, which carries localized titles and more fields.
/// </summary>
public class ServiceResource
{
    /// <summary>
    /// The unique identifier of the resource, for example "slf-bier-prod".
    /// </summary>
    [JsonPropertyName("identifier")]
    public string Identifier { get; set; } = string.Empty;

    /// <summary>
    /// The resource version. Null for most resources in the source data.
    /// </summary>
    [JsonPropertyName("version")]
    public string? Version { get; set; }

    /// <summary>
    /// The Norwegian (bokmål) title of the resource, taken from title.nb in the source data.
    /// </summary>
    [JsonPropertyName("norwegianTitle")]
    public string? NorwegianTitle { get; set; }

    /// <summary>
    /// The lifecycle status of the resource. Observed values are "Active", "Completed",
    /// "Deprecated", "Discontinued", "UnderDevelopment" and "Withdrawn". Null when not set.
    /// </summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    /// <summary>
    /// Contact points for user support. Null when the resource has none.
    /// </summary>
    [JsonPropertyName("contactPoints")]
    public List<ContactPoint>? ContactPoints { get; set; }

    /// <summary>
    /// References to the resource in other systems, such as the Altinn 2 service code.
    /// </summary>
    [JsonPropertyName("resourceReferences")]
    public List<ResourceReference> ResourceReferences { get; set; } = [];

    /// <summary>
    /// Whether rights to the resource can be delegated.
    /// </summary>
    [JsonPropertyName("delegable")]
    public bool Delegable { get; set; }

    /// <summary>
    /// Whether the resource is visible to end users.
    /// </summary>
    [JsonPropertyName("visible")]
    public bool Visible { get; set; }

    /// <summary>
    /// The organization that owns the resource.
    /// </summary>
    [JsonPropertyName("competentAuthority")]
    public CompetentAuthority CompetentAuthority { get; set; } = new();

    /// <summary>
    /// The type of resource. Observed values are "AltinnApp", "BrokerService", "Consent",
    /// "CorrespondenceService", "GenericAccessResource", "MaskinportenSchema", "MigratedApp"
    /// and "Systemresource".
    /// </summary>
    [JsonPropertyName("resourceType")]
    public string ResourceType { get; set; } = string.Empty;

    /// <summary>
    /// The party types the resource is available for, such as "PrivatePerson" or "Company".
    /// Null when not restricted to specific party types.
    /// </summary>
    [JsonPropertyName("availableForType")]
    public List<string>? AvailableForType { get; set; }

    /// <summary>
    /// The urn-based references used when authorizing access to the resource.
    /// </summary>
    [JsonPropertyName("authorizationReference")]
    public List<AuthorizationReference> AuthorizationReferences { get; set; } = [];
}

/// <summary>
/// A contact point for user support related to a resource.
/// </summary>
public class ContactPoint
{
    /// <summary>
    /// The category of the contact point, for example "Brukerstøtte".
    /// </summary>
    [JsonPropertyName("category")]
    public string? Category { get; set; }

    /// <summary>
    /// The support email address.
    /// </summary>
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    /// <summary>
    /// The support telephone number.
    /// </summary>
    [JsonPropertyName("telephone")]
    public string? Telephone { get; set; }

    /// <summary>
    /// The url of a support web page.
    /// </summary>
    [JsonPropertyName("contactPage")]
    public string? ContactPage { get; set; }
}

/// <summary>
/// A reference identifying the resource in another system.
/// </summary>
public class ResourceReference
{
    /// <summary>
    /// The system the reference points into. Observed values are "Altinn2", "Altinn3",
    /// "Default" and "ExternalPlatform".
    /// </summary>
    [JsonPropertyName("referenceSource")]
    public string ReferenceSource { get; set; } = string.Empty;

    /// <summary>
    /// The reference value, for example an Altinn 2 service code such as "5789".
    /// </summary>
    [JsonPropertyName("reference")]
    public string Reference { get; set; } = string.Empty;

    /// <summary>
    /// The kind of reference. Observed values are "ApplicationId", "Default",
    /// "DelegationSchemeId", "MaskinportenScope", "ServiceCode", "ServiceEditionCode",
    /// "ServiceEditionVersion" and "Uri".
    /// </summary>
    [JsonPropertyName("referenceType")]
    public string ReferenceType { get; set; } = string.Empty;
}

/// <summary>
/// The organization owning a resource.
/// </summary>
public class CompetentAuthority
{
    /// <summary>
    /// The Norwegian (bokmål) name of the organization, taken from name.nb in the source data.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// The organization number.
    /// </summary>
    [JsonPropertyName("organization")]
    public string Organization { get; set; } = string.Empty;

    /// <summary>
    /// The Altinn org code. Note that the casing is inconsistent in the source data,
    /// both "acn" and "ACN" occur, so compare case-insensitively.
    /// </summary>
    [JsonPropertyName("orgcode")]
    public string OrgCode { get; set; } = string.Empty;
}

/// <summary>
/// A urn-based reference used when authorizing access to a resource.
/// </summary>
public class AuthorizationReference
{
    /// <summary>
    /// The urn key. Observed values are "urn:altinn:app", "urn:altinn:org" and
    /// "urn:altinn:resource".
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The value belonging to the urn key, typically the resource identifier.
    /// </summary>
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
}
