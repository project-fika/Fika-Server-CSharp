using FikaServer.Models.Fika.Headless;
using FikaServer.Services.Headless;
using FikaServer.Utils;
using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using System.Collections.Concurrent;

namespace FikaServer.Helpers
{
    [Injectable]
    public class HeadlessHelper(Config fikaConfig, SaveServer saveServer, ConfigServer configServer, HeadlessService headlessService, HeadlessProfileService headlessProfileService, ISptLogger<HeadlessHelper> logger)
    {
        /// <summary>
        /// Gets all currently logged in headlesses
        /// </summary>
        /// <returns>A ConcurrentDictionary where the key is the sessionID and the value is an IHeadlessClientInfo object</returns>
        public ConcurrentDictionary<string, HeadlessClientInfo> GetHeadlessClients()
        {
            //Todo: Stub for now, implement method.
            return [];
        }

        /// <summary>
        /// Allows for checking if a SessionID is a headless client
        /// </summary>
        /// <param name="sessionId">The sessionID to check</param>
        /// <returns>Returns true if the passed sessionID is a headless, returns false if not.</returns>
        public bool IsHeadlessClient(string sessionId)
        {
            //Todo: Stub for now, implement method.
            return false;
        }

        /// <summary>
        /// Allows for checking if the given headless client is available
        /// </summary>
        /// <param name="headlessSessionID"></param>
        /// <returns>Returns true if it's available, returns false if it isn't available.</returns>
        public bool IsHeadlessClientAvailable(string headlessSessionID)
        {
            //Todo: Stub for now, implement method.
            return false;
        }

        /// <summary>
        /// Gets the requester's username for a headless client if there is any.
        /// </summary>
        /// <param name="HeadlessSessionID"></param>
        /// <returns>The nickname if the headless has been requested by a user, returns null if not.</returns>
        public string? GetRequesterUsername(string HeadlessSessionID)
        {
            //Todo: Stub for now, implement method.
            return null;
        }

        /// <summary>
        /// Gets the alias (If it has been given one) or nickname of the headless client
        /// </summary>
        /// <param name="HeadlessSessionID"></param>
        /// <returns>the alias, or nickname or the headless client.</returns>
        public string GetHeadlessNickname(string HeadlessSessionID)
        {
            //Todo: Stub for now, implement method.
            return string.Empty;
        }

        /// <summary>
        /// Gets all available headless clients
        /// </summary>
        /// <returns>Returns an array of available headless clients</returns>
        public HeadlessAvailableClients[] GetAvailableHeadlessClients()
        {
            //Todo: Stub for now, implement method.
            return [];
        }
    }
}
