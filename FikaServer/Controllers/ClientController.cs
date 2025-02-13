using Core.Models.Eft.Profile;
using FikaServer.Models.Fika.Config;
using FikaServer.Models.Fika.Routes.Client.Check;
using FikaServer.Services;
using SptCommon.Annotations;

namespace FikaServer.Controllers
{
    [Injectable(InjectionType.Transient)]
    public class ClientController(ClientService fikaClientService)
    {
        public FikaConfigClient HandleClientConfig()
        {
            FikaConfigClient clientConfig = fikaClientService.GetClientConfig();

            //Todo: implement item sending here, maybe get rid of the scuff and just ship it with both?

            return clientConfig;
        }

        /// <summary>
        /// Handle /fika/natpunchserver/config
        /// </summary>
        public FikaConfigNatPunchServer HandleNatPunchServerConfig()
        {
            return fikaClientService.GetNatPunchServerConfig();
        }

        /// <summary>
        /// Handle /fika/client/check/mods
        /// </summary>
        public FikaCheckModResponse HandleCheckMods(Dictionary<string, int> request)
        {
            return fikaClientService.GetCheckModsResponse(request);
        }

        /// <summary>
        /// Handle /fika/client/check/mods
        /// </summary>
        public SptProfile HandleProfileDownload(string sessionId)
        {
            return fikaClientService.GetProfileBySessionID(sessionId);
        }

        /// <summary>
        /// Handle /fika/client/check/version
        /// </summary>
        public VersionCheckResponse HandleVersionCheck()
        {
            return fikaClientService.GetVersion();
        }
    }
}
