using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace MageSim.Infrastructure.Config
{
    public sealed class ConfigService
    {
        private readonly string _path;
        public ConfigService(string path) => _path = path;

        public async Task<RootConfig> LoadAsync()
        {
            string json;
            using (var reader = new StreamReader(_path, Encoding.UTF8))
            {
                json = await reader.ReadToEndAsync();
            }

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<RootConfig>(json, options) ?? new RootConfig();
        }

        public async Task SaveAsync(RootConfig config)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(config, options);

            using (var writer = new StreamWriter(_path, false, Encoding.UTF8))
            {
                await writer.WriteAsync(json);
            }
        }
    }
}