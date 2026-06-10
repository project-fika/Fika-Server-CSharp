using System.Collections.Concurrent;
using FikaServer.Models.Fika.Insurance;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
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
    private readonly ConcurrentDictionary<MongoId, List<FikaInsurancePlayer>> _matchInsuranceInfo = [];

    /// <summary>
    /// Gets the match the player is part of
    /// </summary>
    /// <param name="sessionID">The profile id to search for</param>
    /// <returns>The match id or <see langword="null"/> if not found</returns>
    public MongoId? GetMatchId(MongoId sessionID)
    {
        foreach ((var matchId, var players) in _matchInsuranceInfo)
        {
            if (players.Any(player => player.SessionID == sessionID))
            {
                return matchId;
            }
        }

        return null;
    }

    /// <summary>
    /// Adds a player to the a match
    /// </summary>
    /// <param name="matchId">The id of the match</param>
    /// <param name="sessionID">The profile id to add</param>
    public void AddPlayerToMatchId(MongoId matchId, MongoId sessionID)
    {
        if (!_matchInsuranceInfo.TryGetValue(matchId, out var players))
        {
            players = [];
            _matchInsuranceInfo.TryAdd(matchId, players);
        }

        var profile = saveServer.GetProfile(sessionID)
            ?? throw new NullReferenceException("[Fika Insurance] Profile was null");

        var pmcData = profile.CharacterData?.PmcData;
        var equipmentId = pmcData!.Inventory?.Equipment!;
        var equipment = pmcData!.Inventory!.Items!
            .Where(i => i.ParentId! == equipmentId || pmcData.DoesItemHaveRootId(i, equipmentId.Value))
            .Select(i => i.Id)
            .ToArray();
        var insuredItems = pmcData.InsuredItems!
            .Where(i => i.ItemId.HasValue && equipment.Contains(i.ItemId.Value))
            .Select(i => i.ItemId!.Value)
            .ToArray();

        logger.Debug($"[Fika Insurance] {sessionID} brought {insuredItems.Length} items into the raid");

        FikaInsurancePlayer player = new()
        {
            SessionID = sessionID,
            EndedRaid = false,
            InsuredItemsBroughtToRaid = insuredItems
        };

        _matchInsuranceInfo.AddOrUpdate(matchId, [player],
           (_, existingPlayers) =>
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
    public void OnEndLocalRaidRequest(MongoId sessionID, MongoId? matchId, EndLocalRaidRequestData endLocalRaidRequest)
    {
        if (!matchId.HasValue)
        {
            logger.Error($"{sessionID} ended their raid but the match could not be found");
            return;
        }

        if (!_matchInsuranceInfo.TryGetValue(matchId.Value, out var players))
        {
            logger.Error("[Fika Insurance] onEndLocalRaidRequest: matchId not found!");
            return;
        }

        foreach (var player in players)
        {
            if (player.SessionID != sessionID)
            {
                continue;
            }

            // Map both the lost items and the current inventory
            player.InventoryAfterRaid = endLocalRaidRequest.Results?.Profile?.Inventory?.Items?
                .Select(i => i.Id)
                .ToArray();
            player.EndedRaid = true;

            logger.Debug($"[Fika Insurance] {sessionID} brought {player.InventoryAfterRaid?.Length} items out from the raid");
        }
    }

    /// <summary>
    /// Executed when a match ends
    /// </summary>
    /// <param name="matchId">The id of the match</param>
    public void OnMatchEnd(MongoId matchId)
    {
        if (!_matchInsuranceInfo.TryGetValue(matchId, out var players))
        {
            logger.Error($"[Fika Insurance] Could not find match with ID {matchId}");
            return;
        }

        logger.Debug($"[Fika Insurance] Iterating over {players.Count} players");

        var postRaidInventoryMap = players
            .Where(p => p.EndedRaid && p.InventoryAfterRaid != null)
            .ToDictionary(p => p.SessionID, p => p.InventoryAfterRaid!);

        foreach (var player in players)
        {
            if (!player.EndedRaid || player.InsuredItemsBroughtToRaid == null)
            {
                continue;
            }

            var otherPlayersPostRaidInventories = new HashSet<MongoId>(postRaidInventoryMap
                .Where(kvp => kvp.Key != player.SessionID)
                .SelectMany(kvp => kvp.Value));

            var lootedItemsForThisPlayer = player.InsuredItemsBroughtToRaid
                .Where(otherPlayersPostRaidInventories.Contains)
                .ToArray();

            if (lootedItemsForThisPlayer.Length > 0)
            {
                logger.Debug($"{player.SessionID} will lose {lootedItemsForThisPlayer.Length} items in insurance");
                RemoveItemsFromInsurance(player.SessionID, lootedItemsForThisPlayer);
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
    private void RemoveItemsFromInsurance(MongoId sessionID, MongoId[] ids)
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

            foreach (var itemId in ids)
            {
                var item = insurance.Items!
                    .FirstOrDefault(x => x.Id == itemId);

                if (item is null)
                {
                    continue;
                }

                // Remove soft inserts out of armor and helmets
                if (itemHelper.IsOfBaseclasses(item.Template, [BaseClasses.ARMOR, BaseClasses.HEADWEAR]))
                {
                    logger.Debug($"[Fika Insurance] {itemId} is armor or helmet");
                    var children = insurance.Items!
                        .Where(x => x.ParentId! == itemId && itemHelper.IsOfBaseclass(x.Template, BaseClasses.BUILT_IN_INSERTS));

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
                    if (!insurance.Items!.Remove(itemToRemove))
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
