using FikaServer.Callbacks;
using FikaServer.Models.Fika;
using FikaServer.Models.Fika.SendItem;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Request;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;

namespace FikaServer.Routers.ItemEvents;

[Injectable]
public class SendItemEventRouter(SendItemCallbacks sendItemCallbacks) : ItemEventRouterDefinition
{
    public override async ValueTask<ItemEventRouterResponse> HandleItemEvent(string url, PmcData pmcData, BaseInteractionRequestData body, MongoId sessionID, ItemEventRouterResponse output)
    {
        return url switch
        {
            FikaItemEventRouter.SENDTOPLAYER => await sendItemCallbacks.HandleSendItem(pmcData, body as SendItemRequestData, sessionID),
            _ => throw new Exception($"SendItemEventRouter being used when it cant handle route {url}")
        };
    }

    protected override List<HandledRoute> GetHandledRoutes()
    {
        return [new(FikaItemEventRouter.SENDTOPLAYER, false)];
    }
}
