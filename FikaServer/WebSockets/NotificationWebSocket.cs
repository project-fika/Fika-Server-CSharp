using FikaServer.Models.Fika.WebSocket;
using FikaServer.Services;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Servers.Ws;
using SPTarkov.Server.Core.Utils;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace FikaServer.WebSockets
{
    [Injectable(InjectionType.Singleton)]
    public class NotificationWebSocket(SaveServer saveServer, JsonUtil jsonUtil, ISptLogger<NotificationWebSocket> logger, PresenceService fikaPresenceService) : IWebSocketConnectionHandler
    {
        private ConcurrentDictionary<string, WebSocket> clientWebSockets = [];

        public string GetHookUrl()
        {
            return "/fika/notification/";
        }

        public string GetSocketId()
        {
            return "Fika Notification Manager";
        }

        public async Task OnConnection(WebSocket ws, HttpContext context, string sessionIdContext)
        {
            string authHeader = context.Request.Headers.Authorization.ToString();

            if (string.IsNullOrEmpty(authHeader))
            {
                await ws.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, "", CancellationToken.None);
                return;
            }

            string base64EncodedString = authHeader.Split(' ')[1];
            string decodedString = Encoding.UTF8.GetString(Convert.FromBase64String(base64EncodedString));
            string[] authorization = decodedString.Split(':');
            string userSessionID = authorization[0];

            logger.Debug($"[{GetSocketId()}] User is {userSessionID}");

            if (!clientWebSockets.TryAdd(userSessionID, ws))
            {
                logger.Warning($"[{GetSocketId()}] Could not add {userSessionID} as it already exists?");
                return;
            }

            fikaPresenceService.AddPlayerPresence(userSessionID);
        }

        public Task OnMessage(byte[] rawData, WebSocketMessageType messageType, WebSocket ws, HttpContext context)
        {
            // Do nothing
            return Task.CompletedTask;
        }

        public Task OnClose(WebSocket ws, HttpContext context, string sessionIdContext)
        {
            KeyValuePair<string, WebSocket> client = clientWebSockets.Where(x => x.Value == ws).FirstOrDefault();

            if (client.Key != null)
            {
                logger.Debug($"[{GetSocketId()}] Deleting client ${client.Key}");

                clientWebSockets.TryRemove(client.Key, out _);
                fikaPresenceService.RemovePlayerPresence(client.Key);
            }

            return Task.CompletedTask;
        }

        public async Task SendAsync<T>(MongoId sessionID, T message) where T : IFikaNotificationBase
        {
            // Client is not online or not currently connected to the websocket.
            if (!clientWebSockets.TryGetValue(sessionID, out WebSocket ws))
            {
                return;
            }

            // Client was formerly connected to the websocket, but may have connection issues as it didn't run onClose
            if (ws.State == WebSocketState.Closed)
            {
                return;
            }

            await ws.SendAsync(Encoding.UTF8.GetBytes(jsonUtil.Serialize(message)), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public async Task BroadcastAsync<T>(T message) where T : IFikaNotificationBase
        {
            foreach (KeyValuePair<string, WebSocket> websocket in clientWebSockets)
            {
                await SendAsync(websocket.Key, message);
            }
        }
    }
}
