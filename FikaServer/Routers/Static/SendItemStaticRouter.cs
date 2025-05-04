using FikaServer.Callbacks;
using FikaServer.Models.Fika.SendItem.AvailableReceivers;
using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Utils;

namespace FikaServer.Routers.Static
{
    [Injectable(InjectableTypeOverride = typeof(StaticRouter))]
    public class SendItemStaticRouter(SendItemCallbacks sendItemCallbacks, JsonUtil jsonUtil) : StaticRouter(jsonUtil, [
            new RouteAction(
                "/fika/senditem/availablereceivers",
                (
                    url,
                    info,
                    sessionId,
                    output
                ) => sendItemCallbacks.HandleAvailableReceivers(sessionId),
                typeof(SendItemAvailableReceiversRequestData)
                )
        ]);
}
