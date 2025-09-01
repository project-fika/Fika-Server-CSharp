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

namespace FikaServer.Routers.Static;

[Injectable]
public class RaidStaticRouter(RaidCallbacks fikaRaidCallbacks, JsonUtil jsonUtil) : StaticRouter(jsonUtil, [
        new RouteAction<FikaRaidCreateRequestData>(
            "/fika/raid/create",
            async (
                url,
                info,
                sessionId,
                output
            ) => await fikaRaidCallbacks.HandleRaidCreate(url, info, sessionId)
            ),
        new RouteAction<FikaRaidJoinRequestData>(
            "/fika/raid/join",
            async (
                url,
                info,
                sessionId,
                output
            ) => await fikaRaidCallbacks.HandleRaidJoin(url, info, sessionId)
            ),
         new RouteAction<FikaRaidLeaveRequestData>(
            "/fika/raid/leave",
            async (
                url,
                info,
                sessionId,
                output
            ) => await fikaRaidCallbacks.HandleRaidLeave(url, info, sessionId)
            ),
        new RouteAction<FikaRaidServerIdRequestData>(
            "/fika/raid/gethost",
            async (
                url,
                info,
                sessionId,
                output
            ) => await fikaRaidCallbacks.HandleRaidGetHost(url, info, sessionId)
            ),
        new RouteAction<FikaRaidServerIdRequestData>(
            "/fika/raid/getsettings",
            async (
                url,
                info,
                sessionId,
                output
            ) => await fikaRaidCallbacks.HandleRaidGetSettings(url, info, sessionId)
            ),
        new RouteAction<StartHeadlessRequest>(
            "/fika/raid/headless/start",
            async (
                url,
                info,
                sessionId,
                output
            ) => await fikaRaidCallbacks.HandleRaidStartHeadless(url, info, sessionId)
            ),
        new RouteAction<RegisterPlayerRequestData>(
            "/fika/raid/registerPlayer",
            async (
                url,
                info,
                sessionId,
                output
            ) => await fikaRaidCallbacks.HandleRaidRegisterPlayer(url, info, sessionId)
            ),
    ])
{
}
