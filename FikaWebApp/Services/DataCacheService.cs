using System.Net;
using FikaShared.Responses;
using FikaWebApp.Models;

namespace FikaWebApp.Services;

public sealed class DataCacheService(ILogger<DataCacheService> logger, HttpClient client)
{
    private OrderedDictionary<string, ItemData> Items { get; set; } = [];
    private OrderedDictionary<string, QuestData> Quests { get; set; } = [];

    private DataSearchResultDto[] _itemSearchCache = [];
    private DataSearchResultDto[] _questSearchCache = [];

    public string[] ItemNames { get; private set; } = [];

    public async Task<bool> PopulateDictionary()
    {
        try
        {
            var result = await client.GetFromJsonAsync<GetDataResponse>("fika/api/data");
            if (result != null)
            {
                var amount = result.Items.Count;
                Items = new OrderedDictionary<string, ItemData>(amount);

                var valueCounts = new Dictionary<string, int>();

                foreach (var (key, value) in result.Items.OrderBy(x => x.Value.Name))
                {
                    if (!valueCounts.TryGetValue(value.Name, out var count))
                    {
                        count = 1;
                        valueCounts[value.Name] = count;
                    }
                    else
                    {
                        count++;
                        valueCounts[value.Name] = count;
                    }

                    value.Name = (count > 1) ? $"{value.Name} ({count})" : value.Name;
                    Items.Add(key, value);
                }

                var questAmount = result.Quests.Count;
                Quests = new OrderedDictionary<string, QuestData>(questAmount);
                var questValueCounts = new Dictionary<string, int>();

                foreach (var (key, value) in result.Quests.OrderBy(x => x.Value.Name))
                {
                    if (!questValueCounts.TryGetValue(value.Name, out var count))
                    {
                        count = 1;
                        questValueCounts[value.Name] = count;
                    }
                    else
                    {
                        count++;
                        questValueCounts[value.Name] = count;
                    }

                    value.Name = (count > 1) ? $"{value.Name} ({count})" : value.Name;
                    Quests.Add(key, value);
                }

                ItemNames = [.. Items.Values.Select(x => x.Name)];
                _itemSearchCache = [.. Items.Select(x => new DataSearchResultDto(x.Key, x.Value.Name))];
                _questSearchCache = [.. Quests.Select(x => new DataSearchResultDto(x.Key, x.Value.Name))];

                logger.LogInformation("Loaded {ItemAmount} item(s) and {QuestAmount} quest(s) to the database", Items.Count, Quests.Count);
                return true;
            }

            Items = [];
            Quests = [];
            logger.LogError("Unable to get items and quests from server");
            return false;
        }
        catch (HttpRequestException httpEx)
        {
            if (httpEx.StatusCode is HttpStatusCode.Forbidden)
            {
                logger.LogError("There was an error retrieving the items/quests from the server: 403 Forbidden. Are you using the wrong API key?");
                return false;
            }

            if (httpEx.StatusCode is HttpStatusCode.NotFound)
            {
                logger.LogError("There was an error retrieving the items/quests from the server: 404 NotFound. Are you missing the Fika server mod?");
                return false;
            }

            logger.LogError("There was a HttpRequestException caught when retrieving the items/quests from the server: {Exception}", httpEx.Message);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError("There was an error retrieving the items/quests from the server: {Exception}", ex.Message);
            return false;
        }
    }

    public ItemData? IdToName(string tpl)
    {
        return Items.TryGetValue(tpl, out var itemData) ? itemData : null;
    }

    public IEnumerable<DataSearchResultDto> SearchItems(string query, int limit = 25)
    {
        return PerformSearch(_itemSearchCache, query, limit);
    }

    public IEnumerable<DataSearchResultDto> SearchQuests(string query, int limit = 25)
    {
        return PerformSearch(_questSearchCache, query, limit);
    }

    private static List<DataSearchResultDto> PerformSearch(ReadOnlySpan<DataSearchResultDto> span, string query, int limit)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        var results = new List<DataSearchResultDto>(limit);
        for (var i = 0; i < span.Length; i++)
        {
            if (span[i].Name.Contains(query, StringComparison.OrdinalIgnoreCase))
            {
                results.Add(span[i]);
                if (results.Count >= limit)
                {
                    break;
                }
            }
        }

        return results;
    }

    public bool TryGetItem(string tpl, out ItemData? itemData)
    {
        return Items.TryGetValue(tpl, out itemData);
    }

    public bool TryGetQuest(string questId, out QuestData? questData)
    {
        return Quests.TryGetValue(questId, out questData);
    }
}