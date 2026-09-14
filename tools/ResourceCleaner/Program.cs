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

List<string> distinctProfileResourceIds = await profileClient.GetDistinctUserPartyResourceIdsAsync(); // <-  Get list of distinct Profile resources

// 'Valid' meaning 
List<string> profileResourceIdsMatchingAFIncludeList = [];
List<string> profileResourceIdsMatchingAFExcludeList = [];
List<string> profileResourceIdsWithoutRRMatch = [];

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
    else
    {
        profileResourceIdsWithoutRRMatch.Add(pr);
    }
});

StringBuilder resultStringBuilder = new();
resultStringBuilder.Append($"Count of distinct ProfileDB resource-IDs: {distinctProfileResourceIds.Count,10:C}");
resultStringBuilder.AppendLine($"Count of ProfileDB resource-IDs that match AFs include-list: {profileResourceIdsMatchingAFIncludeList.Count,10:C}");
resultStringBuilder.AppendLine($"Count of ProfileDB resource-IDs that match AFs exclude-list: {profileResourceIdsMatchingAFExcludeList.Count,10:C}");
resultStringBuilder.AppendLine($"Count of ProfileDB resource-IDs that have no match in RRs resourcelist: {profileResourceIdsWithoutRRMatch.Count,10:C}");

Console.Write(resultStringBuilder);

//  rrResourceList.Any(r => r.Identifier == profileResourceId)