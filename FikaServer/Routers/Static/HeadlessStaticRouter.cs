using FikaServer.Callbacks;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Utils;

namespace FikaServer.Routers.Static;

[Injectable]
public class HeadlessStaticRouter(HeadlessCallbacks fikaHeadlessCallbacks, JsonUtil jsonUtil) : StaticRouter(jsonUtil, [
        new RouteAction<EmptyRequestData>(
            "/fika/headless/get",
            async (
                url,
                info,
                sessionId,
                output
            ) => await fikaHeadlessCallbacks.HandleGetHeadlesses(url, info, sessionId)
            ),
        new RouteAction<EmptyRequestData>(
            "/fika/headless/available",
            async (
                url,
                info,
                sessionId,
                output
            ) => await fikaHeadlessCallbacks.HandleAvailableHeadlesses(url, info, sessionId)
            ),
         new RouteAction<EmptyRequestData>(
            "/fika/headless/restartafterraidamount",
            async (
                url,
                info,
                sessionId,
                output
            ) => await fikaHeadlessCallbacks.HandleRestartAfterRaidAmount(url, info, sessionId)
            ),
    ])
{
}
