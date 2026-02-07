using System.Collections.Concurrent;
using FikaServer.Models.Fika;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;

namespace FikaServer.Services.Cache;

[Injectable(InjectionType.Singleton)]
public class PlayerRelationsService(ProfileHelper profileHelper, ConfigService FikaConfig, JsonUtil jsonUtil, ISptLogger<PlayerRelationsService> logger)
{
    private readonly string _playerRelationsFullPath = Path.Join(FikaConfig.ModPath, "database");
    private ConcurrentDictionary<MongoId, FikaPlayerRelations> _playerRelations = [];

    public List<MongoId> Keys
    {
        get
        {
            return [.. _playerRelations.Keys];
        }
    }

    public List<FikaPlayerRelations> Values
    {
        get
        {
            return [.. _playerRelations.Values];
        }
    }

    public async Task OnPreLoad()
    {
        if (!Directory.Exists(_playerRelationsFullPath))
        {
            Directory.CreateDirectory(_playerRelationsFullPath);
        }

        var file = $"{_playerRelationsFullPath}/playerRelations.json";
        if (!File.Exists(file))
        {
            await SaveProfileRelationsAsync();
        }
        else
        {
            _playerRelations = await jsonUtil.DeserializeFromFileAsync<ConcurrentDictionary<MongoId, FikaPlayerRelations>>(file);
        }
    }

    public async Task OnPostLoad()
    {
        var profiles = profileHelper.GetProfiles();
        var shouldSave = false;

        foreach (var profileId in profiles.Keys)
        {
            if (!_playerRelations.TryGetValue(profileId, out var value))
            {
                value = new FikaPlayerRelations();
                if (!_playerRelations.TryAdd(profileId, value))
                {
                    logger.Error($"Failed to add {profileId} to relations database");
                    continue;
                }
                shouldSave = true;

                continue;
            }

            var Friends = value.Friends;

            foreach (var friend in Friends.ToList())
            {
                if (!profiles.ContainsKey(friend))
                {
                    Friends.Remove(friend);
                    shouldSave = true;
                }
            }

            var Ignored = value.Ignore;

            foreach (var ignore in Ignored.ToList())
            {
                Ignored.Remove(ignore);
                shouldSave = true;
            }
        }

        if (shouldSave)
        {
            await SaveProfileRelationsAsync();
        }
    }

    public void SaveProfileRelations()
    {
        File.WriteAllText($"{_playerRelationsFullPath}/playerRelations.json", jsonUtil.Serialize(_playerRelations, true));
    }

    public async Task SaveProfileRelationsAsync()
    {
        await File.WriteAllTextAsync($"{_playerRelationsFullPath}/playerRelations.json", jsonUtil.Serialize(_playerRelations, true));
    }

    public FikaPlayerRelations GetStoredValue(string profileId)
    {
        if (!_playerRelations.ContainsKey(profileId))
        {
            StoreValue(profileId, new FikaPlayerRelations());
        }

        return _playerRelations[profileId];
    }

    public void StoreValue(MongoId profileId, FikaPlayerRelations value)
    {
        if (!_playerRelations.TryAdd(profileId, value))
        {
            logger.Error($"Failed to add {profileId} to relations database");
            return;
        }

        SaveProfileRelations();
    }
}
