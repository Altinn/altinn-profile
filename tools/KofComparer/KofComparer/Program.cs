using System.Diagnostics;
using System.Globalization;
using System.Reflection;

using KofComparer.Clients;
using KofComparer.Configuration;
using KofComparer.Models;

using Microsoft.Extensions.Configuration;

const string KoFuViDumpFile = "varsladr_oppdatert_foer_2026-11-04.csv";
const string DifferencesFile = "kof-differences.csv";

// The dump holds every KoFuVi change before this instant. Profile changes at or after it are
// newer than the dump and therefore not interesting for the comparison.
DateTime dumpCutoffUtc = new(2026, 11, 4, 12, 0, 0, DateTimeKind.Utc);

var builder = new ConfigurationBuilder()
    .AddJsonFile($"appsettings.json", true, true)
    .AddUserSecrets(Assembly.GetExecutingAssembly());
var config = builder.Build();

ProfileDbSettings postgreSqlSettings = new();
var dbSettingsSection = config.GetRequiredSection("ProfileDbSettings");
dbSettingsSection.Bind(postgreSqlSettings);

Stopwatch stopwatch = Stopwatch.StartNew();

ProfileClient profileClient = new(postgreSqlSettings.ConnectionString, postgreSqlSettings.CommandTimeoutSeconds);

Console.WriteLine("Loading notification addresses from Profile...");
List<ProfileOrgAddress> profileOrgAddresses = await profileClient.GetProfileOrgAddressesAsync();
Console.WriteLine($"  {profileOrgAddresses.Count:N0} rows loaded in {stopwatch.Elapsed.TotalSeconds:N1} s");

// Indexed on registry id alone: it is unique both in Profile (unique index on registry_id) and
// in the KoFuVi dump, and it is the identity the sync itself matches on. Matched ids are removed
// while streaming the dump, so whatever is left afterwards is only present in Profile.
Dictionary<string, ProfileOrgAddress> unmatchedProfileAddresses = new(profileOrgAddresses.Count, StringComparer.Ordinal);
foreach (ProfileOrgAddress profileOrgAddress in profileOrgAddresses)
{
    unmatchedProfileAddresses.Add(profileOrgAddress.RegistryID, profileOrgAddress);
}

profileOrgAddresses = [];

Console.WriteLine($"Comparing against {KoFuViDumpFile}...");

List<RowDifference> differences = [];
long koFuViRowCount = 0;
long malformedLineCount = 0;
long matchingCount = 0;
long deletedAndAbsentCount = 0;
long changedAfterCutoffCount = 0;

await foreach (KoFuViOrgAddress koFuVi in KoFuViOrgAddressLoader.Load(KoFuViDumpFile, ReportMalformedLine))
{
    koFuViRowCount++;

    if (!unmatchedProfileAddresses.Remove(koFuVi.RegistryID, out ProfileOrgAddress? profile))
    {
        if (koFuVi.IsDelete)
        {
            // KoFuVi keeps deleted records, Profile removes them. This is the expected state.
            deletedAndAbsentCount++;
        }
        else
        {
            differences.Add(new RowDifference(DifferenceKind.MissingInProfile, koFuVi.RegistryID, koFuVi.IsDelete, koFuVi.RegistryLastUpdatedUtc, null));
        }

        continue;
    }

    DateTime? profileUpdatedUtc = profile.RegistryUpdatedDateTime;

    if (profileUpdatedUtc >= dumpCutoffUtc)
    {
        changedAfterCutoffCount++;
        continue;
    }

    if (koFuVi.IsDelete)
    {
        differences.Add(new RowDifference(DifferenceKind.DeleteNotApplied, koFuVi.RegistryID, koFuVi.IsDelete, koFuVi.RegistryLastUpdatedUtc, profileUpdatedUtc));
    }
    else if (!TimestampsMatch(koFuVi, profileUpdatedUtc))
    {
        differences.Add(new RowDifference(DifferenceKind.TimestampMismatch, koFuVi.RegistryID, koFuVi.IsDelete, koFuVi.RegistryLastUpdatedUtc, profileUpdatedUtc));
    }
    else
    {
        matchingCount++;
    }
}

Console.WriteLine($"  {koFuViRowCount:N0} rows read in {stopwatch.Elapsed.TotalSeconds:N1} s");
if (malformedLineCount > 0)
{
    Console.WriteLine($"  {malformedLineCount:N0} malformed lines skipped");
}

foreach (ProfileOrgAddress profile in unmatchedProfileAddresses.Values)
{
    if (profile.RegistryUpdatedDateTime is null)
    {
        differences.Add(new RowDifference(DifferenceKind.ExtraInProfileWithoutTimestamp, profile.RegistryID, null, null, null));
    }
    else if (profile.RegistryUpdatedDateTime >= dumpCutoffUtc)
    {
        changedAfterCutoffCount++;
    }
    else
    {
        differences.Add(new RowDifference(DifferenceKind.ExtraInProfile, profile.RegistryID, null, null, profile.RegistryUpdatedDateTime));
    }
}

differences.Sort((a, b) =>
{
    int byKind = a.Kind.CompareTo(b.Kind);
    return byKind != 0 ? byKind : Nullable.Compare(a.SortTimestamp, b.SortTimestamp);
});

int writtenRowCount = await RowDifferenceWriter.WriteAsync(DifferencesFile, differences);

Console.WriteLine();
Console.WriteLine($"Wrote {writtenRowCount:N0} differing rows to {Path.GetFullPath(DifferencesFile)}");

PrintSummary(differences, koFuViRowCount, matchingCount, deletedAndAbsentCount, changedAfterCutoffCount, dumpCutoffUtc);

