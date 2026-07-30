using FikaServer.Models.Fika.Config;
using SPTarkov.Server.Core.DI;
using System.Reflection;
using System.Security.Cryptography;

namespace FikaServer.OnLoad;

public sealed class FikaDIConstruct : IOnDIConstruct
{
    public static async Task OnDIConstructAsync(IServiceCollection serviceCollection, CancellationToken cancellationToken)
    {
        FikaPaths paths = new(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!);

        Directory.CreateDirectory(paths.ConfigsPath);

        FikaServerConfig config = await FikaConfigFile.LoadAsync(paths.ConfigFilePath);

        if (string.IsNullOrEmpty(config.Server.ApiKey))
        {
            config.Server.ApiKey = await GenerateAPIKey();
        }

        EnforceReviveValues(config);

        // No need to do any fancyness around sorting properties and writing them if they weren't set before here
        // We store default values in the config models, and if one is missing this will write it to the file in the correct place
        await FikaConfigFile.SaveAsync(config, paths.ConfigFilePath);

#if DEBUG
        config.Client.AllowFreeCam = true;
        config.Server.ShowDevProfile = true;
#endif

        serviceCollection.AddSingleton(paths);
        serviceCollection.AddSingleton(config);
    }

    private static void EnforceReviveValues(FikaServerConfig config)
    {
        if (Math.Abs(config.Client.ReviveConfig.BleedoutTime) >= float.Epsilon)
        {
            config.Client.ReviveConfig.BleedoutTime = Math.Clamp(config.Client.ReviveConfig.BleedoutTime, 10f, 600f);
        }
        config.Client.ReviveConfig.MaxRevives = Math.Clamp(config.Client.ReviveConfig.MaxRevives, 0, 10);
        config.Client.ReviveConfig.ReviveTime = Math.Clamp(config.Client.ReviveConfig.ReviveTime, 3f, 30f);
    }

    private static Task<string> GenerateAPIKey(int size = 32)
    {
        var keyBytes = RandomNumberGenerator.GetBytes(size);
        return Task.FromResult(Convert.ToBase64String(keyBytes)
                     .Replace("+", "")
                     .Replace("/", "")
                     .Replace("=", ""));
    }
}
