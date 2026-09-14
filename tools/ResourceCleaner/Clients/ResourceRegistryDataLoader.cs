using System.Text.Json;

using ResourceCleaner.Models;

namespace ResourceCleaner.Clients;

public static class ResourceRegistryDataLoader
{
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task<List<ServiceResource>> Load(string type)
    {
        string path = $"./resourcelist.{type}.json";
        string fileContent = await File.ReadAllTextAsync(path);

        List<ServiceResource> data = JsonSerializer.Deserialize<List<ServiceResource>>(fileContent, _options)!;
        return data;
    }

}