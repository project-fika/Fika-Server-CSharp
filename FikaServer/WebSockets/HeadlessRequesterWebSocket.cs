using FikaServer.Models.Fika.Headless;
using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Servers.Ws;
using SPTarkov.Server.Core.Utils;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace FikaServer.WebSockets
{
    /*
    [Injectable(InjectionType.Singleton)]
    public class HeadlessRequesterWebSocket(SaveServer saveServer, JsonUtil jsonUtil, ISptLogger<HeadlessRequesterWebSocket> logger) : IWebSocketConnectionHandler
    {
        private readonly ConcurrentDictionary<string, WebSocket> requesterWebSockets = [];

        public string GetHookUrl()
        {
            return "/fika/headless/requester";
        }

        public string GetSocketId()
        {
            return "Fika Headless Requester";
        }

        public async Task OnConnection(WebSocket ws, HttpContext context)
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

            if (!saveServer.ProfileExists(userSessionID))
            {
                logger.Debug($"[{GetSocketId()}] Invalid user {userSessionID} tried to authenticate!");
                return;
            }

            requesterWebSockets.TryAdd(userSessionID, ws);
        }

        public Task OnMessage(byte[] rawData, WebSocketMessageType messageType, WebSocket ws, HttpContext context)
        {
            // Do nothing
            return Task.CompletedTask;
        }

        public Task OnClose(WebSocket ws, HttpContext context)
        {
            string userSessionID = requesterWebSockets.FirstOrDefault(x => x.Value == ws).Key;

            if (!string.IsNullOrEmpty(userSessionID))
            {
                logger.Debug($"[{GetSocketId()}] Deleting requester {userSessionID}");

                requesterWebSockets.TryRemove(userSessionID, out _);
            }

            return Task.CompletedTask;
        }

        public async Task SendAsync(string sessionID, HeadlessBase message)
        {
            // Client is not online or not currently connected to the websocket.
            if (!requesterWebSockets.TryGetValue(sessionID, out WebSocket ws))
            {
                logger.Warning($"[{GetSocketId()}] Requester ({sessionID}) is not connected yet?");
                return;
            }

            // Client was formerly connected to the websocket, but may have connection issues as it didn't run onClose
            if (ws.State == WebSocketState.Closed)
            {
                logger.Warning($"[{GetSocketId()}] Requester ({sessionID})'s websocket is closed?");
                return;
            }

            await ws.SendAsync(Encoding.UTF8.GetBytes(jsonUtil.Serialize(message)), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
    */
}
