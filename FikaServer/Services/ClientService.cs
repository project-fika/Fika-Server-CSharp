using Core.Models.Eft.Game;
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
    public class ClientService(ISptLogger<ClientService> logger, SaveServer saveServer, Config fikaConfig)
    {
        public void PreSptLoad()
        {
            //todo: stub, implement
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
            var version = fikaConfig.GetVersion();

            return new VersionCheckResponse {
                Version = version
            };
        }

        public FikaCheckModResponse GetCheckModsResponse(Dictionary<string, int> request)
        {
            //todo: stub, implement
            return null;
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
