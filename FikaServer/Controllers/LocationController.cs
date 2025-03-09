using FikaServer.Helpers;
using FikaServer.Models.Fika.Routes.Location;
using FikaServer.Services;
using SPTarkov.Common.Annotations;
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

            foreach (var kvp in matchService.Matches)
            {
                Dictionary<string, bool> players = [];

                foreach (var playerkvp in kvp.Value.Players)
                {
                    players[playerkvp.Key] = playerkvp.Value.IsDead;
                }

                string hostUsername = kvp.Value.HostUsername;

                if (kvp.Value.IsHeadless)
                {
                    hostUsername = headlessHelper.GetHeadlessNickname(hostUsername);
                }

                matches.Add(new FikaRaidResponse
                {
                    ServerId = kvp.Key,
                    HostUsername = hostUsername,
                    PlayerCount = kvp.Value.Players.Count,
                    Status = kvp.Value.Status,
                    Location = kvp.Value.RaidConfig.Location,
                    Side = kvp.Value.Side,
                    Time = kvp.Value.Time,
                    Players = players,
                    IsHeadless = kvp.Value.IsHeadless,
                    HeadlessRequesterNickname = headlessHelper.GetRequesterUsername(hostUsername) ?? "" // Set this to an empty string if there is no requester.
                });

            }

            return matches;
        }
    }
}
