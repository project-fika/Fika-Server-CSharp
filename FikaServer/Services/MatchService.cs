using FikaServer.Helpers;
using FikaServer.Models.Enums;
using FikaServer.Models.Fika;
using FikaServer.Models.Fika.Presence;
using FikaServer.Models.Fika.Routes.Raid.Create;
using FikaServer.Services.Headless;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using System.Collections.Concurrent;

namespace FikaServer.Services;

[Injectable(InjectionType.Singleton)]
public class MatchService(ISptLogger<MatchService> logger, LocationLifecycleService locationLifecycleService,
    SaveServer saveServer, ConfigService fikaConfig, HeadlessHelper headlessHelper, HeadlessService headlessService,
    InsuranceService insuranceService, PresenceService presenceService, WebhookService webhookService)
{
    public readonly ConcurrentDictionary<MongoId, FikaMatch> Matches = [];
    protected readonly ConcurrentDictionary<MongoId, Timer> _timeoutIntervals = [];

    /// <summary>
    /// Adds a timeout interval for the given match
    /// </summary>
    /// <param name="matchId">The match ID to add a timeout for</param>
    private void AddTimeoutInterval(MongoId matchId)
    {
        if (_timeoutIntervals.ContainsKey(matchId))
        {
            RemoveTimeoutInterval(matchId);
        }

        Timer timer = new(_ =>
        {
            var match = GetMatch(matchId);
            if (match != null)
            {
                if (match.Timeout++ >= fikaConfig.Config.Server.SessionTimeout)
                {
                    EndMatch(matchId, EFikaMatchEndSessionMessage.PingTimeout);
                }
            }

        }, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));

        if (!_timeoutIntervals.TryAdd(matchId, timer))
        {
            logger.Error($"Failed to add match {matchId} to timers");
            timer.Dispose();
        }
    }

    /// <summary>
    /// Removes the timeout interval for the given match
    /// </summary>
    /// <param name="matchId">The match ID to remove a timeout for</param>
    private void RemoveTimeoutInterval(MongoId matchId)
    {
        if (_timeoutIntervals.TryRemove(matchId, out var timer))
        {
            timer.Dispose();
        }
        else
        {
            logger.Error($"Could not delete match {matchId} from timers");
        }
    }

    /// <summary>
    /// Get an ongoing match
    /// </summary>
    /// <param name="matchId">The match ID of the match to get</param>
    /// <returns>Returns the match object if a match is found with this match ID, returns null if not.</returns>
    public FikaMatch? GetMatch(MongoId matchId)
    {
        if (Matches.TryGetValue(matchId, out var match))
        {
            return match;
        }

        return null;
    }

    /// <summary>
    /// Gets a player in an ongoing match
    /// </summary>
    /// <param name="matchId">The match ID of what match the player is in</param>
    /// <param name="playerId">The player ID to look for</param>
    /// <returns>Returns a FikaPlayer object if the player is found, returns null if not.</returns>
    public FikaPlayer? GetPlayerInMatch(MongoId matchId, MongoId playerId)
    {
        if (!Matches.TryGetValue(matchId, out var match))
        {
            return null;
        }

        if (!match.Players.TryGetValue(playerId, out var value))
        {
            return null;
        }

        return value;
    }

    /// <summary>
    /// Returns the match ID that has a player with the given player ID.
    /// </summary>
    /// <param name="playerId">The ID of the player whose match ID is to be found.</param>
    /// <returns>The match ID containing the player, or null if the player isn't in a match.</returns>
    public MongoId? GetMatchIdByPlayer(MongoId playerId)
    {
        foreach ((var matchId, var match) in Matches)
        {
            if (match.Players.ContainsKey(playerId))
            {
                return matchId;
            }
        }

        return null;
    }

    /// <summary>
    /// Retrieves the match identifier associated with the specified profile session.
    /// </summary>
    /// <param name="sessionId">The unique identifier of the profile session for which to retrieve the match ID.</param>
    /// <returns>A nullable match identifier associated with the profile, or null if no match is found or the profile does exist.</returns>
    public MongoId? GetMatchIdByProfile(MongoId sessionId)
    {
        var profile = saveServer.GetProfile(sessionId);
        if (profile == null)
        {
            return null;
        }

        return GetMatchIdByPlayer(profile.CharacterData.PmcData.Id.GetValueOrDefault())
            ?? GetMatchIdByPlayer(profile.CharacterData.ScavData.Id.GetValueOrDefault());
    }

    /// <summary>
    /// Creates a new match with the specified configuration and associates it with the given session.
    /// </summary>
    /// <remarks>If a match with the same server ID already exists, it is deleted before creating the new
    /// match. The method also initializes the match with the provided settings and adds the host player to the
    /// match.</remarks>
    /// <param name="data">The configuration data for the match to be created, including server information, host details, and raid
    /// settings. Cannot be null.</param>
    /// <param name="sessionId">The unique identifier of the session to associate with the new match.</param>
    /// <returns>true if the match was successfully created and registered; otherwise, false.</returns>
    public bool CreateMatch(FikaRaidCreateRequestData data, MongoId sessionId)
    {
        if (Matches.ContainsKey(data.ServerId))
        {
            DeleteMatch(data.ServerId);
        }

        var locationData = locationLifecycleService.GenerateLocationAndLoot(sessionId, data.Settings.Location);

        FikaMatch match = new()
        {
            Ips = [],
            ServerGuid = data.ServerGuid,
            Port = 0,
            HostUsername = data.HostUsername,
            Timestamp = data.Timestamp,
            RaidConfig = data.Settings,
            LocationData = locationData,
            Status = EFikaMatchStatus.LOADING,
            Timeout = 0,
            Players = [],
            GameVersion = data.GameVersion,
            CRC32 = data.CRC32,
            Side = data.Side,
            Time = data.Time,
            RaidCode = data.RaidCode,
            NatPunch = false,
            IsHeadless = false,
            Raids = 0
        };

        if (!Matches.TryAdd(data.ServerId, match))
        {
            logger.Error($"Failed to create match {data.ServerId}");
        }

        AddTimeoutInterval(data.ServerId);

        AddPlayerToMatch(data.ServerId, data.ServerId, new FikaPlayer
        {
            GroupId = string.Empty,
            IsDead = false,
            IsSpectator = data.IsSpectator
        });

        return Matches.ContainsKey(data.ServerId) && _timeoutIntervals.ContainsKey(data.ServerId);
    }

    /// <summary>
    /// Deletes a coop match and removes the timeout interval
    /// </summary>
    /// <param name="matchId">The match Id to remove</param>
    public async ValueTask DeleteMatch(MongoId matchId)
    {
        if (Matches.TryRemove(matchId, out var match))
        {
            RemoveTimeoutInterval(matchId);
            logger.Info($"Deleted match [{matchId}]");

            await webhookService.SendWebhookMessage($"{match.HostUsername}'s raid has ended.");
        }
        else
        {
            logger.Warning($"Failed to delete match: {matchId}");
        }
    }

    /// <summary>
    /// Ends the given match, logs a reason and removes the timeout interval
    /// </summary>
    /// <param name="matchId"></param>
    /// <param name="reason"></param>
    public async ValueTask EndMatch(MongoId matchId, EFikaMatchEndSessionMessage reason)
    {
        logger.Info($"Coop session {matchId} has ended {Enum.GetName(reason)}");

        if (headlessHelper.IsHeadlessClient(matchId))
        {
            headlessService.EndHeadlessRaid(matchId);
        }

        insuranceService.OnMatchEnd(matchId);
        await DeleteMatch(matchId);
    }

    /// <summary>
    /// Updates the status of the given match
    /// </summary>
    /// <param name="matchId"></param>
    /// <param name="status"></param>
    public async Task SetMatchStatus(MongoId matchId, EFikaMatchStatus status)
    {
        if (Matches.TryGetValue(matchId, out var match))
        {
            match.Status = status;

            if (status == EFikaMatchStatus.COMPLETE && match.IsHeadless)
            {
                await headlessService.SendJoinMessageToRequester(matchId);
            }
        }
        else
        {
            logger.Error($"Failed to find match {matchId} when trying to set status to {status}");
        }
    }

    /// <summary>
    /// Sets the ip and port for the given match
    /// </summary>
    /// <param name="matchId"></param>
    /// <param name="ips"></param>
    /// <param name="port"></param>
    /// <param name="natPunch"></param>
    /// <param name="isHeadless"></param>
    public void SetMatchHost(MongoId matchId, string[] ips, int port, bool natPunch, bool isHeadless)
    {
        if (Matches.TryGetValue(matchId, out var match))
        {
            match.Ips = ips;
            match.Port = port;
            match.NatPunch = natPunch;
            match.IsHeadless = isHeadless;
        }
    }

    /// <summary>
    /// Resets the timeout of the given match
    /// </summary>
    /// <param name="matchId"></param>
    public void ResetTimeout(MongoId matchId)
    {
        if (Matches.TryGetValue(matchId, out var match))
        {
            match.Timeout = 0;
            if (_timeoutIntervals.TryGetValue(matchId, out var timer))
            {
                // Restart the 1-minute countdown
                timer.Change(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
            }
        }
    }

    /// <summary>
    /// Adds a player to a match
    /// </summary>
    /// <param name="matchId"></param>
    /// <param name="playerId"></param>
    /// <param name="data"></param>
    public void AddPlayerToMatch(MongoId matchId, MongoId playerId, FikaPlayer data)
    {
        if (Matches.TryGetValue(matchId, out var match))
        {
            if (!match.Players.TryAdd(playerId, data))
            {
                logger.Error($"Could not add player ({playerId}) to match {matchId}, they were most likely already in it");
            }
        }
        else
        {
            logger.Error($"Could not add player ({playerId}) to match {matchId}");
            return;
        }

        insuranceService.AddPlayerToMatchId(matchId, playerId);

        if (headlessHelper.IsHeadlessClient(matchId))
        {
            headlessService.AddPlayerToHeadlessMatch(matchId, playerId);
        }

        presenceService.UpdatePlayerPresence(playerId, new FikaSetPresence
        {
            Activity = EFikaPlayerPresences.IN_RAID,
            RaidInformation = new FikaRaidPresence
            {
                Location = match.LocationData.Id,
                Side = match.Side,
                Time = match.Time,
            }
        });
    }

    /// <summary>
    /// Sets a player to dead
    /// </summary>
    /// <param name="matchId"></param>
    /// <param name="playerId"></param>
    public void SetPlayerDead(MongoId matchId, MongoId playerId)
    {
        if (Matches.TryGetValue(matchId, out var match))
        {
            if (!match.Players.TryGetValue(playerId, out var value))
            {
                return;
            }

            value.IsDead = true;
        }
    }

    /// <summary>
    /// Sets the groupId for a player
    /// </summary>
    /// <param name="matchId"></param>
    /// <param name="playerId"></param>
    /// <param name="groupId"></param>
    public void SetPlayerGroup(MongoId matchId, MongoId playerId, string groupId)
    {
        if (Matches.TryGetValue(matchId, out var match))
        {
            if (!match.Players.TryGetValue(playerId, out var value))
            {
                return;
            }

            value.GroupId = groupId;
        }
    }

    /// <summary>
    /// Removes a player from a match
    /// </summary>
    /// <param name="matchId"></param>
    /// <param name="playerId"></param>
    public void RemovePlayerFromMatch(MongoId matchId, MongoId playerId)
    {
        if (Matches.TryGetValue(matchId, out var match))
        {
            match.Players.Remove(playerId);

            presenceService.UpdatePlayerPresence(playerId, new FikaSetPresence
            {
                Activity = EFikaPlayerPresences.IN_MENU
            });
        }
    }
}
