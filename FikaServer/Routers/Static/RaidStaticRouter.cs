using FikaServer.Callbacks;
using FikaServer.Models.Fika.Routes.Headless;
using FikaServer.Models.Fika.Routes.Raid;
using FikaServer.Models.Fika.Routes.Raid.Create;
using FikaServer.Models.Fika.Routes.Raid.Join;
using FikaServer.Models.Fika.Routes.Raid.Leave;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.InRaid;
using SPTarkov.Server.Core.Utils;

namespace FikaServer.Routers.Static
{
    [Injectable]
    public class RaidStaticRouter(RaidCallbacks fikaRaidCallbacks, JsonUtil jsonUtil) : StaticRouter(jsonUtil, [
            new RouteAction(
                "/fika/raid/create",
                async (
                    url,
                    info,
                    sessionId,
                    output
                ) => await fikaRaidCallbacks.HandleRaidCreate(url, info as FikaRaidCreateRequestData, sessionId),
                typeof(FikaRaidCreateRequestData)
                ),
            new RouteAction(
                "/fika/raid/join",
                async (
                    url,
                    info,
                    sessionId,
                    output
                ) => await fikaRaidCallbacks.HandleRaidJoin(url, info as FikaRaidJoinRequestData, sessionId),
                typeof(FikaRaidJoinRequestData)
                ),
             new RouteAction(
                "/fika/raid/leave",
                async (
                    url,
                    info,
                    sessionId,
                    output
                ) => await fikaRaidCallbacks.HandleRaidLeave(url, info as FikaRaidLeaveRequestData, sessionId),
                typeof(FikaRaidLeaveRequestData)
                ),
            new RouteAction(
                "/fika/raid/gethost",
                async (
                    url,
                    info,
                    sessionId,
                    output
                ) => await fikaRaidCallbacks.HandleRaidGetHost(url, info as FikaRaidServerIdRequestData, sessionId),
                typeof(FikaRaidServerIdRequestData)
                ),
            new RouteAction(
                "/fika/raid/getsettings",
                async (
                    url,
                    info,
                    sessionId,
                    output
                ) => await fikaRaidCallbacks.HandleRaidGetSettings(url, info as FikaRaidServerIdRequestData, sessionId),
                typeof(FikaRaidServerIdRequestData)
                ),
            new RouteAction(
                "/fika/raid/headless/start",
                async (
                    url,
                    info,
                    sessionId,
                    output
                ) => await fikaRaidCallbacks.HandleRaidStartHeadless(url, info as StartHeadlessRequest, sessionId),
                typeof(StartHeadlessRequest)
                ),
            new RouteAction(
                "/fika/raid/registerPlayer",
                async (
                    url,
                    info,
                    sessionId,
                    output
                ) => await fikaRaidCallbacks.HandleRaidRegisterPlayer(url, info as RegisterPlayerRequestData, sessionId),
                typeof(RegisterPlayerRequestData)
                ),
        ])
    {
    }
}
