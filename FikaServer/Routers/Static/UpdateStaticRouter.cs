using FikaServer.Callbacks;
using FikaServer.Models.Fika.Routes.Raid.Join;
using FikaServer.Models.Fika.Routes.Update;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Utils;

namespace FikaServer.Routers.Static
{
    [Injectable]
    public class UpdateStaticRouter(UpdateCallbacks fikaUpdateCallbacks, JsonUtil jsonUtil) : StaticRouter(jsonUtil, [
            new RouteAction(
                "/fika/update/ping",
                async (
                    url,
                    info,
                    sessionId,
                    output
                ) => await fikaUpdateCallbacks.HandlePing(url, info as FikaUpdatePingRequestData, sessionId),
                typeof(FikaUpdatePingRequestData)
                ),
            new RouteAction(
                "/fika/update/playerspawn",
                async (
                    url,
                    info,
                    sessionId,
                    output
                ) => await fikaUpdateCallbacks.HandlePlayerSpawn(url, info as FikaUpdatePlayerSpawnRequestData, sessionId),
                typeof(FikaUpdatePlayerSpawnRequestData)
                ),
             new RouteAction(
                "/fika/update/sethost",
                async (
                    url,
                    info,
                    sessionId,
                    output
                ) => await fikaUpdateCallbacks.HandleSetHost(url, info as FikaUpdateSethostRequestData, sessionId),
                typeof(FikaUpdateSethostRequestData)
                ),
            new RouteAction(
                "/fika/update/setstatus",
                async (
                    url,
                    info,
                    sessionId,
                    output
                ) => await fikaUpdateCallbacks.HandleSetStatus(url, info as FikaUpdateSetStatusRequestData, sessionId),
                typeof(FikaUpdateSetStatusRequestData)
                ),
            new RouteAction(
                "/fika/update/addplayer",
                async (
                    url,
                    info,
                    sessionId,
                    output
                ) => await fikaUpdateCallbacks.HandleRaidAddPlayer(url, info as FikaUpdateRaidAddPlayerData, sessionId),
                typeof(FikaUpdateRaidAddPlayerData)
                ),
            new RouteAction(
                "/fika/update/playerdied",
                async (
                    url,
                    info,
                    sessionId,
                    output
                ) => await fikaUpdateCallbacks.HandlePlayerDied(url, info as FikaUpdateRaidAddPlayerData, sessionId),
                typeof(FikaUpdateRaidAddPlayerData)
                ),
        ])
    {
    }
}
