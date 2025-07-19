using FikaServer.Models.Fika.Config;
using FikaServer.Models.Fika.Routes.Client.Check;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;

namespace FikaServer.Services
{
    [Injectable(InjectionType.Singleton)]
    public class ClientService(ISptLogger<ClientService> logger, SaveServer saveServer,
        ClientModHashesService fikaClientModHashesService, ConfigService fikaConfig)
    {
        private readonly List<string> _requiredMods = ["com.fika.core", "com.SPT.custom", "com.SPT.singleplayer", "com.SPT.core", "com.SPT.debugging"];
        private readonly List<string> _allowedMods = ["com.bepis.bepinex.configurationmanager", "com.fika.headless"];
        private bool _hasRequiredOrOptionalMods = false;

        public void OnPreLoad()
        {
            FikaConfig config = fikaConfig.Config;

            List<string> sanitizedRequiredMods = FilterEmptyMods(config.Client.Mods.Required);
            List<string> sanitizedOptionalMods = FilterEmptyMods(config.Client.Mods.Optional);

            if (sanitizedRequiredMods.Count == 0 && sanitizedOptionalMods.Count == 0)
            {
                _hasRequiredOrOptionalMods = false;
            }

            foreach (string mod in sanitizedRequiredMods)
            {
                _requiredMods.Add(mod);
                _allowedMods.Add(mod);
            }

            foreach (string mod in sanitizedOptionalMods)
            {
                _allowedMods.Add(mod);
            }
        }

        protected List<string> FilterEmptyMods(List<string> list)
        {
            return [.. list.Where(str => !string.IsNullOrWhiteSpace(str))];
        }

        public FikaConfigClient GetClientConfig()
        {
            return fikaConfig.Config.Client;
        }

        public bool GetIsItemSendingAllowed()
        {
            return fikaConfig.Config.Server.AllowItemSending;
        }

        public FikaConfigNatPunchServer GetNatPunchServerConfig()
        {
            return fikaConfig.Config.NatPunchServer;
        }

        public VersionCheckResponse GetVersion()
        {
            return new VersionCheckResponse
            {
                Version = fikaConfig.Version
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
            if (!_hasRequiredOrOptionalMods)
            {
                return mismatchedMods;
            }

            // check for missing required mods first
            foreach (string pluginId in _requiredMods)
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
                if (!_allowedMods.Contains(pluginId))
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

        public SptProfile? GetProfileBySessionID(MongoId sessionId)
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
