using FikaServer.Models.Fika.Insurance;
using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Match;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using System.Collections.Concurrent;

namespace FikaServer.Services
{
    [Injectable(InjectionType.Singleton)]
    public class InsuranceService(SaveServer saveServer, ItemHelper itemHelper, ISptLogger<InsuranceService> logger)
    {
        private ConcurrentDictionary<string, List<FikaInsurancePlayer>> _matchInsuranceInfo = [];

        public string GetMatchId(string sessionID)
        {
            foreach (var matchKvP in _matchInsuranceInfo)
            {
                var matchId = matchKvP.Key;
                var players = matchKvP.Value;

                if (players.Any(player => player.SessionID == sessionID))
                {
                    return matchId;
                }
            }

            //Todo: Nullable? Check?
            return string.Empty;
        }

        public void AddPlayerToMatchId(string matchId, string sessionID)
        {
            if (!_matchInsuranceInfo.TryGetValue(matchId, out var players))
            {
                players = [];

                _matchInsuranceInfo.TryAdd(matchId, players);
            }

            FikaInsurancePlayer player = new()
            {
                SessionID = sessionID,
                EndedRaid = false,
                LostItems = [],
                FoundItems = [],
                Inventory = [],
            };

            _matchInsuranceInfo.AddOrUpdate(matchId, [player],
               (key, existingPlayers) =>
               {
                   existingPlayers.Add(player);
                   return existingPlayers;
               });
        }

        public void OnEndLocalRaidRequest(string sessionID, string matchId, EndLocalRaidRequestData endLocalRaidRequest)
        {
            if (!_matchInsuranceInfo.TryGetValue(matchId, out var players))
            {
                logger.Error("[Fika Insurance] onEndLocalRaidRequest: matchId not found!");
                return;
            }

            foreach (var player in players)
            {
                if (player.SessionID == sessionID)
                {
                    continue;
                }

                // Map both the lost items and the current inventory
                player.LostItems = endLocalRaidRequest.LostInsuredItems?.Select((i) => i.Id).ToList() ?? [];
                player.Inventory = endLocalRaidRequest.Results?.Profile?.Inventory?.Items?.Select(i => i.Id).ToList() ?? [];
                player.EndedRaid = true;
            }
        }

        public void OnMatchEnd(string matchId)
        {
            if (!_matchInsuranceInfo.TryGetValue(matchId, out var players))
            {
                return;
            }

            foreach (var player in players)
            {
                // This player either crashed or the raid ended prematurely, eitherway we skip him.
                if (!player.EndedRaid)
                {
                    continue;
                }

                foreach (var nextPlayer in players)
                {
                    // Don't need to check the player we have in the base loop
                    if (player.SessionID == nextPlayer.SessionID)
                    {
                        continue;
                    }

                    // This player either crashed or the raid ended prematurely, eitherway we skip him.
                    if (!nextPlayer.EndedRaid)
                    {
                        continue;
                    }

                    // Find overlap between players other than the initial player we're looping over, if it contains the lost item id of the initial player we add it to foundItems
                    var overlap = nextPlayer.Inventory.Where(player.LostItems.Contains).ToList() ?? [];

                    // Add said overlap to player's found items
                    player.FoundItems.AddRange(overlap);
                }

                if (player.FoundItems.Count > 0)
                {
                    logger.Debug($"{player.SessionID} will lose ${player.FoundItems.Count}/${player.LostItems.Count} items in insurance`");
                    RemoveItemsFromInsurance(player.SessionID, player.FoundItems);
                }
            }

            _matchInsuranceInfo.TryRemove(matchId, out _);
        }

        private void RemoveItemsFromInsurance(string sessionID, List<string?> ids)
        {
            //Todo: Stub for now, implement method.
        }
    }
}
