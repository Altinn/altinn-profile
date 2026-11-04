using System.Reflection;

using ResourceCleaner.Configuration;

using Microsoft.Extensions.Configuration;
using ResourceCleaner.Clients;
using ResourceCleaner.Models;
using System.Text;

// #if DEBUG
// args = ["-u"];
// #endif

bool loadMode = args.Contains("-l") || args.Contains("--load");
bool updateMode = args.Contains("-u") || args.Contains("--update");

var builder = new ConfigurationBuilder()
    .AddJsonFile($"appsettings.json", true, true)
    .AddUserSecrets(Assembly.GetExecutingAssembly());
var config = builder.Build();


var env = "prod";
ProfileDbSettings postgreSqlSettings = new();
var dbSettingsSection = config.GetRequiredSection($"ProfileDbSettings-{env}");
dbSettingsSection.Bind(postgreSqlSettings);


ProfileClient profileClient = new(postgreSqlSettings.ConnectionString, postgreSqlSettings.CommandTimeoutSeconds);

if (updateMode)
{
    List<Mapping> mappings = await FileDataLoader.LoadSafeMappingResults(env);
    // TODO: perform DB update operations for the safe mappings
}

else if (loadMode)
{
    // Read our distinct resource IDs in Profile and the resource list of RR
    List<string> distinctProfileResourceIds = await profileClient.GetDistinctUserPartyResourceIdsAsync();
    List<ServiceResource> RR_resources_displayed_to_users = await FileDataLoader.LoadResourcesOfType($"af-included-{env}"); // The subset of resources that AF display
    List<ServiceResource> RR_resources_not_displayed = await FileDataLoader.LoadResourcesOfType($"af-excluded-{env}"); // The subset of resources that AF filter out, not displayed in UI
    List<ServiceResource> RR_resources_with_A2_servicecode_refs = [.. RR_resources_displayed_to_users.Where(ResourceHasA2Ref), .. RR_resources_not_displayed.Where(ResourceHasA2Ref)]; // The subset of resources that contain an A2 service code reference

    // Helper methods
    static bool ResourceHasA2Ref(ServiceResource resource) => resource.ResourceReferences.Any(r_ref => r_ref.ReferenceSource == "Altinn2" && r_ref.ReferenceType == "ServiceCode");
    bool ProfileResourceIdMatchesAFIncludeResource(string profileResourceId) => RR_resources_displayed_to_users.Any(r => r.Identifier == profileResourceId);
    bool ProfileResourceIdMatchesAFExcludeResource(string profileResourceId) => RR_resources_not_displayed.Any(r => r.Identifier == profileResourceId);
    List<ServiceResource> FindServiceResourceMatchesOnA2Ref(string profileResourceId) => [.. RR_resources_with_A2_servicecode_refs.FindAll(serviceResource => serviceResource.ResourceReferences.Any(r_ref => r_ref.ReferenceSource == "Altinn2" && r_ref.ReferenceType == "ServiceCode" && r_ref.Reference == profileResourceId)).OrderBy(s => s.Identifier)];
    static bool IsSafeMappingCandidate(string resourceId, ServiceResource matchingResource)
    {
        var editionCodeRef = matchingResource.ResourceReferences.Find(r_ref => r_ref.ReferenceSource == "Altinn2" && r_ref.ReferenceType == "ServiceEditionCode");
        return editionCodeRef != null && matchingResource.Identifier.ToLower().EndsWith($"{resourceId.ToLower()}-{editionCodeRef.Reference}");
    }

    // Collect results in different groups as we process Profile's resource IDs
    List<string> profileResourceIdsMatchingAFIncludeList = [];
    List<string> profileResourceIdsMatchingAFExcludeList = [];
    Dictionary<string, List<ServiceResource>> serviceResourcesMatchingRests = [];
    List<string> profileResourceIdsWithoutAnyMatchesInRR = [];
    Dictionary<string, List<string>> safeMappingCandidates = [];
    int numOfProfileIdsNotMatchingRRIdentifier = 0;


    // Currently (i.e. pre clean up), ~43% of Profile-stored 'resource IDs are not valid identifiers in the Resource Registry (RR)
    // For each Profile 'resource ID', look through the RR resource list to find resources that have the 'resource ID' has an A2 service code reference
    // If we can find any such resource(s), we can replace the Profile 'resource ID' with the RR resource identifier(s)
    // We consider a mapping as 'safe' or 'reliable' if the RR resource has the profile resource ID as an A2 service code AND the resource identifier ends with {profileResourceId}-{editionCode}
    // The reason for this is that this naming pattern was used in A2->A3 service migrations

    distinctProfileResourceIds.ForEach(pr =>
    {
        if (ProfileResourceIdMatchesAFIncludeResource(pr))
        {
            profileResourceIdsMatchingAFIncludeList.Add(pr);
        }
        else if (ProfileResourceIdMatchesAFExcludeResource(pr))
        {
            profileResourceIdsMatchingAFExcludeList.Add(pr);
        }
        else // The profile-stored resource-ID does not match any identifier in RRs resource-list. Proceed to look at the A2 reference of RR resources
        {
            List<string> safeMappingCandidateBuilder = [];
            List<ServiceResource> unsafeMappingCandidateBuilder = [];
            numOfProfileIdsNotMatchingRRIdentifier++;


            List<ServiceResource> RR_resources_with_this_id_as_A2_service_code = FindServiceResourceMatchesOnA2Ref(pr);

            if (RR_resources_with_this_id_as_A2_service_code.Count == 0)
            {
                profileResourceIdsWithoutAnyMatchesInRR.Add(pr);
            }

            foreach (var matchingResource in RR_resources_with_this_id_as_A2_service_code)
            {
                if (IsSafeMappingCandidate(pr, matchingResource))
                {
                    safeMappingCandidateBuilder.Add(matchingResource.Identifier);
                }
                else
                {
                    unsafeMappingCandidateBuilder.Add(matchingResource);
                }
            }

            if (unsafeMappingCandidateBuilder.Count > 0)
            {
                serviceResourcesMatchingRests[pr] = unsafeMappingCandidateBuilder;
            }

            if (safeMappingCandidateBuilder.Count > 0)
            {
                safeMappingCandidates[pr] = safeMappingCandidateBuilder;
            }
        }
    });

    var unsafeMappableResourceIds = serviceResourcesMatchingRests.Where(kvPair => kvPair.Value.Count > 0);

    // Write overview of counts for the various result groups to console

    string bold = "\x1b[1m";
    string reset = "\x1b[0m";
    StringBuilder resultStringBuilder = new();
    resultStringBuilder.AppendLine($"{bold}{"Count of distinct ProfileDB resource-IDs: ",-90}{reset}{distinctProfileResourceIds.Count,10:D}");
    resultStringBuilder.AppendLine($"{bold}{"Count of ProfileDB resource-IDs that match AFs include-list: ",-90}{reset}{profileResourceIdsMatchingAFIncludeList.Count,10:D}");

    resultStringBuilder.AppendLine($"{bold}{"Count of ProfileDB resource-IDs that match AFs exclude-list: ",-90}{reset}{profileResourceIdsMatchingAFExcludeList.Count,10:D}");
    resultStringBuilder.AppendLine();
    resultStringBuilder.AppendLine($"{bold}{"Count of ProfileDB resource-IDs that have no match in RRs resourcelist: ",-90}{reset}{numOfProfileIdsNotMatchingRRIdentifier,10:D}");
    resultStringBuilder.AppendLine($"{bold}{".. of which, we have safe mappings for: ",-90}{reset}{safeMappingCandidates.Count,10:D}");
    resultStringBuilder.AppendLine($"{bold}{".. and *unsafe* mappings for: ",-90}{reset}{unsafeMappableResourceIds.Count(),10:D}");
    resultStringBuilder.AppendLine($"{bold}{".. while the rest count (neither matching RR identifier or A2 service code ref) is: ",-90}{reset}{profileResourceIdsWithoutAnyMatchesInRR.Count,10:D}");

    Console.Write(resultStringBuilder);

    // Write the resulting groups to different files
    await FileWriter.WriteAsyncWithLeadStr(env, "profile-resource-IDs-AF-included.txt", "ProfileDB resource-IDs that match AFs include-list", profileResourceIdsMatchingAFIncludeList);
    await FileWriter.WriteAsyncWithLeadStr(env, "profile-resource-IDs-AF-excluded.txt", "ProfileDB resource-IDs that match AFs exclude-list", profileResourceIdsMatchingAFExcludeList);
    await FileWriter.WriteAsync(env, "profile-safe-mapping-candidates.txt", safeMappingCandidates.Select(kvPair => $"{kvPair.Key} {string.Join(",", kvPair.Value)}"));
    await FileWriter.WriteAsync(env, "profile-unsafe-mapping-candidates.txt", unsafeMappableResourceIds.Select(kvPair => $"{kvPair.Key} - {string.Join(", ", kvPair.Value.Select(sr => sr.Identifier))}"));
    await FileWriter.WriteAsyncWithLeadStr(env, "profile-resource-IDs-not-in-RR-not-mappable.txt", "ProfileDB resource-IDs that have no match in RRs resourcelist, neither as identifier nor as a2-servicecode-ref:", profileResourceIdsWithoutAnyMatchesInRR);
}
else
{
    Console.Write("No mode set - rerun with either -l or -u flag");
}

