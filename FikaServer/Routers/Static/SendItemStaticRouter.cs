using FikaServer.Callbacks;
using FikaServer.Models.Fika.SendItem.AvailableReceivers;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Utils;

namespace FikaServer.Routers.Static;

[Injectable]
public class SendItemStaticRouter(SendItemCallbacks sendItemCallbacks, JsonUtil jsonUtil) : StaticRouter(jsonUtil, [
        new RouteAction<SendItemAvailableReceiversRequestData>(
            "/fika/senditem/availablereceivers",
            async (
                url,
                info,
                sessionId,
                output
            ) => await sendItemCallbacks.HandleAvailableReceivers(sessionId)
            )
    ]);
