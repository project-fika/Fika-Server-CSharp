using FikaServer.Callbacks;
using FikaServer.Models.Fika.Routes.Headless;
using FikaServer.Models.Fika.Routes.Raid;
using FikaServer.Models.Fika.Routes.Raid.Create;
using FikaServer.Models.Fika.Routes.Raid.Join;
using FikaServer.Models.Fika.Routes.Raid.Leave;
using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.InRaid;
using SPTarkov.Server.Core.Utils;

namespace FikaServer.Routers.Static
{
    [Injectable(InjectableTypeOverride = typeof(StaticRouter))]
    public class RaidStaticRouter(RaidCallbacks fikaRaidCallbacks, JsonUtil jsonUtil) : StaticRouter(jsonUtil, [
            new RouteAction(
                "/fika/raid/create",
                (
                    url,
                    info,
                    sessionId,
                    output
                ) => fikaRaidCallbacks.HandleRaidCreate(url, info as FikaRaidCreateRequestData, sessionId),
                typeof(FikaRaidCreateRequestData)
                ),
            new RouteAction(
                "/fika/raid/join",
                (
                    url,
                    info,
                    sessionId,
                    output
                ) => fikaRaidCallbacks.HandleRaidJoin(url, info as FikaRaidJoinRequestData, sessionId),
                typeof(FikaRaidJoinRequestData)
                ),
             new RouteAction(
                "/fika/raid/leave",
                (
                    url,
                    info,
                    sessionId,
                    output
                ) => fikaRaidCallbacks.HandleRaidLeave(url, info as FikaRaidLeaveRequestData, sessionId),
                typeof(FikaRaidLeaveRequestData)
                ),
            new RouteAction(
                "/fika/raid/gethost",
                (
                    url,
                    info,
                    sessionId,
                    output
                ) => fikaRaidCallbacks.HandleRaidGetHost(url, info as FikaRaidServerIdRequestData, sessionId),
                typeof(FikaRaidServerIdRequestData)
                ),
            new RouteAction(
                "/fika/raid/getsettings",
                (
                    url,
                    info,
                    sessionId,
                    output
                ) => fikaRaidCallbacks.HandleRaidGetSettings(url, info as FikaRaidServerIdRequestData, sessionId),
                typeof(FikaRaidServerIdRequestData)
                ),
            new RouteAction(
                "/fika/raid/headless/start",
                (
                    url,
                    info,
                    sessionId,
                    output
                ) => fikaRaidCallbacks.HandleRaidStartHeadless(url, info as StartHeadlessRequest, sessionId),
                typeof(StartHeadlessRequest)
                ),
            new RouteAction(
                "/fika/raid/registerPlayer",
                (
                    url,
                    info,
                    sessionId,
                    output
                ) => fikaRaidCallbacks.HandleRaidRegisterPlayer(url, info as RegisterPlayerRequestData, sessionId),
                typeof(RegisterPlayerRequestData)
                ),
        ])
    {
    }
}
