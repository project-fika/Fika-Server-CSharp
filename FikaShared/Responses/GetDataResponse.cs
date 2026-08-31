using System.Text.Json.Serialization;
using FikaShared.Enums;

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

public sealed record ActiveQuestData(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("completed")] bool Completed,
    [property: JsonPropertyName("objectives")] List<ActiveObjectiveData> Objectives
);

public sealed record DetailedQuestData(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("completed")] bool Completed,
    [property: JsonPropertyName("objectives")] List<DetailedQuestObjective> Objectives
);

public sealed record QuestObjective(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("description")] string Description
);

public sealed record ActiveObjectiveData(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("progress")] int Progress,
    [property: JsonPropertyName("target")] int Target,
    [property: JsonPropertyName("state")] EQuestState State
);

public sealed record DetailedQuestObjective(
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("progress")] int Progress,
    [property: JsonPropertyName("target")] int Target,
    [property: JsonPropertyName("state")] EQuestState State
);