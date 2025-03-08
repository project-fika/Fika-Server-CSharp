using FikaServer.Callbacks;
using FikaServer.Models.Fika.Routes.Client.Check;
using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Utils;

namespace FikaServer.Routers.Static
{
    [Injectable(InjectableTypeOverride = typeof(StaticRouter))]
    public class ClientStaticRouter(ClientCallbacks fikaClientCallbacks, JsonUtil jsonUtil) : StaticRouter(jsonUtil, [
            new RouteAction(
                "/fika/client/config",
                (
                    url,
                    info,
                    sessionId,
                    output
                ) => fikaClientCallbacks.HandleClientConfig(url, info, sessionId)),
            new RouteAction(
                "/fika/natpunchserver/config",
                (
                    url,
                    info,
                    sessionId,
                    output
                ) => fikaClientCallbacks.HandleNatPunchConfig(url, info, sessionId)),
            new RouteAction(
                "/fika/client/check/mods",
                (
                    url,
                    info,
                    sessionId,
                    output
                ) => fikaClientCallbacks.HandleCheckMods(url, info as FikaCheckModRequestData, sessionId)),
            new RouteAction(
                "/fika/profile/download",
                (
                    url,
                    info,
                    sessionId,
                    output
                ) => fikaClientCallbacks.HandleProfileDownload(url, info, sessionId)),
            new RouteAction(
                "/fika/client/check/version",
                (
                    url,
                    info,
                    sessionId,
                    output
                ) => fikaClientCallbacks.HandleVersionCheck(url, info, sessionId))
        ])
    {
    }
}
