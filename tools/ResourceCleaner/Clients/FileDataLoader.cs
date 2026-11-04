using System.Text.Json;

using ResourceCleaner.Models;

namespace ResourceCleaner.Clients;

public static class FileDataLoader
{
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task<List<ServiceResource>> LoadResourcesOfType(string type)
    {
        string path = $"./resourcelist.{type}.json";
        string fileContent = await File.ReadAllTextAsync(path);

        List<ServiceResource> data = JsonSerializer.Deserialize<List<ServiceResource>>(fileContent, _options)!;
        return data;
    }

    public static async Task<List<Mapping>> LoadSafeMappingResults(string env)
    {
        string path = $"./results/{env}/profile-safe-mapping-candidates.txt";
        string[] lines = await File.ReadAllLinesAsync(path);

        List<Mapping> resultBuilder = [];


        foreach (var line in lines)
        {
            string[] parts = line.Split(" ");
            string a2ServiceCode = parts[0];
            string mappingCandidates = parts[1];

            List<string> candidates = [.. mappingCandidates.Split(",")];
            resultBuilder.Add(new Mapping { A2ServiceCode = a2ServiceCode, SafeMappingCandidates = candidates });
        }

        return resultBuilder;
    }

}