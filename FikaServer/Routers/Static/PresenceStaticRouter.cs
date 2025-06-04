using FikaServer.Callbacks;
using FikaServer.Models.Fika.Presence;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Utils;

namespace FikaServer.Routers.Static
{
    [Injectable]
    public class PresenceStaticRouter(PresenceCallbacks fikaPresenceCallbacks, JsonUtil jsonUtil) : StaticRouter(jsonUtil, [
            new RouteAction(
                "/fika/presence/get",
                async (
                    url,
                    info,
                    sessionId,
                    output
                ) => await fikaPresenceCallbacks.HandleGetPresence(url, info, sessionId)),
            new RouteAction(
                "/fika/presence/set",
                async (
                    url,
                    info,
                    sessionId,
                    output
                ) => await fikaPresenceCallbacks.HandleSetPresence(url, info as FikaSetPresence, sessionId),
                typeof(FikaSetPresence)
                ),
            new RouteAction(
                "/fika/presence/setget",
                async (
                    url,
                    info,
                    sessionId,
                    output
                ) => await fikaPresenceCallbacks.HandleSetGetPresence(url, info as FikaSetPresence, sessionId),
                typeof(FikaSetPresence)
                ),
        ])
    {
    }
}
