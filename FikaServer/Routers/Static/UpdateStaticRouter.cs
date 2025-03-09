using FikaServer.Callbacks;
using FikaServer.Models.Fika.Routes.Raid.Join;
using FikaServer.Models.Fika.Routes.Update;
using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Utils;

namespace FikaServer.Routers.Static
{
    [Injectable(InjectableTypeOverride = typeof(StaticRouter))]
    public class UpdateStaticRouter(UpdateCallbacks fikaUpdateCallbacks, JsonUtil jsonUtil) : StaticRouter(jsonUtil, [
            new RouteAction(
                "/fika/update/ping",
                (
                    url,
                    info,
                    sessionId,
                    output
                ) => fikaUpdateCallbacks.HandlePing(url, info as FikaUpdatePingRequestData, sessionId),
                typeof(FikaUpdatePingRequestData)
                ),
            new RouteAction(
                "/fika/update/playerspawn",
                (
                    url,
                    info,
                    sessionId,
                    output
                ) => fikaUpdateCallbacks.HandlePlayerSpawn(url, info as FikaUpdatePlayerSpawnRequestData, sessionId),
                typeof(FikaUpdatePlayerSpawnRequestData)
                ),
             new RouteAction(
                "/fika/update/sethost",
                (
                    url,
                    info,
                    sessionId,
                    output
                ) => fikaUpdateCallbacks.HandleSetHost(url, info as FikaUpdateSethostRequestData, sessionId),
                typeof(FikaUpdateSethostRequestData)
                ),
            new RouteAction(
                "/fika/update/setstatus",
                (
                    url,
                    info,
                    sessionId,
                    output
                ) => fikaUpdateCallbacks.HandleSetStatus(url, info as FikaUpdateSetStatusRequestData, sessionId),
                typeof(FikaUpdateSetStatusRequestData)
                ),
            new RouteAction(
                "/fika/update/addplayer",
                (
                    url,
                    info,
                    sessionId,
                    output
                ) => fikaUpdateCallbacks.HandleRaidAddPlayer(url, info as FikaUpdateRaidAddPlayerData, sessionId),
                typeof(FikaUpdateRaidAddPlayerData)
                ),
            new RouteAction(
                "/fika/update/playerdied",
                (
                    url,
                    info,
                    sessionId,
                    output
                ) => fikaUpdateCallbacks.HandlePlayerDied(url, info as FikaUpdateRaidAddPlayerData, sessionId),
                typeof(FikaUpdateRaidAddPlayerData)
                ),
        ])
    {
    }
}
