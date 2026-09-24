using System.Globalization;

using KofComparer.Models;

namespace KofComparer.Clients;

/// <summary>
/// Writes the found differences as csv with a header row. Timestamps are written in UTC.
/// </summary>
public static class RowDifferenceWriter
{
    private const string Header = "kind,registry_id,kofuvi_is_delete,kofuvi_updated_utc,profile_updated_utc";

    /// <summary>
    /// Writes the given differences to <paramref name="path"/>, replacing any existing file.
    /// </summary>
    /// <returns>The number of rows written, excluding the header.</returns>
    public static async Task<int> WriteAsync(string path, IEnumerable<RowDifference> differences)
    {
        int written = 0;

        await using StreamWriter writer = new(path, append: false);
        await writer.WriteLineAsync(Header);

        foreach (RowDifference difference in differences)
        {
            string isDelete = difference.KoFuViIsDelete switch
            {
                true => "true",
                false => "false",
                null => string.Empty,
            };

            await writer.WriteLineAsync(
                $"{difference.Kind},{difference.RegistryID},{isDelete},{Format(difference.KoFuViUpdatedUtc)},{Format(difference.ProfileUpdatedUtc)}");
            written++;
        }

        return written;
    }

    private static string Format(DateTime? timestamp) =>
        timestamp?.ToString(KoFuViOrgAddressLoader.TimestampFormat, CultureInfo.InvariantCulture) ?? string.Empty;
}
