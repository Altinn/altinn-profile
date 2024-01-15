using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Altinn.Profile.Tests.Testdata
{
    public static class TestDataLoader
    {
        public static async Task<T> Load<T>(string id)
        {
            string path = $"../../../Testdata/{typeof(T).Name}/{id}.json";
            string fileContent = await File.ReadAllTextAsync(path);
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            T data = JsonSerializer.Deserialize<T>(fileContent, options);
            return data;
        }
    }
}
