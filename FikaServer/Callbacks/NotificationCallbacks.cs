using FikaServer.Models.Enums;
using FikaServer.Models.Fika.WebSocket.Notifications;
using FikaServer.WebSockets;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers.Ws;
using SPTarkov.Server.Core.Utils;

namespace FikaServer.Callbacks
{
    [Injectable]
    public class NotificationCallbacks(IEnumerable<IWebSocketConnectionHandler> sptWebSocketConnectionHandlers, ISptLogger<NotificationWebSocket> logger, HttpResponseUtil httpResponseUtil)
    {
        private static NotificationWebSocket? NotificationWebSocket = null;

        /// <summary>
        /// Handle /fika/notification/push
        /// </summary>
        public string HandlePushNotification(string url, PushNotification info, string sessionID)
        {
            // Yes, technically this needs a controller to fit into this format. But I cant be bothered setting up a whole controller for a few checks.
            if (NotificationWebSocket == null)
            {
                NotificationWebSocket = sptWebSocketConnectionHandlers
                .OfType<NotificationWebSocket>()
                .FirstOrDefault(wsh => wsh.GetSocketId() == "Fika Notification Manager");
            }

            if (info.Notification == null)
            {
                return httpResponseUtil.NullResponse();
            }

            // Do some exception handling for the client, icon 6 seems to cause an exception as well as going out of the enum's bounds.
            if (info.NotificationIcon == EEFTNotificationIconType.Achievement || (int)info.NotificationIcon > 14)
            {
                info.NotificationIcon = EEFTNotificationIconType.Default;
            }

            logger.Error("broadcasting");
            NotificationWebSocket.BroadcastAsync(info);

            return httpResponseUtil.NullResponse();
        }
    }
}
