using FikaShared.Responses;

namespace FikaWebApp.Services
{
    public class ItemCacheService
    {
        public ItemCacheService(ILogger<ItemCacheService> logger, HttpClient client)
        {
            _logger = logger;
            _client = client;

            _ = PopulateDictionary();
        }

        private async Task PopulateDictionary()
        {
            try
            {
                var result = await _client.GetFromJsonAsync<ItemsResponse>("get/items");
                if (result != null)
                {
                    var amount = result.Items.Count;
                    _itemDict = new(amount);

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

                    _itemDescriptions = [];
                    foreach (var item in _itemDict.Keys)
                    {
                        if (result.Descriptions.TryGetValue(item, out string? description))
                        {
                            _itemDescriptions.Add(item, description);
                        }
                    }

                    _logger.LogInformation("Loaded {Amount} item(s) to the database, {Filtered} were filtered out", _itemDict.Count, amount - _itemDict.Count);
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
        private readonly HttpClient _client;
        private OrderedDictionary<string, string> _itemDict;
        private Dictionary<string, string> _itemDescriptions;

        public string IdToName(string tpl)
        {
            return _itemDict[tpl];
        }

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

        public string GetDescription(string tpl)
        {
            if (_itemDescriptions.TryGetValue(tpl, out string? desc))
            {
                return desc;
            }

            return string.Empty;
        }
    }
}
