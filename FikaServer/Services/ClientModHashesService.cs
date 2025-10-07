using SPTarkov.DI.Annotations;

namespace FikaServer.Services;

[Injectable(InjectionType.Singleton)]
public class ClientModHashesService
{
    //Todo: ConcurrentDictionary
    private readonly Dictionary<string, uint> _hashes = [];

    public int GetLength()
    {
        return _hashes.Count;
    }

    public bool Exists(string pluginId)
    {
        return _hashes.ContainsKey(pluginId);
    }

    public uint GetHash(string pluginId)
    {
        return _hashes[pluginId];
    }

    public void AddHash(string pluginId, uint hash)
    {
        _hashes.Add(pluginId, hash);
    }
}
