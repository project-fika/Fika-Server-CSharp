using FikaServer.Callbacks;
using FikaServer.Models.Fika.Routes.Raid.Join;
using FikaServer.Models.Fika.Routes.Update;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Utils;

namespace FikaServer.Routers.Static;

[Injectable]
public class UpdateStaticRouter(UpdateCallbacks fikaUpdateCallbacks, JsonUtil jsonUtil) : StaticRouter(jsonUtil, [
        new RouteAction<FikaUpdatePingRequestData>(
            "/fika/update/ping",
            async (
                url,
                info,
                sessionId,
                output
            ) => await fikaUpdateCallbacks.HandlePing(url, info, sessionId)
            ),
        new RouteAction<FikaUpdatePlayerSpawnRequestData>(
            "/fika/update/playerspawn",
            async (
                url,
                info,
                sessionId,
                output
            ) => await fikaUpdateCallbacks.HandlePlayerSpawn(url, info, sessionId)
            ),
         new RouteAction<FikaUpdateSetHostRequestData>(
            "/fika/update/sethost",
            async (
                url,
                info,
                sessionId,
                output
            ) => await fikaUpdateCallbacks.HandleSetHost(url, info, sessionId)
            ),
        new RouteAction<FikaUpdateSetStatusRequestData>(
            "/fika/update/setstatus",
            async (
                url,
                info,
                sessionId,
                output
            ) => await fikaUpdateCallbacks.HandleSetStatus(url, info, sessionId)
            ),
        new RouteAction<FikaUpdateRaidAddPlayerData>(
            "/fika/update/addplayer",
            async (
                url,
                info,
                sessionId,
                output
            ) => await fikaUpdateCallbacks.HandleRaidAddPlayer(url, info, sessionId)
            ),
        new RouteAction<FikaUpdateRaidAddPlayerData>(
            "/fika/update/playerdied",
            async (
                url,
                info,
                sessionId,
                output
            ) => await fikaUpdateCallbacks.HandlePlayerDied(url, info, sessionId)
            ),
    ])
{
}
