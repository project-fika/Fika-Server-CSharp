using FikaShared.Responses;

namespace FikaWebApp.Services;

public class ItemCacheService(ILogger<ItemCacheService> logger, HttpClient client)
{
    public async Task PopulateDictionary()
    {
        try
        {
            var result = await client.GetFromJsonAsync<GetItemsResponse>("get/items");
            if (result != null)
            {
                var amount = result.Items.Count;
                _itemDict = new(amount);

                var valueCounts = new Dictionary<string, int>();

                foreach (var (key, value) in result.Items.OrderBy(x => x.Value.Name))
                {
                    if (!valueCounts.TryGetValue(value.Name, out int count))
                    {
                        count = 1;
                        valueCounts[value.Name] = count;
                    }
                    else
                    {
                        count++;
                        valueCounts[value.Name] = count;
                    }

                    var newValue = (count > 1) ? $"{value.Name} ({count})" : value.Name;
                    value.Name = newValue;

                    _itemDict.Add(key, value);
                }

                _names = [.. _itemDict.Values
                    .Select(x => x.Name)];

                logger.LogInformation("Loaded {Amount} item(s) to the database", _itemDict.Count);
            }
            else
            {
                _itemDict = [];
                logger.LogError("Unable to get items from server");
            }
        }
        catch (Exception ex)
        {
            logger.LogError("There was an error retrieving the items from the server: {Exception}", ex);
        }
    }

    public OrderedDictionary<string, ItemData> Items
    {
        get
        {
            return _itemDict;
        }
    }

    public string[] ItemNames
    {
        get
        {
            return _names;
        }
    }

    private OrderedDictionary<string, ItemData> _itemDict = [];
    private string[] _names = [];

    public ItemData IdToName(string tpl)
    {
        return _itemDict[tpl];
    }

    public string NameToId(string itemName)
    {
        return _itemDict
            .Where(x => x.Value.Name.Contains(itemName, StringComparison.InvariantCultureIgnoreCase))
            .Select(x => x.Key)
            .FirstOrDefault()!;
    }

    public ItemData NameToData(string itemName)
    {
        return _itemDict.
            Where(x => x.Value.Name.Contains(itemName, StringComparison.InvariantCultureIgnoreCase))
            .Select(x => x.Value)
            .FirstOrDefault()!;
    }

    public KeyValuePair<string, ItemData> NameToKvp(string itemName)
    {
        return _itemDict
            .Where(x => x.Value.Name.Contains(itemName, StringComparison.InvariantCultureIgnoreCase))
            .FirstOrDefault();
    }

    public IEnumerable<string> NameToIdSearch(string itemName)
    {
        return _itemDict
            .Where(x => x.Value.Name.Contains(itemName, StringComparison.InvariantCultureIgnoreCase))
            .Select(x => x.Value.Name);
    }
}
