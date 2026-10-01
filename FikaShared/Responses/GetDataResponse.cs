using System.Text.Json.Serialization;
using FikaShared.Enums;

namespace FikaShared.Responses;

public sealed record GetDataResponse
{
    [JsonPropertyName("items")]
    public required Dictionary<string, ItemData> Items { get; init; } = [];

    [JsonPropertyName("quests")]
    public required Dictionary<string, QuestData> Quests { get; set; } = [];
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
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("completed")]
    public bool? Completed { get; set; }

    [JsonPropertyName("objectives")]
    public List<QuestObjective>? Objectives { get; set; }

    [JsonPropertyName("itemRewards")]
    public List<ItemReward>? ItemRewards { get; set; }

    [JsonPropertyName("traderRewards")]
    public List<TraderReward>? TraderRewards { get; set; }

    [JsonPropertyName("experienceRewards")]
    public List<ExperienceReward>? ExperienceRewards { get; set; }
}

public sealed record QuestObjective
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("progress")]
    public double? Progress { get; set; }

    [JsonPropertyName("target")]
    public double? Target { get; set; }

    [JsonPropertyName("state")]
    public EQuestState? State { get; set; }
}

public sealed record ItemReward
{
    [JsonPropertyName("amount")]
    public double? Amount { get; set; }

    [JsonPropertyName("itemId")]
    public required string ItemId { get; set; }
}

public sealed record TraderReward
{
    [JsonPropertyName("amount")]
    public double? Amount { get; set; }

    [JsonPropertyName("traderId")]
    public required string TraderId { get; set; }
}

public sealed record ExperienceReward
{
    [JsonPropertyName("amount")]
    public double? Amount { get; set; }
}