using Core.Models.Eft.Profile;
using Core.Models.Utils;
using Core.Servers;
using FikaServer.Models.Fika.Config;
using FikaServer.Models.Fika.Routes.Client.Check;
using FikaServer.Utils;
using SptCommon.Annotations;

namespace FikaServer.Services
{
    [Injectable(InjectionType.Singleton)]
    public class ClientService(ISptLogger<ClientService> logger, SaveServer saveServer, ClientModHashesService fikaClientModHashesService, Config fikaConfig)
    {
        private List<string> requiredMods = ["com.fika.core", "com.SPT.custom", "com.SPT.singleplayer", "com.SPT.core", "com.SPT.debugging"];
        private List<string> allowedMods = ["com.bepis.bepinex.configurationmanager", "com.fika.headless"];
        private bool hasRequiredOrOptionalMods = false;


        public void PreSptLoad()
        {
            FikaConfig config = fikaConfig.GetConfig();

            List<string> sanitizedRequiredMods = FilterEmptyMods(config.Client.Mods.Required);
            List<string> sanitizedOptionalMods = FilterEmptyMods(config.Client.Mods.Optional);

            if (sanitizedRequiredMods.Count == 0 && sanitizedOptionalMods.Count == 0)
            {
                hasRequiredOrOptionalMods = false;
            }

            foreach (string mod in sanitizedRequiredMods)
            {
                requiredMods.Add(mod);
                allowedMods.Add(mod);
            }

            foreach (string mod in sanitizedOptionalMods)
            {
                allowedMods.Add(mod);
            }
        }

        protected List<string> FilterEmptyMods(List<string> list)
        {
            return list.Where(str => !string.IsNullOrWhiteSpace(str)).ToList();
        }

        public FikaConfigClient GetClientConfig()
        {
            return fikaConfig.GetConfig().Client;
        }

        public bool GetIsItemSendingAllowed()
        {
            return fikaConfig.GetConfig().Server.AllowItemSending;
        }

        public FikaConfigNatPunchServer GetNatPunchServerConfig()
        {
            return fikaConfig.GetConfig().NatPunchServer;
        }

        public VersionCheckResponse GetVersion()
        {
            return new VersionCheckResponse
            {
                Version = fikaConfig.GetVersion()
            };
        }

        public FikaCheckModResponse GetCheckModsResponse(FikaCheckModRequestData request)
        {
            FikaCheckModResponse mismatchedMods = new()
            {
                Forbidden = [],
                MissingRequired = [],
                HashMismatch = []
            };

            // if no configuration was made, allow all mods
            if (!hasRequiredOrOptionalMods)
            {
                return mismatchedMods;
            }

            // check for missing required mods first
            foreach (string pluginId in requiredMods)
            {
                if (!request.ContainsKey(pluginId))
                {
                    mismatchedMods.MissingRequired.Add(pluginId);
                }
            }

            // no need to check anything else since it's missing required mods
            if (mismatchedMods.MissingRequired.Count > 0)
            {
                return mismatchedMods;
            }

            foreach (string pluginId in request.Keys)
            {
                int hash = request[pluginId];

                // check if the mod isn't allowed
                if (!allowedMods.Contains(pluginId))
                {
                    mismatchedMods.Forbidden.Add(pluginId);
                    continue;
                }

                // first request made will fill in at the very least all the required mods hashes, following requests made by different clients will add any optional mod not added by the first request, otherwise will check against the first request data
                if (!fikaClientModHashesService.Exists(pluginId))
                {
                    fikaClientModHashesService.AddHash(pluginId, hash);
                    continue;
                }

                if (fikaClientModHashesService.GetHash(pluginId) != hash)
                {
                    mismatchedMods.HashMismatch.Add(pluginId);
                }
            }

            return mismatchedMods;
        }

        public SptProfile? GetProfileBySessionID(string sessionId)
        {
            SptProfile profile = saveServer.GetProfile(sessionId);

            if (profile != null)
            {
                logger.Info($"{sessionId} has downloaded their profile");
                return profile;
            }

            logger.Info($"{sessionId} wants to download their profile but we don't have it");
            return null;
        }
    }
}
