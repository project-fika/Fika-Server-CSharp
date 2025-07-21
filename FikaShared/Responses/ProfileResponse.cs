using System.Text.Json.Serialization;

namespace FikaShared.Responses
{
    public record ProfileResponse
    {
        [JsonPropertyName("nickname")]
        public string Nickname { get; set; } = string.Empty;

        [JsonPropertyName("profileId")]
        public string ProfileId { get; set; } = string.Empty;

        [JsonPropertyName("hasFleaBan")]
        public bool HasFleaBan { get; set; }

        [JsonPropertyName("level")]
        public int Level { get; set; }
    }
}
