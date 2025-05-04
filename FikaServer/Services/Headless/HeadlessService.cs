using FikaServer.Models.Enums;
using FikaServer.Models.Fika.Headless;
using FikaServer.Models.Fika.Routes.Headless;
using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace FikaServer.Services.Headless
{
    [Injectable(InjectionType.Singleton)]
    public class HeadlessService(ISptLogger<HeadlessService> logger, JsonUtil jsonUtil)
    {
        private readonly ISptLogger<HeadlessService> _logger = logger;
        public ConcurrentDictionary<string, HeadlessClientInfo> HeadlessClients { get; private set; } = [];

        public async Task<string?> StartHeadlessRaid(string headlessSessionID, string requesterSessionID, StartHeadlessRequest info)
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
            await webSocket.SendAsync(Encoding.UTF8.GetBytes(data),
                WebSocketMessageType.Text, true, CancellationToken.None);

            return headlessSessionID;
        }

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
            //Todo: Stub for now, implement method.
        }

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
