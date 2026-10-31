using System.Reflection;

using ResourceCleaner.Configuration;

using Microsoft.Extensions.Configuration;
using ResourceCleaner.ProfileClient;
using ResourceCleaner.Clients;
using ResourceCleaner.Models;
using System.Text;
using System.Resources;


var builder = new ConfigurationBuilder()
    .AddJsonFile($"appsettings.json", true, true)
    .AddUserSecrets(Assembly.GetExecutingAssembly());
var config = builder.Build();


var env = "at23";
ProfileDbSettings postgreSqlSettings = new();
var dbSettingsSection = config.GetRequiredSection($"ProfileDbSettings-{env}");
dbSettingsSection.Bind(postgreSqlSettings);


ProfileClient profileClient = new(postgreSqlSettings.ConnectionString, postgreSqlSettings.CommandTimeoutSeconds);
List<ServiceResource> RR_resources_displayed_to_users = await ResourceRegistryDataLoader.Load($"af-included-{env}");
List<ServiceResource> RR_resources_not_displayed = await ResourceRegistryDataLoader.Load($"af-excluded-{env}");
List<ServiceResource> RR_resources_with_A2_servicecode_refs = [.. RR_resources_displayed_to_users.Where(ResourceHasA2Ref), .. RR_resources_not_displayed.Where(ResourceHasA2Ref)];

static bool ResourceHasA2Ref(ServiceResource resource) => resource.ResourceReferences.Any(r_ref => r_ref.ReferenceSource == "Altinn2" && r_ref.ReferenceType == "ServiceCode");

List<string> distinctProfileResourceIds = await profileClient.GetDistinctUserPartyResourceIdsAsync(); // <-  Get list of distinct Profile resources

List<string> profileResourceIdsMatchingAFIncludeList = [];
List<string> profileResourceIdsMatchingAFExcludeList = [];

Dictionary<string, List<ServiceResource>> serviceResourcesMatchingRests = [];
List<string> profileResourceIdsNotMatchingA2Ref = [];

distinctProfileResourceIds.ForEach(pr =>
{
    bool profileResourceIdMatchesAFIncludeResource = RR_resources_displayed_to_users.Any(r => r.Identifier == pr);
    bool profileResourceIdMatchesAFExcludeResource = RR_resources_not_displayed.Any(r => r.Identifier == pr);

    if (profileResourceIdMatchesAFIncludeResource)
    {
        profileResourceIdsMatchingAFIncludeList.Add(pr);
    }
    else if (profileResourceIdMatchesAFExcludeResource)
    {
        profileResourceIdsMatchingAFExcludeList.Add(pr);
    }
    else // The profile-stored resource-ID does not match any identifier in RRs resource-list. Check for A2 references
    {
        List<ServiceResource> A2ServiceCodeRefMatches = RR_resources_with_A2_servicecode_refs.FindAll(serviceResource => serviceResource.ResourceReferences.Any(r_ref => r_ref.ReferenceSource == "Altinn2" && r_ref.ReferenceType == "ServiceCode" && r_ref.Reference == pr));
        serviceResourcesMatchingRests[pr] = A2ServiceCodeRefMatches;
    }
});

string bold = "\x1b[1m";
string reset = "\x1b[0m";
StringBuilder resultStringBuilder = new();
resultStringBuilder.AppendLine($"{bold}{"Count of distinct ProfileDB resource-IDs: ",-90}{reset}{distinctProfileResourceIds.Count,10:D}");
resultStringBuilder.AppendLine($"{bold}{"Count of ProfileDB resource-IDs that match AFs include-list: ",-90}{reset}{profileResourceIdsMatchingAFIncludeList.Count,10:D}");
await FileWriter.WriteAsync(env, "profile-resource-IDs-AF-included.txt", "ProfileDB resource-IDs that match AFs include-list", profileResourceIdsMatchingAFIncludeList);

resultStringBuilder.AppendLine($"{bold}{"Count of ProfileDB resource-IDs that match AFs exclude-list: ",-90}{reset}{profileResourceIdsMatchingAFExcludeList.Count,10:D}");
await FileWriter.WriteAsync(env, "profile-resource-IDs-AF-excluded.txt", "ProfileDB resource-IDs that match AFs exclude-list", profileResourceIdsMatchingAFExcludeList);
resultStringBuilder.AppendLine();
resultStringBuilder.AppendLine($"{bold}{"Count of ProfileDB resource-IDs that have no match in RRs resourcelist: ",-90}{reset}{serviceResourcesMatchingRests.Keys.Count,10:D}");
resultStringBuilder.AppendLine($"{bold}{".. of which, we can map a number of: ",-90}{reset}{serviceResourcesMatchingRests.Count(kvPair => kvPair.Value.Count > 0),10:D}");
var singleMappableResourceIds = serviceResourcesMatchingRests.Where(kvPair => kvPair.Value.Count == 1);
resultStringBuilder.AppendLine($"{bold}{"  .. where these have a single mapping candidate: ",-90}{reset}{singleMappableResourceIds.Count(),10:D}");
await FileWriter.WriteAsync(env, "profile-resource-IDs-not-in-RR-with-single-mapping-candidate.txt", "ProfileDB resource-IDs that have no match in RRs resourcelist - but we can map to a single:", singleMappableResourceIds.Select(kvPair => $"{kvPair.Key} - {kvPair.Value.Select(sr => sr.Identifier)}"));
var multiMappableResourceIds = serviceResourcesMatchingRests.Where(kvPair => kvPair.Value.Count > 1);
resultStringBuilder.AppendLine($"{bold}{"  .. and these have a several mapping candidates: ",-90}{reset}{multiMappableResourceIds.Count(),10:D}");
await FileWriter.WriteAsync(env, "profile-resource-IDs-not-in-RR-with-multi-mapping-candidates.txt", "ProfileDB resource-IDs that have no match in RRs resourcelist - but we can map to multiple:", multiMappableResourceIds.Select(kvPair => $"{kvPair.Key} - {string.Join(", ", kvPair.Value)}"));
var nonMappableResourceIds = serviceResourcesMatchingRests.Where(kvPair => kvPair.Value.Count == 0);
resultStringBuilder.AppendLine($"{bold}{".. while the rest count (neither matching RR identifier or A2 service code ref) is: ",-90}{reset}{nonMappableResourceIds.Count(),10:D}");
await FileWriter.WriteAsync(env, "profile-resource-IDs-not-in-RR-not-mappable.txt", "ProfileDB resource-IDs that have no match in RRs resourcelist, neither as identifier nor as a2-servicecode-ref:", nonMappableResourceIds.Select(kvPair => kvPair.Key));

Console.Write(resultStringBuilder);