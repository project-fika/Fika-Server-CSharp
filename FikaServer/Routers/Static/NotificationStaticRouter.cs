using FikaServer.Callbacks;
using FikaServer.Models.Fika.WebSocket.Notifications;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Utils;

namespace FikaServer.Routers.Static
{
    [Injectable]
    public class NotificationStaticRouter(NotificationCallbacks fikaNotificationCallbacks, JsonUtil jsonUtil) : StaticRouter(jsonUtil, [
            new RouteAction(
                "/fika/notification/push",
                (
                    url,
                    info,
                    sessionId,
                    output
                ) => fikaNotificationCallbacks.HandlePushNotification(url, info as PushNotification, sessionId),
                typeof(PushNotification)
                )
        ])
    {
    }
}