Console.WriteLine();
Console.WriteLine($"Done in {stopwatch.Elapsed.TotalSeconds:N1} s.");

void ReportMalformedLine(long lineNumber, string line)
{
    malformedLineCount++;

    if (malformedLineCount <= 10)
    {
        Console.WriteLine($"  Skipped malformed line {lineNumber}: {line}");
    }
}

// Timestamps from the repeated hour at the end of daylight saving time cannot be resolved from
// the dump alone, so for those both readings are accepted.
static bool TimestampsMatch(KoFuViOrgAddress koFuVi, DateTime? profileUpdatedUtc)
{
    if (profileUpdatedUtc is not DateTime profileUtc)
    {
        return false;
    }

    TimeSpan delta = profileUtc - koFuVi.RegistryLastUpdatedUtc;
    return delta == TimeSpan.Zero || (koFuVi.IsAmbiguousLocalTime && delta.Duration() == TimeSpan.FromHours(1));
}

static void PrintSummary(
    List<RowDifference> differences,
    long koFuViRowCount,
    long matchingCount,
    long deletedAndAbsentCount,
    long changedAfterCutoffCount,
    DateTime dumpCutoffUtc)
{
    Console.WriteLine();
    Console.WriteLine("=== Overview ===");
    Console.WriteLine($"  {"KoFuVi rows",-40} {koFuViRowCount,12:N0}");
    Console.WriteLine($"  {"Matching",-40} {matchingCount,12:N0}");
    Console.WriteLine($"  {"Deleted in KoFuVi, absent in Profile",-40} {deletedAndAbsentCount,12:N0}  (expected, ignored)");
    Console.WriteLine($"  {$"Changed in Profile after {dumpCutoffUtc:u}",-40} {changedAfterCutoffCount,12:N0}  (ignored)");

    var byKind = differences.GroupBy(d => d.Kind).ToDictionary(g => g.Key, g => g.ToList());
    foreach (DifferenceKind kind in Enum.GetValues<DifferenceKind>())
    {
        int count = byKind.TryGetValue(kind, out var rows) ? rows.Count : 0;
        Console.WriteLine($"  {kind,-40} {count,12:N0}");
    }

    foreach ((DifferenceKind kind, List<RowDifference> rows) in byKind.OrderBy(kv => kv.Key))
    {
        PrintKindDetails(kind, rows);
    }
}

static void PrintKindDetails(DifferenceKind kind, List<RowDifference> rows)
{
    Console.WriteLine();
    Console.WriteLine($"=== {kind} ({rows.Count:N0}) ===");

    List<DateTime> timestamps = [.. rows.Where(r => r.SortTimestamp.HasValue).Select(r => r.SortTimestamp!.Value)];
    if (timestamps.Count > 0)
    {
        Console.WriteLine($"Oldest: {timestamps.Min():u}    Newest: {timestamps.Max():u}");

        Console.WriteLine();
        Console.WriteLine("By year of last registry update:");
        foreach (var group in timestamps.GroupBy(t => t.Year).OrderBy(g => g.Key))
        {
            Console.WriteLine($"  {group.Key,-20} {group.Count(),10:N0}");
        }

        Console.WriteLine();
        Console.WriteLine("Ten heaviest months:");
        var byMonth = timestamps
            .GroupBy(t => new DateTime(t.Year, t.Month, 1))
            .OrderByDescending(g => g.Count())
            .Take(10);
        foreach (var group in byMonth)
        {
            Console.WriteLine($"  {group.Key.ToString("yyyy-MM", CultureInfo.InvariantCulture),-20} {group.Count(),10:N0}");
        }
    }

    if (kind == DifferenceKind.TimestampMismatch)
    {
        PrintDeltaHistogram(rows);
    }
}

// Profile minus KoFuVi. Exact whole-hour offsets are split by whether KoFuVi's timestamp falls in
// Norwegian summer time, which shows whether the dump's time zone has been interpreted correctly.
static void PrintDeltaHistogram(List<RowDifference> rows)
{
    Console.WriteLine();
    Console.WriteLine("Profile minus KoFuVi timestamp:");

    var buckets = rows
        .GroupBy(DeltaBucket)
        .OrderByDescending(g => g.Count());
    foreach (var group in buckets)
    {
        Console.WriteLine($"  {group.Key,-40} {group.Count(),10:N0}");
    }
}

static string DeltaBucket(RowDifference row)
{
    if (row.ProfileUpdatedUtc is not DateTime profileUtc || row.KoFuViUpdatedUtc is not DateTime koFuViUtc)
    {
        return "no timestamp in Profile";
    }

    TimeSpan delta = profileUtc - koFuViUtc;
    string sign = delta < TimeSpan.Zero ? "-" : "+";
    TimeSpan size = delta.Duration();

    if (size.Ticks % TimeSpan.TicksPerHour == 0)
    {
        string season = KoFuViOrgAddressLoader.DumpTimeZone.IsDaylightSavingTime(koFuViUtc) ? "summer" : "winter";
        return $"exactly {sign}{size.TotalHours:N0}h ({season})";
    }

    string magnitude = size switch
    {
        _ when size < TimeSpan.FromMilliseconds(1) => "< 1 ms",
        _ when size < TimeSpan.FromSeconds(1) => "< 1 s",
        _ when size < TimeSpan.FromMinutes(1) => "< 1 min",
        _ when size < TimeSpan.FromHours(1) => "< 1 h",
        _ when size < TimeSpan.FromDays(1) => "< 1 day",
        _ => ">= 1 day",
    };

    return $"{sign} {magnitude}";
}
