using System.Text.Json.Serialization;

namespace FikaShared.Responses
{
    public record ItemsResponse
    {
        [JsonPropertyName("items")]
        public required Dictionary<string, string> Items { get; set; } = [];

        [JsonPropertyName("descriptions")]
        public required Dictionary<string, string> Descriptions { get; set; } = [];
    }
}
