using System.Text.Json.Serialization;

namespace FikaShared.Responses
{
    public record GetItemsResponse
    {
        [JsonPropertyName("items")]
        public required Dictionary<string, ItemData> Items { get; init; } = [];
    }

    public record ItemData
    {
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("description")]
        public required string Description { get; init; }

        [JsonPropertyName("stackable")]
        public required int StackAmount { get; init; }
    }
}
