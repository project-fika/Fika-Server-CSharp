using FikaServer.Controllers;
using FikaServer.Models.Fika.Routes.Client.Check;
using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;

namespace FikaServer.Callbacks
{
    [Injectable]
    public class ClientCallbacks(HttpResponseUtil httpResponseUtil, ClientController fikaClientController)
    {
        /// <summary>
        /// Handle /fika/client/config
        /// </summary>
        public string HandleClientConfig(string url, IRequestData info, string sessionID)
        {
            //return httpResponseUtil.NoBody(fikaClientController.HandleClientConfig());
            return fikaClientController.HandleClientConfig();
        }

        /// <summary>
        /// Handle /fika/natpunchserver/config
        /// </summary>
        public string HandleNatPunchConfig(string url, IRequestData info, string sessionID)
        {
            return httpResponseUtil.NoBody(fikaClientController.HandleNatPunchServerConfig());
        }

        /// <summary>
        /// Handle /fika/client/check/mods
        /// </summary>
        public string HandleCheckMods(string url, FikaCheckModRequestData info, string sessionID)
        {
            return httpResponseUtil.NoBody(fikaClientController.HandleCheckMods(info));
        }

        /// <summary>
        /// Handle /fika/profile/download
        /// </summary>
        public string HandleProfileDownload(string url, IRequestData info, string sessionID)
        {
            return httpResponseUtil.NoBody(fikaClientController.HandleProfileDownload(sessionID));
        }

        /// <summary>
        /// Handle /fika/client/check/version
        /// </summary>
        public string HandleVersionCheck(string url, IRequestData info, string sessionID)
        {
            return httpResponseUtil.NoBody(fikaClientController.HandleVersionCheck());
        }
    }
}
