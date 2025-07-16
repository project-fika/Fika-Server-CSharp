using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;

namespace FikaServer.Services.Cache
{
    /// <summary>
    /// Used to cache profiles by nicknames
    /// </summary>
    /// <param name="saveServer"></param>
    [Injectable(InjectionType.Singleton)]
    public class FikaProfileService(ISptLogger<FikaProfileService> logger, SaveServer saveServer)
    {
        public Dictionary<string, SptProfile> AllProfiles
        {
            get
            {
                if (_profiles.Count == 0)
                {
                    RefreshProfiles();
                }

                return _profiles;
            }
        }

        private readonly Dictionary<string, SptProfile> _profiles = [];

        public SptProfile? GetProfileByName(string nickname)
        {
            if (_profiles.Count == 0)
            {
                RefreshProfiles();
            }

            if (_profiles.TryGetValue(nickname, out SptProfile? foundProfile))
            {
                return foundProfile;
            }
            else
            {
                RefreshProfiles();
                if (_profiles.TryGetValue(nickname, out foundProfile))
                {
                    return foundProfile;
                }
            }

            return null;
        }

        private void RefreshProfiles()
        {
            _profiles.Clear();
            Dictionary<MongoId, SptProfile>.ValueCollection profiles = saveServer.GetProfiles().Values;
            foreach (SptProfile profile in profiles)
            {
                string? nick = profile.CharacterData?.PmcData?.Info?.Nickname;
                if (!string.IsNullOrEmpty(nick))
                {
                    if (!_profiles.TryAdd(nick, profile))
                    {
                        logger.Error($"Failed to add {nick} to the profile cache. Someone is possibly using the same nickname");
                    }
                }
            }
        }
    }
}
