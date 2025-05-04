using FikaServer.Models.Enums;
using FikaServer.Models.Fika.Headless;
using FikaServer.Models.Fika.Routes.Headless;
using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace FikaServer.Services.Headless
{
    [Injectable(InjectionType.Singleton)]
    public class HeadlessService(ISptLogger<HeadlessService> logger, JsonUtil jsonUtil, SaveServer saveServer)
    {
        public ConcurrentDictionary<string, HeadlessClientInfo> HeadlessClients { get; private set; } = [];

        private readonly ISptLogger<HeadlessService> _logger = logger;


        /// <summary>
        /// Begin setting up a raid for a headless client
        /// </summary>
        /// <returns>returns the SessionID of the headless client that is starting this raid, returns null if no client could be found or there was an error.</returns>
        public string? StartHeadlessRaid(string headlessSessionID, string requesterSessionID, StartHeadlessRequest info)
        {
            if (!HeadlessClients.TryGetValue(headlessSessionID, out HeadlessClientInfo? headlessClientInfo))
            {
                _logger.LogWithColor($"Could not find HeadlessSessionID '{headlessSessionID}'", LogTextColor.Red);
                return string.Empty;
            }

            if (headlessClientInfo.State is not EHeadlessStatus.READY)
            {
                _logger.LogWithColor($"HeadlessSessionID '{headlessSessionID}' was not ready, was {headlessClientInfo.State}", LogTextColor.Yellow);
                return string.Empty;
            }

            WebSocket webSocket = headlessClientInfo.WebSocket;
            if (webSocket == null)
            {
                return string.Empty;
            }

            if (webSocket.State is WebSocketState.Closed)
            {
                return string.Empty;
            }

            StartHeadlessRaid startHeadlessRequest = new(EFikaHeadlessWSMessageType.HeadlessStartRaid, info);
            string data = jsonUtil.Serialize(startHeadlessRequest) ?? throw new NullReferenceException("StartHeadlessRaid:: Data was null after serializing");
            webSocket.SendAsync(Encoding.UTF8.GetBytes(data),
                WebSocketMessageType.Text, true, CancellationToken.None).Wait();

            return headlessSessionID;
        }

        /// <summary>
        /// Sends a join message to the requester of a headless client
        /// </summary>
        public void SendJoinMessageToRequester(string headlessClientId)
        {
            if (!HeadlessClients.TryGetValue(headlessClientId, out HeadlessClientInfo? headlessClientInfo))
            {
                _logger.LogWithColor($"Could not find HeadlessSessionID '{headlessClientId}'", LogTextColor.Red);
                return;
            }

            if (headlessClientInfo.State is not EHeadlessStatus.READY)
            {
                _logger.LogWithColor($"HeadlessSessionID '{headlessClientId}' was not ready, was {headlessClientInfo.State}", LogTextColor.Yellow);
                return;
            }

            HeadlessRequesterJoinRaid headlessRequesterJoinRaid = new(EFikaHeadlessWSMessageType.RequesterJoinMatch, headlessClientId);
            string data = jsonUtil.Serialize(headlessRequesterJoinRaid) ?? throw new NullReferenceException("SendJoinMessageToRequester:: Data was null after serializing");
            // TODO: Implement FikaHeadlessRequesterWebSocket

            /*await webSocket.SendAsync(Encoding.UTF8.GetBytes(data),
                WebSocketMessageType.Text, true, CancellationToken.None);*/
        }

        public void AddPlayerToHeadlessMatch(string headlessClientId, string sessionID)
        {
            if (HeadlessClients.TryGetValue(headlessClientId, out HeadlessClientInfo? headlessClientInfo))
            {
                if (headlessClientInfo == null)
                {
                    throw new NullReferenceException($"AddPlayerToHeadlessMatch:: HeadlessClientInfo was null on {headlessClientId}");
                }

                headlessClientInfo.Players?.Add(sessionID);
            }
        }

        public void SetHeadlessLevel(string headlessClientId)
        {
            if (!HeadlessClients.TryGetValue(headlessClientId,out HeadlessClientInfo? headlessClientInfo))
            {
                throw new NullReferenceException($"SetHeadlessLevel:: Could not find headlessClientId '{headlessClientId}'");
            }

            if (headlessClientInfo.State is not EHeadlessStatus.IN_RAID)
            {
                return;
            }

            var headlessProfile = saveServer.GetProfile(headlessClientId)
                ?? throw new NullReferenceException($"Could not find headlessProfile {headlessClientId}");

            int baseHeadlessLevel = 0;
            int players = headlessClientInfo.Players.Count;

            foreach (string profileId in headlessClientInfo.Players)
            {
                SptProfile profile = saveServer.GetProfile(profileId);
                if (profile == null)
                {
                    continue;
                }

                baseHeadlessLevel += profile.CharacterData.PmcData.Info.Level.GetValueOrDefault(1);
            }

            baseHeadlessLevel /= players;

            logger.Log(SPTarkov.Server.Core.Models.Spt.Logging.LogLevel.Debug,
                $"[{headlessClientId}] Settings headless level to: {baseHeadlessLevel} | Players: {players}");

            headlessProfile.CharacterData.PmcData.Info.Level = baseHeadlessLevel;
        }

        /// <summary>
        /// End the raid for the specified headless client, sets the state back to READY so that he can be requested to host again.
        /// </summary>
        public void EndHeadlessRaid(string headlessClientId)
        {
            if (!HeadlessClients.TryGetValue(headlessClientId, out HeadlessClientInfo? headlessClientInfo))
            {
                _logger.LogWithColor($"EndHeadlessRaid:: Could not find '{headlessClientId}' to remove");
                return;
            }

            headlessClientInfo.Reset();
        }
    }
}
