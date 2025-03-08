using FikaServer.Models.Fika.Config;
using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using System.Reflection;
using System.Text.Json;

namespace FikaServer.Utils
{
    [Injectable(InjectionType.Singleton)]
    public class Config(ISptLogger<Config> logger, ConfigServer configServer,
        ModHelper modHelper)
    {
        private FikaConfig loadedFikaConfig = new();
        private readonly PackageJsonData packageJsonData = modHelper.GetJsonDataFromFile<PackageJsonData>(modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly()), "package.json");
        public static readonly JsonSerializerOptions serializerOptions = new() { WriteIndented = true };

        public FikaConfig GetConfig()
        {
            return loadedFikaConfig;
        }

        public string GetModPath()
        {
            return modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        }

        public string? GetVersion()
        {
            return packageJsonData.Version;
        }

        public void PreSptLoad()
        {
            string configFolderPath = Path.Join(modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly()), "assets/configs");

            //This is debug, probably wont exist in the final release.
            if (!Directory.Exists(configFolderPath))
            {
                Directory.CreateDirectory(configFolderPath);
            }

            if (File.Exists($"{configFolderPath}/fika.jsonc"))
            {
                loadedFikaConfig = modHelper.GetJsonDataFromFile<FikaConfig>(configFolderPath, "fika.jsonc");
            }

            // No need to do any fancyness around sorting properties and writing them if they weren't set before here
            // We store default values in the config models, and if one is missing this will write it to the file in the correct place
            WriteConfig(configFolderPath);

            ApplySPTConfig(loadedFikaConfig.Server.SPT);
        }

        private void WriteConfig(string ConfigFolderPath)
        {
            File.WriteAllText($"{ConfigFolderPath}/fika.jsonc", JsonSerializer.Serialize(loadedFikaConfig, serializerOptions));
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

            httpConfig.Ip = config.HTTP.Ip;
            httpConfig.Port = config.HTTP.Port;
            httpConfig.BackendIp = config.HTTP.BackendIp;
            httpConfig.BackendPort = config.HTTP.BackendPort;
        }
    }
}
