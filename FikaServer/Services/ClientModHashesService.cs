using SPTarkov.DI.Annotations;

namespace FikaServer.Services;

[Injectable(InjectionType.Singleton)]
public class ClientModHashesService
{
    //Todo: ConcurrentDictionary
    private readonly Dictionary<string, int> _hashes = [];

    public int GetLength()
    {
        return _hashes.Count;
    }

    public bool Exists(string pluginId)
    {
        return _hashes.ContainsKey(pluginId);
    }

    public int GetHash(string pluginId)
    {
        return _hashes[pluginId];
    }

    public void AddHash(string pluginId, int hash)
    {
        _hashes.Add(pluginId, hash);
    }
}
