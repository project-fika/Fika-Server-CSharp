using FikaServer.Controllers;
using FikaServer.Models.Fika.SendItem;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Utils;

namespace FikaServer.Callbacks;

[Injectable]
public class SendItemCallbacks(HttpResponseUtil httpResponseUtil, SendItemController sendItemController)
{
    public async ValueTask<ItemEventRouterResponse> HandleSendItem(SendItemRequestData body, MongoId sessionID)
    {
        return await sendItemController.SendItem(body, sessionID);
    }

    /// <summary>
    /// Handle /fika/senditem/availablereceivers
    /// </summary>
    public ValueTask<string> HandleAvailableReceivers(string sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.NoBody(sendItemController.HandleAvailableReceivers(sessionID)));
    }
}
