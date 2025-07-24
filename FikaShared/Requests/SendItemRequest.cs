using System.Text.Json.Serialization;

namespace FikaShared.Requests
{
    public record SendItemRequest : BaseSendItemRequest
    {
        [JsonPropertyName("profileId")]
        public required string ProfileId { get; set; }
    }
}
