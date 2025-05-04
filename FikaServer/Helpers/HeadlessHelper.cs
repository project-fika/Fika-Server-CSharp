using FikaServer.Models.Enums;
using FikaServer.Models.Fika.Config;
using FikaServer.Models.Fika.Headless;
using FikaServer.Services;
using FikaServer.Services.Headless;
using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using System.Collections.Concurrent;

namespace FikaServer.Helpers
{
    [Injectable]
    public class HeadlessHelper(ConfigService fikaConfig, SaveServer saveServer, ConfigServer configServer,
        HeadlessService headlessService, HeadlessProfileService headlessProfileService, ISptLogger<HeadlessHelper> logger)
    {
        /// <summary>
        /// Gets all currently logged in headlesses
        /// </summary>
        /// <returns>A <see cref="ConcurrentDictionary{TKey, TValue}"/> where the key is the sessionID and the value is an IHeadlessClientInfo object</returns>
        public ConcurrentDictionary<string, HeadlessClientInfo> HeadlessClients
        {
            get
            {
                return headlessService.HeadlessClients;
            }
        }

        /// <summary>
        /// Allows for checking if a SessionID is a headless client
        /// </summary>
        /// <param name="sessionId">The sessionID to check</param>
        /// <returns>Returns true if the passed sessionID is a headless, returns false if not.</returns>
        public bool IsHeadlessClient(string sessionId)
        {
            return headlessProfileService.HeadlessProfiles
                .Where(x => x.ProfileInfo?.ProfileId == sessionId)
                .Any();
        }

        /// <summary>
        /// Allows for checking if the given headless client is available
        /// </summary>
        /// <param name="headlessSessionID"></param>
        /// <returns>Returns true if it's available, returns false if it isn't available.</returns>
        public bool IsHeadlessClientAvailable(string headlessSessionID)
        {
            if (headlessService.HeadlessClients.TryGetValue(headlessSessionID, out HeadlessClientInfo? headlessClientInfo))
            {
                return headlessClientInfo.State is EHeadlessStatus.READY;
            }

            return false;
        }

        /// <summary>
        /// Gets the requester's username for a headless client if there is any.
        /// </summary>
        /// <param name="headlessSessionID"></param>
        /// <returns>The nickname if the headless has been requested by a user, returns null if not.</returns>
        public string? GetRequesterUsername(string headlessSessionID)
        {
            if (headlessService.HeadlessClients.TryGetValue(headlessSessionID, out HeadlessClientInfo? headlessClientInfo))
            {
                if (string.IsNullOrEmpty(headlessClientInfo.RequesterSessionID))
                {
                    return null;
                }

                string? nickname = saveServer.GetProfile(headlessClientInfo.RequesterSessionID).CharacterData?.PmcData?.Info?.Nickname;
                if (string.IsNullOrEmpty(nickname))
                {
                    return null;
                }

                return nickname;
            }

            return null;
        }

        /// <summary>
        /// Gets the alias (If it has been given one) or nickname of the headless client
        /// </summary>
        /// <param name="headlessSessionID"></param>
        /// <returns>the alias, or nickname or the headless client.</returns>
        public string GetHeadlessNickname(string headlessSessionID)
        {
            FikaConfig config = fikaConfig.Config;
            if (config.Headless.Profiles.Aliases.TryGetValue(headlessSessionID, out string? alias))
            {
                return alias;
            }

            string? nickname = saveServer.GetProfile(headlessSessionID).CharacterData?.PmcData?.Info?.Nickname;
            if (string.IsNullOrEmpty(nickname))
            {
                return "ERROR";
            }

            return nickname;
        }

        /// <summary>
        /// Gets all available headless clients
        /// </summary>
        /// <returns>Returns an array of available headless clients</returns>
        public HeadlessAvailableClients[] GetAvailableHeadlessClients()
        {
            List<string> availableClients = [.. HeadlessClients
                .Where(x => x.Value.State == EHeadlessStatus.READY)
                .Select(x => x.Key)];

            List<HeadlessAvailableClients> result = [];
            foreach (string sessionId in availableClients)
            {
                result.Add(new(sessionId, GetHeadlessNickname(sessionId)));
            }

            return [.. result];
        }
    }
}
