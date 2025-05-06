using FikaServer.Callbacks;
using FikaServer.Models.Fika.Routes.Raid;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Utils;

namespace FikaServer.Routers.Static
{
    [Injectable]
    public class HeadlessStaticRouter(HeadlessCallbacks fikaHeadlessCallbacks, JsonUtil jsonUtil) : StaticRouter(jsonUtil, [
            new RouteAction(
                "/fika/headless/get",
                (
                    url,
                    info,
                    sessionId,
                    output
                ) => fikaHeadlessCallbacks.HandleGetHeadlesses(url, info as FikaRaidServerIdRequestData, sessionId),
                typeof(FikaRaidServerIdRequestData)
                ),
            new RouteAction(
                "/fika/headless/available",
                (
                    url,
                    info,
                    sessionId,
                    output
                ) => fikaHeadlessCallbacks.HandleAvailableHeadlesses(url, info as FikaRaidServerIdRequestData, sessionId),
                typeof(FikaRaidServerIdRequestData)
                ),
             new RouteAction(
                "/fika/headless/restartafterraidamount",
                (
                    url,
                    info,
                    sessionId,
                    output
                ) => fikaHeadlessCallbacks.HandleRestartAfterRaidAmount(url, info as FikaRaidServerIdRequestData, sessionId),
                typeof(FikaRaidServerIdRequestData)
                ),
        ])
    {
    }
}
