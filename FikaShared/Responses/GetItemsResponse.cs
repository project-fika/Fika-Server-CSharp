using System.Text.Json.Serialization;

namespace FikaShared.Responses
{
    public record GetItemsResponse
    {
        [JsonPropertyName("items")]
        public required Dictionary<string, ItemData> Items { get; set; } = [];
    }

    public record ItemData
    {
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("description")]
        public required string Description { get; set; }

        [JsonPropertyName("stackable")]
        public required int StackAmount { get; set; }
    }
}
