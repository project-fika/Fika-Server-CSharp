using FikaServer.Controllers;
using FikaServer.Models.Fika.Routes.Raid;
using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Utils;

namespace FikaServer.Callbacks
{
    [Injectable]
    public class HeadlessCallbacks(HttpResponseUtil httpResponseUtil, HeadlessController headlessController)
    {
        /// <summary>
        /// Handle /fika/headless/get
        /// </summary>
        /// <param name="url"></param>
        /// <param name="info"></param>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        public string HandleGetHeadlesses(string url, FikaRaidServerIdRequestData info, string sessionID)
        {
            return httpResponseUtil.NoBody(headlessController.HandleGetHeadlesses());
        }

        /// <summary>
        /// Handle /fika/headless/available
        /// </summary>
        /// <param name="url"></param>
        /// <param name="info"></param>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        public string HandleAvailableHeadlesses(string url, FikaRaidServerIdRequestData info, string sessionID)
        {
            return httpResponseUtil.NoBody(headlessController.HandleGetAvailableHeadlesses());
        }

        /// <summary>
        /// Handle /fika/headless/restartafterraidamount
        /// </summary>
        /// <param name="url"></param>
        /// <param name="info"></param>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        public string HandleRestartAfterRaidAmount(string url, FikaRaidServerIdRequestData info, string sessionID)
        {
            return httpResponseUtil.NoBody(headlessController.HandleRestartAfterRaidAmount);
        }
    }
}
