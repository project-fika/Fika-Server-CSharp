using FikaServer.Models.Fika.Routes.Raid.Join;
using FikaServer.Models.Fika.Routes.Update;
using FikaServer.Services;
using SPTarkov.DI.Annotations;

namespace FikaServer.Controllers
{
    [Injectable]
    public class UpdateController(MatchService matchService)
    {
        /// <summary>
        /// Handle /fika/update/ping
        /// </summary>
        /// <param name="request"></param>
        public void HandlePing(FikaUpdatePingRequestData request)
        {
            matchService.ResetTimeout(request.ServerId);
        }

        /// <summary>
        /// /fika/update/playerspawn
        /// </summary>
        /// <param name="request"></param>
        public void HandlePlayerSpawn(FikaUpdatePlayerSpawnRequestData request)
        {
            matchService.SetPlayerGroup(request.ServerId, request.ProfileId, request.GroupId);
        }

        /// <summary>
        /// Handle /fika/update/sethost
        /// </summary>
        /// <param name="request"></param>
        public void HandleSetHost(FikaUpdateSethostRequestData request)
        {
            matchService.SetMatchHost(request.ServerId, request.Ips, request.Port, request.NatPunch, request.IsHeadless);
        }

        /// <summary>
        /// Handle /fika/update/setstatus
        /// </summary>
        /// <param name="request"></param>
        public void HandleSetStatus(FikaUpdateSetStatusRequestData request)
        {
            matchService.SetMatchStatus(request.ServerId, request.Status);
        }

        /// <summary>
        /// Handle /fika/update/addplayer
        /// </summary>
        /// <param name="request"></param>
        public void HandleRaidAddPlayer(FikaUpdateRaidAddPlayerData request)
        {
            matchService.AddPlayerToMatch(request.ServerId, request.ProfileId, new Models.Fika.FikaPlayer
            {
                GroupId = string.Empty,
                IsDead = false,
                IsSpectator = request.IsSpectator
            });
        }

        /// <summary>
        /// Handle /fika/update/playerdied
        /// </summary>
        /// <param name="request"></param>
        public void HandleRaidPlayerDied(FikaUpdateRaidAddPlayerData request)
        {
            matchService.SetPlayerDead(request.ServerId, request.ProfileId);
        }
    }
}
