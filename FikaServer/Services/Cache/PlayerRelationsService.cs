using FikaServer.Models.Fika;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Utils;
using System.Collections.Concurrent;
using System.Text.Json;

namespace FikaServer.Services.Cache
{
    [Injectable(InjectionType.Singleton)]
    public class PlayerRelationsService(ProfileHelper profileHelper, ConfigService FikaConfig, ISptLogger<PlayerRelationsService> logger)
    {
        private readonly string _playerRelationsFullPath = Path.Join(FikaConfig.GetModPath(), "database");
        private readonly ConcurrentDictionary<string, FikaPlayerRelations> _playerRelations = [];

        public List<string> Keys
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

        public void OnPreLoad()
        {
            if (!Directory.Exists(_playerRelationsFullPath))
            {
                Directory.CreateDirectory(_playerRelationsFullPath);
            }

            if (!File.Exists($"{_playerRelationsFullPath}/playerRelations.json"))
            {
                SaveProfileRelations();
            }
        }

        public void OnPostLoad()
        {
            Dictionary<string, SptProfile> profiles = profileHelper.GetProfiles();
            bool shouldSave = false;

            foreach (string profileId in profiles.Keys)
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
                SaveProfileRelations();
            }
        }

        private void SaveProfileRelations()
        {
            File.WriteAllText($"{_playerRelationsFullPath}/playerRelations.json", JsonSerializer.Serialize(_playerRelations, ConfigService.serializerOptions));
        }

        public FikaPlayerRelations GetStoredValue(string key)
        {
            if (!_playerRelations.ContainsKey(key))
            {
                StoreValue(key, new FikaPlayerRelations());
            }

            return _playerRelations[key];
        }

        public void StoreValue(string key, FikaPlayerRelations value)
        {
            if (!_playerRelations.TryAdd(key, value))
            {
                logger.Error($"Failed to add {key} to relations database");
                return;
            }

            SaveProfileRelations();
        }
    }
}
