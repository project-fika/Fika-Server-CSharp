using FikaServer.Models.Fika.Config;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using System.Reflection;
using System.Security.Cryptography;

namespace FikaServer.Services;

[Injectable(InjectionType.Singleton)]
public class ConfigService(ISptLogger<ConfigService> logger, ConfigServer configServer,
    ModHelper modHelper, JsonUtil jsonUtil)
{
    public FikaConfig Config { get; private set; } = new();
    private readonly string _configFolderPath = Path.Join(modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly()), "assets/configs");
    private readonly FikaModMetadata _fikaModMetaData = new();

    public string ModPath
    {
        get
        {
            return modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        }
    }

    public string? Version
    {
        get
        {
            return _fikaModMetaData.Version.ToString();
        }
    }

    public async Task OnWebAppBuildAsync()
    {
        //This is debug, probably wont exist in the final release.
        if (!Directory.Exists(_configFolderPath))
        {
            Directory.CreateDirectory(_configFolderPath);
        }

        string configPath = Path.Combine(_configFolderPath, "fika.jsonc");

        Config = await jsonUtil.DeserializeFromFileAsync<FikaConfig>(configPath) ?? new();

        if (string.IsNullOrEmpty(Config.Server.ApiKey))
        {
            Config.Server.ApiKey = await GenerateAPIKey();
        }

        // No need to do any fancyness around sorting properties and writing them if they weren't set before here
        // We store default values in the config models, and if one is missing this will write it to the file in the correct place
        await SaveConfig();

#if DEBUG
        Config.Client.AllowFreeCam = true;
        Config.Server.ShowDevProfile = true;
#endif

        ApplySPTConfig(Config.Server.SPT);
    }

    private static Task<string> GenerateAPIKey(int size = 32)
    {
        byte[] keyBytes = RandomNumberGenerator.GetBytes(size);
        return Task.FromResult(Convert.ToBase64String(keyBytes)
                     .Replace("+", "")
                     .Replace("/", "")
                     .Replace("=", ""));
    }

    public async Task SaveConfig()
    {
        await File.WriteAllTextAsync($"{_configFolderPath}/fika.jsonc", jsonUtil.Serialize(Config, true));
    }

    private void ApplySPTConfig(FikaSPTServerConfig config)
    {
        logger.Info("[Fika Server] Overriding SPT configuration");

        CoreConfig coreConfig = configServer.GetConfig<CoreConfig>();
        HttpConfig httpConfig = configServer.GetConfig<HttpConfig>();

        if (config.DisableSPTChatBots)
        {
            string commandoId = coreConfig.Features.ChatbotFeatures.Ids["commando"];
            string sptFriendId = coreConfig.Features.ChatbotFeatures.Ids["spt"];

            coreConfig.Features.ChatbotFeatures.EnabledBots[commandoId] = false;
            coreConfig.Features.ChatbotFeatures.EnabledBots[sptFriendId] = false;
        }

        httpConfig.Ip = config.Http.Ip;
        httpConfig.Port = config.Http.Port;
        httpConfig.BackendIp = config.Http.BackendIp;
        httpConfig.BackendPort = config.Http.BackendPort;
    }
}
