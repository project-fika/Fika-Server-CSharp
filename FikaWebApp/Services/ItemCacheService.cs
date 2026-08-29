using System.Net;
using FikaShared.Responses;
using FikaWebApp.Models;

namespace FikaWebApp.Services;

public sealed class ItemCacheService(ILogger<ItemCacheService> logger, HttpClient client)
{
    private OrderedDictionary<string, ItemData> Items { get; set; } = [];

    public string[] ItemNames { get; private set; } = [];

    public async Task<bool> PopulateDictionary()
    {
        try
        {
            var result = await client.GetFromJsonAsync<GetItemsResponse>("fika/api/items");
            if (result != null)
            {
                var amount = result.Items.Count;
                Items = new(amount);

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

                    Items.Add(key, value);
                }

                ItemNames = [.. Items.Values.Select(x => x.Name)];

                logger.LogInformation("Loaded {Amount} item(s) to the database", Items.Count);
                return true;
            }

            Items = [];
            logger.LogError("Unable to get items from server");
            return false;
        }
        catch (HttpRequestException httpEx)
        {
            if (httpEx.StatusCode is HttpStatusCode.Forbidden)
            {
                logger.LogError("There was an error retrieving the items from the server: 403 Forbidden. Are you using the wrong API key?");
                return false;
            }

            if (httpEx.StatusCode is HttpStatusCode.NotFound)
            {
                logger.LogError("There was an error retrieving the items from the server: 404 NotFound. Are you missing the Fika server mod?");
                return false;
            }

            logger.LogError("There was a HttpRequestException caught when retrieving the items from the server: {Exception}", httpEx.Message);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError("There was an error retrieving the items from the server: {Exception}", ex.Message);
            return false;
        }
    }

    public ItemData? IdToName(string tpl)
    {
        return Items.TryGetValue(tpl, out var itemData) ? itemData : null;
    }

    public IEnumerable<ItemSearchResultDto> NameToIdSearch(string itemName, int limit = 25)
    {
        if (string.IsNullOrWhiteSpace(itemName))
        {
            return [];
        }

        return Items
            .Where(x => x.Value.Name.Contains(itemName, StringComparison.OrdinalIgnoreCase))
            .Select(x => new ItemSearchResultDto(x.Key, x.Value.Name))
            .Take(limit);
    }

    public bool TryGetItem(string tpl, out ItemData? itemData)
    {
        return Items.TryGetValue(tpl, out itemData);
    }
}