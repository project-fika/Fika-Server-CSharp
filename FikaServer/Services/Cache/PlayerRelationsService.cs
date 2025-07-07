using FikaServer.Models.Fika;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
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

        public void OnPreLoad()
        {
            if (!Directory.Exists(_playerRelationsFullPath))
            {
                Directory.CreateDirectory(_playerRelationsFullPath);
            }

            string file = $"{_playerRelationsFullPath}/playerRelations.json";
            if (!File.Exists(file))
            {
                SaveProfileRelations();
            }
            else
            {
                string data = File.ReadAllText(file);
                _playerRelations = JsonSerializer.Deserialize<ConcurrentDictionary<MongoId, FikaPlayerRelations>>(data, ConfigService.SerializerOptions);
            }
        }

        public void OnPostLoad()
        {
            Dictionary<MongoId, SptProfile> profiles = profileHelper.GetProfiles();
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

        public void SaveProfileRelations()
        {
            File.WriteAllText($"{_playerRelationsFullPath}/playerRelations.json", JsonSerializer.Serialize(_playerRelations, ConfigService.SerializerOptions));
        }

        public FikaPlayerRelations GetStoredValue(MongoId profileId)
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
