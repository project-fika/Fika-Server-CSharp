using FikaServer.Models.Fika.Config;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using System.Reflection;
using System.Text.Json;

namespace FikaServer.Services
{
    [Injectable(InjectionType.Singleton)]
    public class ConfigService(ISptLogger<ConfigService> logger, ConfigServer configServer,
        ModHelper modHelper, JsonUtil jsonUtil)
    {
        private readonly string _configFolderPath = Path.Join(modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly()), "assets/configs");
        public FikaConfig Config { get; private set; } = new();
        private readonly FikaModMetadata _fikaModMetaData = new();
        public static readonly JsonSerializerOptions serializerOptions = new() { WriteIndented = true };

        public string GetModPath()
        {
            return modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        }

        public string? GetVersion()
        {
            return _fikaModMetaData.Version;
        }

        public async Task OnPreLoad()
        {
            //This is debug, probably wont exist in the final release.
            if (!Directory.Exists(_configFolderPath))
            {
                Directory.CreateDirectory(_configFolderPath);
            }

            string configPath = Path.Combine(_configFolderPath, "fika.jsonc");

            Config = await jsonUtil.DeserializeFromFileAsync<FikaConfig>(configPath) ?? new();

            // No need to do any fancyness around sorting properties and writing them if they weren't set before here
            // We store default values in the config models, and if one is missing this will write it to the file in the correct place
            await WriteConfig(_configFolderPath);

#if DEBUG
            Config.Client.AllowFreeCam = true;
            Config.Server.ShowDevProfile = true;
#endif

            ApplySPTConfig(Config.Server.SPT);
        }

        private async Task WriteConfig(string ConfigFolderPath)
        {
            await File.WriteAllTextAsync($"{ConfigFolderPath}/fika.jsonc", JsonSerializer.Serialize(Config, serializerOptions));
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
}
