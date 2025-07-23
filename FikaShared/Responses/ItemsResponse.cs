using System.Text.Json.Serialization;

namespace FikaShared.Responses
{
    public record ItemsResponse
    {
        [JsonPropertyName("items")]
        public required Dictionary<string, string> Items { get; set; } = [];
    }
}
