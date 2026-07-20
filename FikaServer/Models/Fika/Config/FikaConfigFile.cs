using SPTarkov.Server.Core.Utils.Json.Converters;
using System.Text.Json;

namespace FikaServer.Models.Fika.Config;

public static class FikaConfigFile
{
    private static readonly JsonSerializerOptions _options = new()
    {
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        WriteIndented = true,
        Converters = { new StringToMongoIdConverter() }
    };

    public static async Task<FikaServerConfig> LoadAsync(string configPath)
    {
        if (!File.Exists(configPath))
        {
            return new();
        }

        await using FileStream fs = new(configPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);

        return await JsonSerializer.DeserializeAsync<FikaServerConfig>(fs, _options) ?? new();
    }

    public static async Task SaveAsync(FikaServerConfig config, string configPath)
    {
        await File.WriteAllTextAsync(configPath, JsonSerializer.Serialize(config, _options));
    }
}
