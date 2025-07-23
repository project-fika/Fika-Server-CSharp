using FikaShared.Responses;

namespace FikaWebApp.Services
{
    public class ItemCacheService
    {
        public ItemCacheService(ILogger<ItemCacheService> logger, HttpClient client)
        {
            _logger = logger;

            _ = PopulateDictionary(client);
        }

        private async Task PopulateDictionary(HttpClient client)
        {
            try
            {
                var result = await client.GetFromJsonAsync<ItemsResponse>("get/items");
                if (result != null)
                {
                    _itemDict = new(result.Items.Count);

                    var filtered = result.Items
                        .Where(kvp => !string.IsNullOrEmpty(kvp.Value)
                            && !kvp.Value.StartsWith("!!!DO_NOT_USE!!!", StringComparison.Ordinal)
                            && !kvp.Value.StartsWith("!!!DO NOT USE!!!", StringComparison.Ordinal));

                    var valueCounts = new Dictionary<string, int>();

                    foreach (var (key, value) in filtered.OrderBy(x => x.Value))
                    {
                        if (!valueCounts.TryGetValue(value, out int count))
                        {
                            count = 1;
                            valueCounts[value] = count;
                        }
                        else
                        {
                            count++;
                            valueCounts[value] = count;
                        }

                        var newValue = (count > 1) ? $"{value} ({count})" : value;

                        _itemDict.Add(key, newValue);
                    }
                }
                else
                {
                    _itemDict = [];
                    _logger.LogError("Unable to get items from server");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("There was an error retrieving the items from the server: {Exception}", ex);
            }
        }

        public OrderedDictionary<string, string> Items
        {
            get
            {
                return _itemDict;
            }
        }

        private readonly ILogger<ItemCacheService> _logger;
        private OrderedDictionary<string, string> _itemDict;

        public string NameToId(string itemName)
        {
            return _itemDict
                .Where(x => x.Value.Contains(itemName, StringComparison.InvariantCultureIgnoreCase))
                .Select(x => x.Key)
                .FirstOrDefault()!;
        }

        public KeyValuePair<string, string> NameToKvp(string itemName)
        {
            return _itemDict
                .Where(x => x.Value.Contains(itemName, StringComparison.InvariantCultureIgnoreCase))
                .FirstOrDefault();
        }

        public IEnumerable<string> NameToIdSearch(string itemName)
        {
            return _itemDict
                .Where(x => x.Value.Contains(itemName, StringComparison.InvariantCultureIgnoreCase))
                .Select(x => x.Value);
        }
    }
}
