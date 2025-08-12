using FikaServer.Callbacks;
using FikaServer.Models.Fika.Routes.Client.Check;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Utils;

namespace FikaServer.Routers.Static;

[Injectable]
public class ClientStaticRouter(ClientCallbacks fikaClientCallbacks, JsonUtil jsonUtil) : StaticRouter(jsonUtil, [
        new RouteAction(
            "/fika/client/config",
            async (
                url,
                info,
                sessionId,
                output
            ) => await fikaClientCallbacks.HandleClientConfig(url, info, sessionId)),
        new RouteAction(
            "/fika/natpunchserver/config",
            async (
                url,
                info,
                sessionId,
                output
            ) => await fikaClientCallbacks.HandleNatPunchConfig(url, info, sessionId)),
        new RouteAction(
            "/fika/client/check/mods",
            async (
                url,
                info,
                sessionId,
                output
            ) => await fikaClientCallbacks.HandleCheckMods(url, info as FikaCheckModRequestData, sessionId)),
        new RouteAction(
            "/fika/profile/download",
            async (
                url,
                info,
                sessionId,
                output
            ) => await fikaClientCallbacks.HandleProfileDownload(url, info, sessionId)),
        new RouteAction(
            "/fika/client/check/version",
            async (
                url,
                info,
                sessionId,
                output
            ) => await fikaClientCallbacks.HandleVersionCheck(url, info, sessionId))
    ])
{
}
