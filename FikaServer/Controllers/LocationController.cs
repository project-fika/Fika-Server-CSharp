using FikaServer.Helpers;
using FikaServer.Models.Fika;
using FikaServer.Models.Fika.Routes.Location;
using FikaServer.Services;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Match;

namespace FikaServer.Controllers
{
    [Injectable]
    public class LocationController(MatchService matchService, HeadlessHelper headlessHelper)
    {

        /// <summary>
        /// Handle /fika/location/raids
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public List<FikaRaidResponse> HandleGetRaids(GetRaidConfigurationRequestData request)
        {
            List<FikaRaidResponse> matches = [];

            foreach ((MongoId serverId, FikaMatch match) in matchService.Matches)
            {
                Dictionary<MongoId, bool> players = [];

                foreach ((MongoId playerId, FikaPlayer player) in match.Players)
                {
                    players[playerId] = player.IsDead;
                }

                string hostUsername = match.HostUsername;
                if (match.IsHeadless)
                {
                    hostUsername = headlessHelper.GetHeadlessNickname(serverId);
                }

                matches.Add(new FikaRaidResponse
                {
                    ServerId = serverId,
                    HostUsername = hostUsername,
                    PlayerCount = match.Players.Count,
                    Status = match.Status,
                    Location = match.RaidConfig.Location,
                    Side = match.Side,
                    Time = match.Time,
                    Players = players,
                    IsHeadless = match.IsHeadless,
                    HeadlessRequesterNickname = headlessHelper.GetRequesterUsername(serverId) ?? "" // Set this to an empty string if there is no requester.
                });

            }

            return matches;
        }


    }
}
