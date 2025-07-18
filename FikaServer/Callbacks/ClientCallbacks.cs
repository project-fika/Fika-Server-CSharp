using FikaServer.Controllers;
using FikaServer.Models.Fika.Routes.Client.Check;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
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
        public ValueTask<string> HandleClientConfig(string url, IRequestData info, MongoId sessionID)
        {
            //return httpResponseUtil.NoBody(fikaClientController.HandleClientConfig());
            return new ValueTask<string>(fikaClientController.HandleClientConfig());
        }

        /// <summary>
        /// Handle /fika/natpunchserver/config
        /// </summary>
        public ValueTask<string> HandleNatPunchConfig(string url, IRequestData info, MongoId sessionID)
        {
            return new ValueTask<string>(httpResponseUtil.NoBody(fikaClientController.HandleNatPunchServerConfig()));
        }

        /// <summary>
        /// Handle /fika/client/check/mods
        /// </summary>
        public ValueTask<string> HandleCheckMods(string url, FikaCheckModRequestData info, MongoId sessionID)
        {
            return new ValueTask<string>(httpResponseUtil.NoBody(fikaClientController.HandleCheckMods(info)));
        }

        /// <summary>
        /// Handle /fika/profile/download
        /// </summary>
        public ValueTask<string> HandleProfileDownload(string url, IRequestData info, MongoId sessionID)
        {
            return new ValueTask<string>(httpResponseUtil.NoBody(fikaClientController.HandleProfileDownload(sessionID)));
        }

        /// <summary>
        /// Handle /fika/client/check/version
        /// </summary>
        public ValueTask<string> HandleVersionCheck(string url, IRequestData info, MongoId sessionID)
        {
            return new ValueTask<string>(httpResponseUtil.NoBody(fikaClientController.HandleVersionCheck()));
        }
    }
}
