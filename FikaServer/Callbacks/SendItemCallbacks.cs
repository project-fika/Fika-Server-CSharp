using FikaServer.Controllers;
using FikaServer.Models.Fika.SendItem;
using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Utils;

namespace FikaServer.Callbacks
{
    [Injectable]
    public class SendItemCallbacks(HttpResponseUtil httpResponseUtil, SendItemController sendItemController)
    {
        public ItemEventRouterResponse HandleSendItem(PmcData pmcData, SendItemRequestData body, string sessionID)
        {
            return sendItemController.SendItem(pmcData, body, sessionID);
        }

        /// <summary>
        /// Handle /fika/senditem/availablereceivers
        /// </summary>
        public string HandleAvailableReceivers(string sessionID)
        {
            return httpResponseUtil.NoBody(sendItemController.HandleAvailableReceivers(sessionID));
        }
    }
}
