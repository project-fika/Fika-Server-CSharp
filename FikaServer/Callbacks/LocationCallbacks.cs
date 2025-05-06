using FikaServer.Controllers;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Eft.Match;
using SPTarkov.Server.Core.Utils;

namespace FikaServer.Callbacks
{
    [Injectable]
    public class LocationCallbacks(HttpResponseUtil httpResponseUtil, LocationController locationController)
    {
        /// <summary>
        /// Handle /fika/location/raids
        /// </summary>
        /// <param name="url"></param>
        /// <param name="info"></param>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        public string HandleGetRaids(string url, GetRaidConfigurationRequestData info, string sessionID)
        {
            return httpResponseUtil.NoBody(locationController.HandleGetRaids(info));
        }
    }
}
