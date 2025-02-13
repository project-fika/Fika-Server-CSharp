using Core.DI;
using Core.Utils;
using FikaServer.Callbacks;
using SptCommon.Annotations;

namespace FikaServer.Routers.Static
{
    [Injectable(InjectableTypeOverride = typeof(StaticRouter))]
    public class ClientStaticRouter(ClientCallbacks fikaClientCallbacks, JsonUtil jsonUtil) : StaticRouter(jsonUtil, [new RouteAction(
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
                ) => fikaClientCallbacks.HandleNatPunchConfig(url, info, sessionId))])
    {
    }
}
