using FikaServer.Helpers;
using FikaServer.Models.Enums;
using FikaServer.Models.Fika;
using FikaServer.Models.Fika.Presence;
using FikaServer.Models.Fika.Routes.Raid.Create;
using FikaServer.Services.Headless;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using System.Collections.Concurrent;

namespace FikaServer.Services
{
    [Injectable(InjectionType.Singleton)]
    public class MatchService(ISptLogger<MatchService> logger, LocationLifecycleService locationLifecycleService, SaveServer saveServer, ConfigService fikaConfig, HeadlessHelper headlessHelper, HeadlessService headlessService, InsuranceService insuranceService, PresenceService presenceService)
    {
        public readonly ConcurrentDictionary<string, FikaMatch> Matches = [];
        protected readonly ConcurrentDictionary<string, System.Timers.Timer> _timeoutIntervals = [];

        /// <summary>
        /// Adds a timeout interval for the given match
        /// </summary>
        /// <param name="matchId">The match ID to add a timeout for</param>
        private void AddTimeoutInterval(string matchId)
        {
            if (_timeoutIntervals.ContainsKey(matchId))
            {
                RemoveTimeoutInterval(matchId);
            }

            System.Timers.Timer timer = new(60 * 1000);
            timer.Elapsed += (sender, e) =>
            {
                FikaMatch? match = GetMatch(matchId);

                if (match != null)
                {
                    match.Timeout++;

                    if (match.Timeout >= fikaConfig.Config.Server.SessionTimeout)
                    {
                        EndMatch(matchId, EFikaMatchEndSessionMessage.PingTimeout);
                    }
                }

            };
        }

        /// <summary>
        /// Removes the timeout interval for the given match
        /// </summary>
        /// <param name="matchId">The match ID to remove a timeout for</param>
        private void RemoveTimeoutInterval(string matchId)
        {
            if (_timeoutIntervals.TryGetValue(matchId, out System.Timers.Timer? timer))
            {
                timer.Stop();
                timer.Dispose();
            }

            _timeoutIntervals.TryRemove(matchId, out _);
        }

        /// <summary>
        /// Get an ongoing match
        /// </summary>
        /// <param name="matchId">The match ID of the match to get</param>
        /// <returns>Returns the match object if a match is found with this match ID, returns null if not.</returns>
        public FikaMatch? GetMatch(string matchId)
        {
            if (Matches.TryGetValue(matchId, out FikaMatch? match))
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
        public FikaPlayer? GetPlayerInMatch(string matchId, string playerId)
        {
            if (!Matches.TryGetValue(matchId, out FikaMatch? match))
            {
                return null;
            }

            if (!match.Players.TryGetValue(playerId, out FikaPlayer? value))
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
        public string? GetMatchIdByPlayer(string playerId)
        {
            foreach ((string matchId, FikaMatch match) in Matches)
            {
                if (match.Players.ContainsKey(playerId))
                {
                    return matchId;
                }
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public string? GetMatchIdByProfile(string sessionId)
        {
            SptProfile profile = saveServer.GetProfile(sessionId);

            string? matchid = GetMatchIdByPlayer(profile.CharacterData.PmcData.Id) ?? GetMatchIdByPlayer(profile.CharacterData.ScavData.Id);

            return matchid;
        }

        /// <summary>
        /// Creates a new coop match
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool CreateMatch(FikaRaidCreateRequestData data, string sessionId)
        {
            if (Matches.ContainsKey(data.ServerId))
            {
                DeleteMatch(data.ServerId);
            }

            SPTarkov.Server.Core.Models.Eft.Common.LocationBase locationData = locationLifecycleService.GenerateLocationAndLoot(sessionId, data.Settings.Location);

            Matches.TryAdd(data.ServerId, new FikaMatch
            {
                Ips = [],
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
            });

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
        public void DeleteMatch(string matchId)
        {
            if (!Matches.ContainsKey(matchId))
            {
                return;
            }

            if (Matches.TryRemove(matchId, out _))
            {
                RemoveTimeoutInterval(matchId);
            }
            else
            {
                logger.Warning($"Failed to remove match: {matchId}");
            }
        }

        /// <summary>
        /// Ends the given match, logs a reason and removes the timeout interval
        /// </summary>
        /// <param name="matchId"></param>
        /// <param name="reason"></param>
        public void EndMatch(string matchId, EFikaMatchEndSessionMessage reason)
        {
            logger.Info($"Coop session {matchId} has ended {Enum.GetName(reason)}");

            if (headlessHelper.IsHeadlessClient(matchId))
            {
                headlessService.EndHeadlessRaid(matchId);
            }

            insuranceService.OnMatchEnd(matchId);
            DeleteMatch(matchId);
        }

        /// <summary>
        /// Updates the status of the given match
        /// </summary>
        /// <param name="matchId"></param>
        /// <param name="status"></param>
        public async Task SetMatchStatus(string matchId, EFikaMatchStatus status)
        {
            if (Matches.TryGetValue(matchId, out FikaMatch? match))
            {
                match.Status = status;
            }

            if (status == EFikaMatchStatus.COMPLETE)
            {
                await headlessService.SendJoinMessageToRequester(matchId);
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
        public void SetMatchHost(string matchId, string[] ips, int port, bool natPunch, bool isHeadless)
        {
            if (Matches.TryGetValue(matchId, out FikaMatch? match))
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
        public void ResetTimeout(string matchId)
        {
            if (Matches.TryGetValue(matchId, out FikaMatch? match))
            {
                match.Timeout = 0;
            }
        }

        /// <summary>
        /// Adds a player to a match
        /// </summary>
        /// <param name="matchId"></param>
        /// <param name="playerId"></param>
        /// <param name="data"></param>
        public void AddPlayerToMatch(string matchId, string playerId, FikaPlayer data)
        {
            if (Matches.TryGetValue(matchId, out FikaMatch? match))
            {
                match.Players.Add(playerId, data);
            }
            else
            {
                logger.Error($"Could not add player({playerId}) to match {matchId}");
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
        public void SetPlayerDead(string matchId, string playerId)
        {
            if (Matches.TryGetValue(matchId, out FikaMatch? match))
            {
                if (!match.Players.TryGetValue(playerId, out FikaPlayer? value))
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
        public void SetPlayerGroup(string matchId, string playerId, string groupId)
        {
            if (Matches.TryGetValue(matchId, out FikaMatch? match))
            {
                if (!match.Players.TryGetValue(playerId, out FikaPlayer? value))
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
        public void RemovePlayerFromMatch(string matchId, string playerId)
        {
            if (Matches.TryGetValue(matchId, out FikaMatch? match))
            {
                match.Players.Remove(playerId);

                presenceService.UpdatePlayerPresence(playerId, new FikaSetPresence
                {
                    Activity = EFikaPlayerPresences.IN_MENU
                });
            }
        }
    }
}
