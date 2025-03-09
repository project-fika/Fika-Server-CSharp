using FikaServer.Callbacks;
using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Match;
using SPTarkov.Server.Core.Utils;

namespace FikaServer.Routers.Static
{
    [Injectable(InjectableTypeOverride = typeof(StaticRouter))]
    public class LocationStaticRouter(LocationCallbacks locationCallbacks, JsonUtil jsonUtil) : StaticRouter(jsonUtil, [
            new RouteAction(
                "/fika/location/raids",
                (
                    url,
                    info,
                    sessionId,
                    output
                ) => locationCallbacks.HandleGetRaids(url, info as GetRaidConfigurationRequestData, sessionId),
                typeof(GetRaidConfigurationRequestData)
                )
        ])
    {
    }
}
