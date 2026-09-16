using System.Reflection;

using ResourceCleaner.Configuration;

using Microsoft.Extensions.Configuration;
using ResourceCleaner.ProfileClient;
using ResourceCleaner.Clients;
using ResourceCleaner.Models;
using System.Text;


var builder = new ConfigurationBuilder()
    .AddJsonFile($"appsettings.json", true, true)
    .AddUserSecrets(Assembly.GetExecutingAssembly());
var config = builder.Build();


ProfileDbSettings postgreSqlSettings = new();
var dbSettingsSection = config.GetRequiredSection("ProfileDbSettings");
dbSettingsSection.Bind(postgreSqlSettings);

ProfileClient profileClient = new(postgreSqlSettings.ConnectionString, postgreSqlSettings.CommandTimeoutSeconds);
List<ServiceResource> RR_resources_displayed_to_users = await ResourceRegistryDataLoader.Load("af-included");
List<ServiceResource> RR_resources_not_displayed = await ResourceRegistryDataLoader.Load("af-excluded");
List<ServiceResource> RR_resources_with_A2_servicecode_refs = [..RR_resources_displayed_to_users.Where(ResourceHasA2Ref), ..RR_resources_not_displayed.Where(ResourceHasA2Ref)];

static bool ResourceHasA2Ref(ServiceResource resource) => resource.ResourceReferences.Any(r_ref => r_ref.ReferenceSource == "Altinn2" && r_ref.ReferenceType=="ServiceCode");

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
        List<ServiceResource> A2ServiceCodeRefMatches = RR_resources_with_A2_servicecode_refs.FindAll(serviceResource => serviceResource.ResourceReferences.Any(r_ref => r_ref.ReferenceSource == "Altinn2" && r_ref.ReferenceType=="ServiceCode" && string.eq r_ref.Reference == pr));
        serviceResourcesMatchingRests[pr] = A2ServiceCodeRefMatches;
    }
});

string bold = "\x1b[1m";
string reset = "\x1b[0m";
StringBuilder resultStringBuilder = new();
resultStringBuilder.AppendLine($"{bold}{"Count of distinct ProfileDB resource-IDs: ", -90}{reset}{distinctProfileResourceIds.Count,10:D}");
resultStringBuilder.AppendLine($"{bold}{"Count of ProfileDB resource-IDs that match AFs include-list: ", -90}{reset}{profileResourceIdsMatchingAFIncludeList.Count,10:D}");
resultStringBuilder.AppendLine($"{bold}{"Count of ProfileDB resource-IDs that match AFs exclude-list: ", -90}{reset}{profileResourceIdsMatchingAFExcludeList.Count,10:D}");
resultStringBuilder.AppendLine();
resultStringBuilder.AppendLine($"{bold}{"Count of ProfileDB resource-IDs that have no match in RRs resourcelist: ", -90}{reset}{serviceResourcesMatchingRests.Keys.Count,10:D}");
resultStringBuilder.AppendLine($"{bold}{".. of which, we can map a number of: ", -90}{reset}{serviceResourcesMatchingRests.Count(kvPair => kvPair.Value.Count > 0),10:D}");
resultStringBuilder.AppendLine($"{bold}{"  .. where these have a single mapping candidate: ", -90}{reset}{serviceResourcesMatchingRests.Count(kvPair => kvPair.Value.Count == 1),10:D}");
resultStringBuilder.AppendLine($"{bold}{"  .. and these have a several mapping candidates: ", -90}{reset}{serviceResourcesMatchingRests.Count(kvPair => kvPair.Value.Count > 1),10:D}");
resultStringBuilder.AppendLine($"{bold}{".. while the rest count (neither matching RR identifier or A2 service code ref) is: ", -90}{reset}{serviceResourcesMatchingRests.Count(kvPair => kvPair.Value.Count == 0),10:D}");

Console.Write(resultStringBuilder);

Console.WriteLine();
Console.WriteLine($"{bold}{"Service codes with several mapping candidates: ", -90}{reset}");
foreach (var kvPair in serviceResourcesMatchingRests)
{
    if (kvPair.Value.Count > 1)
    {
        Console.WriteLine(kvPair.Key);
    }
}