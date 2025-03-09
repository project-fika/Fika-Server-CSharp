using FikaServer.Controllers;
using FikaServer.Models.Fika.Routes.Raid.Join;
using FikaServer.Models.Fika.Routes.Update;
using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Utils;

namespace FikaServer.Callbacks
{
    [Injectable]
    public class UpdateCallbacks(HttpResponseUtil httpResponseUtil, UpdateController updateController)
    {
        /// <summary>
        /// Handle /fika/update/ping
        /// </summary>
        /// <param name="url"></param>
        /// <param name="info"></param>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        public string HandlePing(string url, FikaUpdatePingRequestData info, string sessionID)
        {
            updateController.HandlePing(info);

            return httpResponseUtil.NullResponse();
        }

        /// <summary>
        /// Handle /fika/update/playerspawn
        /// </summary>
        /// <param name="url"></param>
        /// <param name="info"></param>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        public string HandlePlayerSpawn(string url, FikaUpdatePlayerSpawnRequestData info, string sessionID)
        {
            updateController.HandlePlayerSpawn(info);

            return httpResponseUtil.NullResponse();
        }

        /// <summary>
        /// Handle /fika/update/sethost
        /// </summary>
        /// <param name="url"></param>
        /// <param name="info"></param>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        public string HandleSetHost(string url, FikaUpdateSethostRequestData info, string sessionID)
        {
            updateController.HandleSetHost(info);

            return httpResponseUtil.NullResponse();
        }

        /// <summary>
        /// Handle /fika/update/setstatus
        /// </summary>
        /// <param name="url"></param>
        /// <param name="info"></param>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        public string HandleSetStatus(string url, FikaUpdateSetStatusRequestData info, string sessionID)
        {
            updateController.HandleSetStatus(info);

            return httpResponseUtil.NullResponse();
        }

        /// <summary>
        /// Handle /fika/update/addplayer
        /// </summary>
        /// <param name="url"></param>
        /// <param name="info"></param>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        public string HandleRaidAddPlayer(string url, FikaUpdateRaidAddPlayerData info, string sessionID)
        {
            updateController.HandleRaidAddPlayer(info);

            return httpResponseUtil.NullResponse();
        }

        /// <summary>
        /// Handle /fika/update/playerdied
        /// </summary>
        /// <param name="url"></param>
        /// <param name="info"></param>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        public string HandlePlayerDied(string url, FikaUpdateRaidAddPlayerData info, string sessionID)
        {
            updateController.HandleRaidPlayerDied(info);

            return httpResponseUtil.NullResponse();
        }
    }
}
