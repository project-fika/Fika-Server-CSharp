using FikaServer.Callbacks;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Match;
using SPTarkov.Server.Core.Utils;

namespace FikaServer.Routers.Static
{
    [Injectable]
    public class LocationStaticRouter(LocationCallbacks locationCallbacks, JsonUtil jsonUtil) : StaticRouter(jsonUtil, [
            new RouteAction(
                "/fika/location/raids",
                async (
                    url,
                    info,
                    sessionId,
                    output
                ) => await locationCallbacks.HandleGetRaids(url, info as GetRaidConfigurationRequestData, sessionId),
                typeof(GetRaidConfigurationRequestData)
                )
        ])
    {
    }
}
