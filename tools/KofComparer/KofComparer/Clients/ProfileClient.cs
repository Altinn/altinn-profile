using KofComparer.Models;

using Npgsql;

namespace KofComparer.Clients;

internal class ProfileClient
{
    // No filter on registry_updated_date_time: a filter here would make rows updated after the
    // dump cutoff, and rows created through the Altinn API (registry_updated_date_time = NULL),
    // look missing. The cutoff is applied in Program.cs instead, when classifying differences.
    // is_soft_deleted and has_registry_accepted are deliberately not filtered either - the row
    // existing at all means the sync did reach us.
    private const string _getOrgNotificationAddresses = """
        SELECT o.registry_organization_number, na.registry_id, na.address_type, na.registry_updated_date_time
        FROM organization_notification_address.notifications_address na
        JOIN organization_notification_address.organizations o ON o.registry_organization_id = na.fk_registry_organization_id
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

    internal async Task<List<ProfileOrgAddress>> GetProfileOrgAddressesAsync()
    {
        await using NpgsqlCommand pgcom = _dataSource.CreateCommand(_getOrgNotificationAddresses);
        pgcom.CommandTimeout = _commandTimeoutSeconds;
        List<ProfileOrgAddress> profileOrgAddresses = [];
        await using NpgsqlDataReader reader = await pgcom.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            profileOrgAddresses.Add(new ProfileOrgAddress(
                RegistryOrganizationNumber: reader.GetString(0),
                RegistryID: reader.GetString(1),
                AddressType: reader.GetInt32(2),
                RegistryUpdatedDateTime: reader.IsDBNull(3) ? null : reader.GetDateTime(3)
            ));
        }

        return profileOrgAddresses;
    }
}
