namespace ResourceCleaner.ProfileClient;

internal class ProfileClient
{
    internal record UserPartyContactInfoResource(
        long UserPartyContactInfoResourceId,
        long UserPartyContactInfoId,
        string ResourceId
    );

    private const string _getDistinctUserPartyContactInfoResourceIds = """
        SELECT DISTINCT resource_id
        FROM professional_notification_settings.user_party_contact_info_resources
    """;

    private const string _getUserPartyContactInfoResources = """
        SELECT user_party_contact_info_resource_id, user_party_contact_info_id, resource_id
        FROM professional_notification_settings.user_party_contact_info_resources
        WHERE registeredtime > $1
        AND cloudevent->>'resource' = $2
        AND cloudevent->>'resourceinstance' = $3
        ORDER BY sequenceno
        LIMIT 100
        """;

    private readonly NpgsqlDataSource _dataSource;
    private readonly int _commandTimeoutSeconds;

    internal ProfileClient(string connectionString, int commandTimeoutSeconds)
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.EnableDynamicJson();
        _dataSource = dataSourceBuilder.Build();
        _commandTimeoutSeconds = commandTimeoutSeconds;
    }

    internal async Task<List<UserPartyResource>> GetUserPartyResourcesAsync()
    {
        await using NpgsqlCommand pgcom = _dataSource.CreateCommand(_getUserPartyContactInfoResources);
        pgcom.CommandTimeout = _commandTimeoutSeconds;
        List<UserPartyContactInfoResource> userPartyContactInfoResources = [];
        await using NpgsqlDataReader reader = await pgcom.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            userPartyContactInfoResources.Add(new UserPartyContactInfoResource(
                UserPartyContactInfoResourceId: reader.GetInt64(0),
                UserPartyContactInfoId: reader.GetInt64(1),
                ResourceId: reader.GetString(2)
            ));
        }

        return userPartyContactInfoResources;

        // Read all rows from DB
        // Init different lists:
        //   resourcesMatchingIdentifier
        //   otherResourcesNotMatchingIdentifier

    }
    internal async Task<List<UserPartyResource>> GetDistinctUserPartyResourceIdsAsync()
    {
        await using NpgsqlCommand pgcom = _dataSource.CreateCommand(_getDistinctUserPartyContactInfoResourceIds);
        pgcom.CommandTimeout = _commandTimeoutSeconds;
        List<string> distinctResourceIds = [];
        await using NpgsqlDataReader reader = await pgcom.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            distinctResourceIds.Add(reader.GetString(0));
        }

        return distinctResourceIds;

        // Read all rows from DB
        // Init different lists:
        //   resourcesMatchingIdentifier
        //   otherResourcesNotMatchingIdentifier

    }
}
