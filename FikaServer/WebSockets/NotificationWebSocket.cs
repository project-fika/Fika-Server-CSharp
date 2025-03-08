using SPTarkov.Server.Core.Models.Eft.Ws;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Servers.Ws;
using SPTarkov.Server.Core.Utils;
using FikaServer.Services;
using SPTarkov.Common.Annotations;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace FikaServer.WebSockets
{
    [Injectable(InjectionType.Singleton)]
    public class NotificationWebSocket(SaveServer saveServer, ISptLogger<NotificationWebSocket> logger, PresenceService fikaPresenceService) : IWebSocketConnectionHandler
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

        public bool IsWebSocketConnected(string sessionId)
        {
            return true;
        }

        public Task OnConnection(WebSocket ws, HttpContext context)
        {
            return Task.Factory.StartNew(async () =>
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

                logger.Debug($"[{GetSocketId()}] User is ${userSessionID}");
            });
        }

        public void SendMessage(string sessionID, WsNotificationEvent output)
        {
        }
    }
}
