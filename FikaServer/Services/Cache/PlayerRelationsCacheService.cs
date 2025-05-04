using FikaServer.Models.Fika;
using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Profile;
using System.Text.Json;

namespace FikaServer.Services.Cache
{
    [Injectable(InjectionType.Singleton)]
    public class PlayerRelationsCacheService(ProfileHelper profileHelper, ConfigService FikaConfig)
    {
        private string playerRelationsFullPath = Path.Join(FikaConfig.GetModPath(), "cache");
        private Dictionary<string, FikaPlayerRelations> playerRelationsCache = [];

        public void PreSptLoad()
        {
            if (!Directory.Exists(playerRelationsFullPath))
            {
                Directory.CreateDirectory(playerRelationsFullPath);
            }

            if (!File.Exists($"{playerRelationsFullPath}/playerRelations.json"))
            {
                SaveProfileRelations();
            }
        }

        public void PostSptLoad()
        {
            Dictionary<string, SptProfile> profiles = profileHelper.GetProfiles();
            bool shouldSave = false;

            foreach (string profileId in profiles.Keys)
            {
                if (!playerRelationsCache.ContainsKey(profileId))
                {
                    playerRelationsCache.Add(profileId, new FikaPlayerRelations());
                    shouldSave = true;

                    continue;
                }

                List<string> Friends = this.playerRelationsCache[profileId].Friends;

                foreach (string friend in Friends.ToList())
                {
                    if (!profiles.ContainsKey(friend))
                    {
                        Friends.Remove(friend);
                        shouldSave = true;
                    }
                }

                List<string> Ignored = this.playerRelationsCache[profileId].Ignore;

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
            File.WriteAllText($"{playerRelationsFullPath}/playerRelations.json", JsonSerializer.Serialize(playerRelationsCache, ConfigService.serializerOptions));
        }

        public List<string> GetKeys()
        {
            return [.. playerRelationsCache.Keys];
        }

        public FikaPlayerRelations GetStoredValue(string key)
        {
            if (!playerRelationsCache.ContainsKey(key))
            {
                StoreValue(key, new FikaPlayerRelations());
            }

            return playerRelationsCache[key];
        }

        public void StoreValue(string key, FikaPlayerRelations value)
        {
            this.playerRelationsCache.Add(key, value);

            SaveProfileRelations();
        }
    }
}
