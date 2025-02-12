using Core.Models.External;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Routers;
using Core.Servers;
using FikaServer.Models.Fika.Config;
using FikaServer.Services;
using FikaServer.Services.Cache;
using SptCommon.Annotations;

namespace FikaServer
{
    [Injectable(InjectionType.Singleton, InjectableTypeOverride = typeof(IPreSptLoadMod))]
    [Injectable(InjectionType.Singleton, InjectableTypeOverride = typeof(IPostSptLoadMod))]
    public class FikaServer(ConfigServer configServer, ImageRouter imageRouter,
        HeadlessProfileService HeadlessProfileService, PlayerRelationsCacheService playerRelationsCacheService, Utils.Config fikaConfig) : IPreSptLoadMod, IPostSptLoadMod
    {
        public void PreSptLoad()
        {
            fikaConfig.PreSptLoad();
            playerRelationsCacheService.PreSptLoad();
        }

        public void PostSptLoad()
        {
            FikaConfig config = fikaConfig.GetConfig();

            if(config.Headless.Profiles.Amount > 0)
            {
                HeadlessProfileService.PostSptLoad();
            }

            AddFikaClientLocales();
            BlacklistSpecialProfiles();
            playerRelationsCacheService.PostSptLoad();

            if (config.Background.Enable)
            {
                string imagePath = "assets/images/launcher/bg.png";
                imageRouter.AddRoute("/files/launcher/bg", Path.Join(fikaConfig.GetModPath(), imagePath));
            }
        }

        private void AddFikaClientLocales()
        {
            //Todo: Need to implement, not currently available in C# SPT.
        }

        private void BlacklistSpecialProfiles()
        {
            CoreConfig coreConfig = configServer.GetConfig<CoreConfig>();
            List<string> profileBlacklist = coreConfig.Features.CreateNewProfileTypesBlacklist;

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
