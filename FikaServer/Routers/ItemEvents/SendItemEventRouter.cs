using FikaServer.Callbacks;
using FikaServer.Models.Fika;
using FikaServer.Models.Fika.SendItem;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI.Routing;

namespace FikaServer.Routers.ItemEvents;

[Injectable]
public sealed class SendItemEventRouter(SendItemCallbacks sendItemCallbacks)
    : ItemEventRouter([
        new ItemRouteAction<SendItemRequestData>(
            FikaItemEventRouter.SENDTOPLAYER,
            async (url, pmcData, body, sessionID, output, cancellationToken) => await sendItemCallbacks.HandleSendItem(body, sessionID)
        )
    ])
{ }
