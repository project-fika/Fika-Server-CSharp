using System.Text.Json.Serialization;

namespace FikaShared.Responses
{
    public record CreateHeadlessProfileResponse
    {
        [JsonPropertyName("id")]
        public required string Id { get; init; }
    }
}
