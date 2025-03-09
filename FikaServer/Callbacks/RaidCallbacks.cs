using FikaServer.Controllers;
using FikaServer.Models.Fika.Routes.Headless;
using FikaServer.Models.Fika.Routes.Raid;
using FikaServer.Models.Fika.Routes.Raid.Create;
using FikaServer.Models.Fika.Routes.Raid.Join;
using FikaServer.Models.Fika.Routes.Raid.Leave;
using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Models.Eft.InRaid;
using SPTarkov.Server.Core.Utils;

namespace FikaServer.Callbacks
{
    [Injectable]
    public class RaidCallbacks(HttpResponseUtil httpResponseUtil, RaidController raidController)
    {
        /// <summary>
        /// Handle /fika/raid/create
        /// </summary>
        /// <param name="url"></param>
        /// <param name="info"></param>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        public string HandleRaidCreate(string url, FikaRaidCreateRequestData info, string sessionID)
        {
            return httpResponseUtil.NoBody(raidController.HandleRaidCreate(info));
        }

        /// <summary>
        /// Handle /fika/raid/join
        /// </summary>
        /// <param name="url"></param>
        /// <param name="info"></param>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        public string HandleRaidJoin(string url, FikaRaidJoinRequestData info, string sessionID)
        {
            return httpResponseUtil.NoBody(raidController.HandleRaidJoin(info));
        }

        /// <summary>
        /// Handle /fika/raid/leave
        /// </summary>
        /// <param name="url"></param>
        /// <param name="info"></param>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        public string HandleRaidLeave(string url, FikaRaidLeaveRequestData info, string sessionID)
        {
            raidController.HandleRaidLeave(info);

            return httpResponseUtil.NullResponse();
        }

        /// <summary>
        /// Handle /fika/raid/gethost
        /// </summary>
        /// <param name="url"></param>
        /// <param name="info"></param>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        public string HandleRaidGetHost(string url, FikaRaidServerIdRequestData info, string sessionID)
        {
            return httpResponseUtil.NoBody(raidController.HandleRaidGetHost(info));
        }

        /// <summary>
        /// Handle /fika/raid/getsettings
        /// </summary>
        /// <param name="url"></param>
        /// <param name="info"></param>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        public string HandleRaidGetSettings(string url, FikaRaidServerIdRequestData info, string sessionID)
        {
            return httpResponseUtil.NoBody(raidController.HandleRaidGetSettings(info));
        }

        /// <summary>
        /// Handle /fika/raid/headless/start
        /// </summary>
        /// <param name="url"></param>
        /// <param name="info"></param>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        public string HandleRaidStartHeadless(string url, StartHeadlessRequest info, string sessionID)
        {
            return httpResponseUtil.NoBody(raidController.HandleRaidStartHeadless(sessionID, info));
        }

        /// <summary>
        /// Handle /fika/raid/registerPlayer
        /// </summary>
        /// <param name="url"></param>
        /// <param name="info"></param>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        public string HandleRaidRegisterPlayer(string url, RegisterPlayerRequestData info, string sessionID)
        {
            raidController.HandleRaidRegisterPlayer(sessionID, info);

            return httpResponseUtil.NullResponse();
        }
    }
}
