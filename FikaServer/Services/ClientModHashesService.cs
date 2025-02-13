using SptCommon.Annotations;

namespace FikaServer.Services
{
    [Injectable(InjectionType.Singleton)]
    public class ClientModHashesService
    {
        private readonly Dictionary<string, int> hashes = [];

        public int GetLength()
        {
            return hashes.Count;
        }

        public bool Exists(string pluginId)
        {
            return hashes.ContainsKey(pluginId);
        }

        public int GetHash(string pluginId)
        {
            return hashes[pluginId];
        }

        public void AddHash(string pluginId, int hash)
        {
            hashes.Add(pluginId, hash);
        }
    }
}
