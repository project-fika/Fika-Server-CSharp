using System.Collections.Concurrent;
using FikaServer.Models.Fika.Insurance;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Match;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;

namespace FikaServer.Services;

[Injectable(InjectionType.Singleton)]
public class InsuranceService(SaveServer saveServer, ItemHelper itemHelper, ISptLogger<InsuranceService> logger)
{
    private readonly ConcurrentDictionary<string, List<FikaInsurancePlayer>> _matchInsuranceInfo = [];

    /// <summary>
    /// Gets the match the player is part of
    /// </summary>
    /// <param name="sessionID">The profile id to search for</param>
    /// <returns></returns>
    public string GetMatchId(string sessionID)
    {
        foreach ((var matchId, var players) in _matchInsuranceInfo)
        {
            if (players.Any(player => player.SessionID == sessionID))
            {
                return matchId;
            }
        }

        //Todo: Nullable? Check?
        return string.Empty;
    }

    /// <summary>
    /// Adds a player to the a match
    /// </summary>
    /// <param name="matchId">The id of the match</param>
    /// <param name="sessionID">The profile id to add</param>
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

    /// <summary>
    /// Executed when a clients requests to end a raid
    /// </summary>
    /// <param name="sessionID">The profile id that requested the end</param>
    /// <param name="matchId">The id of the match</param>
    /// <param name="endLocalRaidRequest">The request data</param>
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
            player.LostItems = endLocalRaidRequest.LostInsuredItems?
                .Select((i) => i.Id)
                .ToList() ?? [];
            player.Inventory = endLocalRaidRequest.Results?.Profile?.Inventory?.Items?
                .Select(i => i.Id)
                .ToList() ?? [];
            player.EndedRaid = true;
        }
    }

    /// <summary>
    /// Executed when a match ends
    /// </summary>
    /// <param name="matchId">The id of the match</param>
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
                var overlap = nextPlayer.Inventory
                    .Where(player.LostItems.Contains)
                    .ToList() ?? [];

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

    /// <summary>
    /// Removes all provided itemIds from a profile's <see cref="Insurance"/>
    /// </summary>
    /// <param name="sessionID">The profile to remove from</param>
    /// <param name="ids">The item ids to search for and remove</param>
    /// <exception cref="NullReferenceException"></exception>
    private void RemoveItemsFromInsurance(string sessionID, List<MongoId> ids)
    {
        var profile = saveServer.GetProfile(sessionID)
            ?? throw new NullReferenceException("[Fika Insurance] Profile was null");

        List<Item> toRemove = [];
        for (var i = 0; i < profile.InsuranceList?.Count; i++)
        {
            var insurance = profile.InsuranceList[i];
            if (insurance.Items == null)
            {
                continue;
            }

            foreach (string? itemId in ids)
            {
                var item = insurance.Items
                    .FirstOrDefault(x => x.Id == itemId);

                if (item is null)
                {
                    continue;
                }

                // Remove soft inserts out of armor and helmets
                if (itemHelper.IsOfBaseclasses(item.Template, [BaseClasses.ARMOR, BaseClasses.HEADWEAR]))
                {
                    logger.Debug($"[Fika Insurance] {itemId} is armor or helmet");
                    var children = insurance.Items
                        .Where(x => x.ParentId == itemId && itemHelper.IsOfBaseclass(x.Template, BaseClasses.BUILT_IN_INSERTS));

                    foreach (var childItem in children)
                    {
                        // Soft inserts are not insured
                        if (!ids.Contains(childItem.Id))
                        {
                            toRemove.Add(childItem);
                        }
                    }
                }

                // Remove children of the item
                foreach (var itemToRemove in toRemove)
                {
                    if (!insurance.Items.Remove(itemToRemove))
                    {
                        logger.Debug($"[Fika Insurance] Unable to remove the item {itemToRemove.Id}");
                    }
                }

                // Clear list for next iteration
                toRemove.Clear();

                // Remove the original item (parent)
                insurance.Items?.Remove(item);
            }

            // If all insured items are gone, remove the entry
            if (insurance.Items?.Count == 0)
            {
                profile.InsuranceList.Remove(insurance);
            }
        }
    }
}
