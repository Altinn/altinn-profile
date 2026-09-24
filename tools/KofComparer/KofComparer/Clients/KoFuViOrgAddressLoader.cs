using System.Diagnostics.CodeAnalysis;
using System.Globalization;

using KofComparer.Models;

namespace KofComparer.Clients;

/// <summary>
/// Reads a KoFuVi snapshot dump. The dump has no header, no quoted fields and exactly three
/// comma separated columns per row (registry id, is deleted, last changed), so rows are parsed
/// with spans and streamed rather than materialized - the file is roughly 5.9 million rows.
/// </summary>
public static class KoFuViOrgAddressLoader
{
    /// <summary>
    /// The timestamp format used by the KoFuVi dump. The values carry no offset and are
    /// Norwegian local time, see <see cref="DumpTimeZone"/>.
    /// </summary>
    public const string TimestampFormat = "yyyy-MM-dd HH:mm:ss.ffffff";

    /// <summary>
    /// The time zone the dump timestamps are expressed in.
    /// </summary>
    public static readonly TimeZoneInfo DumpTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Oslo");

    private const int ExpectedColumnCount = 3;

    /// <summary>
    /// Streams the addresses in the given dump file.
    /// </summary>
    /// <param name="path">Path to the KoFuVi csv dump.</param>
    /// <param name="onMalformedLine">Called with the line number and content of every line that could not be parsed. Such lines are skipped.</param>
    public static async IAsyncEnumerable<KoFuViOrgAddress> Load(string path, Action<long, string>? onMalformedLine = null)
    {
        long lineNumber = 0;

        await foreach (string line in File.ReadLinesAsync(path))
        {
            lineNumber++;

            if (line.Length == 0)
            {
                continue;
            }

            if (!TryParseLine(line, out KoFuViOrgAddress? address))
            {
                onMalformedLine?.Invoke(lineNumber, line);
                continue;
            }

            yield return address;
        }
    }

    private static bool TryParseLine(ReadOnlySpan<char> line, [NotNullWhen(true)] out KoFuViOrgAddress? address)
    {
        address = null;

        // One extra range so a row with too many columns is detected rather than silently truncated.
        Span<Range> fields = stackalloc Range[ExpectedColumnCount + 1];
        if (line.Split(fields, ',') != ExpectedColumnCount)
        {
            return false;
        }

        ReadOnlySpan<char> registryId = line[fields[0]];

        if (registryId.IsEmpty || !bool.TryParse(line[fields[1]], out bool isDelete))
        {
            return false;
        }

        if (!DateTime.TryParseExact(
                line[fields[2]],
                TimestampFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime registryLastUpdatedLocal))
        {
            return false;
        }

        // A local time inside the spring-forward gap never existed, so the row cannot be trusted.
        if (DumpTimeZone.IsInvalidTime(registryLastUpdatedLocal))
        {
            return false;
        }

        address = new KoFuViOrgAddress
        {
            RegistryID = registryId.ToString(),
            IsDelete = isDelete,

            // Ambiguous times (the repeated hour when daylight saving ends) are resolved as
            // standard time. The flag lets the comparison accept the other reading as well.
            RegistryLastUpdatedUtc = TimeZoneInfo.ConvertTimeToUtc(registryLastUpdatedLocal, DumpTimeZone),
            IsAmbiguousLocalTime = DumpTimeZone.IsAmbiguousTime(registryLastUpdatedLocal),
        };

        return true;
    }
}
