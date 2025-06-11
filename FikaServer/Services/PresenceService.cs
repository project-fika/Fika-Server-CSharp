using FikaServer.Models.Enums;
using FikaServer.Models.Fika.Presence;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using System.Collections.Concurrent;

namespace FikaServer.Services
{
    [Injectable(InjectionType.Singleton)]
    public class PresenceService(SaveServer saveServer, TimeUtil timeUtil, ISptLogger<PresenceService> logger)
    {
        private readonly ConcurrentDictionary<string, FikaPlayerPresence> _onlinePlayers = [];

        public void AddPlayerPresence(string sessionID)
        {
            SptProfile profile = saveServer.GetProfile(sessionID);

            if (profile == null)
            {
                return;
            }

            FikaPlayerPresence data = new()
            {
                Nickname = profile.CharacterData.PmcData.Info.Nickname,
                Level = profile.CharacterData.PmcData.Info.Level ?? 0,
                Activity = EFikaPlayerPresences.IN_MENU,
                ActivityStartedTimestamp = timeUtil.GetTimeStamp()
            };

            logger.Debug($"[Fika Presence] Adding player: {data.Nickname}");

            _onlinePlayers.TryAdd(sessionID, data);
        }

        public List<FikaPlayerPresence> GetAllPlayersPresence()
        {
            return [.. _onlinePlayers.Values];
        }

        public void UpdatePlayerPresence(string sessionID, FikaSetPresence NewPresence)
        {
            if (!_onlinePlayers.TryGetValue(sessionID, out FikaPlayerPresence currentPresence))
            {
                return;
            }

            SptProfile profile = saveServer.GetProfile(sessionID);

            _onlinePlayers.TryUpdate(sessionID, new FikaPlayerPresence
            {
                Nickname = profile.CharacterData.PmcData.Info.Nickname,
                Level = profile.CharacterData.PmcData.Info.Level ?? 0,
                Activity = NewPresence.Activity,
                ActivityStartedTimestamp = timeUtil.GetTimeStamp(),
                RaidInformation = NewPresence.RaidInformation
            }, currentPresence);
        }

        public void RemovePlayerPresence(string sessionID)
        {
            _onlinePlayers.TryRemove(sessionID, out _);
        }
    }
}
