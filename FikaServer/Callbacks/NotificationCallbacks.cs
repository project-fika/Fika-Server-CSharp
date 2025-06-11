using FikaServer.Models.Enums;
using FikaServer.Models.Fika.WebSocket.Notifications;
using FikaServer.WebSockets;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;

namespace FikaServer.Callbacks
{
    [Injectable]
    public class NotificationCallbacks(NotificationWebSocket notificationWebSocket,
        ISptLogger<NotificationWebSocket> logger, HttpResponseUtil httpResponseUtil)
    {
        /// <summary>
        /// Handle /fika/notification/push
        /// </summary>
        public async ValueTask<string> HandlePushNotification(string url, PushNotification info, string sessionID)
        {
            if (info.Notification == null)
            {
                return httpResponseUtil.NullResponse();
            }

            // Do some exception handling for the client, icon 6 seems to cause an exception as well as going out of the enum's bounds.
            if (info.NotificationIcon == EEFTNotificationIconType.Achievement || (int)info.NotificationIcon > 14)
            {
                info.NotificationIcon = EEFTNotificationIconType.Default;
            }

            //Todo: Debug log
            logger.Error("broadcasting");
            await notificationWebSocket.BroadcastAsync(info);

            return httpResponseUtil.NullResponse();
        }
    }
}
