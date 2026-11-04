using System.Globalization;

namespace ResourceCleaner.Clients;

public static class FileWriter
{
    /// <summary>
    /// Writes the given resources to <paramref name="path"/>, replacing any existing file.
    /// </summary>
    /// <returns>The number of rows written.</returns>
    public static async Task<int> WriteAsyncWithLeadStr(string env, string path, string leadStr, IEnumerable<string> stringLinesToWrite)
    {
        int written = 0;

        await using StreamWriter writer = new($"results/{env}/{path}", append: false);
        await writer.WriteLineAsync(leadStr);
        foreach (string s in stringLinesToWrite)
        {
            await writer.WriteLineAsync(s);
            written++;
        }

        return written;
    }

    public static async Task<int> WriteAsync(string env, string path, IEnumerable<string> stringLinesToWrite)
    {
        int written = 0;

        await using StreamWriter writer = new($"results/{env}/{path}", append: false);
        foreach (string s in stringLinesToWrite)
        {
            await writer.WriteLineAsync(s);
            written++;
        }

        return written;
    }
}