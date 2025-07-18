using FikaServer.Models.Fika.Presence;
using FikaServer.Services;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;

namespace FikaServer.Callbacks
{
    [Injectable]
    public class PresenceCallbacks(HttpResponseUtil httpResponseUtil, PresenceService fikaPresenceService)
    {
        /// <summary>
        /// Handle /fika/presence/get
        /// </summary>
        public ValueTask<string> HandleGetPresence(string url, IRequestData info, MongoId sessionID)
        {
            return new ValueTask<string>(httpResponseUtil.NoBody(fikaPresenceService.GetAllPlayersPresence()));
        }

        /// <summary>
        /// Handle /fika/presence/set
        /// </summary>
        public ValueTask<string> HandleSetPresence(string url, FikaSetPresence info, MongoId sessionID)
        {
            fikaPresenceService.UpdatePlayerPresence(sessionID, info);

            return new ValueTask<string>(httpResponseUtil.NullResponse());
        }

        /// <summary>
        /// Handle /fika/presence/setget
        /// </summary>
        public ValueTask<string> HandleSetGetPresence(string url, FikaSetPresence info, MongoId sessionID)
        {
            fikaPresenceService.UpdatePlayerPresence(sessionID, info);

            return new ValueTask<string>(httpResponseUtil.NoBody(fikaPresenceService.GetAllPlayersPresence()));
        }
    }
}
