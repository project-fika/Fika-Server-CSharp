using Core.Models.Eft.Profile;
using FikaServer.Models.Fika.Config;
using FikaServer.Models.Fika.Routes.Client.Check;
using FikaServer.Services;
using FikaServer.Utils;
using SptCommon.Annotations;
using System.Dynamic;

namespace FikaServer.Controllers
{
    [Injectable]
    public class ClientController(ClientService fikaClientService)
    {
        /// <summary>
        /// Handle /fika/client/config
        /// </summary>
        public ExpandoObject HandleClientConfig()
        {
            ExpandoObject clientConfig = fikaClientService.GetClientConfig().ToDynamicObject();

            //Todo: Maybe get rid of the scuff ExpandoObject and just ship it with both?

            clientConfig.AddOrReplaceProperty("allowItemSending", fikaClientService.GetIsItemSendingAllowed());

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
        public FikaCheckModResponse HandleCheckMods(FikaCheckModRequestData request)
        {
            return fikaClientService.GetCheckModsResponse(request);
        }

        /// <summary>
        /// Handle /fika/client/check/mods
        /// </summary>
        public SptProfile? HandleProfileDownload(string sessionId)
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
