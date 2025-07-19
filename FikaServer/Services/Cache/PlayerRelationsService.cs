using FikaServer.Models.Fika;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;
using System.Collections.Concurrent;

namespace FikaServer.Services.Cache
{
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

            string file = $"{_playerRelationsFullPath}/playerRelations.json";
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
            Dictionary<MongoId, SptProfile> profiles = profileHelper.GetProfiles();
            bool shouldSave = false;

            foreach (MongoId profileId in profiles.Keys)
            {
                if (!_playerRelations.TryGetValue(profileId, out FikaPlayerRelations? value))
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

                List<string> Friends = value.Friends;

                foreach (string friend in Friends.ToList())
                {
                    if (!profiles.ContainsKey(friend))
                    {
                        Friends.Remove(friend);
                        shouldSave = true;
                    }
                }

                List<string> Ignored = value.Ignore;

                foreach (string ignore in Ignored.ToList())
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
}
