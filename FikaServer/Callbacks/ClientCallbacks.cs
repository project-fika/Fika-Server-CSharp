using Core.Models.Utils;
using Core.Utils;
using FikaServer.Controllers;
using SptCommon.Annotations;

namespace FikaServer.Callbacks
{
    [Injectable(InjectionType.Transient)]
    public class ClientCallbacks(HttpResponseUtil httpResponseUtil, ClientController fikaClientController)
    {
        /// <summary>
        /// Handle /fika/client/config
        /// </summary>
        public string HandleClientConfig(string url, IRequestData info, string sessionID)
        {
            return httpResponseUtil.NoBody(fikaClientController.HandleClientConfig());
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
        public string HandleCheckMods(string url, Dictionary<string, int> info, string sessionID)
        {
            return httpResponseUtil.NoBody(fikaClientController.HandleCheckMods(info));
        }
    }
}
