using FikaServer.Controllers;
using FikaServer.Models.Fika.Routes.Headless;
using FikaServer.Models.Fika.Routes.Raid;
using FikaServer.Models.Fika.Routes.Raid.Create;
using FikaServer.Models.Fika.Routes.Raid.Join;
using FikaServer.Models.Fika.Routes.Raid.Leave;
using SPTarkov.DI.Annotations;
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
        public async ValueTask<string> HandleRaidCreate(string url, FikaRaidCreateRequestData info, string sessionID)
        {
            return httpResponseUtil.NoBody(await raidController.HandleRaidCreate(info, sessionID));
        }

        /// <summary>
        /// Handle /fika/raid/join
        /// </summary>
        /// <param name="url"></param>
        /// <param name="info"></param>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        public ValueTask<string> HandleRaidJoin(string url, FikaRaidJoinRequestData info, string sessionID)
        {
            return new ValueTask<string>(httpResponseUtil.NoBody(raidController.HandleRaidJoin(info)));
        }

        /// <summary>
        /// Handle /fika/raid/leave
        /// </summary>
        /// <param name="url"></param>
        /// <param name="info"></param>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        public ValueTask<string> HandleRaidLeave(string url, FikaRaidLeaveRequestData info, string sessionID)
        {
            raidController.HandleRaidLeave(info);

            return new ValueTask<string>(httpResponseUtil.NullResponse());
        }

        /// <summary>
        /// Handle /fika/raid/gethost
        /// </summary>
        /// <param name="url"></param>
        /// <param name="info"></param>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        public ValueTask<string> HandleRaidGetHost(string url, FikaRaidServerIdRequestData info, string sessionID)
        {
            return new ValueTask<string>(httpResponseUtil.NoBody(raidController.HandleRaidGetHost(info)));
        }

        /// <summary>
        /// Handle /fika/raid/getsettings
        /// </summary>
        /// <param name="url"></param>
        /// <param name="info"></param>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        public ValueTask<string> HandleRaidGetSettings(string url, FikaRaidServerIdRequestData info, string sessionID)
        {
            return new ValueTask<string>(httpResponseUtil.NoBody(raidController.HandleRaidGetSettings(info)));
        }

        /// <summary>
        /// Handle /fika/raid/headless/start
        /// </summary>
        /// <param name="url"></param>
        /// <param name="info"></param>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        public ValueTask<string> HandleRaidStartHeadless(string url, StartHeadlessRequest info, string sessionID)
        {
            return new ValueTask<string>(httpResponseUtil.NoBody(raidController.HandleRaidStartHeadless(sessionID, info)));
        }

        /// <summary>
        /// Handle /fika/raid/registerPlayer
        /// </summary>
        /// <param name="url"></param>
        /// <param name="info"></param>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        public ValueTask<string> HandleRaidRegisterPlayer(string url, RegisterPlayerRequestData info, string sessionID)
        {
            raidController.HandleRaidRegisterPlayer(sessionID, info);

            return new ValueTask<string>(httpResponseUtil.NullResponse());
        }
    }
}
