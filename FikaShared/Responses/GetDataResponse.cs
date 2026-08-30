using System.Text.Json.Serialization;

namespace FikaShared.Responses;

public sealed record GetDataResponse
{
    [JsonPropertyName("items")]
    public required Dictionary<string, ItemData> Items { get; init; } = [];

    [JsonPropertyName("quests")]
    public required Dictionary<string, QuestData> Quests { get; set; }
}

public sealed record ItemData
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }

    [JsonPropertyName("stackable")]
    public required int StackAmount { get; init; }
}


public sealed record QuestData
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("description")]
    public required string Description { get; set; }

    [JsonPropertyName("objectives")]
    public required List<QuestObjective> Objectives { get; set; }
}

public sealed record QuestObjective([property: JsonPropertyName("description")] string Description);