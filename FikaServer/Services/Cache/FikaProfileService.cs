using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;

namespace FikaServer.Services.Cache;

/// <summary>
/// Used to cache profiles by nicknames
/// </summary>
/// <param name="saveServer"></param>
[Injectable(InjectionType.Singleton)]
public class FikaProfileService(ISptLogger<FikaProfileService> logger, SaveServer saveServer)
{
    private readonly Dictionary<string, MongoId> _profiles = [];

    public Dictionary<string, MongoId> GetAllProfiles()
    {
        if (_profiles.Count != saveServer.GetProfiles().Count)
        {
            RefreshProfiles();
        }

        return _profiles;
    }

    public MongoId? GetProfileIdByNickname(string nickname)
    {
        Dictionary<string, MongoId> profiles = GetAllProfiles();

        if (profiles.TryGetValue(nickname, out MongoId foundId))
        {
            return foundId;
        }
        else
        {
            RefreshProfiles();
            if (profiles.TryGetValue(nickname, out foundId))
            {
                return foundId;
            }
        }

        return null;
    }

    public SptProfile? GetProfileByNickname(string nickname)
    {
        Dictionary<string, MongoId> profiles = GetAllProfiles();

        if (profiles.TryGetValue(nickname, out MongoId foundId))
        {
            SptProfile profile = saveServer.GetProfile(foundId);
            if (profile != null)
            {
                return profile;
            }
        }
        else
        {
            RefreshProfiles();
            if (profiles.TryGetValue(nickname, out foundId))
            {
                SptProfile profile = saveServer.GetProfile(foundId);
                if (profile != null)
                {
                    return profile;
                }
            }
        }

        return null;
    }

    private void RefreshProfiles()
    {
        _profiles.Clear();
        Dictionary<MongoId, SptProfile> profiles = saveServer.GetProfiles();
        foreach ((MongoId id, SptProfile profile) in profiles)
        {
            if (!profile.HasProfileData())
            {
                continue;
            }

            string? nick = profile?.CharacterData?.PmcData?.Info?.Nickname;
            if (!string.IsNullOrEmpty(nick))
            {
                if (!_profiles.TryAdd(nick, id))
                {
                    logger.Error($"Failed to add {nick} to the profile cache. Someone is possibly using the same nickname");
                }
            }
        }
    }
}
