using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Common.Annotations;
using FikaServer.Models.Fika.Presence;
using FikaServer.Services;

namespace FikaServer.Callbacks
{
    [Injectable]
    public class PresenceCallbacks(HttpResponseUtil httpResponseUtil, PresenceService fikaPresenceService)
    {
        /// <summary>
        /// Handle /fika/presence/get
        /// </summary>
        public string HandleGetPresence(string url, IRequestData info, string sessionID)
        {
            return httpResponseUtil.NoBody(fikaPresenceService.GetAllPlayersPresence());
        }

        /// <summary>
        /// Handle /fika/presence/set
        /// </summary>
        public string HandleSetPresence(string url, FikaSetPresence info, string sessionID)
        {
            fikaPresenceService.UpdatePlayerPresence(sessionID, info);

            return httpResponseUtil.NullResponse();
        }

        /// <summary>
        /// Handle /fika/presence/setget
        /// </summary>
        public string HandleSetGetPresence(string url, FikaSetPresence info, string sessionID)
        {
            fikaPresenceService.UpdatePlayerPresence(sessionID, info);

            return httpResponseUtil.NoBody(fikaPresenceService.GetAllPlayersPresence());
        }
    }
}
