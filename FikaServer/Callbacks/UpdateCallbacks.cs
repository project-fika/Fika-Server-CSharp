using FikaServer.Controllers;
using FikaServer.Models.Fika.Routes.Raid.Join;
using FikaServer.Models.Fika.Routes.Update;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
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
        public ValueTask<string> HandlePing(string url, FikaUpdatePingRequestData info, MongoId sessionID)
        {
            updateController.HandlePing(info);

            return new ValueTask<string>(httpResponseUtil.NullResponse());
        }

        /// <summary>
        /// Handle /fika/update/playerspawn
        /// </summary>
        /// <param name="url"></param>
        /// <param name="info"></param>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        public ValueTask<string> HandlePlayerSpawn(string url, FikaUpdatePlayerSpawnRequestData info, MongoId sessionID)
        {
            updateController.HandlePlayerSpawn(info);

            return new ValueTask<string>(httpResponseUtil.NullResponse());
        }

        /// <summary>
        /// Handle /fika/update/sethost
        /// </summary>
        /// <param name="url"></param>
        /// <param name="info"></param>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        public ValueTask<string> HandleSetHost(string url, FikaUpdateSethostRequestData info, MongoId sessionID)
        {
            updateController.HandleSetHost(info);

            return new ValueTask<string>(httpResponseUtil.NullResponse());
        }

        /// <summary>
        /// Handle /fika/update/setstatus
        /// </summary>
        /// <param name="url"></param>
        /// <param name="info"></param>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        public async ValueTask<string> HandleSetStatus(string url, FikaUpdateSetStatusRequestData info, MongoId sessionID)
        {
            await updateController.HandleSetStatus(info);

            return httpResponseUtil.NullResponse();
        }

        /// <summary>
        /// Handle /fika/update/addplayer
        /// </summary>
        /// <param name="url"></param>
        /// <param name="info"></param>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        public ValueTask<string> HandleRaidAddPlayer(string url, FikaUpdateRaidAddPlayerData info, MongoId sessionID)
        {
            updateController.HandleRaidAddPlayer(info);

            return new ValueTask<string>(httpResponseUtil.NullResponse());
        }

        /// <summary>
        /// Handle /fika/update/playerdied
        /// </summary>
        /// <param name="url"></param>
        /// <param name="info"></param>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        public ValueTask<string> HandlePlayerDied(string url, FikaUpdateRaidAddPlayerData info, MongoId sessionID)
        {
            updateController.HandleRaidPlayerDied(info);

            return new ValueTask<string>(httpResponseUtil.NullResponse());
        }
    }
}
