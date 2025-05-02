using FikaServer.Models.Enums;
using FikaServer.Models.Fika.Config;
using FikaServer.Services;
using FikaServer.Services.Cache;
using FikaServer.Services.Headless;
using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Models.External;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Server;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Json.Converters;

namespace FikaServer
{
    [Injectable(InjectionType.Singleton, InjectableTypeOverride = typeof(IPreSptLoadMod))]
    [Injectable(InjectionType.Singleton, InjectableTypeOverride = typeof(IPostSptLoadMod))]
    public class FikaServer(ISptLogger<FikaServer> logger, ConfigServer configServer, ImageRouter imageRouter,
        HeadlessProfileService HeadlessProfileService, LocaleService localeService, PlayerRelationsCacheService playerRelationsCacheService, ClientService clientService, JsonUtil jsonUtil, Utils.Config fikaConfig) : IPreSptLoadMod, IPostSptLoadMod
    {
        public void PreSptLoad()
        {
            jsonUtil.RegisterJsonConverter(new EftEnumConverter<EFikaSide>());
            jsonUtil.RegisterJsonConverter(new EftEnumConverter<EFikaTime>());
            jsonUtil.RegisterJsonConverter(new EftEnumConverter<EFikaMatchStatus>());
            jsonUtil.RegisterJsonConverter(new EftEnumConverter<EFikaPlayerPresences>());
            jsonUtil.RegisterJsonConverter(new EftEnumConverter<EFikaNotifications>());
            jsonUtil.RegisterJsonConverter(new EftEnumConverter<EEFTNotificationIconType>());

            fikaConfig.PreSptLoad();
            clientService.PreSptLoad();
            playerRelationsCacheService.PreSptLoad();
        }

        public void PostSptLoad()
        {
            FikaConfig config = fikaConfig.GetConfig();

            if (config.Headless.Profiles.Amount > 0)
            {
                HeadlessProfileService.PostSptLoad();
            }

            localeService.PostSptLoad();
            BlacklistSpecialProfiles();
            playerRelationsCacheService.PostSptLoad();

            if (config.Background.Enable)
            {
                string imagePath = "assets/images/launcher/bg.png";
                imageRouter.AddRoute("/files/launcher/bg", Path.Join(fikaConfig.GetModPath(), imagePath));
            }
        }

        private void BlacklistSpecialProfiles()
        {
            CoreConfig coreConfig = configServer.GetConfig<CoreConfig>();
            HashSet<string> profileBlacklist = coreConfig.Features.CreateNewProfileTypesBlacklist;

            if (!fikaConfig.GetConfig().Server.ShowDevProfile)
            {
                profileBlacklist.Add("SPT Developer");
            }

            if (!fikaConfig.GetConfig().Server.ShowNonStandardProfile)
            {
                List<string> disallowedProfiles = ["Tournament", "SPT Easy start", "SPT Zero to hero"];

                foreach (string profile in disallowedProfiles)
                {
                    profileBlacklist.Add(profile);
                }
            }
        }
    }
}
